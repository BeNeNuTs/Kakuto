using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerBaseAttackConfig", order = 0)]
public class PlayerBaseAttackConfig : ScriptableObject
{
    public virtual PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerBaseAttackLogic();
    }
}
