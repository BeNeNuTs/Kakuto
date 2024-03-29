﻿using UnityEngine;
using Cinemachine;
using UnityEngine.Audio;

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

    [Tooltip("Time to wait before playing round UI animations"), Min(0.5f)]
    public float m_StartRoundUIDelay = 1.5f;

    [Tooltip("Time to wait in seconds while playing round entry animation"), Min(2)]
    public float m_RoundEntryTime = 5f;

    [Tooltip("The duration of one round"), Range(0,99)]
    public float m_RoundDuration = 60f;

    [Tooltip("Normalized time to wait while playing end round UI animation before playing won and lost round animation when round over"), Range(0f, 1f)]
    public float m_NormalizedTimeToWaitBeforeEndRoundAnimations = 0.8f;

    [Tooltip("Time to wait in seconds after won and lost round animations are finished in order to restart the round")]
    public float m_TimeToWaitAfterEndRoundAnimations = 2f;

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

    [Separator("Loading screen")]
    public GameObject m_LoadingScreenPrefab;

    [Separator("Audio")]
    public AudioMixer m_MainMixer;
    public AudioMixerGroup m_MusicMixerGroup;
    public AudioMixerGroup m_SFXMixerGroup;
    public AudioMixerGroup m_VoiceMixerGroup;

    public AudioMixerSnapshot m_DefaultSnapshot;
    public AudioMixerSnapshot m_DuckMusicSnapshot;
}