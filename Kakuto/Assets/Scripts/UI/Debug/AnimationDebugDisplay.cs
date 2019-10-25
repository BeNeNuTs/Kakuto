using UnityEngine;

public class AnimationDebugDisplay : MonoBehaviour
{
    public bool m_DisplayAnimationDebug = false;

    private Animator m_Player1Animator;
    private Animator m_Player2Animator;

    private string m_Player1AnimationInfo;
    private string m_Player2AnimationInfo;

    void Update()
    {
        if(m_Player1Animator == null)
        {
            m_Player1Animator = GameManager.Instance.GetPlayerComponent<Animator>(EPlayer.Player1);
        }
        
        if(m_Player2Animator == null)
        {
            m_Player2Animator = GameManager.Instance.GetPlayerComponent<Animator>(EPlayer.Player2);
        }

        ComputeAnimationInfo();
    }

    void ComputeAnimationInfo()
    {
        ComputeAnimationInfo(m_Player1Animator, out m_Player1AnimationInfo);
        ComputeAnimationInfo(m_Player2Animator, out m_Player2AnimationInfo);
    }

    void ComputeAnimationInfo(Animator animator, out string animationInfo)
    {
        animationInfo = "";
        if (animator != null)
        {
            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (clips.Length > 0)
            {
                float clipLength = clips[0].clip.length;
                float clipFrameRate = clips[0].clip.frameRate;
                float clipFrameCount = clipLength * clipFrameRate;
                animationInfo = clips[0].clip.name + " (" + Mathf.Floor(clipFrameCount * stateInfo.normalizedTime) % clipFrameCount + " / " + clipFrameCount + ")";
            }
        }
    }

    void DisplayAnimationInfo(string text, TextAnchor anchor)
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle
        {
            alignment = anchor,
            fontSize = h * 2 / 100
        };
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        GUI.Label(new Rect(0, h / 2.0f, w, style.fontSize), text, style);
    }

    void OnGUI()
    {
        if (m_DisplayAnimationDebug)
        {
            int w = Screen.width, h = Screen.height;

            // Display Animation Info ///////////////
            {
                DisplayAnimationInfo(m_Player1AnimationInfo, TextAnchor.MiddleLeft);
                DisplayAnimationInfo(m_Player2AnimationInfo, TextAnchor.MiddleRight);
            }
            //////////////////////////////
        }
    }
}