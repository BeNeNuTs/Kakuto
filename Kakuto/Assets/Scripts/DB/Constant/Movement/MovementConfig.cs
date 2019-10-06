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

    [Tooltip("A mask determining what is ground for characters")]
    public LayerMask m_GroundLayerMask;

    [Tooltip("Radius of the overlap circle to determine if the character is jumping or not")]
    public float m_OverlapCircleRadius = 0.05f;

#if UNITY_EDITOR
    [Tooltip("Debug : Display the overlap circle in the scene")]
    public bool d_DisplayOverlapCircle = false;
#endif

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