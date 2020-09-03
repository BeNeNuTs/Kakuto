using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class UnblockAllowedAttack
{
    [SearchableEnum]
    public EAnimationAttackName m_Attack;
    public bool m_OnlyOnHit;
}

[CreateAssetMenu(fileName = "UnblockAttackAnimEventConfig", menuName = "Data/Player/Attacks/AnimEventParameters/UnblockAttackAnimEventConfig", order = 0)]
public class UnblockAttackAnimEventConfig : ScriptableObject
{
    [Tooltip("Allowed attacks to cancel the current attack")]
    public List<UnblockAllowedAttack> m_UnblockAllowedAttacks;
}