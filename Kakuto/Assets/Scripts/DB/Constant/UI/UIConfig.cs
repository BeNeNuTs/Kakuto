using UnityEngine;

[System.Serializable]
public class InputUIInfo
{
    public EInputKey m_Input;
    public Sprite m_XboxSprite;
    public Sprite m_PS4Sprite;
    public Sprite m_GameSprite;
}

public class UIConfig : ScriptableObject
{
    static UIConfig s_Instance = null;
    public static UIConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<UIConfig>("UI/UIConfig");
            return s_Instance;
        }
    }

    [Separator("Health")]
    [Tooltip("Time to fill the health bar")]
    public float m_TimeToFillHealthBar = 0.2f;

    [Tooltip("Time between HealthBar and HealthBarBackground")]
    public float m_TimeBetweenHealthBar = 0.5f;

    [Separator("Combo")]
    [Tooltip("After how much time the hit combo disppear when the combo chain has been broken")]
    public float m_TimeToDisappearAfterComboBreak = 2.0f;

    [Separator("Hit notifications")]
    [Tooltip("After how much time the hit notification is displayed")]
    public float m_MaxHitNotificationDisplayTime = 2.0f;

    [Separator("Inputs")]
    [SerializeField] private float m_DpadNavigationInputPerSecond = 10f;
    public float DpadNavigationInputPerSecond => 1f / m_DpadNavigationInputPerSecond;
    public float m_DpadNavigationInputRepeatDelay = 0.5f;
    public InputUIInfo[] m_InputUIInfos;

    [Separator("SFX")]
    public UISFX m_UISFX;

    public InputUIInfo GetAssociatedInputUIInfo(EInputKey inputKey)
    {
        for (int i = 0; i < m_InputUIInfos.Length; i++)
        {
            if (m_InputUIInfos[i].m_Input == inputKey)
            {
                return m_InputUIInfos[i];
            }
        }
        return null;
    }
}