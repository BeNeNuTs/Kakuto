using UnityEngine;
using Cinemachine;

public class CameraMultiTargetsSubGameManager : SubGameManagerBase
{
    public override void OnPlayerRegistered(GameObject player)
    {
        CinemachineMultipleTargets multiTargetsComp = GetCinemachineMultiTargetsComponent();
        if(multiTargetsComp != null)
        {
            multiTargetsComp.AddTarget(player.transform);
        }
    }

    public override void OnPlayerUnregistered(GameObject player)
    {
        CinemachineMultipleTargets multiTargetsComp = GetCinemachineMultiTargetsComponent();
        if (multiTargetsComp != null)
        {
            multiTargetsComp.RemoveTarget(player.transform);
        }
    }

    CinemachineMultipleTargets GetCinemachineMultiTargetsComponent()
    {
        GameObject virtualCam = GameObject.FindGameObjectWithTag("VirtualCamera");
        if (virtualCam != null)
        {
            return virtualCam.GetComponent<CinemachineMultipleTargets>();
        }

        return null;
    }
}
