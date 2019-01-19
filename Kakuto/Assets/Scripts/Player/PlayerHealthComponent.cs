using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthComponent : MonoBehaviour {

    public PlayerHealthConfig m_HealthConfig;

    private uint m_HP;

    private Animator m_Anim;

    private void Awake()
    {
        m_HP = m_HealthConfig.m_MaxHP;
        m_Anim = GetComponentInChildren<Animator>();
    }

    public bool IsDead()
    {
        return m_HP == 0;
    }

    public void OnHit(PlayerAttack attack)
    {
        if(IsDead())
        {
            return;
        }

        ApplyDamage(attack.m_Damage);
    }

    private void ApplyDamage(uint damage)
    {
        if (damage > m_HP)
        {
            m_HP = 0;
        }
        else
        {
            m_HP -= damage;
        }

        OnDamageTaken();
    }

    private void OnDamageTaken()
    {
        Debug.Log("Player : " + gameObject.name + " HP : " + m_HP);

        if (IsDead())
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        m_Anim.SetTrigger("OnDeath");
    }
}
