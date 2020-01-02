using UnityEngine;
using UnityEngine.Events;

public enum EStunType
{
    Hit,
    Block,
    None
}

public class PlayerStunInfoSubComponent : PlayerBaseSubComponent
{
    struct StunInfo
    {
        public bool m_IsStunned;
        public EStunType m_StunType;
        public bool m_IsDurationAnimDriven;
        public float m_EndOfStunTimestamp;
        public bool m_EndOfStunAnimRequested;
    }

    private PlayerHealthComponent m_HealthComponent;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

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

        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.OnStunAnimEnd, OnStunAnimEnd);
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
                if (!m_StunInfo.m_IsDurationAnimDriven && m_StunInfo.m_EndOfStunTimestamp > 0f && Time.unscaledTime > m_StunInfo.m_EndOfStunTimestamp)
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
                if (Time.unscaledTime > m_DEBUG_BlockingAttacksTimer)
                {
                    DEBUG_StopBlockingAttacks();
                }
            }
        }
        //////////////////////////////////////////////
    }

    private void OnStunAnimEnd(bool dummy)
    {
        if(IsStunned())
        {
            if (m_StunInfo.m_IsDurationAnimDriven || m_StunInfo.m_EndOfStunAnimRequested)
            {
                StopStun();
            }
        }
    }

    public void StartStun(PlayerBaseAttackLogic attackLogic, bool isAttackBlocked)
    {
        m_StunInfo.m_IsStunned = true;
        m_StunInfo.m_StunType = (isAttackBlocked) ? EStunType.Block : EStunType.Hit;
        m_StunInfo.m_IsDurationAnimDriven = m_MovementComponent.IsJumping() || attackLogic.IsHitKO() || attackLogic.GetAttack().m_AnimationAttackName == EAnimationAttackName.Grab; // Stun duration is anim driven if we're jumping / taking a hit KO / or be grabbed
        m_StunInfo.m_EndOfStunAnimRequested = false;

        Utils.GetPlayerEventManager<bool>(m_Owner).TriggerEvent(EPlayerEvent.StunBegin, true);

        if (m_StunInfo.m_IsDurationAnimDriven)
        {
            Debug.Log(Time.time + " | Player : " + m_Owner.name + " is anim stunned");
        }
    }

    public void SetStunDuration(float stunDuration)
    {
        if(m_StunInfo.m_IsStunned && !m_StunInfo.m_IsDurationAnimDriven)
        {
            // retirer de la duration la durée de l'anim de out
            m_StunInfo.m_EndOfStunTimestamp = Time.unscaledTime + stunDuration;

            Debug.Log(Time.time + " | Player : " + m_Owner.name + " is stunned during " + stunDuration + " seconds");
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

    private void StopStun()
    {
        EStunType stunType = m_StunInfo.m_StunType;

        m_StunInfo.m_IsStunned = false;
        m_StunInfo.m_StunType = EStunType.None;
        m_StunInfo.m_EndOfStunTimestamp = 0;
        m_StunInfo.m_IsDurationAnimDriven = false;
        m_StunInfo.m_EndOfStunAnimRequested = false;

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
        m_DEBUG_BlockingAttacksTimer = Time.unscaledTime + m_HealthComponent.m_DEBUG_BlockingAttacksDuration;
        m_HealthComponent.m_DEBUG_IsBlockingAllAttacks = true;

        m_Anim.Play("BlockStand_In", 0, 0);

        Debug.Log("Player : " + m_Owner.name + " will block all attacks during " + m_HealthComponent.m_DEBUG_BlockingAttacksDuration + " seconds");
    }

    private void DEBUG_StopBlockingAttacks()
    {
        m_DEBUG_BlockingAttacksTimer = 0.0f;
        m_HealthComponent.m_DEBUG_IsBlockingAllAttacks = false;

        if (!m_StunInfo.m_IsStunned)
        {
            m_Anim.SetTrigger("OnStunEnd"); // To trigger end of blocking animation
        }

        Debug.Log("Player : " + m_Owner.name + " doesn't block attacks anymore");
    }
    /////////////////////////////////////////////

    public void IncreaseGaugeValue(float value)
    {
        m_CurrentGaugeValue += value;
        ClampGaugeValue();
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
                    m_CurrentGaugeValue -= AttackConfig.Instance.m_StunGaugeDecreaseSpeed * Time.unscaledDeltaTime;
                    ClampGaugeValue();
                    OnGaugeValueChanged?.Invoke();
                }
            }
            else
            {
                m_StabilizeGaugeCooldown -= Time.unscaledDeltaTime;
            }
        }
    }

    public float GetCurrentGaugeValue()
    {
        return m_CurrentGaugeValue;
    }

    private void ClampGaugeValue()
    {
        m_CurrentGaugeValue = Mathf.Clamp(m_CurrentGaugeValue, 0f, AttackConfig.Instance.m_StunGaugeMaxValue);
    }
}
