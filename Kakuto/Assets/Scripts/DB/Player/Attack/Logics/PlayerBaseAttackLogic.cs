using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseAttackLogic
{
    protected GameObject m_Owner;
    protected PlayerAttack m_Attack;
    protected Animator m_Animator;
    protected PlayerMovementComponent m_MovementComponent;

    private bool m_HasHit = false;

    public virtual void OnInit(GameObject owner, PlayerAttack attack)
    {
        m_Owner = owner;
        m_Attack = attack;
        m_Animator = m_Owner.GetComponentInChildren<Animator>();
        m_MovementComponent = m_Owner.GetComponent<PlayerMovementComponent>();
    }

    public virtual void OnShutdown() {}

    public virtual bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = true;

        PlayerAttack attack = GetAttack();
        if (m_MovementComponent != null)
        {
            conditionIsValid &= (attack.m_NeededStance == m_MovementComponent.GetCurrentStance());
        }

        if (attack.m_HasCondition)
        {
            if (attack.m_HasAttackRequirement)
            {
                conditionIsValid &= (currentAttackLogic != null && currentAttackLogic.GetAttack().m_Name == attack.m_AttackRequired);
            }
        }

        return conditionIsValid;
    }

    public virtual void OnAttackLaunched()
    {
        m_HasHit = false;
        Utils.GetEnemyEventManager<DamageTakenInfo>(m_Owner).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);
    }
    public virtual void OnAttackStopped()
    {
        Utils.GetEnemyEventManager<DamageTakenInfo>(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);
    }

    public virtual bool CanBlockAttack(bool isCrouching) { return false; }
    public virtual uint GetHitDamage(bool isAttackBlocked) { return m_Attack.m_Damage; }

    public virtual uint GetMaxHitCount() { return 1; }
    public virtual float GetDelayBetweenHits() { return 0.1f; }
    public virtual bool IsHitKO() { return false; }

    public virtual bool CanStun() { return false; }
    public virtual float GetStunDuration(bool isAttackBlocked) { return 0.0f; }

    public virtual bool CanPushBack() { return false; }
    public virtual float GetPushBackForce(bool isAttackBlocked) { return 0.0f; }
    public virtual float GetAttackerPushBackForce() { return 0.0f; }

    public virtual bool CanPlayDamageTakenAnim() { return false; }
    public virtual string GetBlockAnimName(EPlayerStance playerStance) { return ""; }
    public virtual string GetHitAnimName(EPlayerStance playerStance) { return ""; }

    public bool HasHit() { return m_HasHit; }

    public GameObject GetOwner() { return m_Owner; }
    public PlayerAttack GetAttack() { return m_Attack; }

    public bool IsASuper() { return m_Attack.m_IsASuper; }

    private void OnEnemyTakesDamage(DamageTakenInfo damageTakenInfo)
    {
        if(this == damageTakenInfo.m_AttackLogic)
        {
            m_HasHit = true;
        }
        else
        {
            Debug.LogError("DamageTaken event has been received in " + m_Attack.m_AnimationAttackName + " but damage taken doesn't come from this attack. This attack has not been stopped correctly");
        }
    }
}
