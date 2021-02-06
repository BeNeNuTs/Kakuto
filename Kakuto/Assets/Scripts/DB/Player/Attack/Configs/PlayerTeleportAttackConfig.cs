using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerTeleportAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerTeleportAttackConfig", order = 0)]
public class PlayerTeleportAttackConfig : PlayerBaseAttackConfig
{
    public enum ETeleportCondition
    {
        LastNonSuperProjectile
    }

    public ETeleportCondition m_TeleportCondition;
    public Vector3 m_TeleportOffset;

    [Tooltip("This velocity will be applied on player only if teleport is triggered on an air projectile.")]
    public Vector2 m_FinalTeleportAirVelocity;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerTeleportAttackLogic(this);
    }
}
