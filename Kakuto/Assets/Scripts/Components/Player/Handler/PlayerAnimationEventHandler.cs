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

    public void EndOfGrab()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.EndOfGrab, EAnimationAttackName.Grab);
    }

    public void ApplyGrabDamages()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.ApplyGrabDamages, EAnimationAttackName.Grab);
    }

    public void TriggerProjectile()
    {
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.TriggerProjectile, true);
    }

    public void StopMovement()
    {
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.StopMovement, true);
    }
}
