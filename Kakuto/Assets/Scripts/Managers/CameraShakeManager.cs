using UnityEngine;
using Cinemachine;

public static class CameraShakeManager
{
    public static void GenerateImpulseAt(CameraShakeParams shakeParams, Vector3 position, Vector3 direction)
    {
        CinemachineImpulseDefinition impulseDefinition = new CinemachineImpulseDefinition();
        impulseDefinition.m_ImpulseChannel = GameConfig.Instance.m_ImpulseChannel;
        impulseDefinition.m_RawSignal = GameConfig.Instance.m_ImpulseRawSignal;
        impulseDefinition.m_AmplitudeGain = shakeParams.m_AmplitudeGain;
        impulseDefinition.m_FrequencyGain = shakeParams.m_FrequencyGain;
        impulseDefinition.m_RepeatMode = CinemachineImpulseDefinition.RepeatMode.Stretch;
        impulseDefinition.m_Randomize = true;

        impulseDefinition.m_TimeEnvelope.m_AttackTime = shakeParams.m_AttackTime;
        impulseDefinition.m_TimeEnvelope.m_SustainTime = shakeParams.m_SustainTime;
        impulseDefinition.m_TimeEnvelope.m_DecayTime = shakeParams.m_DecayTime;

        impulseDefinition.m_ImpactRadius = 100f;
        impulseDefinition.m_DirectionMode = CinemachineImpulseManager.ImpulseEvent.DirectionMode.Fixed;
        impulseDefinition.m_DissipationMode = CinemachineImpulseManager.ImpulseEvent.DissipationMode.ExponentialDecay;
        impulseDefinition.m_DissipationDistance = 1000f;

        impulseDefinition.CreateEvent(position, direction);
    }
}