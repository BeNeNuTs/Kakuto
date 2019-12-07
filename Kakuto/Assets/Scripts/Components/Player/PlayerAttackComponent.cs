using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackComponent : MonoBehaviour
{
    struct TriggeredInput
    {
        private readonly char m_Input;
        private readonly float m_TriggeredTime;

        public TriggeredInput(char input, float time)
        {
            m_Input = input;
            m_TriggeredTime = time;
        }

        public char GetInput() { return m_Input; }

        public bool IsElapsed(float time, float persistency)
        {
            return time > (m_TriggeredTime + persistency);
        }
    }

    public PlayerAttacksConfig m_AttacksConfig;
    private List<PlayerBaseAttackLogic> m_AttackLogics;

    private PlayerMovementComponent m_MovementComponent;

    private List<TriggeredInput> m_TriggeredInputsList;
    private string m_TriggeredInputsString;

    private PlayerBaseAttackLogic m_CurrentAttackLogic;

    private UnblockAttackAnimEventConfig m_UnblockAttackConfig = null;
    private bool m_IsAttackBlocked = false;
    private bool m_IsAttackBlockedByTakeOff = false;

    private float m_TimeToWaitBeforeEvaluatingAttacks = 0f;
    private float m_TotalTimeWaitingBeforeEvaluatingAttacks = 0f;

    private List<Collider2D> m_HitBoxes;

    /// DEBUG
    [Header("Debug")]
    [Space]

    public bool m_DEBUG_BreakOnTriggerAttack = false;
    /// DEBUG

    void Awake()
    {
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_TriggeredInputsList = new List<TriggeredInput>();

        if(m_AttacksConfig)
        {
            m_AttacksConfig.Init();
            m_AttackLogics = m_AttacksConfig.CreateLogics(this);
        }

        InitHitBoxes();
        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.BlockAttack, BlockAttack);
        Utils.GetPlayerEventManager<UnblockAttackAnimEvent>(gameObject).StartListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunEnd, OnStunEnd);

        Utils.GetEnemyEventManager<DamageTakenInfo>(gameObject).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);

        RoundSubGameManager.OnRoundOver += OnRoundOver;
    }

    void OnDestroy()
    {
        UnregisterListeners();

        if (m_AttacksConfig)
        {
            m_AttacksConfig.Shutdown();
        }

        foreach (PlayerBaseAttackLogic attackLogic in m_AttackLogics)
        {
            if (attackLogic != null)
            {
                attackLogic.OnShutdown();
            }
        }
        m_AttackLogics.Clear();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.BlockAttack, BlockAttack);
        Utils.GetPlayerEventManager<UnblockAttackAnimEvent>(gameObject).StopListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunEnd, OnStunEnd);

        Utils.GetEnemyEventManager<DamageTakenInfo>(gameObject).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);

        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void Update()
    {
        if (CanUpdateInputs())
        {
            UpdateTriggerInputList();
            UpdateTriggerInputString();
        }
    }

    bool CanUpdateInputs()
    {
        if (!m_IsAttackBlocked || (m_IsAttackBlocked && m_UnblockAttackConfig != null))
        {
            return true;
        }

        return false;
    }

    void UpdateTriggerInputList()
    {
        float currentTime = Time.unscaledTime;

        string attackInputs = InputManager.GetAttackInputString(m_MovementComponent.GetPlayerIndex(), m_MovementComponent.IsLeftSide());
        foreach (char c in attackInputs)
        {
            m_TriggeredInputsList.Add(new TriggeredInput(c, currentTime));
        }

        while (m_TriggeredInputsList.Count > AttackConfig.Instance.m_MaxInputs)
        {
            m_TriggeredInputsList.RemoveAt(0);
        }

        m_TriggeredInputsList.RemoveAll(input => input.IsElapsed(currentTime, AttackConfig.Instance.m_InputPersistency));

        if(attackInputs.Length > 0)
        {
            m_TimeToWaitBeforeEvaluatingAttacks = AttackConfig.Instance.TimeToWaitBeforeEvaluatingAttacks;
        }
    }

    void UpdateTriggerInputString()
    {
        m_TriggeredInputsString = string.Empty;
        foreach (TriggeredInput triggeredInput in m_TriggeredInputsList)
        {
            m_TriggeredInputsString += triggeredInput.GetInput();
        }
    }

    void ClearTriggeredInputs()
    {
        m_TriggeredInputsList.Clear();
        m_TriggeredInputsString = "";
    }

    void LateUpdate()
    {
        if(CanUpdateAttack())
        {
            UpdateAttack();

            m_TimeToWaitBeforeEvaluatingAttacks = 0f;
            m_TotalTimeWaitingBeforeEvaluatingAttacks = 0f;
        }
        else
        {
            m_TotalTimeWaitingBeforeEvaluatingAttacks += Time.deltaTime;
            m_TimeToWaitBeforeEvaluatingAttacks -= Time.deltaTime;
        }
        
    }

    bool CanUpdateAttack()
    {
        // Time condition
        if (m_TimeToWaitBeforeEvaluatingAttacks <= 0 || m_TotalTimeWaitingBeforeEvaluatingAttacks > AttackConfig.Instance.MaxTimeToWaitBeforeEvaluatingAttacks)
        {
            // Input condition
            if (m_TriggeredInputsList.Count > 0)
            {
                // Block/Unblock condition
                if (!m_IsAttackBlocked)
                {
                    return true;
                }

                if (m_IsAttackBlocked && m_UnblockAttackConfig != null)
                {
                    if (m_CurrentAttackLogic.HasHit())
                    {
                        // Can update attacks and cancel the current one if it has hit the enemy
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void UpdateAttack()
    {
        foreach (PlayerBaseAttackLogic attackLogic in m_AttackLogics)
        {
            if (EvaluateAttackConditions(attackLogic) && CheckAttackInputs(attackLogic))
            {
                bool canTriggerAttack = true;
                if (m_IsAttackBlocked && m_UnblockAttackConfig != null)
                {
                    canTriggerAttack = m_UnblockAttackConfig.m_AllowedAttacks.Contains(attackLogic.GetAttack().m_AnimationAttackName);
                }

                if(canTriggerAttack)
                {
                    TriggerAttack(attackLogic);
                    break;
                }
            }
        }
    }

    bool EvaluateAttackConditions(PlayerBaseAttackLogic attackLogic)
    {
        return attackLogic.EvaluateConditions(m_CurrentAttackLogic);
    }

    bool CheckAttackInputs(PlayerBaseAttackLogic attackLogic)
    {
        foreach(string inputString in attackLogic.GetAttack().GetInputStringList())
        {
            if (m_TriggeredInputsString.Contains(inputString))
            {
                return true;
            }
        }
        return false;
    }

    void TriggerAttack(PlayerBaseAttackLogic attackLogic)
    {
        // If current attack logic is != null, this means we're canceling this attack by another one
        // In that case, we need to trigger EndOfAttack of this current attack before triggering the other one
        // As the EndOfAttack triggered by the animation will happen only 1 frame later and the current attack logic will already be replaced
        if(m_CurrentAttackLogic != null)
        {
            EndOfAttack(m_CurrentAttackLogic.GetAttack().m_AnimationAttackName);
        }

        PlayerAttack attack = attackLogic.GetAttack();

        ClearTriggeredInputs();

        attackLogic.OnAttackLaunched();
        m_CurrentAttackLogic = attackLogic;

        m_IsAttackBlocked = true;
        m_UnblockAttackConfig = null;
        m_MovementComponent.SetMovementBlockedByAttack(attack.m_BlockMovement);

        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.AttackLaunched, m_CurrentAttackLogic);

        // DEBUG ///////////////////////////////////
        if (m_DEBUG_BreakOnTriggerAttack)
        {
            Debug.Break();
        }
        ////////////////////////////////////////////
    }

    void OnEnemyTakesDamage(DamageTakenInfo damageTakenInfo)
    {
        // If enemy takes damage from the current attack logic
        if(damageTakenInfo.m_AttackLogic == m_CurrentAttackLogic)
        {
            if (m_CurrentAttackLogic.CanPushBack())
            {
                float pushBackForce = m_CurrentAttackLogic.GetAttackerPushBackForce(false);
                if (pushBackForce > 0.0f && m_MovementComponent)
                {
                    m_MovementComponent.PushBack(pushBackForce);
                }
            }
        }
    }

    bool CheckIsCurrentAttack(EAnimationAttackName attackName, string methodName)
    {
        if (m_CurrentAttackLogic == null)
        {
            Debug.LogError("There is no current attack");
            return false;
        }

        PlayerAttack currentAttack = m_CurrentAttackLogic.GetAttack();
        if (currentAttack.m_AnimationAttackName != attackName)
        {
            Debug.LogError("Trying to " + methodName + " from " + attackName.ToString() + " but current attack is " + currentAttack.m_AnimationAttackName.ToString());
            return false;
        }
        return true;
    }

    void EndOfAttack(EAnimationAttackName attackName)
    {
        if(CheckIsCurrentAttack(attackName, "EndOfAttack"))
        {
            PlayerAttack currentAttack = m_CurrentAttackLogic.GetAttack();

            bool attackWasBlocked = m_IsAttackBlocked;
            m_IsAttackBlocked = false;

            // If attack was blocked and there is still unblock attack parameters
            // This means no allowed attack have been triggered
            // So need to clear triggered inputs in order to not launch unwanted attacks
            if(attackWasBlocked && m_UnblockAttackConfig != null)
            {
                ClearTriggeredInputs();
                m_UnblockAttackConfig = null;
            }

            m_CurrentAttackLogic.OnAttackStopped();
            m_CurrentAttackLogic = null;

            // Prevent to keep hit/grab boxes enabled if cancelling an attack by another while hit/grab boxes still enabled
            DisableAllHitBoxes(); 
        }
    }

    void BlockAttack(EAnimationAttackName attackName)
    {
        if (CheckIsCurrentAttack(attackName, "BlockAttack"))
        {
            if (m_IsAttackBlocked && m_UnblockAttackConfig == null)
            {
                Debug.LogError("Attack was already blocked by " + attackName);
                return;
            }

            ClearTriggeredInputs();
            m_IsAttackBlocked = true;
            m_UnblockAttackConfig = null;
        }
    }

    void UnblockAttack(UnblockAttackAnimEvent unblockEvent)
    {
        if (CheckIsCurrentAttack(unblockEvent.m_AttackToUnblock, "UnblockAttack"))
        {
            if(m_IsAttackBlocked == false)
            {
                Debug.LogError("Attack was not blocked by " + unblockEvent.m_AttackToUnblock);
                return;
            }

            m_UnblockAttackConfig = unblockEvent.m_Config;
        }
    }

    void OnStunBegin(float stunTimeStamp)
    {
        ClearTriggeredInputs();
        m_IsAttackBlocked = true;
    }

    void OnStunEnd(float stunTimeStamp)
    {
        if(m_IsAttackBlocked == false)
        {
            Debug.LogError("Attack was not blocked by stun");
            return;
        }
        m_IsAttackBlocked = false;
        m_UnblockAttackConfig = null;
    }

    public void SetAttackBlockedByTakeOff(bool attackBlocked)
    {
        if(attackBlocked)
        {
            OnStunBegin(0f);
            m_IsAttackBlockedByTakeOff = true;
        }
        else if(m_IsAttackBlockedByTakeOff)
        {
            OnStunEnd(0f);
            m_IsAttackBlockedByTakeOff = false;
        }
    }

    void OnRoundOver()
    {
        enabled = false;
        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void InitHitBoxes()
    {
        int hitBoxLayer = LayerMask.NameToLayer("HitBox");
        int grabBoxLayer = LayerMask.NameToLayer("GrabBox");

        int colliderLayer = 0;
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        m_HitBoxes = new List<Collider2D>(allColliders.Length);
        foreach (Collider2D collider in allColliders)
        {
            colliderLayer = collider.gameObject.layer;
            if (colliderLayer == hitBoxLayer || colliderLayer == grabBoxLayer)
            {
                m_HitBoxes.Add(collider);
            }
        }
    }

    void DisableAllHitBoxes()
    {
        if(m_HitBoxes.Count == 0)
        {
            Debug.LogError("HitBoxes have not been initialized.");
        }

        foreach(Collider2D hitBox in m_HitBoxes)
        {
            if(hitBox != null)
            {
                hitBox.enabled = false;
            }   
        }
    }

    public PlayerBaseAttackLogic GetCurrentAttackLogic()
    {
        return m_CurrentAttackLogic;
    }

    public PlayerAttack GetCurrentAttack()
    {
        if(m_CurrentAttackLogic != null)
        {
            return m_CurrentAttackLogic.GetAttack();
        }
        return null;
    }
}
