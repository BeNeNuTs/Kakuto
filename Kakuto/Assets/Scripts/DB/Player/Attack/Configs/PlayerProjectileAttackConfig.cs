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
    [Tooltip("The speed of the projectile after hit")]
    public float m_ProjectileConstantSpeedAfterHit;
    public bool m_ApplyConstantSpeedOnPlayerHit = true;
    public bool m_ApplyConstantSpeedOnProjectileHit = true;

    [Tooltip("How many frames after hit to keep constant speed. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_FramesToKeepProjectileAtConstantSpeed = 3;
    [HideInInspector]
    public int FramesToKeepProjectileAtConstantSpeed
    {
        get
        {
            return (int)(m_FramesToKeepProjectileAtConstantSpeed * (GameConfig.Instance.m_GameFPS / GameConfig.K_ANIMATION_FPS));
        }
    }

    [Separator("Guard crush")]
    public bool m_UseSpecificGuardCrushAnim = false;
    [ConditionalField(true, "m_UseSpecificGuardCrushAnim"), Tooltip("Animation to play if this projectile is guard crush.")]
    public EAnimationAttackName m_AnimationGuardCrushAttackName;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerProjectileAttackLogic(this);
    }
}
