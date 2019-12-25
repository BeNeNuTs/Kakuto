using UnityEngine;
using System.Collections;
using UnityEditor;

public class AttackConfig : ScriptableObject
{
    static AttackConfig s_Instance = null;
    public static AttackConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<AttackConfig>("Attack/AttackConfig");
            return s_Instance;
        }
    }

    [Header("Inputs")]

    [Tooltip("How much time by default each input will be kept before being deleted")]
    public float m_DefaultInputPersistency = 0.3f;

    [Tooltip("How much time we're going to add to each previous inputs when a new one is triggered")]
    public float m_InputPersistencyBonus = 0.1f;

    [Tooltip("How much time max an input can be kept before being deleted")]
    public float m_MaxInputPersistency = 0.8f;

    [Tooltip("How many input can be stacked before being deleted")]
    public uint m_MaxInputs = 10;

    [Header("Attack Evaluation")]

    [Tooltip("Each time an input is triggered, we're going to wait x frames before evaluating the sequence to find out if there is an attack matching with it. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_FramesToWaitBeforeEvaluatingAttacks = 5;

    [HideInInspector]
    public float TimeToWaitBeforeEvaluatingAttacks
    {
        get
        {
            return (float)m_FramesToWaitBeforeEvaluatingAttacks / (float)GameConfig.K_ANIMATION_FPS;
        }
    }

    [Tooltip("Max frames to wait before evaluating attacks, even if we're still trying to wait due to FramesToWaitBeforeEvaluatingAttacks (in case of button mashing, we have to try to trigger an attack anyway each x frames). /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_MaxFramesToWaitBeforeEvaluatingAttacks = 24;

    [HideInInspector]
    public float MaxTimeToWaitBeforeEvaluatingAttacks
    {
        get
        {
            return (float)m_MaxFramesToWaitBeforeEvaluatingAttacks / (float)GameConfig.K_ANIMATION_FPS;
        }
    }

    [Header("Super Gauge")]

    [Tooltip("The max amount of the super gauge")]
    public float m_SuperGaugeMaxValue = 100f;

    [Tooltip("The amount that have to be added to the super gauge of the defender if he blocks the attack or takes the hit")]
    public float m_DefenderSuperGaugeBonus = 0f;

    [Header("Damage Scaling")]

    [Tooltip("X = Hit Counter [0;+infinity] | Y = Damage ratio [0;1]")]
    public AnimationCurve m_DamageScaling;
}