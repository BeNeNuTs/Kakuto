using UnityEngine;
using System.Collections;
using UnityEditor;

public class GameConfig : ScriptableObject
{
    static GameConfig s_Instance = null;
    public static GameConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<GameConfig>("Game/GameConfig");
            return s_Instance;
        }
    }

    [Header("Round")]

    [Tooltip("Time to wait in seconds before restarting round after a player death")]
    public float m_TimeToWaitBetweenRounds = 5f;
}