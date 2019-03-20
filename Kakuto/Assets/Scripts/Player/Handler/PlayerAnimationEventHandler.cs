using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    public void EndOfAttack(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.EndOfAttack, attackName);
    }

    public void UnblockAttack(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.UnblockAttack, attackName);
    }

    public void UnblockMovement(EAnimationAttackName attackName)
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.UnblockMovement, attackName);
    }
}
