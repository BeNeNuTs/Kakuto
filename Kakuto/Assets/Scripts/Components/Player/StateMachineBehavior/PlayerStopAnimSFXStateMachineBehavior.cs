using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStopAnimSFXStateMachineBehavior : StateMachineBehaviour
{
    public EAnimSFXType m_AnimSFXToStop;

    private int m_PlayerIndex = -1;
    private AudioSubGameManager m_AudioManager = null;
    private bool m_Initialized = false;

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

    void InitIfNeeded(GameObject owner)
    {
        if(!m_Initialized)
        {
            m_PlayerIndex = owner.GetComponentInParent<PlayerInfoComponent>().GetPlayerIndex();
            m_AudioManager = GameManager.Instance.GetSubManager<AudioSubGameManager>(ESubManager.Audio);
            m_Initialized = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitIfNeeded(animator.gameObject);
        m_AudioManager.StopAnimSFX(m_PlayerIndex, m_AnimSFXToStop);
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
