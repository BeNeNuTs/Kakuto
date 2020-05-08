using UnityEngine;
using Cinemachine;

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

    [Tooltip("To define an offset for the out of bounds")]
    [Min(0)]
    public float m_BoundsOffset = 0.1f;

    [Tooltip("To define the distance to be considered in a corner. If a player is considered in a corner, the pushback can be applied to the attacker")]
    [Min(0)]
    public float m_MaxDistanceToBeConsideredInACorner = 0.1f;

    [Separator("Gamepad")]

    [Tooltip("If a gamepad is not recognized, define the default gamepad type to use")]
    public EGamePadType m_DefaultGamepadType = EGamePadType.PS4;

    [Separator("Camera")]

    [CinemachineImpulseChannelProperty]
    [Tooltip("Impulse events generated here will appear on the channels included in the mask.")]
    public int m_ImpulseChannel;

    [CinemachineEmbeddedAssetProperty(true)]
    [Header("Signal Shape")]
    [Tooltip("Defines the signal that will be generated.")]
    public SignalSourceAsset m_ImpulseRawSignal;
}