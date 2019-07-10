﻿using System.Collections;
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

    public virtual void OnAttackLaunched()
    {
        m_Animator.Play(m_Attack.m_AnimationAttackName.ToString());
    }

    public virtual void OnAttackStopped() { }

    public PlayerAttack GetAttack() { return m_Attack; }
}
