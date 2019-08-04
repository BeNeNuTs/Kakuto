using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProjectileAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerProjectileAttackConfig", order = 2)]
public class PlayerProjectileAttackConfig : PlayerNormalAttackConfig
{
    [Header("Projectile")]
    [Tooltip("The projectile prefab to emit")]
    public GameObject m_ProjectilePrefab;
    [Tooltip("The angle to emit the projectile")]
    public float m_ProjectileAngle = 0.0f;
    [Tooltip("The speed of the projectile"), Range(0.1f, 10f)]
    public float m_ProjectileSpeed = 10.0f;
    [Tooltip("The number of hit count before destroying projectile"), Range(1, 100)]
    public uint m_ProjectileHitCount = 1;
    [Tooltip("How much time to wait to apply another hit")]
    public float m_ProjectileDelayBetweenHits = 0.1f;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerProjectileAttackLogic(this);
    }
}
