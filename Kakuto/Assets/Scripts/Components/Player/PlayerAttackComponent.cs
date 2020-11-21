using System.Collections.Generic;
using UnityEngine;

public enum EAttackState
{
    Startup,
    Active,
    Recovery
}

public class PlayerAttackComponent : MonoBehaviour
{
    public PlayerAttacksConfig m_AttacksConfig;
    private List<PlayerBaseAttackLogic> m_AttackLogics;

    private PlayerMovementComponent m_MovementComponent;
    private PlayerHealthComponent m_HealthComponent;
    private PlayerInfoComponent m_InfoComponent;
    private Animator m_Animator;

    private PlayerSuperGaugeSubComponent m_SuperGaugeSC;
    private PlayerComboCounterSubComponent m_ComboCounterSC;

    private List<TriggeredGameInput> m_TriggeredInputsList;
    private string m_TriggeredInputsString;

    private PlayerBaseAttackLogic m_CurrentAttackLogic;
    private EAttackState m_CurrentAttackState = EAttackState.Startup;

    private UnblockAttackAnimEventConfig m_UnblockAttackConfig = null;
    private bool m_IsAttackBlocked = false;
    private bool m_IsAttackBlockedByTakeOff = false;
    private bool m_IsAttackTriggeredInAir = false;

    private int m_FramesToWaitBeforeEvaluatingAttacks = 0;
    private int m_TotalFramesWaitingBeforeEvaluatingAttacks = 0;

    private List<Collider2D> m_HitBoxes;

#if UNITY_EDITOR || DEBUG_DISPLAY
    /// DEBUG
    [Separator("Debug")]
    [Space]

    public bool m_DEBUG_BreakOnTriggerAttack = false;
    /// DEBUG
#endif

