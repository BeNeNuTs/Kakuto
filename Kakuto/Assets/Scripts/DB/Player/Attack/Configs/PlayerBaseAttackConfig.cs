using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseAttackConfig : ScriptableObject
{
    public abstract PlayerBaseAttackLogic CreateLogic();
}
