using System;
using UnityEngine;

[Serializable]
public class AudioEntry
{
    public AudioClip m_Clip;
    [Range(0, 1)]
    public float m_Volume = 1f;
}

public enum EAnimSFXType
{
    Knockdown,

    Parry_Whiff_DEPRECATED,
    Parry_Success_DEPRECATED,

    Jump,
    Land,
    Backdash,
    Forwarddash,
    Stun,
    TP,
    EX,
    Launch,
    Trigger,
    Super,
    Launch_Crush,
    Papers,
    Blast,
    MalikEntry_SuitcaseImpact,
    MalikEntry_SuitcaseSlide,
    MalikEntry_Smoke,

    // Please update AnimSFX.COUNT
}

[Serializable]
public class AnimSFX
{
    public static int COUNT = 19;

#if UNITY_EDITOR
    [SerializeField][ReadOnly] private string m_Name;
#endif
    public AudioEntry m_SFX;

    public AnimSFX(EAnimSFXType type)
    {
#if UNITY_EDITOR
        m_Name = type.ToString();
#endif
    }
}

