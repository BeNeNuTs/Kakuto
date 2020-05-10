using UnityEngine;

public class PlayerParryAttackAnimationStateMachineBehavior : StateMachineBehaviour
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

        //If the next clip to play is not a parry animation, then this is the end of the parry
        string lowerClipName = clipInfoList[0].clip.name.ToLower();
        if (!lowerClipName.Contains(PlayerAnimationHelper.K_PARRY_SUCCESS_ANIM_STANDARD_NAME))
        {
            Utils.GetPlayerEventManager<EAnimationAttackName>(animator.gameObject).TriggerEvent(EPlayerEvent.EndOfAttack, EAnimationAttackName.Parry);
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
