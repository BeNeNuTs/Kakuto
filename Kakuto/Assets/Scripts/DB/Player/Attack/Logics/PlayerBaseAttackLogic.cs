using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseAttackLogic : Object
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
}
