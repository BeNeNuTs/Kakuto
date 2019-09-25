using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    public void BlockAttack(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.BlockAttack, attackName);
    }

    public void UnblockAttack(UnblockAttackAnimEventParameters param)
    {
        Utils.GetPlayerEventManager<UnblockAttackAnimEventParameters>(gameObject).TriggerEvent(EPlayerEvent.UnblockAttack, param);
    }

    public void BlockMovement(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.BlockMovement, attackName);
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
