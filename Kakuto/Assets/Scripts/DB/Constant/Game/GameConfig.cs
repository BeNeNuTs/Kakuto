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

    public static uint K_ANIMATION_FPS = 30;

    [Separator("FPS")]
    [Range(10, 120)]
    public int m_GameFPS = 30;

    [Separator("Round")]

    [Tooltip("The duration of one round"), Range(0,99)]
    public float m_RoundDuration = 60f;

    [Tooltip("Time to wait in seconds before restarting round after a player death")]
    public float m_TimeToWaitBetweenRounds = 5f;

    [Tooltip("The number of rounds a player should win to win the game"), Min(1)]
    public uint m_MaxRoundsToWin = 3;

    [Separator("Out of Bounds")]

    [Tooltip("To define an offset for the out of bounds (in pixels)")]
    public uint m_BoundsOffset = 50;
}