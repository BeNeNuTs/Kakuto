using UnityEngine;

[CreateAssetMenu(fileName = "PlayerParryAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerParryAttackConfig", order = 2)]
public class PlayerParryAttackConfig : PlayerBaseAttackConfig
{
    [Separator("Attacker effect")]
    [Tooltip("The amount that have to be added to the super gauge if this attack is triggered")]
    public float m_SuperGaugeBaseBonus = 0f;
    [Tooltip("The amount that have to be added to the super gauge if this attack success")]
    public float m_SuperGaugeParrySuccessBonus = 0f;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerParryAttackLogic(this);
    }
}
