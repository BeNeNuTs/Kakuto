using UnityEngine;
using TMPro;

public class ComboCounterDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public TextMeshProUGUI m_ComboText;
    public TextMeshProUGUI m_HitText;

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
                m_ComboText.enabled = m_PlayerComboCounterSC.GetComboCounter() >= 2;
                m_HitText.enabled = m_ComboText.enabled;
                m_ComboText.text = m_PlayerComboCounterSC.GetComboCounter().ToString();
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
