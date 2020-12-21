using System;
using UnityEngine;

public enum EFXType
{
    ThrowTech,
    Dash,
    Jump,
    Landing,
    Teleport

    // Please update FX.COUNT
}

[Serializable]
public class FX
{
    public static int COUNT = 5;

#if UNITY_EDITOR
    [SerializeField][ReadOnly] private string m_Name;
#endif
    public GameObject m_FX;

    public FX(EFXType type)
    {
#if UNITY_EDITOR
        m_Name = type.ToString();
#endif
    }
}

