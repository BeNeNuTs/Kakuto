using UnityEngine;
using UnityEngine.UI;

public class TriggerPointDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public Image m_TriggerImage;

    private void Awake()
    {
        m_TriggerImage.enabled = false;
    }

    private void Start()
    {
        PlayerGuardCrushTriggerAttackLogic.OnTriggerPointStatusChanged[(int)m_Target] += OnTriggerPointStatusChanged;
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
