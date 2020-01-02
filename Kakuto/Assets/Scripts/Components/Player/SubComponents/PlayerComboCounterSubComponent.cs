using UnityEngine;
using UnityEngine.Events;

public class PlayerComboCounterSubComponent : PlayerBaseSubComponent
{
    private uint m_ComboCounter = 0;

    public event UnityAction OnHitCounterChanged;

    public PlayerComboCounterSubComponent(GameObject owner) : base(owner)
    {
        Utils.GetEnemyEventManager<DamageTakenInfo>(owner).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakeDamage);
        Utils.GetEnemyEventManager<bool>(owner).StartListening(EPlayerEvent.StunEnd, OnEnemyStunEnd);
    }

    ~PlayerComboCounterSubComponent()
    {
        Utils.GetEnemyEventManager<DamageTakenInfo>(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakeDamage);
        Utils.GetEnemyEventManager<bool>(m_Owner).StartListening(EPlayerEvent.StunEnd, OnEnemyStunEnd);
    }

    private void OnEnemyTakeDamage(DamageTakenInfo damageTakenInfo)
    {
        if(damageTakenInfo.m_IsAlreadyHitStunned || m_ComboCounter == 0)
        {
            m_ComboCounter++;
            OnHitCounterChanged?.Invoke();
        }
    }

    private void OnEnemyStunEnd(bool isStunned = false)
    {
        m_ComboCounter = 0;
        OnHitCounterChanged?.Invoke();
    }

    public uint GetComboCounter()
    {
        return m_ComboCounter;
    }
}
