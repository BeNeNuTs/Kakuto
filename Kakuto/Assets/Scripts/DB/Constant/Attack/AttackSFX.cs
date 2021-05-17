using System;
using UnityEngine;

public enum EWhiffSFXType
{
    None,
    Whiff_LP,
    Whiff_LK,
    Whiff_HP,
    Whiff_HK,
    Whiff_Parry
}

public enum EAttackSFXType
{
    Whiff_LP,
    Whiff_LK,
    Whiff_HP,
    Whiff_HK,
    Whiff_Parry,

    Hit_Light,
    Hit_Heavy,
    Hit_Throw,
    Hit_Special,
    Hit_KO,

    Blocked_Hit,
    Parry_Hit,
    GuardCrush_Hit,
    Counter_Hit,
    Final_Hit

    // Please update AttackSFX.COUNT
}

[Serializable]
public class AttackSFX
{
    public static int COUNT = 15;

#if UNITY_EDITOR
    [SerializeField][ReadOnly] private string m_Name;
#endif
    public AudioEntry[] m_SFXList;

    public AttackSFX(EAttackSFXType type)
    {
#if UNITY_EDITOR
        m_Name = type.ToString();
#endif
    }
}

[Serializable]
public class HeavyHitGruntSFX
{
    [Range(0, 1)]
    public float m_GruntProbability = 0.5f;
    public AudioEntry[] m_GruntSFX;
}

