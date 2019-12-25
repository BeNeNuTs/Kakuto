﻿using UnityEngine;
using UnityEngine.Events;

public class PlayerComboCounterSubComponent : PlayerBaseSubComponent
{
    private uint m_HitCounter = 0;

    public event UnityAction OnHitCounterChanged;

    public PlayerComboCounterSubComponent(GameObject owner) : base(owner)
    {
        Utils.GetEnemyEventManager<DamageTakenInfo>(owner).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakeDamage);
        Utils.GetEnemyEventManager<float>(owner).StartListening(EPlayerEvent.StunEnd, OnEnemyStunEnd);
    }

    ~PlayerComboCounterSubComponent()
    {
        Utils.GetEnemyEventManager<DamageTakenInfo>(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakeDamage);
        Utils.GetEnemyEventManager<float>(m_Owner).StartListening(EPlayerEvent.StunEnd, OnEnemyStunEnd);
    }

    private void OnEnemyTakeDamage(DamageTakenInfo damageTakenInfo)
    {
        if(damageTakenInfo.m_IsAlreadyHitStunned || m_HitCounter == 0)
        {
            m_HitCounter++;
            OnHitCounterChanged();
        }
    }

    private void OnEnemyStunEnd(float stunTimer)
    {
        m_HitCounter = 0;
        OnHitCounterChanged();
    }

    public uint GetHitCounter()
    {
        return m_HitCounter;
    }
}
