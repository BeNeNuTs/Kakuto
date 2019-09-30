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

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerNormalAttackLogic(this);
    }
}
