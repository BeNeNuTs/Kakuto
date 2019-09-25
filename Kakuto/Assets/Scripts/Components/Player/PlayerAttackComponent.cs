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

    private UnblockAttackAnimEventParameters m_UnblockAttackParameters = null;
    private bool m_IsAttackBlocked = false;

    private uint m_FramesToWaitBeforeEvaluatingAttacksCount = 0;
    private uint m_TotalFramesWaitingBeforeEvaluatingAttacksCount = 0;

    void Awake()
    {
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_TriggeredInputsList = new List<TriggeredInput>();

        if(m_AttacksConfig)
        {
            m_AttacksConfig.Init();
            m_AttackLogics = m_AttacksConfig.CreateLogics(this);
        }

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.BlockAttack, BlockAttack);
        Utils.GetPlayerEventManager<UnblockAttackAnimEventParameters>(gameObject).StartListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunEnd, OnStunEnd);
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
        Utils.GetPlayerEventManager<UnblockAttackAnimEventParameters>(gameObject).StopListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    void Update()
    {
        if (m_IsAttackBlocked && m_UnblockAttackParameters == null)
        {
            return;
        }

        UpdateTriggerInputList();
        UpdateTriggerInputString();
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
            m_FramesToWaitBeforeEvaluatingAttacksCount = AttackConfig.Instance.m_FramesToWaitBeforeEvaluatingAttacks;
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
        if(m_IsAttackBlocked && m_UnblockAttackParameters == null)
        {
            return;
        }

        if(CanUpdateAttack())
        {
            UpdateAttack();

            m_FramesToWaitBeforeEvaluatingAttacksCount = 0;
            m_TotalFramesWaitingBeforeEvaluatingAttacksCount = 0;
        }
        else
        {
            m_TotalFramesWaitingBeforeEvaluatingAttacksCount++;
            m_FramesToWaitBeforeEvaluatingAttacksCount--;
        }
        
    }

    bool CanUpdateAttack()
    {
        return m_FramesToWaitBeforeEvaluatingAttacksCount == 0 || m_TotalFramesWaitingBeforeEvaluatingAttacksCount > AttackConfig.Instance.m_MaxFramesToWaitBeforeEvaluatingAttacks;
    }

    void UpdateAttack()
    {
        if (m_TriggeredInputsList.Count > 0)
        {
            foreach (PlayerBaseAttackLogic attackLogic in m_AttackLogics)
            {
                if (EvaluateAttackConditions(attackLogic) && CheckAttackInputs(attackLogic))
                {
                    bool canTriggerAttack = true;
                    if (m_IsAttackBlocked && m_UnblockAttackParameters != null)
                    {
                        canTriggerAttack = m_UnblockAttackParameters.m_AllowedAttacks.Contains(attackLogic.GetAttack().m_AnimationAttackName);
                    }

                    if(canTriggerAttack)
                    {
                        TriggerAttack(attackLogic);
                        break;
                    }
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
        PlayerAttack attack = attackLogic.GetAttack();

        ClearTriggeredInputs();

        attackLogic.OnAttackLaunched();
        m_CurrentAttackLogic = attackLogic;

        m_IsAttackBlocked = true;
        m_UnblockAttackParameters = null;
        m_MovementComponent.SetMovementBlockedByAttack(attack.m_BlockMovement);

        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.AttackLaunched, m_CurrentAttackLogic);
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
            if(attackWasBlocked && m_UnblockAttackParameters != null)
            {
                ClearTriggeredInputs();
                m_UnblockAttackParameters = null;
            }

            m_CurrentAttackLogic.OnAttackStopped();
            m_CurrentAttackLogic = null;
        }
    }

    void BlockAttack(EAnimationAttackName attackName)
    {
        if (CheckIsCurrentAttack(attackName, "BlockAttack"))
        {
            if (m_IsAttackBlocked)
            {
                Debug.LogError("Attack was already blocked by " + attackName);
                return;
            }

            ClearTriggeredInputs();
            m_IsAttackBlocked = true;
        }
    }

    void UnblockAttack(UnblockAttackAnimEventParameters param)
    {
        if (CheckIsCurrentAttack(param.m_UnblockAttackName, "UnblockAttack"))
        {
            if(m_IsAttackBlocked == false)
            {
                Debug.LogError("Attack was not blocked by " + param.m_UnblockAttackName);
                return;
            }

            m_UnblockAttackParameters = param;
        }
    }

    void OnStunBegin(float stunTimeStamp)
    {
        ClearTriggeredInputs();

        if(m_CurrentAttackLogic != null)
        {
            m_CurrentAttackLogic.OnAttackStopped();
            m_CurrentAttackLogic = null;
        }
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
        m_UnblockAttackParameters = null;
    }

    public string GetTriggeredInputString()
    {
        return m_TriggeredInputsString;
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
