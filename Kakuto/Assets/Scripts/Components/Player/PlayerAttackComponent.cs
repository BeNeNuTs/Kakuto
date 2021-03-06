﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public enum EAttackState
{
    Startup,
    Active,
    Recovery
}

public class PlayerAttackComponent : MonoBehaviour
{
    private int K_HITBOXLAYER = 0;
    private int K_GRABOXLAYER = 0;
    private int K_PROXIMITYBOXLAYER = 0;

    public PlayerAttacksConfig m_AttacksConfig;
    private List<PlayerBaseAttackLogic> m_AttackLogics;

    public PlayerMovementComponent m_MovementComponent;
    public PlayerHealthComponent m_HealthComponent;
    public PlayerInfoComponent m_InfoComponent;
    public Animator m_Animator;

    public AudioSubGameManager m_AudioManager;

    private PlayerSuperGaugeSubComponent m_SuperGaugeSC;
    private PlayerComboCounterSubComponent m_ComboCounterSC;

    private List<GameInput> m_AttackInputs = new List<GameInput>();
    private List<TriggeredGameInput> m_TriggeredInputsList;
    private string m_TriggeredInputsString;

    private PlayerBaseAttackLogic m_CurrentAttackLogic;
    private EAttackState m_CurrentAttackState = EAttackState.Startup;
    public EAttackState CurrentAttackState
    {
        get => m_CurrentAttackState;
        private set
        {
            m_CurrentAttackState = value;
            OnCurrentAttackStateChanged?.Invoke(GetCurrentAttackLogic(), value);
        }
    }
    public Action<PlayerBaseAttackLogic, EAttackState> OnCurrentAttackStateChanged;

    private UnblockAttackAnimEventConfig m_UnblockAttackConfig = null;
    private bool m_IsAttackBlocked = false;
    private bool m_IsAttackBlockedByTakeOff = false;
    private bool m_IsAttackTriggeredInAir = false;

    private int m_FramesToWaitBeforeEvaluatingAttacks = 0;
    private int m_TotalFramesWaitingBeforeEvaluatingAttacks = 0;

    private List<Collider2D> m_HitBoxes;

    private AttackConfig m_AttackConfig;

#if UNITY_EDITOR || DEBUG_DISPLAY
    /// DEBUG
    [Separator("Debug")]
    [Space]

    public bool m_DEBUG_BreakOnTriggerAttack = false;
    /// DEBUG
#endif

