using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuperGaugeDisplayer : MonoBehaviour
{
    private static float K_CHUNK_VALUE = 25f;

    public EPlayer m_Target;
    public Image m_GaugeImage;
    public Image[] m_GaugeChunks;

    private PlayerSuperGaugeSubComponent m_PlayerSuperGaugeSC;

    private void Awake()
    {
        m_GaugeImage.fillAmount = 0f;
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
            float gaugeValue = m_PlayerSuperGaugeSC.GetCurrentGaugeValue();
            float gaugeRatio = gaugeValue / AttackConfig.Instance.m_SuperGaugeMaxValue;
            m_GaugeImage.fillAmount = gaugeRatio;
            int gaugeChunksToDisplay = Mathf.FloorToInt(gaugeValue / K_CHUNK_VALUE);
            for(int i = 0; i < m_GaugeChunks.Length; i++)
            {
                m_GaugeChunks[i].enabled = gaugeChunksToDisplay > i;
            }
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
