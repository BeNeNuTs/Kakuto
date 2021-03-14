using System;
using System.Collections.Generic;
using UnityEngine;

public enum EStunType
{
    Hit,
    Block,
    Gauge,
    None
}

public class PlayerStunInfoSubComponent : PlayerBaseSubComponent
{
    struct StunInfo
    {
        public bool m_IsStunned;
        public EStunType m_StunType;
        public bool m_StunByGrabAttack;
        public bool m_IsDurationAnimDriven;
        public float m_EndOfStunAnimTimestamp;
        public bool m_EndOfStunAnimRequested;
        public bool m_IsInJuggleState;

        public void Reset()
        {
            m_IsStunned = false;
            m_StunType = EStunType.None;
            m_StunByGrabAttack = false;
            m_IsDurationAnimDriven = false;
            m_EndOfStunAnimTimestamp = 0;
            m_EndOfStunAnimRequested = false;
            m_IsInJuggleState = false;
        }
    }

    private static readonly string K_ANIM_ONSTUNEND_TRIGGER = "OnStunEnd";
    private static readonly string K_ANIM_BLOCKSTAND_IN = "BlockStand_In";
    private static readonly string K_ANIM_BLOCKSTAND_OUT = "BlockStand_Out";
    private static readonly string K_ANIM_BLOCKCROUCH_IN = "BlockCrouch_In";
    private static readonly string K_ANIM_BLOCKCROUCH_OUT = "BlockCrouch_Out";
    private static readonly string K_START_AUTOBLOCKING_ATTACK = "StartAutoBlockingAttacks";

    private static readonly string K_STUN = "Stun";
    private static readonly string K_STUNNED = "Stunned";
    private static readonly string K_KO = "KO";

    private PlayerHealthComponent m_HealthComponent;
    private PlayerInfoComponent m_InfoComponent;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    Dictionary<string, float> m_AnimationsLengthDictionary = new Dictionary<string, float>();

    private StunInfo m_StunInfo;

    private bool m_CanIncreaseGaugeValue = true;
    private float m_CurrentGaugeValue = 0f;
    private float m_StabilizeGaugeCooldown = 0f;
    public Action OnGaugeValueChanged;

    public PlayerStunInfoSubComponent(PlayerHealthComponent healthComponent, PlayerInfoComponent infoComponent, PlayerMovementComponent movementComp, Animator anim) : base(infoComponent.gameObject)
    {
        m_HealthComponent = healthComponent;
        m_InfoComponent = infoComponent;
        m_MovementComponent = movementComp;
        m_Anim = anim;

        InitAnimationsLengthDictionary();

        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.OnStunAnimEnd, OnStunAnimEnd);
    }

    void InitAnimationsLengthDictionary()
    {
        if(m_Anim != null)
        {
            AnimatorOverrideController overrideController = m_Anim.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController != null)
            {
                List<KeyValuePair<AnimationClip, AnimationClip>> animationsOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(animationsOverrides);
                foreach (KeyValuePair<AnimationClip, AnimationClip> animOverride in animationsOverrides)
                {
                    if(animOverride.Key != null && animOverride.Value != null)
                    {
                        m_AnimationsLengthDictionary.Add(animOverride.Key.name, animOverride.Value.length);
                    }
                }
            }
            else
            {
                KakutoDebug.LogError(m_Owner + " doesn't use an AnimatorOverrideController");
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.OnStunAnimEnd, OnStunAnimEnd);
    }

    public void Update()
    {
        if (IsStunned())
        {
            if(!m_StunInfo.m_IsDurationAnimDriven)
            {
                if (!m_StunInfo.m_EndOfStunAnimRequested && m_StunInfo.m_EndOfStunAnimTimestamp > 0f && Time.time > m_StunInfo.m_EndOfStunAnimTimestamp)
                {
                    TriggerOnStunEndAnim();
                }
            }
        }

        StabilizeGauge();
    }

    public void StartStun(PlayerBaseAttackLogic attackLogic, EAttackResult attackResult)
    {
        if(attackResult != EAttackResult.Parried)
        {
            StartStun_Internal(attackLogic.IsHitKO(), attackLogic.GetAttack().m_CanJuggleLaunch, attackLogic.GetAttack().m_AnimationAttackName == EAnimationAttackName.Grab, (attackResult == EAttackResult.Blocked) ? EStunType.Block : EStunType.Hit);
        }
    }

    void StartStun_Internal(bool isHitKO, bool canJuggleLaunch, bool isGrabAttack, EStunType stunType)
    {
        if (IsGaugeStunned())
        {
            ResetGaugeValue();
        }

        m_StunInfo.m_IsStunned = true;
        m_StunInfo.m_StunType = stunType;
        m_StunInfo.m_StunByGrabAttack = isGrabAttack;
        m_StunInfo.m_IsDurationAnimDriven = IsStunDurationAnimDriven(isHitKO, isGrabAttack, stunType); 
        m_StunInfo.m_EndOfStunAnimTimestamp = 0f;
        m_StunInfo.m_EndOfStunAnimRequested = false;
        if (m_MovementComponent.IsJumping())
        {
            if (canJuggleLaunch)
            {
                m_StunInfo.m_IsInJuggleState = true;
            }
        }

        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "Start stun | Type : " + stunType.ToString() + ", Duration anim driven : " + m_StunInfo.m_IsDurationAnimDriven);

        Utils.GetPlayerEventManager(m_Owner).TriggerEvent(EPlayerEvent.StunBegin, new StunBeginEventParameters(stunType, isGrabAttack));

