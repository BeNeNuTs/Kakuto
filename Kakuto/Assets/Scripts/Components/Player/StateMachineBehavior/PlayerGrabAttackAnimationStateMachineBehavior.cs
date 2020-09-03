using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabAttackAnimationStateMachineBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AnimatorClipInfo[] clipInfoList = animator.GetNextAnimatorClipInfo(0);
        if(clipInfoList.Length != 1)
        {
            Debug.LogError("clipInfoList should have one element");
            Debug.Break();
            return;
        }

        //If the next clip to play is not a throw animation, then this is the end of the grab
        string lowerClipName = clipInfoList[0].clip.name.ToLower();
        if (!lowerClipName.Contains(PlayerAnimationHelper.K_GRAB_MISS_ANIM_STANDARD_NAME) && !lowerClipName.Contains(PlayerAnimationHelper.K_GRAB_CANCEL_ANIM_STANDARD_NAME))
        {
            Utils.GetPlayerEventManager(animator.gameObject).TriggerEvent(EPlayerEvent.EndOfAttack, new EndOfAttackEventParameters(EAnimationAttackName.Grab));
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
