using System;
using UnityEngine;

public enum EAnimSFXType
{
    Knockdown,

    Parry_Whiff,
    Parry_Success,

    Jump,
    Land,
    Backdash,
    Forwarddash,
    Stun,
    TP,
    EX,
    Launch,
    Trigger,
    Super

    // Please update AnimSFX.COUNT
}

[Serializable]
public class AnimSFX
{
    public static int COUNT = 13;

#if UNITY_EDITOR
    [SerializeField][ReadOnly] private string m_Name;
#endif
    public AudioClip m_SFX;

    public AnimSFX(EAnimSFXType type)
    {
#if UNITY_EDITOR
        m_Name = type.ToString();
#endif
    }
}

