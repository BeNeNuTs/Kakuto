using UnityEngine;
using UnityEngine.Events;

public enum EStunType
{
    Hit,
    Block,
    Grab,
    None
}

public class PlayerStunInfoSubComponent : PlayerBaseSubComponent
{
    struct StunInfo
    {
        public bool m_IsStunned;
        public bool m_StunnedWhileJumping;
        public bool m_StunnedByHitKO;
        public bool m_IsAnimStunned;
        public EStunType m_StunType;
        public float m_EndOfStunTimestamp;
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

        Utils.GetPlayerEventManager<AnimStunInfo>(m_Owner).StartListening(EPlayerEvent.StartAnimStun, OnStartAnimStun);
        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.StopAnimStun, OnStopAnimStun);
    }

    ~PlayerStunInfoSubComponent()
    {
        Utils.GetPlayerEventManager<AnimStunInfo>(m_Owner).StopListening(EPlayerEvent.StartAnimStun, OnStartAnimStun);
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.StopAnimStun, OnStopAnimStun);
    }

    public void Update()
    {
        if (IsStunned())
        {
            if (Time.unscaledTime > m_StunInfo.m_EndOfStunTimestamp)
            {
                StopStun();
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

    private void OnStartAnimStun(AnimStunInfo stunInfo)
    {
        StartStun(stunInfo.m_StunDuration, stunInfo.m_IsHitKO, stunInfo.m_StunType, true);
    }

    private void OnStopAnimStun(bool dummy)
    {
        // OnStopAnimStun is allowed to stop only anim stunned 
        // because the machinebehavior will go through state exit and then state enter
        // but on code side, we trigger the animation and in the same frame we start the stun
        // So StopAnimStun could stun a new stun applied by code
        if (IsAnimStunned())
        {
            StopStun();
        }
    }

    public void StartStun(float stunDuration, bool isHitKO, EStunType stunType, bool animStunned)
    {
        m_StunInfo.m_EndOfStunTimestamp = Time.unscaledTime + stunDuration;
        if (!IsStunned())
        {
            m_StunInfo.m_IsStunned = true;
            m_StunInfo.m_StunnedWhileJumping = m_MovementComponent.IsJumping();
            m_StunInfo.m_StunnedByHitKO = isHitKO;
            m_StunInfo.m_StunType = stunType;
            m_StunInfo.m_IsAnimStunned = animStunned;
            Utils.GetPlayerEventManager<float>(m_Owner).TriggerEvent(EPlayerEvent.StunBegin, m_StunInfo.m_EndOfStunTimestamp);
        }

        if (animStunned)
        {
            Debug.Log("Player : " + m_Owner.name + " is anim stunned");
        }
        else
        {
            Debug.Log("Player : " + m_Owner.name + " is stunned during " + stunDuration + " seconds");
        }

    }

    private void StopStun()
    {
        EStunType stunType = m_StunInfo.m_StunType;

        m_StunInfo.m_IsStunned = false;
        m_StunInfo.m_StunnedWhileJumping = false;
        m_StunInfo.m_StunnedByHitKO = false;
        m_StunInfo.m_StunType = EStunType.None;
        m_StunInfo.m_EndOfStunTimestamp = 0;
        m_StunInfo.m_IsAnimStunned = false;

        m_StabilizeGaugeCooldown = AttackConfig.Instance.m_StunGaugeDecreaseCooldown;

        Utils.GetPlayerEventManager<float>(m_Owner).TriggerEvent(EPlayerEvent.StunEnd, m_StunInfo.m_EndOfStunTimestamp);

        // DEBUG ///////////////////////////////////
        if (stunType == EStunType.Hit && m_HealthComponent.m_DEBUG_IsBlockingAllAttacksAfterHitStun)
        {
            DEBUG_StartBlockingAttacks();
        }
        ////////////////////////////////////////////
        else
        {
            m_Anim.SetTrigger("OnStunEnd");
            Debug.Log("Player : " + m_Owner.name + " is no more stunned");
        }
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

    public bool IsAnimStunned()
    {
        return m_StunInfo.m_IsStunned && m_StunInfo.m_IsAnimStunned;
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
