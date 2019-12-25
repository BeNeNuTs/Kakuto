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
    [Separator("Settings")]
    public EGrabType m_GrabType;

    [Separator("Enemy effect")]
    [Range(0, 100)]
    public uint m_Damage = 10;

    [Space]

    [Tooltip("The amount that have to be added to the stun gauge of the enemy if this attack hit him")]
    public float m_StunGaugeHitAmount = 0f;

    [Separator("Attacker effect")]
    [Tooltip("The amount that have to be added to the super gauge if this attack hit")]
    public float m_SuperGaugeHitBonus = 0f;
    [Tooltip("The amount that have to be added to the super gauge if this attack is blocked")]
    public float m_SuperGaugeBlockBonus = 0f;
    [Tooltip("The amount that have to be added to the super gauge if this attack whiff")]
    public float m_SuperGaugeWhiffBonus = 0f;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerGrabAttackLogic(this);
    }
}
