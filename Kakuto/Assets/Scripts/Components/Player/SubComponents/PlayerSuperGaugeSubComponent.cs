using System;
using UnityEngine;

public class PlayerSuperGaugeSubComponent : PlayerBaseSubComponent
{
    private float m_CurrentGaugeValue = 0f;

    private PlayerInfoComponent m_InfoComponent;

    public Action OnGaugeValueChanged;

    public PlayerSuperGaugeSubComponent(PlayerInfoComponent infoComp) : base(infoComp.gameObject)
    {
        m_InfoComponent = infoComp;

        if (m_InfoComponent.GetPlayerSettings().SuperGaugeAlwaysFilled)
        {
            IncreaseGaugeValue(AttackConfig.Instance.m_SuperGaugeMaxValue);
        }
        else
        {
            IncreaseGaugeValue(GameManager.Instance.GetSubManager<RoundSubGameManager>(ESubManager.Round).GetPlayerSuperGaugeValue(m_InfoComponent.GetPlayerEnum()));
        }
        m_InfoComponent.GetPlayerSettings().OnSuperGaugeAlwaysFilledChanged += OnSuperGaugeAlwaysFilledChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_InfoComponent.GetPlayerSettings().OnSuperGaugeAlwaysFilledChanged -= OnSuperGaugeAlwaysFilledChanged;
        GamePauseMenuComponent.IsInPauseChanged -= FillSuperGaugeAfterPause;
    }

    public void IncreaseGaugeValue(float value)
    {
        m_CurrentGaugeValue += value;
        ClampGaugeValue();
        OnGaugeValueChanged?.Invoke();
    }

    public void DecreaseGaugeValue(float value)
    {
        if (m_InfoComponent.GetPlayerSettings().SuperGaugeAlwaysFilled)
        {
            return;
        }

        if (value > m_CurrentGaugeValue)
        {
            KakutoDebug.LogError("The amount to decrease to the super gauge is superior to the current amount: Current Amount(" + m_CurrentGaugeValue + ") - Amount to Decrease(" + value + ")");
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

    private void OnSuperGaugeAlwaysFilledChanged(bool superGaugeAlwaysFilled)
    {
        if (superGaugeAlwaysFilled)
        {
            GamePauseMenuComponent.IsInPauseChanged += FillSuperGaugeAfterPause;
        }
        else
        {
            GamePauseMenuComponent.IsInPauseChanged -= FillSuperGaugeAfterPause;
        }
    }

    private void FillSuperGaugeAfterPause(bool isInPause)
    {
        if (!isInPause)
        {
            IncreaseGaugeValue(AttackConfig.Instance.m_SuperGaugeMaxValue);
            GamePauseMenuComponent.IsInPauseChanged -= FillSuperGaugeAfterPause;
        }
    }
}
