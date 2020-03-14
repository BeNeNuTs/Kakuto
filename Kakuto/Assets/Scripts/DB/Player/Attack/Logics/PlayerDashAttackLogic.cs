
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

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();

        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.ApplyDashImpulse, ApplyDashImpulse);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.ApplyDashImpulse, ApplyDashImpulse);
    }

    private void ApplyDashImpulse(bool dummy)
    {
        switch (m_Config.m_DashType)
        {
            case EDashType.Forward:
                m_MovementComponent.PushForward(m_Config.m_Impulse);
                break;
            case EDashType.Backward:
                m_MovementComponent.PushBack(m_Config.m_Impulse);
                break;
            default:
                break;
        }
    }
}
