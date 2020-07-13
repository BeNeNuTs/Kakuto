using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProjectileAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerProjectileAttackConfig", order = 2)]
public class PlayerProjectileAttackConfig : PlayerNormalAttackConfig
{
    [Separator("Projectile")]
    [Tooltip("The projectile prefab to emit")]
    public GameObject m_ProjectilePrefab;
    [Tooltip("The angle to emit the projectile")]
    public float m_ProjectileAngle = 0.0f;
    [Tooltip("The speed of the projectile over time")]
    public AnimationCurve m_ProjectileSpeedOverTime;

    [Separator("Guard crush")]
    public bool m_UseSpecificGuardCrushAnim = false;
    [SearchableEnum, ConditionalField(true, "m_UseSpecificGuardCrushAnim"), Tooltip("Animation to play if this projectile is guard crush.")]
    public EAnimationAttackName m_AnimationGuardCrushAttackName;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerProjectileAttackLogic(this);
    }
}
