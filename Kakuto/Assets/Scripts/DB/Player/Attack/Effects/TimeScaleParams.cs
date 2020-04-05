using UnityEngine;

[CreateAssetMenu(fileName = "TimeScaleParamsPreset", menuName = "Data/Player/Attacks/Effects/TimeScaleParamsPreset", order = 0)]
public class TimeScaleParams : ScriptableObject
{
    public float m_TimeScaleAmount = 0.5f;
    public float m_TimeScaleDuration = 0.1f;
    [Tooltip("Smooth will gradually smoothing time scale effect from timeScaleAmount to 1 in timeScaleDuration. Instant will set back instantly timeScale to 1 after timeScaleDuration")]
    public ETimeScaleBackToNormal m_TimeScaleBackToNormal = ETimeScaleBackToNormal.Instant;
}