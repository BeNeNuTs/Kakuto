using UnityEngine;
using UnityEngine.UI;

public class TriggerPointDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public Image m_TriggerActiveImage;
    public Image m_TriggeredImage;

    private void Start()
    {
        PlayerGuardCrushTriggerAttackLogic.OnTriggerPointStatusChanged[(int)m_Target] += OnTriggerPointStatusChanged;

        PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus triggerPointStatusAtStart = PlayerGuardCrushTriggerAttackLogic.GetTriggerPointStatus((int)m_Target);
        OnTriggerPointStatusChanged(triggerPointStatusAtStart);
    }

    private void OnDestroy()
    {
        PlayerGuardCrushTriggerAttackLogic.OnTriggerPointStatusChanged[(int)m_Target] -= OnTriggerPointStatusChanged;
    }

    private void OnTriggerPointStatusChanged(PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus status)
    {
        m_TriggerActiveImage.enabled = status == PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus.Active;
        m_TriggeredImage.enabled = status == PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus.Triggered;
    }
}
