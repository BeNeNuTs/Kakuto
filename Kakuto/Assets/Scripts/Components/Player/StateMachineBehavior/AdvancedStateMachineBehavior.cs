using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedStateMachineBehaviour : StateMachineBehaviour
{
    protected AnimatorStateInfo m_StateInfo;

    public AnimatorStateInfo StateInfo
    {
        get { return m_StateInfo; }
    }

    // Use this for initialization
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_StateInfo = stateInfo;
    }
}
