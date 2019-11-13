﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    public void BlockAttack()
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if (behaviour)
        {
            Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.BlockAttack, behaviour.m_AnimationAttackName);
        }
        else
        {
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
    }

    public void UnblockAttack(UnblockAttackAnimEventConfig param)
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if(behaviour)
        {
            UnblockAttackAnimEvent unblockAnimEvent = new UnblockAttackAnimEvent(behaviour.m_AnimationAttackName, param);
            Utils.GetPlayerEventManager<UnblockAttackAnimEvent>(gameObject).TriggerEvent(EPlayerEvent.UnblockAttack, unblockAnimEvent);
        }
        else
        {
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
    }

    public void BlockMovement()
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if (behaviour)
        {
            Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.BlockMovement, behaviour.m_AnimationAttackName);
        }
        else
        {
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
    }

    public void UnblockMovement()
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if (behaviour)
        {
            Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.UnblockMovement, behaviour.m_AnimationAttackName);
        }
        else
        {
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
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

    public void TriggerJumpImpulse()
    {
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.TriggerJumpImpulse, true);
    }
}
