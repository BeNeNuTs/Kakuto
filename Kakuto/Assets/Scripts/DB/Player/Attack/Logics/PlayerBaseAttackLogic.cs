using UnityEngine;

public enum EStunAnimState
{
    In,
    Loop,
    Out
}

public class PlayerBaseAttackLogic
{
    protected GameObject m_Owner;
    protected PlayerAttack m_Attack;
    protected Animator m_Animator;
    protected PlayerMovementComponent m_MovementComponent;
    protected PlayerAttackComponent m_AttackComponent;
    protected PlayerInfoComponent m_InfoComponent;

    protected bool m_HasTouched = false;
    protected float m_DamageRatio = 1f;
    protected bool m_DamageRatioComputed = false;

    protected bool m_AttackLaunched = false;

    public virtual void OnInit(GameObject owner, PlayerAttack attack)
    {
        m_Owner = owner;
        m_Attack = attack;
        m_Animator = m_Owner.GetComponentInChildren<Animator>();
        m_MovementComponent = m_Owner.GetComponent<PlayerMovementComponent>();
        m_AttackComponent = m_Owner.GetComponent<PlayerAttackComponent>();
        m_InfoComponent = m_Owner.GetComponent<PlayerInfoComponent>();
    }

    public virtual void OnShutdown() {}

    public virtual bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = true;

        PlayerAttack attack = GetAttack();
        if (m_MovementComponent != null)
        {
            conditionIsValid &= (attack.m_NeededStanceList.Contains(m_MovementComponent.GetCurrentStance()));
        }

        if (attack.m_HasCondition)
        {
            if(attack.m_SuperGaugeAmountNeeded > 0f)
            {
                PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
                if(superGaugeSC != null)
                {
                    conditionIsValid &= superGaugeSC.GetCurrentGaugeValue() >= attack.m_SuperGaugeAmountNeeded;
                }
            }

            if (attack.m_HasAttackRequirement)
            {
                conditionIsValid &= (currentAttackLogic != null && currentAttackLogic.GetAttack().m_AnimationAttackName == attack.m_AttackRequired);
            }

            if (attack.m_HasMovementRequirement)
            {
                conditionIsValid &= attack.m_MovementRequiredList.Contains(m_MovementComponent.GetMovingDirection());
            }
        }

        return conditionIsValid;
    }

    public virtual void OnAttackLaunched()
    {
        m_Animator.Play(GetAnimationAttackName(), 0, 0);

        if(m_Attack.m_IsEXAttack)
        {
            m_InfoComponent.SetCurrentPalette(EPalette.EX);
        }

        PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
        if (superGaugeSC != null)
        {
            superGaugeSC.DecreaseGaugeValue(GetAttack().m_SuperGaugeAmountNeeded);
        }

        m_HasTouched = false;
        m_DamageRatio = 1f;
        m_DamageRatioComputed = false;
        Utils.GetEnemyEventManager<DamageTakenInfo>(m_Owner).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);

        m_AttackLaunched = true;
    }

    public virtual string GetAnimationAttackName()
    {
        return m_Attack.m_AnimationAttackName.ToString();
    }

    public virtual void OnAttackStopped()
    {
        if (m_Attack.m_IsEXAttack)
        {
            m_InfoComponent.ResetCurrentPalette();
        }

        if (CanStopListeningEnemyTakesDamage())
        {
            Utils.GetEnemyEventManager<DamageTakenInfo>(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);
        }

        m_AttackLaunched = false;
    }

    public virtual bool CanAttackBeBlocked(bool isCrouching) { return true; }
    public virtual bool CanAttackBeParried() { return true; }
    public virtual uint GetHitDamage(EAttackResult attackResult) { return 0; }

    public virtual uint GetCurrentHitCount() { return 0; }
    public virtual uint GetMaxHitCount() { return 1; }
    public virtual float GetDelayBetweenHits() { return 0.1f; }
    public virtual bool IsHitKO() { return false; }

    public virtual bool CanStunOnDamage() { return false; }
    public virtual float GetStunDuration(EAttackResult attackResult) { return 0.0f; }
    public virtual float GetStunGaugeHitAmount() { return 0f; }

    public virtual bool CanPushBack() { return false; }
    public virtual float GetPushBackForce(EAttackResult attackResult) { return 0.0f; }
    public virtual float GetAttackerPushBackForce(EAttackResult attackResult, bool enemyIsInACorner) { return 0.0f; }

    public virtual bool CanPlayDamageTakenAnim() { return false; }
    public virtual string GetBlockAnimName(EPlayerStance playerStance, EStunAnimState state) { return ""; }
    public virtual string GetHitAnimName(EPlayerStance playerStance, EStunAnimState state) { return ""; }

    public virtual void OnHandleCollision(bool triggerHitEvent, Collider2D hitCollider) { }
    public virtual bool NeedPushBoxCollisionCallback() { return false; }
    public virtual void OnHandlePushBoxCollision(Collision2D collision) { }
    public bool HasTouched() { return m_HasTouched; }

    public GameObject GetOwner() { return m_Owner; }
    public PlayerAttack GetAttack() { return m_Attack; }
    public Animator GetAnimator() { return m_Animator; }

    public bool IsASuper() { return m_Attack.m_IsASuper; }

    protected virtual bool CanStopListeningEnemyTakesDamage() { return true; }
    protected virtual void OnEnemyTakesDamage(DamageTakenInfo damageTakenInfo)
    {
        if(this == damageTakenInfo.m_AttackLogic)
        {
            m_HasTouched = true;
        }
        else if(damageTakenInfo.m_AttackLogic.GetType() != typeof(PlayerProjectileAttackLogic))
        {
            Debug.LogError("DamageTaken event has been received in " + m_Attack.m_AnimationAttackName + " but damage taken doesn't come from this attack. This attack has not been stopped correctly");
        }
    }

    protected void IncreaseSuperGauge(float amount)
    {
        if (amount > 0f)
        {
            PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
            if (superGaugeSC != null)
            {
                superGaugeSC.IncreaseGaugeValue(amount);
            }
        }
    }

    protected float GetDamageRatio()
    {
        if(!m_DamageRatioComputed)
        {
            PlayerComboCounterSubComponent comboSC = m_AttackComponent.GetComboCounterSubComponent();
            if (comboSC != null)
            {
                uint hitCounter = comboSC.GetComboCounter();
                m_DamageRatio = AttackConfig.Instance.m_DamageScaling.Evaluate(hitCounter);
                m_DamageRatio = Mathf.Clamp(m_DamageRatio, 0f, 1f);
                m_DamageRatioComputed = true;
            }
        }

        return m_DamageRatio;
    }

    public virtual Collider2D GetLastHitCollider()
    {
        return null;
    }
}