#if DEBUG_DISPLAY || UNITY_EDITOR
        if (m_StunInfo.m_IsDurationAnimDriven)
        {
            KakutoDebug.Log(Time.time + " | Player : " + m_Owner.name + " is anim stunned");
        }
#endif

        m_Anim.ResetTrigger(K_ANIM_ONSTUNEND_TRIGGER);
    }

    bool IsStunDurationAnimDriven(bool isHitKO, bool isGrabAttack, EStunType stunType)
    {
        switch (stunType)
        {
            case EStunType.Hit:
                return m_MovementComponent.IsJumping() || isHitKO || isGrabAttack;  // Hit : Stun duration is anim driven if we're jumping / taking a hit KO / or be grabbed
            case EStunType.Block:
                return isGrabAttack;                                                // Block : Stun duration is anim driven if playing a grab block
            case EStunType.Gauge:
                return true;                                                        // Gauge : Stun duration is always anim drive
            case EStunType.None:
            default:
                KakutoDebug.LogError("Invalid stun type : " + stunType);
                return false;
        }
    }

    public void SetStunDuration(PlayerBaseAttackLogic attackLogic, float stunDuration)
    {
        if (m_StunInfo.m_IsStunned && !m_StunInfo.m_IsDurationAnimDriven)
        {
            string outStunAnimName = "UNKNOWN";
            if (m_StunInfo.m_StunType == EStunType.Hit)
            {
                outStunAnimName = attackLogic.GetHitAnimName(m_MovementComponent.GetCurrentStance(), EStunAnimState.Out);
            }
            else if (m_StunInfo.m_StunType == EStunType.Block)
            {
                outStunAnimName = attackLogic.GetBlockAnimName(m_MovementComponent.GetCurrentStance(), EStunAnimState.Out);
            }
            SetStunDuration_Internal(attackLogic.GetAttack().m_Name, outStunAnimName, stunDuration);
        }
        else
        {
            KakutoDebug.LogError("Trying to set stun duration but " + m_Owner + " is not stunned or duration is anim driven");
        }
    }

    private void SetStunDuration_Internal(string attackName, string outStunAnimName, float stunDuration)
    {
        if (!m_AnimationsLengthDictionary.TryGetValue(outStunAnimName, out float outStunAnimDuration))
        {
            KakutoDebug.LogError(outStunAnimName + " animation can't be found in AnimationsLength dictionary for " + m_Owner);
        }

        if (outStunAnimDuration >= stunDuration)
        {
            KakutoDebug.LogError(outStunAnimName + " animation has a length (" + outStunAnimDuration + ") superior to the " + m_StunInfo.m_StunType.ToString() + " stun duration (" + stunDuration + ") of " + attackName);
            outStunAnimDuration = stunDuration;
        }

        float finalDuration = stunDuration - outStunAnimDuration;
        m_StunInfo.m_EndOfStunAnimTimestamp = Time.time + finalDuration;

#if DEBUG_DISPLAY || UNITY_EDITOR
        KakutoDebug.Log(Time.time + " | Player : " + m_Owner.name + " is stunned during " + stunDuration + " seconds");
#endif
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "Set stun duration : " + stunDuration);
    }

    private void TriggerOnStunEndAnim()
    {
        m_Anim.SetTrigger(K_ANIM_ONSTUNEND_TRIGGER);
        m_StunInfo.m_EndOfStunAnimRequested = true;

#if DEBUG_DISPLAY || UNITY_EDITOR
        KakutoDebug.Log(Time.time + " | TriggerOnStunEndAnim");
#endif
    }

    private void OnStunAnimEnd(BaseEventParameters baseParams)
    {
        if (IsStunned())
        {
            if (m_StunInfo.m_IsDurationAnimDriven || m_StunInfo.m_EndOfStunAnimRequested)
            {
                ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "OnStunAnimEnd | Duration anim driven : " + m_StunInfo.m_IsDurationAnimDriven + ", End of stun anim requested : " + m_StunInfo.m_EndOfStunAnimRequested);
                StopStun();
            }
        }
    }

    private void StopStun()
    {
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "Stop stun");
        
        if (IsGaugeStunned())
        {
            ResetGaugeValue();
        }

        m_CanIncreaseGaugeValue = true;
        m_StabilizeGaugeCooldown = AttackConfig.Instance.m_StunGaugeDecreaseCooldown;

        EStunType stunType = m_StunInfo.m_StunType;
        bool wasStunByGrabAttack = m_StunInfo.m_StunByGrabAttack;
        m_StunInfo.Reset();

        Utils.GetPlayerEventManager(m_Owner).TriggerEvent(EPlayerEvent.StunEnd, new StunEndEventParameters(stunType, wasStunByGrabAttack));
