using System;
using UnityEngine;

public enum EProjectileSFXType
{
    Movement,
    Impact,
    Destroy
}

[Serializable]
public class ProjectileSFX
{
    [Header("Movement")]
    public AudioEntry m_NormalMovementSFX;
    public AudioEntry m_GuardCrushMovementSFX;
    public AudioEntry m_SuperMovementSFX;

    [Header("Impact")]
    public AudioEntry m_NormalImpactSFX;
    public AudioEntry m_GuardCrushImpactSFX;
    public AudioEntry m_SuperImpactSFX;

    [Header("Destroy")]
    public AudioEntry m_NormalDestroySFX;
    public AudioEntry m_GuardCrushDestroySFX;
    public AudioEntry m_SuperDestroySFX;

    public AudioEntry GetSFX(EProjectileType projectileType, EProjectileSFXType projectileSFXType)
    {
        switch (projectileSFXType)
        {
            case EProjectileSFXType.Movement:
                return GetMovementSFX(projectileType);
            case EProjectileSFXType.Impact:
                return GetImpactSFX(projectileType);
            case EProjectileSFXType.Destroy:
                return GetDestroySFX(projectileType);
            default:
                return null;
        }
    }

    public AudioEntry GetMovementSFX(EProjectileType projectileType)
    {
        switch (projectileType)
        {
            case EProjectileType.Normal:
                return m_NormalMovementSFX;
            case EProjectileType.GuardCrush:
                return m_GuardCrushMovementSFX;
            case EProjectileType.Super:
                return m_SuperMovementSFX;
            default:
                return null;
        }
    }

    public AudioEntry GetImpactSFX(EProjectileType projectileType)
    {
        switch (projectileType)
        {
            case EProjectileType.Normal:
                return m_NormalImpactSFX;
            case EProjectileType.GuardCrush:
                return m_GuardCrushImpactSFX;
            case EProjectileType.Super:
                return m_SuperImpactSFX;
            default:
                return null;
        }
    }

    public AudioEntry GetDestroySFX(EProjectileType projectileType)
    {
        switch (projectileType)
        {
            case EProjectileType.Normal:
                return m_NormalDestroySFX;
            case EProjectileType.GuardCrush:
                return m_GuardCrushDestroySFX;
            case EProjectileType.Super:
                return m_SuperDestroySFX;
            default:
                return null;
        }
    }
}

