using UnityEngine;
using UnityEngine.Events;

public class PlayerSuperGaugeSubComponent : PlayerBaseSubComponent
{
    private float m_CurrentGaugeValue = 0f;

    private PlayerAttackComponent m_AttackComponent;

    public event UnityAction OnGaugeValueChanged;

    public PlayerSuperGaugeSubComponent(PlayerAttackComponent attackComp) : base(attackComp.gameObject)
    {
        m_AttackComponent = attackComp;
        if (m_AttackComponent.m_DEBUG_SuperGaugeAlwaysFilled)
        {
            IncreaseGaugeValue(AttackConfig.Instance.m_SuperGaugeMaxValue);
        }
    }

    public void IncreaseGaugeValue(float value)
    {
        m_CurrentGaugeValue += value;
        ClampGaugeValue();
        OnGaugeValueChanged?.Invoke();
    }

    public void DecreaseGaugeValue(float value)
    {
        if (m_AttackComponent.m_DEBUG_SuperGaugeAlwaysFilled)
        {
            return;
        }

        if (value > m_CurrentGaugeValue)
        {
            Debug.LogError("The amount to decrease to the super gauge is superior to the current amount: Current Amount(" + m_CurrentGaugeValue + ") - Amount to Decrease(" + value + ")");
        }

        m_CurrentGaugeValue -= value;
        ClampGaugeValue();
        OnGaugeValueChanged?.Invoke();
    }

    public float GetCurrentGaugeValue()
    {
        return m_CurrentGaugeValue;
    }

    private void ClampGaugeValue()
    {
        m_CurrentGaugeValue = Mathf.Clamp(m_CurrentGaugeValue, 0f, AttackConfig.Instance.m_SuperGaugeMaxValue);
    }
}
