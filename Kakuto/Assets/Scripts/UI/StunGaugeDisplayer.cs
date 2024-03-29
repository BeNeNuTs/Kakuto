﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StunGaugeDisplayer : MonoBehaviour
{
    private static readonly string K_STUNNED_TEXT = "STUNNED";

    public EPlayer m_Target;
    public Image m_GaugeImage;
    public TextMeshProUGUI m_GaugeTextAmount;

    private PlayerStunInfoSubComponent m_PlayerStunInfoSC;

    private void Awake()
    {
        m_GaugeImage.fillAmount = 0f;
        m_GaugeTextAmount.text = "0";
    }

    private void Update()
    {
        if (m_PlayerStunInfoSC == null)
        {
            PlayerHealthComponent playerHealthComponent = GameManager.Instance.GetPlayerComponent<PlayerHealthComponent>(m_Target);
            if (playerHealthComponent != null)
            {
                m_PlayerStunInfoSC = playerHealthComponent.GetStunInfoSubComponent();
                m_PlayerStunInfoSC.OnGaugeValueChanged += OnGaugeValueChanged;
                OnGaugeValueChanged();
            }
        }
    }

    private void OnGaugeValueChanged()
    {
        if (m_PlayerStunInfoSC != null)
        {
            float gaugeRatio = m_PlayerStunInfoSC.GetCurrentGaugeValue() / AttackConfig.Instance.m_StunGaugeMaxValue;
            m_GaugeImage.fillAmount = gaugeRatio;

            if(gaugeRatio < 1f)
            {
                m_GaugeTextAmount.text = ((uint)(m_PlayerStunInfoSC.GetCurrentGaugeValue())).ToString();
            }
            else
            {
                m_GaugeTextAmount.text = K_STUNNED_TEXT;
            }
        }
    }

    private void OnDestroy()
    {
        if (m_PlayerStunInfoSC != null)
        {
            m_PlayerStunInfoSC.OnGaugeValueChanged -= OnGaugeValueChanged;
        }
    }
}
