using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerGrabAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerGrabAttackConfig", order = 1)]
public class PlayerGrabAttackConfig : PlayerBaseAttackConfig
{
    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerGrabAttackLogic(this);
    }
}
