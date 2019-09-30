using UnityEngine;
using System.Collections;
using UnityEditor;

public class MovementConfig : ScriptableObject
{
    static MovementConfig s_Instance = null;
    public static MovementConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<MovementConfig>("Movement/MovementConfig");
            return s_Instance;
        }
    }

    [Tooltip("Each time a jump input is triggered, we're going to wait x frames before jumping to evaluate in which direction we should jump. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_FramesToWaitBeforeJumping = 5;

    [HideInInspector]
    public float TimeToWaitBeforeJumping
    {
        get
        {
            return (float)m_FramesToWaitBeforeJumping / (float)GameConfig.K_ANIMATION_FPS;
        }
    }
}