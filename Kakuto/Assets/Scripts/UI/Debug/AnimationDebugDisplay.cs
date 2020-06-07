using UnityEngine;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
public class AnimationDebugDisplay : MonoBehaviour
{
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
            Utils.GetCurrentAnimInfo(animator, out string clipName, out float currentFrame, out float frameCount);
            animationInfo = clipName + " (" + Mathf.Floor(currentFrame) + " / " + frameCount + ")";
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
        if (ScenesConfig.GetDebugSettings().m_DisplayAnimationInfo)
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
#endif