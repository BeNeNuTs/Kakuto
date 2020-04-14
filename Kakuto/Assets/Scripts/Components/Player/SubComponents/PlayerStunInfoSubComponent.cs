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
        public bool m_IsDurationAnimDriven;
        public float m_EndOfStunAnimTimestamp;
        public bool m_EndOfStunAnimRequested;
    }

    private PlayerHealthComponent m_HealthComponent;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    Dictionary<string, float> m_AnimationsLengthDictionary = new Dictionary<string, float>();

    private StunInfo m_StunInfo;

    private float m_CurrentGaugeValue = 0f;
    private float m_StabilizeGaugeCooldown = 0f;
    public event UnityAction OnGaugeValueChanged;

    // DEBUG /////////////////////////////
    private float m_DEBUG_BlockingAttacksTimer = 0.0f;
    //////////////////////////////////////

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
            if(!m_StunInfo.m_EndOfStunAnimRequested)
            {
                if (!m_StunInfo.m_IsDurationAnimDriven && m_StunInfo.m_EndOfStunAnimTimestamp > 0f && Time.time > m_StunInfo.m_EndOfStunAnimTimestamp)
                {
                    TriggerOnStunEndAnim();
                }
            }
        }

        StabilizeGauge();

        // DEBUG /////////////////////////////////////
        if (m_HealthComponent.m_DEBUG_IsBlockingAllAttacksAfterHitStun)
        {
            if (m_HealthComponent.m_DEBUG_IsBlockingAllAttacks && m_DEBUG_BlockingAttacksTimer > 0.0f)
            {
                if (Time.time > m_DEBUG_BlockingAttacksTimer)
                {
                    DEBUG_StopBlockingAttacks();
                }
            }
        }
        //////////////////////////////////////////////
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
        m_StunInfo.m_IsStunned = true;
        m_StunInfo.m_StunType = stunType;
        m_StunInfo.m_IsDurationAnimDriven = m_MovementComponent.IsJumping() || isHitKO || isGrabAttack; // Stun duration is anim driven if we're jumping / taking a hit KO / or be grabbed
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

    public void SetStunDuration(PlayerBaseAttackLogic attackLogic, float stunDuration)
    {
        if(m_StunInfo.m_IsStunned && !m_StunInfo.m_IsDurationAnimDriven)
        {
            string outStunAnimName = "UNKNOWN";
            if (m_StunInfo.m_StunType == EStunType.Hit)
            {
                outStunAnimName = attackLogic.GetHitAnimName(m_MovementComponent.GetCurrentStance(), EStunAnimState.Out);
            }
            else if(m_StunInfo.m_StunType == EStunType.Block)
            {
                outStunAnimName = attackLogic.GetBlockAnimName(m_MovementComponent.GetCurrentStance(), EStunAnimState.Out);
            }

            if (!m_AnimationsLengthDictionary.TryGetValue(outStunAnimName, out float outStunAnimDuration))
            {
                Debug.LogError(outStunAnimName + " animation can't be found in AnimationsLength dictionary for " + m_Owner);
            }

            if (outStunAnimDuration >= stunDuration)
            {
                Debug.LogError(outStunAnimName + " animation has a length (" + outStunAnimDuration + ") superior to the " + m_StunInfo.m_StunType.ToString() + " stun duration (" + stunDuration + ") of " + attackLogic.GetAttack().m_Name);
                outStunAnimDuration = stunDuration;
            }

            float finalDuration = stunDuration - outStunAnimDuration;
            m_StunInfo.m_EndOfStunAnimTimestamp = Time.time + finalDuration;

            Debug.Log(Time.time + " | Player : " + m_Owner.name + " is stunned during " + stunDuration + " seconds");
            ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "Set stun duration : " + stunDuration);
        }
        else
        {
            Debug.LogError("Trying to set stun duration but " + m_Owner + " is not stunned or duration is anim driven");
        }
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
                StopStun();
            }
        }
    }

    private void StopStun()
    {
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Stun, "Stop stun");

        EStunType stunType = m_StunInfo.m_StunType;

        m_StunInfo.m_IsStunned = false;
        m_StunInfo.m_StunType = EStunType.None;
        m_StunInfo.m_IsDurationAnimDriven = false;
        m_StunInfo.m_EndOfStunAnimTimestamp = 0;
        m_StunInfo.m_EndOfStunAnimRequested = false;

        if(m_CurrentGaugeValue >= AttackConfig.Instance.m_StunGaugeMaxValue)
        {
            ResetGaugeValue();
        }
        m_StabilizeGaugeCooldown = AttackConfig.Instance.m_StunGaugeDecreaseCooldown;

        Utils.GetPlayerEventManager<bool>(m_Owner).TriggerEvent(EPlayerEvent.StunEnd, false);

        Debug.Log(Time.time + " | Player : " + m_Owner.name + " is no more stunned");

        // DEBUG ///////////////////////////////////
        if (stunType == EStunType.Hit && m_HealthComponent.m_DEBUG_IsBlockingAllAttacksAfterHitStun)
        {
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
        return m_StunInfo.m_IsStunned && m_StunInfo.m_StunType == EStunType.Hit;
    }

    public bool IsBlockStunned()
    {
        return m_StunInfo.m_IsStunned && m_StunInfo.m_StunType == EStunType.Block;
    }

    // DEBUG /////////////////////////////////////
    private void DEBUG_StartBlockingAttacks()
    {
        m_DEBUG_BlockingAttacksTimer = Time.time + m_HealthComponent.m_DEBUG_BlockingAttacksDuration;
        m_HealthComponent.m_DEBUG_IsBlockingAllAttacks = true;

        m_Anim.Play("BlockStand_In", 0, 0);

        Debug.Log("Player : " + m_Owner.name + " will block all attacks during " + m_HealthComponent.m_DEBUG_BlockingAttacksDuration + " seconds");
    }

    private void DEBUG_StopBlockingAttacks()
    {
        m_DEBUG_BlockingAttacksTimer = 0.0f;
        m_HealthComponent.m_DEBUG_IsBlockingAllAttacks = false;

        m_Anim.SetTrigger("OnStunEnd"); // To trigger end of blocking animation

        Debug.Log("Player : " + m_Owner.name + " doesn't block attacks anymore");
    }
    /////////////////////////////////////////////

    public void IncreaseGaugeValue(float value)
    {
        if(m_CurrentGaugeValue < AttackConfig.Instance.m_StunGaugeMaxValue)
        {
            m_CurrentGaugeValue += value;
            ClampGaugeValue();
            OnGaugeValueChanged?.Invoke();

            if (m_CurrentGaugeValue >= AttackConfig.Instance.m_StunGaugeMaxValue)
            {
                StartStun_Internal(true, false, EStunType.Gauge);
                PlayStunAnim();
            }
        }
    }

    public void ResetGaugeValue()
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