#if DEBUG_DISPLAY || UNITY_EDITOR
        KakutoDebug.Log(Time.time + " | Player : " + m_Owner.name + " is no more stunned");
#endif

        if (ShouldTriggerGaugeStun() && CanTriggerGaugeStun())
        {
            m_MovementComponent.UpdatePlayerSide();

            // If we were stun by a grab attack, we don't want to play KO anim but stunned anim directly
            bool playKOAnimation = !wasStunByGrabAttack;
            TriggerGaugeStun(playKOAnimation);
        }
        else
        {
            // Can block attack after hit stun only in dummy mode => attack disabled
            PlayerSettings playerSettings = m_InfoComponent.GetPlayerSettings();
            if (stunType == EStunType.Hit && playerSettings.m_IsBlockingAllAttacksAfterHitStun && !playerSettings.m_AttackEnabled)
            {
                if(playerSettings.m_DefaultStance != EPlayerStance.Jump)
                {
                    m_MovementComponent.UpdatePlayerSide();
                    StartAutoBlockingAttacks();
                }
            }
        }
    }

    public bool IsStunned()
    {
        return m_StunInfo.m_IsStunned;
    }

    public bool IsHitStunned()
    {
        return IsStunned() && m_StunInfo.m_StunType == EStunType.Hit;
    }

    public bool IsGrabHitStunned()
    {
        return IsHitStunned() && m_StunInfo.m_StunByGrabAttack;
    }

    public bool IsBlockStunned()
    {
        return IsStunned() && m_StunInfo.m_StunType == EStunType.Block;
    }

    public bool IsGaugeStunned()
    {
        return IsStunned() && m_StunInfo.m_StunType == EStunType.Gauge;
    }

    public bool IsStunDurationAnimDriven()
    {
        return m_StunInfo.m_IsStunned && m_StunInfo.m_IsDurationAnimDriven;
    }

    public bool IsInJuggleState()
    {
        return m_StunInfo.m_IsInJuggleState;
    }

    private void StartAutoBlockingAttacks()
    {
        float blockingAttackDuration = m_InfoComponent.GetPlayerSettings().m_BlockingAttacksDuration;
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "StartAutoBlockingAttacks | Duration : " + blockingAttackDuration);

        StartStun_Internal(false, false, false, EStunType.Block);
        if(m_MovementComponent.IsCrouching())
        {
            SetStunDuration_Internal(K_START_AUTOBLOCKING_ATTACK, K_ANIM_BLOCKCROUCH_OUT, blockingAttackDuration);
            m_Anim.Play(K_ANIM_BLOCKCROUCH_IN, 0, 0);
        }
        else
        {
            SetStunDuration_Internal(K_START_AUTOBLOCKING_ATTACK, K_ANIM_BLOCKSTAND_OUT, blockingAttackDuration);
            m_Anim.Play(K_ANIM_BLOCKSTAND_IN, 0, 0);
        }

