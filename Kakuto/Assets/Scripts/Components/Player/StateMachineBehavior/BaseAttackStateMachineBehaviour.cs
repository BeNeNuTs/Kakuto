using UnityEngine;

public abstract class BaseAttackStateMachineBehaviour : StateMachineBehaviour
{
    PlayerAnimationEventHandler m_PlayerAnimationEventHandler;

    private void InitIfNeeded(GameObject owner)
    {
        if (m_PlayerAnimationEventHandler == null)
        {
            m_PlayerAnimationEventHandler = owner.GetComponent<PlayerAnimationEventHandler>();
#if UNITY_EDITOR
            if (m_PlayerAnimationEventHandler == null)
                Debug.LogError("Can't find PlayerAnimationEventHandler on " + owner);
#endif
        }
    }

    // Use this for initialization
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitIfNeeded(animator.gameObject);
        m_PlayerAnimationEventHandler.UpdateCurrentAttack(this);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_PlayerAnimationEventHandler.UpdateCurrentAttack(null);
    }

    public abstract EAnimationAttackName GetAnimationAttackName();
}
