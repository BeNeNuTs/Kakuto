using UnityEngine;

struct AnimStunInfo
{
    public float m_StunDuration;
    public bool m_IsHitKO;
    public EStunType m_StunType;

    public AnimStunInfo(float stunDuration, bool isHitKO, EStunType stunType)
    {
        m_StunDuration = stunDuration;
        m_IsHitKO = isHitKO;
        m_StunType = stunType;
    }
}

public class PlayerStunAnimationStateMachineBehavior : StateMachineBehaviour
{
    private static readonly float K_DEFAULT_STUN_DURATION = 5.0f;

    [SerializeField] bool m_IsHitKO = false;
    [SerializeField] bool m_IsStartingStun = true;
    [SerializeField] bool m_IsStoppingStun = true;
    [SerializeField] EStunType m_StunType = EStunType.None;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(m_IsStartingStun)
        {
            AnimStunInfo animStunInfo = new AnimStunInfo(K_DEFAULT_STUN_DURATION, m_IsHitKO, m_StunType);
            Utils.GetPlayerEventManager<AnimStunInfo>(animator.gameObject).TriggerEvent(EPlayerEvent.StartAnimStun, animStunInfo);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(m_IsStoppingStun)
        {
            //If we were not using animation length, we need to stop stun on state exit
            if (stateInfo.normalizedTime >= 1.0f)
            {
                Utils.GetPlayerEventManager<bool>(animator.gameObject).TriggerEvent(EPlayerEvent.StopAnimStun, true);
            }
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
