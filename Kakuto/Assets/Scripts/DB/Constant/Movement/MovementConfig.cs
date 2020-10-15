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

    [Range(0, .3f)]
    [Tooltip("How much to smooth out the falling trajectory when falling on another player")]
    public float m_FallingTrajectorySmoothing = .02f;

#if UNITY_EDITOR
    [Separator("Debug")]

    [Tooltip("Debug : Display the overlap circle in the scene")]
    public bool m_DEBUG_DisplayOverlapCircle = false;

    [Tooltip("Debug : Display the rigidbody velocity")]
    public bool m_DEBUG_DisplayVelocity = false;

    [Tooltip("Debug : Display the falling raycast to adjust trajectory")]
    public bool m_DEBUG_DisplayFallingRaycast = false;
#endif
}