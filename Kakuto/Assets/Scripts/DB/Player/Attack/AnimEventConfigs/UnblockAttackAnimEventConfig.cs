using UnityEngine;
using System.Collections.Generic;

public class UnblockAttackAnimEvent
{
    public UnblockAttackAnimEvent(EAnimationAttackName attackToUnblock, UnblockAttackAnimEventConfig config)
    {
        m_AttackToUnblock = attackToUnblock;
        m_Config = config;
    }

    public EAnimationAttackName m_AttackToUnblock;
    public UnblockAttackAnimEventConfig m_Config;
}

[CreateAssetMenu(fileName = "UnblockAttackAnimEventConfig", menuName = "Data/Player/Attacks/AnimEventParameters/UnblockAttackAnimEventConfig", order = 0)]
public class UnblockAttackAnimEventConfig : ScriptableObject
{
    [SearchableEnum, Tooltip("Allowed attacks to cancel the current attack")]
    public List<EAnimationAttackName> m_AllowedAttacks;
}