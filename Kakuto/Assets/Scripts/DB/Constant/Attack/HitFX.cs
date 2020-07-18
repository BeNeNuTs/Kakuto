using System;
using UnityEngine;

public enum EHitFXType
{
    Block,
    Parry,
    Counter,
    GuardCrush,
    HeavyHit,
    LightHit,
    SpecialHit

    // Please update HitFX.COUNT
}

[Serializable]
public class HitFX
{
    public static int COUNT = 7;

#if UNITY_EDITOR
    [SerializeField][ReadOnly] private string m_Name;
#endif
    public GameObject m_FX;

    public HitFX(EHitFXType type)
    {
#if UNITY_EDITOR
        m_Name = type.ToString();
#endif
    }
}

