﻿using UnityEngine;
using TMPro;

public class ComboCounterDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public TextMeshProUGUI m_ComboText;
    public TextMeshProUGUI m_HitText;

    public TextMeshProUGUI m_ComboDamageDisplayer;
    public TextMeshProUGUI m_ComboDamageText;

    private GameObject m_RegisteredPlayer;
    private PlayerComboCounterSubComponent m_PlayerComboCounterSC;
    private PlayerSettings m_PlayerSettings;

    private bool m_FreezeHitCounterDisplay = false;
    private float m_FreezeHitCounterDisplayCooldown = 0f;

    private void Awake()
    {
        GameManager.Instance?.AddOnPlayerRegisteredCallback(OnPlayerRegistered, m_Target);
    }

    private void OnPlayerRegistered(GameObject player)
    {
        m_RegisteredPlayer = player;
        m_PlayerSettings = ScenesConfig.GetPlayerSettings(m_RegisteredPlayer);
        m_ComboDamageDisplayer.enabled = m_PlayerSettings.m_DisplayComboDamage;
        m_ComboDamageText.enabled = m_PlayerSettings.m_DisplayComboDamage;
    }

    private void InitComboCounterSubComponent()
    {
        if (m_PlayerComboCounterSC == null && m_RegisteredPlayer != null)
        {
            PlayerAttackComponent playerAttackComponent = m_RegisteredPlayer.GetComponent<PlayerAttackComponent>();
            if (playerAttackComponent != null)
            {
                m_PlayerComboCounterSC = playerAttackComponent.GetComboCounterSubComponent();
                m_PlayerComboCounterSC.OnHitCounterChanged += OnHitCounterChanged;
            }
        }
    }

    private void OnDestroy()
    {
        if(m_PlayerComboCounterSC != null)
        {
            m_PlayerComboCounterSC.OnHitCounterChanged -= OnHitCounterChanged;
        }
        m_RegisteredPlayer = null;
        GameManager.Instance?.RemoveOnPlayerRegisteredCallback(OnPlayerRegistered, m_Target);
    }

    private void Update()
    {
        InitComboCounterSubComponent();

        if(m_PlayerComboCounterSC != null)
        {
            if (!m_FreezeHitCounterDisplay)
            {
                m_ComboText.enabled = m_PlayerComboCounterSC.GetComboCounter() >= 2;
                m_HitText.enabled = m_ComboText.enabled;
                m_ComboText.text = m_PlayerComboCounterSC.GetComboCounter().ToString();

                // Don't want to display when cumulated combo damage is reset
                if(m_PlayerSettings.m_DisplayComboDamage && m_PlayerComboCounterSC.GetCumulatedComboDamage() > 0)
                    m_ComboDamageDisplayer.text = m_PlayerComboCounterSC.GetCumulatedComboDamage().ToString();
            }
            else
            {
                m_FreezeHitCounterDisplayCooldown -= Time.deltaTime;
                if (m_FreezeHitCounterDisplayCooldown <= 0f)
                {
                    m_FreezeHitCounterDisplay = false;
                    m_FreezeHitCounterDisplayCooldown = 0f;
                }
            }
        }
    }

    private void OnHitCounterChanged()
    {
        if(m_PlayerComboCounterSC != null)
        {
            if(m_PlayerComboCounterSC.GetComboCounter() == 0)
            {
                if (m_ComboText.enabled)
                {
                    m_FreezeHitCounterDisplay = true;
                    m_FreezeHitCounterDisplayCooldown = UIConfig.Instance.m_TimeToDisappearAfterComboBreak;
                }
            }
            else if(m_FreezeHitCounterDisplay && m_PlayerComboCounterSC.GetComboCounter() >= 2)
            {
                m_FreezeHitCounterDisplay = false;
            }
        }
    }
}
