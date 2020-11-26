using UnityEngine;

public abstract class BaseAttackStateMachineBehaviour : StateMachineBehaviour
{
    public static BaseAttackStateMachineBehaviour m_CurrentAttack;

    // Use this for initialization
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_CurrentAttack = this;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_CurrentAttack = null;
    }

    public abstract EAnimationAttackName GetAnimationAttackName();
}
