using UnityEngine;
using Cinemachine;

public class CameraMultiTargetsSubGameManager : SubGameManagerBase
{
    private CinemachineMultipleTargets m_CinemachineMultipleTargets;

    public override void OnPlayerRegistered(GameObject player)
    {
        GetCinemachineMultiTargetsComponent()?.AddTarget(player.transform);
    }

    public override void OnPlayerUnregistered(GameObject player)
    {
        GetCinemachineMultiTargetsComponent()?.RemoveTarget(player.transform);
    }

    CinemachineMultipleTargets GetCinemachineMultiTargetsComponent()
    {
        if(m_CinemachineMultipleTargets == null)
        {
            GameObject virtualCam = GameObject.FindGameObjectWithTag("VirtualCamera");
            if (virtualCam != null)
            {
                m_CinemachineMultipleTargets = virtualCam.GetComponent<CinemachineMultipleTargets>();
            }
        }

        return m_CinemachineMultipleTargets;
    }
}
