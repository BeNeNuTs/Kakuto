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

    [Tooltip("Each time a jump input is triggered, we're going to wait x frames before jumping to evaluate in which direction we should jump.")]
    public uint m_FramesToWaitBeforeJumping = 5;
}