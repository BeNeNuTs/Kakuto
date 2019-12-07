using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerNormalAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerNormalAttackConfig", order = 0)]
public class PlayerNormalAttackConfig : PlayerBaseAttackConfig
{
    [Header("Setting")]
    public EAttackType m_AttackType;

    [Header("Enemy effect")]
    [Range(0, 100)]
    public uint m_CheapDamage = 2;
    /////////////////////////////////////////////////////
    [Tooltip("The max number of hits allowed for this attack"), Range(1, 100)]
    public uint m_MaxHitCount = 1;
    [Tooltip("How much time to wait to apply another hit")]
    public float m_DelayBetweenHits = 0.1f;
    public bool m_HitKO = false;
    [ConditionalField(false, "m_HitKO")]
    public EHitHeight m_HitHeight = EHitHeight.Low;
    [ConditionalField(false, "m_HitKO")]
    public EHitStrength m_HitStrength = EHitStrength.Weak;
    /////////////////////////////////////////////////////
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
    /////////////////////////////////////////////////////
    [Tooltip("The force of the push back if this attack hit")]
    public float m_HitPushBack = 0.0f;
    [Tooltip("The force of the push back if this attack is blocked")]
    public float m_BlockPushBack = 0.0f;
    /////////////////////////////////////////////////////

    [Header("Attacker effect")]
    [Tooltip("The force of the push back applied to the attacker if this attack hit or is blocked")]
    public float m_AttackerPushBack = 0.0f;
    public EAttackerPushBackCondition m_AttackerPushBackCondition = EAttackerPushBackCondition.OnlyIfEnemyIsInACorner;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerNormalAttackLogic(this);
    }
}
