using UnityEngine;
using UnityEngine.UI;

public class TriggerPointDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public Image m_TriggerImage;

    private void Start()
    {
        PlayerGuardCrushTriggerAttackLogic.OnTriggerPointStatusChanged[(int)m_Target] += OnTriggerPointStatusChanged;

        bool isTriggerActiveAtStart = PlayerGuardCrushTriggerAttackLogic.IsTriggerPointActive((int)m_Target);
        OnTriggerPointStatusChanged(isTriggerActiveAtStart);
    }

    private void OnDestroy()
    {
        PlayerGuardCrushTriggerAttackLogic.OnTriggerPointStatusChanged[(int)m_Target] -= OnTriggerPointStatusChanged;
    }

    private void OnTriggerPointStatusChanged(bool isActive)
    {
        m_TriggerImage.enabled = isActive;
    }
}
