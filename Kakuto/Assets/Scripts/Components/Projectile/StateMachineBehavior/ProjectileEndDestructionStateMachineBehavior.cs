using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEndDestructionStateMachineBehavior : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ProjectileComponent projectile = animator.GetComponentInChildren<ProjectileComponent>();
        if(projectile != null)
        {
            projectile.OnEndOfDestructionAnim();
        }
        else
        {
            KakutoDebug.LogError("ProjectileComponent has not been found.");
        }
    }
}
