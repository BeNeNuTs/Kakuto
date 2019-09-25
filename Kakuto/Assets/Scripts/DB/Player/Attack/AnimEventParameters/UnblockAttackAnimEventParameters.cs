using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnblockAttackAnimEventParameters", menuName = "Data/Player/Attacks/AnimEventParameters/UnblockAttackAnimEventParameters", order = 0)]
public class UnblockAttackAnimEventParameters : ScriptableObject
{
    [SearchableEnum]
    public EAnimationAttackName m_UnblockAttackName;
    [SearchableEnum]
    public List<EAnimationAttackName> m_AllowedAttacks;
}