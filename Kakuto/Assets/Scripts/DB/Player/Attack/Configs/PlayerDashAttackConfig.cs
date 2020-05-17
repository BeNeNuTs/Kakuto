using UnityEngine;

public enum EDashType
{
    Forward,
    Backward
}

[CreateAssetMenu(fileName = "PlayerDashAttackConfig", menuName = "Data/Player/Attacks/Configs/PlayerDashAttackConfig", order = 3)]
public class PlayerDashAttackConfig : PlayerBaseAttackConfig
{
    [Separator("Settings")]
    public EDashType m_DashType;

    public float m_MassMultiplier = 1f;
    public float m_Impulse = 3f;

    public override PlayerBaseAttackLogic CreateLogic()
    {
        return new PlayerDashAttackLogic(this);
    }

    void OnValidate()
    {
        m_MassMultiplier = Mathf.Max(1f, m_MassMultiplier);
    }
}
