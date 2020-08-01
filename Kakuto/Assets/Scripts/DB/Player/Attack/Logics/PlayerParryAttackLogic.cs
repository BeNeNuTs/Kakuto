
public enum EParryState
{
    Startup,
    Whiff,
    Counter,
    None
}

public class PlayerParryAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerParryAttackConfig m_Config;

    private static readonly string K_PARRY_COUNTER_ANIM = "ParryCounterAttack";

    private EParryState m_ParryState = EParryState.None;

    public PlayerParryAttackLogic(PlayerParryAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();

        IncreaseSuperGauge(m_Config.m_SuperGaugeBaseBonus);

        m_ParryState = EParryState.Startup;

        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StartListening(EPlayerEvent.ParrySuccess, OnParrySuccess);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StartListening(EPlayerEvent.EndOfParry, OnEndOfParry);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        m_ParryState = EParryState.None;

        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StopListening(EPlayerEvent.ParrySuccess, OnParrySuccess);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StopListening(EPlayerEvent.EndOfParry, OnEndOfParry);
    }

    public bool CanParryAttack(PlayerBaseAttackLogic attackLogic)
    {
        return m_ParryState == EParryState.Startup;
    }

    void OnParrySuccess(EAnimationAttackName attackName)
    {
        if (m_Attack.m_AnimationAttackName == attackName)
        {
            IncreaseSuperGauge(m_Config.m_SuperGaugeParrySuccessBonus);
            m_ParryState = EParryState.Counter;
            m_HasTouched = true; // Set it to true in order to allow unblock attack and cancelling this one by another one if needed (parry success is considered as a regular attack hit)
            PlayerGuardCrushTriggerAttackLogic.SetTriggerPointStatus(m_InfoComponent, PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus.Active); // Successfully parrying a hit activate the trigger

            m_Animator.Play(K_PARRY_COUNTER_ANIM, 0, 0);
        }
    }

    void OnEndOfParry(EAnimationAttackName attackName)
    {
        if (m_Attack.m_AnimationAttackName == attackName)
        {
            m_ParryState = EParryState.Whiff;
        }
    }
}