    void Awake()
    {
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_HealthComponent = GetComponent<PlayerHealthComponent>();
        m_InfoComponent = GetComponent<PlayerInfoComponent>();
        m_Animator = GetComponentInChildren<Animator>();

        m_SuperGaugeSC = new PlayerSuperGaugeSubComponent(m_InfoComponent);
        m_ComboCounterSC = new PlayerComboCounterSubComponent(gameObject);
        m_TriggeredInputsList = new List<TriggeredGameInput>();

        if (m_AttacksConfig)
        {
            m_AttacksConfig.Init();
            m_AttackLogics = m_AttacksConfig.CreateLogics(this);
        }

        InitHitBoxes();
        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.BlockAttack, BlockAttack);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.StunEnd, OnStunEnd);

        Utils.GetEnemyEventManager(gameObject).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);

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

        m_SuperGaugeSC.OnDestroy();
        m_ComboCounterSC.OnDestroy();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.BlockAttack, BlockAttack);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.StunEnd, OnStunEnd);

        Utils.GetEnemyEventManager(gameObject).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);

        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void Update()
    {
        if(!m_InfoComponent.GetPlayerSettings().m_AttackEnabled)
        {
            return;
        }

        UpdateTriggerInputsList();
        UpdateTriggerInputsString();

        if(!IsPlayerTimeFrozen())
        {
            UpdateAttackState();

            if (CanUpdateAttack())
            {
                UpdateAttack();

                m_FramesToWaitBeforeEvaluatingAttacks = 0;
                m_TotalFramesWaitingBeforeEvaluatingAttacks = 0;
            }
            else
            {
                if (m_TriggeredInputsList.Count > 0)
                {
                    m_TotalFramesWaitingBeforeEvaluatingAttacks++;
                    m_FramesToWaitBeforeEvaluatingAttacks--;
                }
                else
                {
                    m_FramesToWaitBeforeEvaluatingAttacks = 0;
                    m_TotalFramesWaitingBeforeEvaluatingAttacks = 0;
                }
            }
        }
    }

    void UpdateTriggerInputsList()
    {
        int currentFrame = Time.frameCount;

        List<GameInput> attackInputs = InputManager.GetAttackInputList(m_InfoComponent.GetPlayerIndex(), m_MovementComponent.IsLeftSide());

        // If there is new inputs
        if (attackInputs.Count > 0) 
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, "New attack inputs : " + attackInputs.ToStringList());
            if(m_TriggeredInputsList.Count > 0)
            {
                ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, "Adding input frame persistency bonus (" + AttackConfig.Instance.InputFramesPersistencyBonus + ") to current attack inputs : " + m_TriggeredInputsList.ToStringList());
            }

            // We need to add persistency bonus to the previous ones
            foreach (TriggeredGameInput triggeredInput in m_TriggeredInputsList)
            {
                triggeredInput.AddPersistency(AttackConfig.Instance.InputFramesPersistencyBonus);
            }
        }

        if(IsPlayerTimeFrozen())
        {
            // If time is frozen, notify previous inputs
            foreach (TriggeredGameInput triggeredInput in m_TriggeredInputsList)
            {
                triggeredInput.OnTimeFreeze();
            }
        }
        
        // Then add those new inputs in the list with the default persistency
        foreach (GameInput input in attackInputs)
        {
            m_TriggeredInputsList.Add(new TriggeredGameInput(input, currentFrame));
        }

        // Remove those which exceeds max allowed inputs
        if (m_TriggeredInputsList.Count > AttackConfig.Instance.m_MaxInputs)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, "Remove attack inputs (reason : exceeds max inputs allowed) : " + m_TriggeredInputsList.GetRange(0, (int)(m_TriggeredInputsList.Count - AttackConfig.Instance.m_MaxInputs)).ToStringList());
        }
        while (m_TriggeredInputsList.Count > AttackConfig.Instance.m_MaxInputs)
        {   
            m_TriggeredInputsList.RemoveAt(0);
        }

        // Also remove those which have their persistency elapsed
        int triggeredInputsElapsedCount = m_TriggeredInputsList.RemoveAll(input => input.IsElapsed(currentFrame));

        if (attackInputs.Count > 0)
        {
            m_FramesToWaitBeforeEvaluatingAttacks = AttackConfig.Instance.FramesToWaitBeforeEvaluatingAttacks;
        }

        if(triggeredInputsElapsedCount > 0 || attackInputs.Count > 0)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, m_TriggeredInputsList.Count == 0 ? "No more attack input" : "Current attack inputs : " + m_TriggeredInputsList.ToStringList());
        }
    }

    void UpdateTriggerInputsString()
    {
        m_TriggeredInputsString = string.Empty;
        foreach (TriggeredGameInput triggeredInput in m_TriggeredInputsList)
        {
            m_TriggeredInputsString += triggeredInput.GetInputString();
        }
    }

    public string GetTriggeredInputsString()
    {
        return m_TriggeredInputsString;
    }

    bool IsPlayerTimeFrozen()
    {
        return Time.timeScale == 0f && m_Animator.updateMode == AnimatorUpdateMode.Normal;
    }

    // Flip directional inputs on side changed
    public void OnSideChanged()
    {
        for (int i = 0; i < m_TriggeredInputsList.Count; i++)
        {
            m_TriggeredInputsList[i].OnSideChanged();
        }
    }

    void ClearTriggeredInputs()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, "Clear attack inputs");

        m_TriggeredInputsList.Clear();
        m_TriggeredInputsString = string.Empty;
    }

    void UpdateAttackState()
    {
        if(GetCurrentAttackLogic() != null)
        {
            if(m_CurrentAttackState == EAttackState.Startup)
            {
                foreach (Collider2D hitBox in m_HitBoxes)
                {
                    if (hitBox != null && hitBox.enabled)
                    {
                        m_CurrentAttackState = EAttackState.Active;
                        break;
                    }
                }
            }
            else if(m_CurrentAttackState == EAttackState.Active)
            {
                bool allHitboxDisabled = true;
                foreach (Collider2D hitBox in m_HitBoxes)
                {
                    if (hitBox != null && hitBox.enabled)
                    {
                        allHitboxDisabled = false;
                        break;
                    }
                }

                if(allHitboxDisabled)
                {
                    m_CurrentAttackState = EAttackState.Recovery;
                }
            }
        }
    }

    bool CanUpdateAttack()
    {
        // Time condition
        if (m_FramesToWaitBeforeEvaluatingAttacks <= 0 || m_TotalFramesWaitingBeforeEvaluatingAttacks > AttackConfig.Instance.MaxFramesToWaitBeforeEvaluatingAttacks)
        {
            // Input condition
            if (m_TriggeredInputsList.Count > 0)
            {
                // Block/Unblock condition
                if (!m_IsAttackBlocked)
                {
                    return true;
                }
                else if (m_IsAttackBlocked && m_UnblockAttackConfig != null)
                {
                    return true;
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
                    canTriggerAttack = false;
                    foreach (UnblockAllowedAttack allowedAttack in m_UnblockAttackConfig.m_UnblockAllowedAttacks)
                    {
                        if(attackLogic.GetAttack().m_AnimationAttackName == allowedAttack.m_Attack)
                        {
                            // Can update attacks and cancel the current one if it has hit the enemy
                            canTriggerAttack = !allowedAttack.m_OnlyOnHit || m_CurrentAttackLogic.HasTouched();
                            break;
                        }
                    }
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
        foreach(GameInputList inputList in attackLogic.GetAttack().GetInputList())
        {
            if(m_TriggeredInputsList.FindSubList(inputList))
            {
                return true;
            }
        }
        return false;
    }

    void TriggerAttack(PlayerBaseAttackLogic attackLogic)
    {
        ClearTriggeredInputs();

        // If current attack logic is != null, this means we're canceling this attack by another one
        // In that case, we need to trigger EndOfAttack of this current attack before triggering the other one
        // As the EndOfAttack triggered by the animation will happen only 1 frame later and the current attack logic will already be replaced
        if (m_CurrentAttackLogic != null)
        {
            Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.EndOfAttack, new EndOfAttackEventParameters(m_CurrentAttackLogic.GetAttack().m_AnimationAttackName));
        }

        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "Trigger attack : " + attackLogic.GetAttack().m_Name);

        m_MovementComponent.UpdatePlayerSide();

        m_IsAttackBlocked = true;
        m_IsAttackTriggeredInAir = m_MovementComponent.IsJumping();
        if(m_IsAttackTriggeredInAir)
        {
            m_MovementComponent.OnLanding += OnLandingAfterAttackInAir;
        }
        m_UnblockAttackConfig = null;
        attackLogic.OnAttackLaunched();
        m_CurrentAttackLogic = attackLogic;
        m_CurrentAttackState = EAttackState.Startup;

        if(attackLogic.GetAttack().m_UseDefaultStance)
        {
            EPlayerStance attackDefaultStance = attackLogic.GetAttack().m_DefaultStance;
            m_MovementComponent.ChangePlayerStance(attackDefaultStance, (attackDefaultStance == EPlayerStance.Jump) ? EJumpPhase.Air : EJumpPhase.None);
        }
        m_MovementComponent.SetMovementBlockedByAttack(attackLogic.GetAttack().m_BlockMovement);

        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.AttackLaunched, new AttackLaunchedEventParameters(m_CurrentAttackLogic));

#if UNITY_EDITOR || DEBUG_DISPLAY
        // DEBUG ///////////////////////////////////
        if (m_DEBUG_BreakOnTriggerAttack)
        {
            Debug.Break();
        }
        ////////////////////////////////////////////
#endif
    }

    void OnEnemyTakesDamage(BaseEventParameters baseParams)
    {
        DamageTakenEventParameters damageTakenInfo = (DamageTakenEventParameters)baseParams;
        // If enemy takes damage from the current attack logic
        if (damageTakenInfo.m_AttackLogic == m_CurrentAttackLogic)
        {
            if(damageTakenInfo.m_AttackResult != EAttackResult.Parried)
            {
                if (m_CurrentAttackLogic.CanPushBack())
                {
                    OutOfBoundsSubGameManager OOBSubManager = GameManager.Instance.GetSubManager<OutOfBoundsSubGameManager>(ESubManager.OutOfBounds);
                    float pushBackForce = m_CurrentAttackLogic.GetAttackerPushBackForce(damageTakenInfo.m_AttackResult, OOBSubManager.IsInACorner(damageTakenInfo.m_Victim));
                    if (pushBackForce > 0.0f && m_MovementComponent)
                    {
                        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "On enemy takes damage, apply attacker pushback.");
                        m_MovementComponent.PushBack(pushBackForce);
                    }
                }
            }
        }
    }

    public bool CheckIsCurrentAttack(EAnimationAttackName attackName)
    {
        if (m_CurrentAttackLogic == null)
        {
            Debug.LogError("There is no current attack");
            return false;
        }

        PlayerAttack currentAttack = m_CurrentAttackLogic.GetAttack();

        // We need to check currentAttack.m_AnimationAttackName AND m_CurrentAttackLogic.GetAnimationAttackName() because for ProjectileAttack, it can happen that
        // a different animation is triggered due to guard crush property, so the animation attack name is changed at runtime
        if (currentAttack.m_AnimationAttackName != attackName && m_CurrentAttackLogic.GetAnimationAttackName() != attackName.ToString())
        {
            // This can happen when attackName has been cancelled by currentAttack (In that case, EndOfAttack has been called in TriggerAttack)
            return false;
        }
        return true;
    }

    void EndOfAttack(BaseEventParameters baseParams)
    {
        EndOfAttackEventParameters endOfAttackParams = (EndOfAttackEventParameters)baseParams;
        if(CheckIsCurrentAttack(endOfAttackParams.m_Attack))
        {
            PlayerAttack currentAttack = m_CurrentAttackLogic.GetAttack();

            bool attackWasBlocked = m_IsAttackBlocked;
            if(CanUnblockAttack())
            {
                m_IsAttackBlocked = false;
                m_IsAttackTriggeredInAir = false;
            }

            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "End of attack : " + endOfAttackParams.m_Attack + " | attack was blocked : " + attackWasBlocked + ", attack is blocked : " + m_IsAttackBlocked);

            // If attack was blocked and there is still unblock attack parameters
            // This means no allowed attack have been triggered
            // So need to clear triggered inputs in order to not launch unwanted attacks
            if (attackWasBlocked && m_UnblockAttackConfig != null)
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

    void BlockAttack(BaseEventParameters baseParams)
    {
        BlockAttackEventParameters blockAttackEventParams = (BlockAttackEventParameters)baseParams;
        if (CheckIsCurrentAttack(blockAttackEventParams.m_CurrentAttack))
        {
            if (m_IsAttackBlocked && m_UnblockAttackConfig == null)
            {
                Debug.LogError("Attack was already blocked by " + blockAttackEventParams.m_CurrentAttack);
                return;
            }

            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "Attack is blocked by : " + blockAttackEventParams.m_CurrentAttack);

            m_IsAttackBlocked = true;
            m_UnblockAttackConfig = null;
        }
    }

    void UnblockAttack(BaseEventParameters baseParams)
    {
        UnblockAttackEventParameters unblockAttackEventParams = (UnblockAttackEventParameters)baseParams;
        if (CheckIsCurrentAttack(unblockAttackEventParams.m_AttackToUnblock))
        {
            if(m_IsAttackBlocked == false)
            {
                Debug.LogError("Attack was not blocked by " + unblockAttackEventParams.m_AttackToUnblock);
                return;
            }

            if(CanUnblockAttack())
            {
                ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "Attack is unblocked by : " + unblockAttackEventParams.m_AttackToUnblock);
                m_UnblockAttackConfig = unblockAttackEventParams.m_Config;
            }
        }
    }

    void OnStunBegin(BaseEventParameters baseParams)
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "On stun begin | Attack is blocked");

        ClearTriggeredInputs();
        m_IsAttackBlocked = true;
    }

    void OnStunEnd(BaseEventParameters baseParams)
    {
        if(m_IsAttackBlocked == false)
        {
            Debug.LogError("Attack was not blocked by stun");
            return;
        }

        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "On stun end | Attack is unblocked");

        m_IsAttackBlocked = false;
        m_IsAttackTriggeredInAir = false;
        m_UnblockAttackConfig = null;
    }

    public void SetAttackBlockedByTakeOff(bool attackBlocked)
    {
        if (attackBlocked)
        {
            ClearTriggeredInputs();
            m_IsAttackBlocked = true;
            m_IsAttackBlockedByTakeOff = true;

            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "Attack blocked by take off");
        }
        else if(m_IsAttackBlockedByTakeOff)
        {
            if(CanUnblockAttack())
            {
                m_IsAttackBlocked = false;
                m_IsAttackTriggeredInAir = false;
                m_UnblockAttackConfig = null;

                ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Attack, "Attack unblocked by take off");
            }
            m_IsAttackBlockedByTakeOff = false;
        }
    }

    bool CanUnblockAttack()
    {
        // Attack can be unblocked only if we're not stunned
        if(!m_HealthComponent.GetStunInfoSubComponent().IsStunned())
        {
            // And if attack has been triggered in the air, we're not in the air anymore (in order to not trigger multiple attacks in one jump)
            if(!m_IsAttackTriggeredInAir || (m_IsAttackTriggeredInAir && !m_MovementComponent.IsJumping()))
            {
                return true;
            }
        }

        return false;
    }

    void OnLandingAfterAttackInAir()
    {
        if(CanUnblockAttack())
        {
            m_IsAttackBlocked = false;
            m_IsAttackTriggeredInAir = false;
        }

        m_MovementComponent.OnLanding -= OnLandingAfterAttackInAir;
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

    public EAttackState GetCurrentAttackState()
    {
        return m_CurrentAttackState;
    }

    public PlayerAttack GetCurrentAttack()
    {
        if(m_CurrentAttackLogic != null)
        {
            return m_CurrentAttackLogic.GetAttack();
        }
        return null;
    }

    public PlayerSuperGaugeSubComponent GetSuperGaugeSubComponent()
    {
        return m_SuperGaugeSC;
    }

    public PlayerComboCounterSubComponent GetComboCounterSubComponent()
    {
        return m_ComboCounterSC;
    }
}
