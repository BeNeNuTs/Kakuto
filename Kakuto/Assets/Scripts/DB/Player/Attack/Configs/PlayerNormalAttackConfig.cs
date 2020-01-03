using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerNormalAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerNormalAttackConfig", order = 0)]
public class PlayerNormalAttackConfig : PlayerBaseAttackConfig
{
    [Separator("Setting")]
    public EAttackType m_AttackType;

    [Separator("Enemy effect")]
    [Range(0, 100)]
    public uint m_Damage = 10;
    [Range(0, 100)]
    public uint m_CheapDamage = 2;
    
    [Space]

    [Tooltip("The max number of hits allowed for this attack"), Range(1, 100)]
    public uint m_MaxHitCount = 1;
    [Tooltip("How much time to wait to apply another hit")]
    public float m_DelayBetweenHits = 0.1f;

    [Space]

    public bool m_HitKO = false;
    [ConditionalField(false, "m_HitKO")]
    public EHitHeight m_HitHeight = EHitHeight.Low;
    [ConditionalField(false, "m_HitKO")]
    public EHitStrength m_HitStrength = EHitStrength.Weak;

    [Space]

    [Tooltip("How many frames does the enemy should be stunned for when hit taken. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_HitStun = 15;
    
    [HideInInspector]
    public float HitStun
    {
        get { return (float)m_HitStun / (float)GameConfig.K_ANIMATION_FPS; }
    }

    [Tooltip("How many frames does the enemy should be stunned for when this attack is blocked. /!\\ based on 30 FPS /!\\")]
    [SerializeField]
    private uint m_BlockStun = 9;

    [HideInInspector]
    public float BlockStun
    {
        get { return (float)m_BlockStun / (float)GameConfig.K_ANIMATION_FPS; }
    }

    [Space]

    [Tooltip("The force of the push back if this attack hit")]
    public float m_HitPushBack = 0.0f;
    [Tooltip("The force of the push back if this attack is blocked")]
    public float m_BlockPushBack = 0.0f;

    [Space]

    [Tooltip("The amount that have to be added to the stun gauge of the enemy if this attack hit him")]
    public float m_StunGaugeHitAmount = 0f;

    [Separator("Attacker effect")]

    public bool m_ApplyAttackerHitPushBack = false;

    [Tooltip("The force of the push back applied to the attacker if this attack hit")]
    [ConditionalField(true, "m_ApplyAttackerHitPushBack")]
    public float m_AttackerHitPushBack = 0.0f;
    [ConditionalField(true, "m_ApplyAttackerHitPushBack")]
    public EAttackerPushBackCondition m_AttackerHitPushBackCondition = EAttackerPushBackCondition.OnlyIfEnemyIsInACorner;

    public bool m_ApplyAttackerBlockPushBack = false;

    [Tooltip("The force of the push back applied to the attacker if this attack is blocked")]
    [ConditionalField(true, "m_ApplyAttackerBlockPushBack")]
    public float m_AttackerBlockPushBack = 0.0f;
    [ConditionalField(true, "m_ApplyAttackerBlockPushBack")]
    public EAttackerPushBackCondition m_AttackerBlockPushBackCondition = EAttackerPushBackCondition.OnlyIfEnemyIsInACorner;

    [Space]

    [Tooltip("The amount that have to be added to the super gauge if this attack is triggered")]
    public float m_SuperGaugeBaseBonus = 0f;
    [Tooltip("The amount that have to be added to the super gauge if this attack hit")]
    public float m_SuperGaugeHitBonus = 0f;
    [Tooltip("The amount that have to be added to the super gauge if this attack is blocked")]
    public float m_SuperGaugeBlockBonus = 0f;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerNormalAttackLogic(this);
    }
}
