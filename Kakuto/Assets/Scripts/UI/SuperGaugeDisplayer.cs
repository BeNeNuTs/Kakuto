using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuperGaugeDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public Image m_GaugeImage;
    public TextMeshProUGUI m_GaugeTextAmount;

    private PlayerSuperGaugeSubComponent m_PlayerSuperGaugeSC;

    private void Awake()
    {
        m_GaugeImage.fillAmount = 0f;
        m_GaugeTextAmount.text = "0";
    }

    private void Update()
    {
        if(m_PlayerSuperGaugeSC == null)
        {
            PlayerAttackComponent playerAttackComponent = GameManager.Instance.GetPlayerComponent<PlayerAttackComponent>(m_Target);
            if(playerAttackComponent != null)
            {
                m_PlayerSuperGaugeSC = playerAttackComponent.GetSuperGaugeSubComponent();
                m_PlayerSuperGaugeSC.OnGaugeValueChanged += OnGaugeValueChanged;
                OnGaugeValueChanged();
            }
        }
    }

    private void OnGaugeValueChanged()
    {
        if(m_PlayerSuperGaugeSC != null)
        {
            float gaugeRatio = m_PlayerSuperGaugeSC.GetCurrentGaugeValue() / AttackConfig.Instance.m_SuperGaugeMaxValue;
            m_GaugeImage.fillAmount = gaugeRatio;
            m_GaugeTextAmount.text = ((uint)(m_PlayerSuperGaugeSC.GetCurrentGaugeValue())).ToString();
        }
    }

    private void OnDestroy()
    {
        if (m_PlayerSuperGaugeSC != null)
        {
            m_PlayerSuperGaugeSC.OnGaugeValueChanged -= OnGaugeValueChanged;
        }
    }
}