#if DEBUG_DISPLAY || UNITY_EDITOR
        KakutoDebug.Log("Player : " + m_Owner.name + " will block all attacks during " + blockingAttackDuration + " seconds");
#endif
    }

    public void IncreaseGaugeValue(float value)
    {
        if(CanIncreaseGaugeValue())
        {
            m_CurrentGaugeValue += value;
            ClampGaugeValue();
            OnGaugeValueChanged?.Invoke();

            if (ShouldTriggerGaugeStun() && CanTriggerGaugeStun())
            {
                TriggerGaugeStun(true);
            }
        }
    }

    private bool CanIncreaseGaugeValue()
    {
        return m_CurrentGaugeValue < AttackConfig.Instance.m_StunGaugeMaxValue && m_CanIncreaseGaugeValue;
    }

    private bool ShouldTriggerGaugeStun()
    {
        return m_CurrentGaugeValue >= AttackConfig.Instance.m_StunGaugeMaxValue && !IsGaugeStunned();
    }

    private bool CanTriggerGaugeStun()
    {
        return !IsStunned() || (IsStunned() && !IsGrabHitStunned());
    }

    private void TriggerGaugeStun(bool playKOAnimation)
    {
        StartStun_Internal(true, false, false, EStunType.Gauge);
        Utils.GetPlayerEventManager(m_Owner).TriggerEvent(EPlayerEvent.StopMovement);
        PlayGaugeStunAnim(playKOAnimation);

        m_CanIncreaseGaugeValue = false;
    }

    private void PlayGaugeStunAnim(bool playKOAnimation)
    {
        string stunAnimName;
        if(playKOAnimation)
        {
            stunAnimName = K_STUN + m_MovementComponent.GetCurrentStance().ToString() + K_KO;
        }
        else
        {
            stunAnimName = K_STUNNED;
        }
        m_Anim.Play(stunAnimName, 0, 0);
    }

    private void ResetGaugeValue()
    {
        m_CurrentGaugeValue = 0f;
        OnGaugeValueChanged?.Invoke();
    }

    private void StabilizeGauge()
    {
        if (!IsStunned())
        {
            if(m_StabilizeGaugeCooldown <= 0f)
            {
                if (m_CurrentGaugeValue > 0f)
                {
                    m_CurrentGaugeValue -= AttackConfig.Instance.m_StunGaugeDecreaseSpeed * Time.deltaTime;
                    ClampGaugeValue();
                    OnGaugeValueChanged?.Invoke();
                }
            }
            else
            {
                m_StabilizeGaugeCooldown -= Time.deltaTime;
            }
        }
    }

    public float GetCurrentGaugeValue()
    {
        return m_CurrentGaugeValue;
    }

    private void ClampGaugeValue()
    {
        if (m_HealthComponent.IsDead() || m_InfoComponent.GetPlayerSettings().m_IsImmuneToStunGauge)
        {
            m_CurrentGaugeValue = Mathf.Clamp(m_CurrentGaugeValue, 0f, AttackConfig.Instance.m_StunGaugeMaxValue - 1);
        }
        else
        {
            m_CurrentGaugeValue = Mathf.Clamp(m_CurrentGaugeValue, 0f, AttackConfig.Instance.m_StunGaugeMaxValue);
        }
    }
}
