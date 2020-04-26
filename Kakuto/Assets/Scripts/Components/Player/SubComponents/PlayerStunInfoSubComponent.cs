using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        public void Reset()
        {
            m_IsStunned = false;
            m_StunType = EStunType.None;
            m_StunByGrabAttack = false;
            m_IsDurationAnimDriven = false;
            m_EndOfStunAnimTimestamp = 0;
            m_EndOfStunAnimRequested = false;
        }
    }

    private PlayerHealthComponent m_HealthComponent;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    Dictionary<string, float> m_AnimationsLengthDictionary = new Dictionary<string, float>();

    private StunInfo m_StunInfo;

    private bool m_CanIncreaseGaugeValue = true;
    private float m_CurrentGaugeValue = 0f;
    private float m_StabilizeGaugeCooldown = 0f;
    public event UnityAction OnGaugeValueChanged;

    public PlayerStunInfoSubComponent(PlayerHealthComponent healthComponent, PlayerMovementComponent movementComp, Animator anim) : base(healthComponent.gameObject)
    {
        m_HealthComponent = healthComponent;
        m_MovementComponent = movementComp;
        m_Anim = anim;

        InitAnimationsLengthDictionary();

        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.OnStunAnimEnd, OnStunAnimEnd);
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
                Debug.LogError(m_Owner + " doesn't use an AnimatorOverrideController");
            }
        }
    }

    ~PlayerStunInfoSubComponent()
    {
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.OnStunAnimEnd, OnStunAnimEnd);
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
            StartStun_Internal(attackLogic.IsHitKO(), attackLogic.GetAttack().m_AnimationAttackName == EAnimationAttackName.Grab, (attackResult == EAttackResult.Blocked) ? EStunType.Block : EStunType.Hit);
        }
    }

    void StartStun_Internal(bool isHitKO, bool isGrabAttack, EStunType stunType)
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

        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "Start stun | Type : " + stunType.ToString() + ", Duration anim driven : " + m_StunInfo.m_IsDurationAnimDriven);

        Utils.GetPlayerEventManager<bool>(m_Owner).TriggerEvent(EPlayerEvent.StunBegin, true);

        if (m_StunInfo.m_IsDurationAnimDriven)
        {
            Debug.Log(Time.time + " | Player : " + m_Owner.name + " is anim stunned");
        }

        m_Anim.ResetTrigger("OnStunEnd");
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
                Debug.LogError("Invalid stun type : " + stunType);
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
            Debug.LogError("Trying to set stun duration but " + m_Owner + " is not stunned or duration is anim driven");
        }
    }

    private void SetStunDuration_Internal(string attackName, string outStunAnimName, float stunDuration)
    {
        if (!m_AnimationsLengthDictionary.TryGetValue(outStunAnimName, out float outStunAnimDuration))
        {
            Debug.LogError(outStunAnimName + " animation can't be found in AnimationsLength dictionary for " + m_Owner);
        }

        if (outStunAnimDuration >= stunDuration)
        {
            Debug.LogError(outStunAnimName + " animation has a length (" + outStunAnimDuration + ") superior to the " + m_StunInfo.m_StunType.ToString() + " stun duration (" + stunDuration + ") of " + attackName);
            outStunAnimDuration = stunDuration;
        }

        float finalDuration = stunDuration - outStunAnimDuration;
        m_StunInfo.m_EndOfStunAnimTimestamp = Time.time + finalDuration;

        Debug.Log(Time.time + " | Player : " + m_Owner.name + " is stunned during " + stunDuration + " seconds");
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "Set stun duration : " + stunDuration);
    }

    private void TriggerOnStunEndAnim()
    {
        m_Anim.SetTrigger("OnStunEnd");
        m_StunInfo.m_EndOfStunAnimRequested = true;

        Debug.Log(Time.time + " | TriggerOnStunEndAnim");
    }

    private void OnStunAnimEnd(bool stunAnimEnd = true)
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
        m_StunInfo.Reset();

        Utils.GetPlayerEventManager<bool>(m_Owner).TriggerEvent(EPlayerEvent.StunEnd, false);

        Debug.Log(Time.time + " | Player : " + m_Owner.name + " is no more stunned");

        // DEBUG ///////////////////////////////////
        if (stunType == EStunType.Hit && m_HealthComponent.m_DEBUG_IsBlockingAllAttacksAfterHitStun)
        {
            m_MovementComponent.UpdatePlayerSide();
            DEBUG_StartBlockingAttacks();
        }
        ////////////////////////////////////////////
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

    // DEBUG /////////////////////////////////////
    private void DEBUG_StartBlockingAttacks()
    {
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "DEBUG_StartBlockingAttacks | Duration : " + m_HealthComponent.m_DEBUG_BlockingAttacksDuration);

        StartStun_Internal(false, false, EStunType.Block);
        SetStunDuration_Internal("DEBUG_StartBlockingAttacks", "BlockStand_Out", m_HealthComponent.m_DEBUG_BlockingAttacksDuration);
        m_Anim.Play("BlockStand_In", 0, 0);

        Debug.Log("Player : " + m_Owner.name + " will block all attacks during " + m_HealthComponent.m_DEBUG_BlockingAttacksDuration + " seconds");
    }
    /////////////////////////////////////////////

    public void IncreaseGaugeValue(float value)
    {
        if(CanIncreaseGaugeValue())
        {
            m_CurrentGaugeValue += value;
            ClampGaugeValue();
            OnGaugeValueChanged?.Invoke();

            if (ShouldTriggerGaugeStun() && CanTriggerGaugeStun())
            {
                TriggerGaugeStun();
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

    private void TriggerGaugeStun()
    {
        StartStun_Internal(true, false, EStunType.Gauge);
        Utils.GetPlayerEventManager<bool>(m_Owner).TriggerEvent(EPlayerEvent.StopMovement, true);
        PlayStunAnim();

        m_CanIncreaseGaugeValue = false;
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
        // DEBUG /////////////////////////////////////
        if (m_HealthComponent.m_DEBUG_IsImmuneToStunGauge)
        {
            m_CurrentGaugeValue = Mathf.Clamp(m_CurrentGaugeValue, 0f, AttackConfig.Instance.m_StunGaugeMaxValue - 1);
        }
        /////////////////////////////////////////////
        else
        {
            m_CurrentGaugeValue = Mathf.Clamp(m_CurrentGaugeValue, 0f, AttackConfig.Instance.m_StunGaugeMaxValue);
        }
    }

    private void PlayStunAnim()
    {
        string stunAnimName = "Stun" + m_MovementComponent.GetCurrentStance().ToString() + "KO";
        m_Anim.Play(stunAnimName, 0, 0);
    }
}
