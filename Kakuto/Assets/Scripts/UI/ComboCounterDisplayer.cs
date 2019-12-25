using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboCounterDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public Text m_ComboText;

    private PlayerComboCounterSubComponent m_PlayerComboCounterSC;

    private bool m_FreezeHitCounterDisplay = false;
    private float m_FreezeHitCounterDisplayCooldown = 0f;

    private void InitComboCounterSubComponent()
    {
        if (m_PlayerComboCounterSC == null)
        {
            PlayerAttackComponent playerAttackComponent = GameManager.Instance.GetPlayerComponent<PlayerAttackComponent>(m_Target);
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
    }

    private void Update()
    {
        InitComboCounterSubComponent();

        if(m_PlayerComboCounterSC != null)
        {
            if (!m_FreezeHitCounterDisplay)
            {
                m_ComboText.enabled = m_PlayerComboCounterSC.GetHitCounter() >= 2;
                m_ComboText.text = m_PlayerComboCounterSC.GetHitCounter() + " HIT";
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
            if(m_PlayerComboCounterSC.GetHitCounter() == 0)
            {
                if (m_ComboText.enabled)
                {
                    m_FreezeHitCounterDisplay = true;
                    m_FreezeHitCounterDisplayCooldown = ComboCounterConfig.Instance.m_TimeToDisappearAfterComboBreak;
                }
            }
        }
    }
}
