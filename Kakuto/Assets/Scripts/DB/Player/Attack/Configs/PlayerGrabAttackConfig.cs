using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EGrabType
{
    FrontThrow,
    BackThrow
}

[CreateAssetMenu(fileName = "PlayerGrabAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerGrabAttackConfig", order = 1)]
public class PlayerGrabAttackConfig : PlayerBaseAttackConfig
{
    public EGrabType m_GrabType;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerGrabAttackLogic(this);
    }
}
