using UnityEngine;
using System.Collections;
using UnityEditor;

public class HealthBarConfig : ScriptableObject
{
    static HealthBarConfig s_Instance = null;
    public static HealthBarConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<HealthBarConfig>("UI/HealthBarConfig");
            return s_Instance;
        }
    }

    [Tooltip("Time to fill the health bar")]
    public float m_TimeToFill = 0.2f;

    [Tooltip("Time between HealthBar and HealthBarBackground")]
    public float m_TimeBetweenHealthBar = 0.5f;
}