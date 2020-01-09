
public class PlayerDashAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerDashAttackConfig m_Config;

    public PlayerDashAttackLogic(PlayerDashAttackConfig config)
    {
        m_Config = config;
    }
    
    protected override string GetAnimationAttackName()
    {
        return m_Attack.m_AnimationAttackName.ToString() + m_Config.m_DashType.ToString();
    }
}
