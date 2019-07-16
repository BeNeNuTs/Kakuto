using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseAttackLogic
{
    protected GameObject m_Owner;
    protected PlayerAttack m_Attack;
    protected Animator m_Animator;

    public virtual void OnInit(GameObject owner, PlayerAttack attack)
    {
        m_Owner = owner;
        m_Attack = attack;
        m_Animator = m_Owner.GetComponentInChildren<Animator>();
    }

    public abstract void OnAttackLaunched();
    public virtual void OnAttackStopped() {}

    public virtual bool CanBlockAttack(bool isCrouching) { return false; }
    public virtual uint GetHitDamage(bool isAttackBlocked) { return m_Attack.m_Damage; }

    public virtual bool CanStun() { return false; }
    public virtual float GetStunDuration(bool isAttackBlocked) { return 0.0f; }

    public virtual bool CanPushBack() { return false; }
    public virtual float GetPushBackForce(bool isAttackBlocked) { return 0.0f; }

    public virtual bool CanPlayDamageTakenAnim() { return false; }
    public virtual string GetBlockAnimName(EPlayerStance playerStance) { return ""; }
    public virtual string GetHitAnimName(EPlayerStance playerStance) { return ""; }

    public PlayerAttack GetAttack() { return m_Attack; }
}
