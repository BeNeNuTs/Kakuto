using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerGuardCrushTriggerAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerGuardCrushTriggerAttackConfig", order = 0)]
public class PlayerGuardCrushTriggerAttackConfig : PlayerBaseAttackConfig
{
    public enum EGuardCrushType
    {
        NextNonSuperProjectile
    }

    public EGuardCrushType m_GuardCrushType;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerGuardCrushTriggerAttackLogic(this);
    }
}