    void Awake()
    {
        K_HITBOXLAYER = LayerMask.NameToLayer("HitBox");
        K_GRABOXLAYER = LayerMask.NameToLayer("GrabBox");
        K_PROXIMITYBOXLAYER = LayerMask.NameToLayer("ProximityBox");

        m_AttackConfig = AttackConfig.Instance;

        m_AudioManager = GameManager.Instance.GetSubManager<AudioSubGameManager>(ESubManager.Audio);

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
        Profiler.BeginSample("PlayerAttackComponent.UpdateTriggerInputsList");
        int currentFrame = Time.frameCount;

        InputManager.GetAttackInputList(m_InfoComponent.GetPlayerIndex(), m_MovementComponent.IsLeftSide(), ref m_AttackInputs);

        int attackInputsCount = m_AttackInputs.Count;
        int triggeredInputsCount = m_TriggeredInputsList.Count;
        // If there is new inputs
        if (attackInputsCount > 0) 
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, "New attack inputs : " + m_AttackInputs.ToStringList());
            if(triggeredInputsCount > 0)
            {
                ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, "Adding input frame persistency bonus (" + m_AttackConfig.InputFramesPersistencyBonus + ") to current attack inputs : " + m_TriggeredInputsList.ToStringList());
            }

            // We need to add persistency bonus to the previous ones
            for (int i = 0; i < triggeredInputsCount; i++)
            {
                m_TriggeredInputsList[i].AddPersistency(m_AttackConfig.InputFramesPersistencyBonus);
            }
        }

        if(IsPlayerTimeFrozen())
        {
            // If time is frozen, notify previous inputs
            for (int i = 0; i < triggeredInputsCount; i++)
            {
                m_TriggeredInputsList[i].OnTimeFreeze();
            }
        }
        
        // Then add those new inputs in the list with the default persistency
        for(int i = 0; i < attackInputsCount; i++)
        {
            m_TriggeredInputsList.Add(new TriggeredGameInput(m_AttackInputs[i], currentFrame));
        }

        // Remove those which exceeds max allowed inputs
        if (m_TriggeredInputsList.Count > m_AttackConfig.m_MaxInputs)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, "Remove attack inputs (reason : exceeds max inputs allowed) : " + m_TriggeredInputsList.GetRange(0, (int)(m_TriggeredInputsList.Count - m_AttackConfig.m_MaxInputs)).ToStringList());
        }
        while (m_TriggeredInputsList.Count > m_AttackConfig.m_MaxInputs)
        {   
            m_TriggeredInputsList.RemoveAt(0);
        }

        // Also remove those which have their persistency elapsed
        int triggeredInputsElapsedCount = m_TriggeredInputsList.RemoveAll(input => input.IsElapsed(currentFrame));

        if (attackInputsCount > 0)
        {
            m_FramesToWaitBeforeEvaluatingAttacks = m_AttackConfig.FramesToWaitBeforeEvaluatingAttacks;
        }

        if(triggeredInputsElapsedCount > 0 || attackInputsCount > 0)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Input, m_TriggeredInputsList.Count == 0 ? "No more attack input" : "Current attack inputs : " + m_TriggeredInputsList.ToStringList());
        }

        Profiler.EndSample();
    }

    void UpdateTriggerInputsString()
    {
        m_TriggeredInputsString = string.Empty;
        for (int i = 0; i < m_TriggeredInputsList.Count; i++)
        {
            m_TriggeredInputsString += m_TriggeredInputsList[i].GetInputString();
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
        Profiler.BeginSample("PlayerAttackComponent.UpdateAttackState");
        if (GetCurrentAttackLogic() != null)
        {
            if(CurrentAttackState == EAttackState.Startup)
            {
                for (int i = 0; i < m_HitBoxes.Count; i++)
                {
                    Collider2D hitBox = m_HitBoxes[i];
                    if (hitBox != null && hitBox.enabled)
                    {
                        CurrentAttackState = EAttackState.Active;
                        break;
                    }
                }
            }
            else if(CurrentAttackState == EAttackState.Active)
            {
                bool allHitboxDisabled = true;
                for (int i = 0; i < m_HitBoxes.Count; i++)
                {
                    Collider2D hitBox = m_HitBoxes[i];
                    if (hitBox != null && hitBox.enabled)
                    {
                        allHitboxDisabled = false;
                        break;
                    }
                }

                if(allHitboxDisabled)
                {
                    CurrentAttackState = EAttackState.Recovery;
                }
            }
        }
        Profiler.EndSample();
    }

    bool CanUpdateAttack()
    {
        // Time condition
        if (m_FramesToWaitBeforeEvaluatingAttacks <= 0 || m_TotalFramesWaitingBeforeEvaluatingAttacks > m_AttackConfig.MaxFramesToWaitBeforeEvaluatingAttacks)
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
        Profiler.BeginSample("PlayerAttackComponent.UpdateAttack");
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
        Profiler.EndSample();
    }

    bool EvaluateAttackConditions(PlayerBaseAttackLogic attackLogic)
    {
        Profiler.BeginSample("PlayerAttackComponent.EvaluateAttackConditions");
        bool evaluateConditions = attackLogic.EvaluateConditions(m_CurrentAttackLogic);
        Profiler.EndSample();
        return evaluateConditions;
    }

    bool CheckAttackInputs(PlayerBaseAttackLogic attackLogic)
    {
        Profiler.BeginSample("PlayerAttackComponent.CheckAttackInputs");
        foreach (GameInputList inputList in attackLogic.GetAttack().GetInputList())
        {
            if(m_TriggeredInputsList.FindSubList(inputList))
            {
                Profiler.EndSample();
                return true;
            }
        }
        Profiler.EndSample();
        return false;
    }

    void TriggerAttack(PlayerBaseAttackLogic attackLogic)
    {
        Profiler.BeginSample("PlayerAttackComponent.TriggerAttack");
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
        CurrentAttackState = EAttackState.Startup;

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

        Profiler.EndSample();
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
                KakutoDebug.LogError("Attack was already blocked by " + blockAttackEventParams.m_CurrentAttack);
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
                KakutoDebug.LogError("Attack was not blocked by " + unblockAttackEventParams.m_AttackToUnblock);
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

        StunBeginEventParameters stunBeginEventParameters = (StunBeginEventParameters)baseParams;
        if(m_CurrentAttackLogic != null && (m_CurrentAttackLogic is PlayerGrabAttackLogic || stunBeginEventParameters.m_StunByGrabAttack))
        {
            EndOfAttack(new EndOfAttackEventParameters(m_CurrentAttackLogic.GetAttack().m_AnimationAttackName));
        }
        
        ClearTriggeredInputs();
        m_IsAttackBlocked = true;
    }

    void OnStunEnd(BaseEventParameters baseParams)
    {
        if(m_IsAttackBlocked == false)
        {
            KakutoDebug.LogError("Attack was not blocked by stun");
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

    void OnRoundOver(RoundSubGameManager.ELastRoundWinner lastRoundWinner)
    {
        enabled = false;
        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void InitHitBoxes()
    {
        int colliderLayer = 0;
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        m_HitBoxes = new List<Collider2D>(allColliders.Length);
        foreach (Collider2D collider in allColliders)
        {
            colliderLayer = collider.gameObject.layer;
            if (colliderLayer == K_HITBOXLAYER || colliderLayer == K_GRABOXLAYER || colliderLayer == K_PROXIMITYBOXLAYER)
            {
                m_HitBoxes.Add(collider);
            }
        }
    }

    void DisableAllHitBoxes()
    {
        int hitBoxCount = m_HitBoxes.Count;
        if (hitBoxCount == 0)
        {
            KakutoDebug.LogError("HitBoxes have not been initialized.");
        }

        for(int i = 0; i < hitBoxCount; i++)
        {
            if(m_HitBoxes[i] != null)
            {
                m_HitBoxes[i].enabled = false;
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

    public PlayerSuperGaugeSubComponent GetSuperGaugeSubComponent()
    {
        return m_SuperGaugeSC;
    }

    public PlayerComboCounterSubComponent GetComboCounterSubComponent()
    {
        return m_ComboCounterSC;
    }
}
