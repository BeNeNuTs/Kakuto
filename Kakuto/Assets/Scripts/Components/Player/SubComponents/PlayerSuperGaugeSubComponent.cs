using UnityEngine;
using UnityEngine.Events;

public class PlayerSuperGaugeSubComponent
{
    private float m_CurrentGaugeValue = 0f;

    public event UnityAction OnGaugeValueChanged;

    public void IncreaseGaugeValue(float value)
    {
        m_CurrentGaugeValue += value;
        ClampGaugeValue();
        OnGaugeValueChanged();
    }

    public void DecreaseGaugeValue(float value)
    {
        if(value > m_CurrentGaugeValue)
        {
            Debug.LogError("The amount to decrease to the super gauge is superior to the current amount: Current Amount(" + m_CurrentGaugeValue + ") - Amount to Decrease(" + value + ")");
        }

        m_CurrentGaugeValue -= value;
        ClampGaugeValue();
        OnGaugeValueChanged();
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
