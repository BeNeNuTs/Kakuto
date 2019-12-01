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
    [Tooltip("The speed of the projectile over time")]
    public AnimationCurve m_ProjectileSpeedOverTime;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerProjectileAttackLogic(this);
    }
}
