using System;
using UnityEngine;

public enum EAttackSFXType
{
    Whiff_LP,
    Whiff_LK,
    Whiff_HP,
    Whiff_HK,

    Hit_Light,
    Hit_Heavy,
    Hit_Throw,
    Hit_Special,

    Blocked_Hit

    // Please update AttackSFX.COUNT
}

[Serializable]
public class AttackSFX
{
    public static int COUNT = 9;

#if UNITY_EDITOR
    [SerializeField][ReadOnly] private string m_Name;
#endif
    public AudioClip[] m_SFXList;

    public AttackSFX(EAttackSFXType type)
    {
#if UNITY_EDITOR
        m_Name = type.ToString();
#endif
    }
}

