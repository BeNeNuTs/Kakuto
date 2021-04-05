using System;
using System.Collections.Generic;
using UnityEngine;

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

    [Separator("Inputs")]

    [Tooltip("How many frames by default each input will be kept before being deleted. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_DefaultInputFramesPersistency = 9;

    [HideInInspector]
    public int DefaultInputFramesPersistency
    {
        get
        {
            return (int)(m_DefaultInputFramesPersistency * (GameConfig.Instance.m_GameFPS / GameConfig.K_ANIMATION_FPS));
        }
    }

    [Tooltip("How many frames we're going to add to each previous inputs when a new one is triggered. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_InputFramesPersistencyBonus = 3;

    [HideInInspector]
    public int InputFramesPersistencyBonus
    {
        get
        {
            return (int)(m_InputFramesPersistencyBonus * (GameConfig.Instance.m_GameFPS / GameConfig.K_ANIMATION_FPS));
        }
    }

    [Tooltip("How much time max an input can be kept before being deleted. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_MaxInputFramesPersistency = 24;

    [HideInInspector]
    public int MaxInputFramesPersistency
    {
        get
        {
            return (int)(m_MaxInputFramesPersistency * (GameConfig.Instance.m_GameFPS / GameConfig.K_ANIMATION_FPS));
        }
    }

    [Tooltip("How many input can be stacked before being deleted")]
    public uint m_MaxInputs = 10;

    [Separator("Attack Evaluation")]

    [Tooltip("Each time an input is triggered, we're going to wait x frames before evaluating the sequence to find out if there is an attack matching with it. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_FramesToWaitBeforeEvaluatingAttacks = 5;

    [HideInInspector]
    public int FramesToWaitBeforeEvaluatingAttacks
    {
        get
        {
            return (int)(m_FramesToWaitBeforeEvaluatingAttacks * (GameConfig.Instance.m_GameFPS / GameConfig.K_ANIMATION_FPS));
        }
    }

    [Tooltip("Max frames to wait before evaluating attacks, even if we're still trying to wait due to FramesToWaitBeforeEvaluatingAttacks (in case of button mashing, we have to try to trigger an attack anyway each x frames). /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_MaxFramesToWaitBeforeEvaluatingAttacks = 24;

    [HideInInspector]
    public int MaxFramesToWaitBeforeEvaluatingAttacks
    {
        get
        {
            return (int)(m_MaxFramesToWaitBeforeEvaluatingAttacks * (GameConfig.Instance.m_GameFPS / GameConfig.K_ANIMATION_FPS));
        }
    }

    [Separator("Super Gauge")]

    [Tooltip("The max amount of the super gauge")]
    public float m_SuperGaugeMaxValue = 100f;

    [Tooltip("The amount that have to be added to the super gauge of the defender if he blocks the attack or takes the hit")]
    public float m_DefenderSuperGaugeBonus = 0f;

    [Separator("Stun Gauge")]

    [Tooltip("The max amount of the stun gauge")]
    public float m_StunGaugeMaxValue = 100f;

    [Tooltip("How much time the stun gauge should be frozen after hitstun/blockstun before starting decreasing")]
    public float m_StunGaugeDecreaseCooldown = 2f;

    [Tooltip("How much amount the stun gauge should decrease every seconds after the cooldown (amount per sec)")]
    public float m_StunGaugeDecreaseSpeed = 2f;

    [Separator("Hit")]

    [Tooltip("X = Hit Counter [0;+infinity] | Y = Damage ratio [0;1]")]
    public AnimationCurve m_DamageScaling;

    [Tooltip("The speed of shake of the opponent while taking a hit when time is frozen")]
    public float m_HitStopShakeSpeed = 75.0f;
    [Tooltip("The amount of shake of the opponent while taking a hit when time is frozen")]
    public float m_HitStopShakeAmount = 0.01f;

    public List<HitFX> m_HitFX;
    public List<FX> m_OtherFX;

    public TimeScaleParams m_OnDeathTimeScaleParams;
    public CameraShakeParams m_OnDeathCamShakeParams;
    public float m_OnDeathPushbackMultiplier = 1.5f;

    [Separator("SFX")]
    public List<AttackSFX> m_AttackSFX;
    public List<AnimSFX> m_AnimSFX;
    public ProjectileSFX m_ProjectileSFX;

    void OnValidate()
    {
        // HIT_FX ///////////////////
        if(m_HitFX == null)
        {
            m_HitFX = new List<HitFX>();
        }

        while(m_HitFX.Count > HitFX.COUNT)
        {
            m_HitFX.RemoveAt(m_HitFX.Count - 1);
        }

        while (m_HitFX.Count < HitFX.COUNT)
        {
            m_HitFX.Add(new HitFX((EHitFXType)m_HitFX.Count));
        }

        // OTHER_FX ///////////////////
        if (m_OtherFX == null)
        {
            m_OtherFX = new List<FX>();
        }

        while (m_OtherFX.Count > FX.COUNT)
        {
            m_OtherFX.RemoveAt(m_OtherFX.Count - 1);
        }

        while (m_OtherFX.Count < FX.COUNT)
        {
            m_OtherFX.Add(new FX((EFXType)m_OtherFX.Count));
        }

        // ATTACK_SFX ///////////////////
        if (m_AttackSFX == null)
        {
            m_AttackSFX = new List<AttackSFX>();
        }

        while (m_AttackSFX.Count > AttackSFX.COUNT)
        {
            m_AttackSFX.RemoveAt(m_AttackSFX.Count - 1);
        }

        while (m_AttackSFX.Count < AttackSFX.COUNT)
        {
            m_AttackSFX.Add(new AttackSFX((EAttackSFXType)m_AttackSFX.Count));
        }

        // ANIM_SFX ///////////////////
        if (m_AnimSFX == null)
        {
            m_AnimSFX = new List<AnimSFX>();
        }

        while (m_AnimSFX.Count > AnimSFX.COUNT)
        {
            m_AnimSFX.RemoveAt(m_AnimSFX.Count - 1);
        }

        if(m_AnimSFX.Count < AnimSFX.COUNT)
        {
            m_AnimSFX.Clear();
            foreach (EAnimSFXType animSFXType in Enum.GetValues(typeof(EAnimSFXType)))
            {
                m_AnimSFX.Add(new AnimSFX(animSFXType));
            }
        }

        for(int i = 0; i < m_AnimSFX.Count; i++)
        {
            if(i == (int)EAnimSFXType.Parry_Whiff_DEPRECATED
                || i == (int)EAnimSFXType.Parry_Success_DEPRECATED)
            {
                m_AnimSFX[i].m_SFX.m_Clip = null;
            }
        }
    }
}