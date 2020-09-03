using UnityEngine;
using UnityEngine.Events;

public class PlayerComboCounterSubComponent : PlayerBaseSubComponent
{
    private uint m_ComboCounter = 0;

    public event UnityAction OnHitCounterChanged;

    public PlayerComboCounterSubComponent(GameObject owner) : base(owner)
    {
        Utils.GetEnemyEventManager(owner).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakeDamage);
        Utils.GetEnemyEventManager(owner).StartListening(EPlayerEvent.StunEnd, OnEnemyStunEnd);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Utils.GetEnemyEventManager(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakeDamage);
        Utils.GetEnemyEventManager(m_Owner).StartListening(EPlayerEvent.StunEnd, OnEnemyStunEnd);
    }

    private void OnEnemyTakeDamage(BaseEventParameters baseParams)
    {
        DamageTakenEventParameters damageTakenInfo = (DamageTakenEventParameters)baseParams;
        if (damageTakenInfo.m_AttackResult == EAttackResult.Hit)
        {
            if (damageTakenInfo.m_IsAlreadyHitStunned || m_ComboCounter == 0)
            {
                m_ComboCounter++;
                OnHitCounterChanged?.Invoke();
            }
        }
    }

    private void OnEnemyStunEnd(BaseEventParameters baseParams)
    {
        m_ComboCounter = 0;
        OnHitCounterChanged?.Invoke();
    }

    public uint GetComboCounter()
    {
        return m_ComboCounter;
    }
}
