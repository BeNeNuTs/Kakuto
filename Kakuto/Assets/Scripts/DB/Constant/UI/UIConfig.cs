﻿using UnityEngine;

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
    public InputUIInfo[] m_InputUIInfos;
}