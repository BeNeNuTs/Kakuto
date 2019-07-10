using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    public void UnblockAttack(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.UnblockAttack, attackName);
    }

    public void UnblockMovement(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.UnblockMovement, attackName);
    }

    public void EndOfGrab(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.EndOfGrab, attackName);
    }
}
