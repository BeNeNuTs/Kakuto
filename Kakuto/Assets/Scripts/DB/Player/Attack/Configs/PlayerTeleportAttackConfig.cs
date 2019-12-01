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

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerTeleportAttackLogic(this);
    }
}
