using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerNormalAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerNormalAttackConfig", order = 0)]
public class PlayerNormalAttackConfig : PlayerBaseAttackConfig
{
    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerNormalAttackLogic(this);
    }
}
