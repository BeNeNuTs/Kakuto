using UnityEngine;

[CreateAssetMenu(fileName = "CameraShakeParamsPreset", menuName = "Data/Player/Attacks/Effects/CameraShakeParamsPreset", order = 0)]
public class CameraShakeParams : ScriptableObject
{
    [Tooltip("Gain to apply to the amplitudes defined in the signal source.  1 is normal.  Setting this to 0 completely mutes the signal.")]
    public float m_AmplitudeGain;
    [Tooltip("Scale factor to apply to the time axis.  1 is normal.  Larger magnitudes will make the signal progress more rapidly.")]
    public float m_FrequencyGain;
    [Tooltip("Duration in seconds of the attack.  Attack curve will be scaled to fit.  Must be >= 0.")]
    public float m_AttackTime;
    [Tooltip("Duration in seconds of the central fully-scaled part of the envelope.  Must be >= 0.")]
    public float m_SustainTime;
    [Tooltip("Duration in seconds of the decay.  Decay curve will be scaled to fit.  Must be >= 0.")]
    public float m_DecayTime;
}