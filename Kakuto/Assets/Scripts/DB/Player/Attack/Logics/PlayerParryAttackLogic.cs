﻿
public enum EParryState
{
    Startup,
    Whiff,
    Counter,
    None
}

public class PlayerParryAttackLogic : PlayerBaseAttackLogic
{
    private static readonly string K_PARRY_COUNTER_ANIM = "ParryCounterAttack";

    private readonly PlayerParryAttackConfig m_Config;
    private readonly TimeScaleSubGameManager m_TimeScaleManager;

    private EParryState m_ParryState = EParryState.None;

    public PlayerParryAttackLogic(PlayerParryAttackConfig config)
    {
        m_Config = config;
        m_TimeScaleManager = GameManager.Instance.GetSubManager<TimeScaleSubGameManager>(ESubManager.TimeScale);
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();

        IncreaseSuperGauge(m_Config.m_SuperGaugeBaseBonus);

        m_ParryState = EParryState.Startup;

        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.ParrySuccess, OnParrySuccess);
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.EndOfParry, OnEndOfParry);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        if (m_ParryState == EParryState.Counter)
        {
            ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "ParrySuccess: Unreeze time");
            m_TimeScaleManager.UnfreezeTime(m_Animator);
        }
        m_ParryState = EParryState.None;

        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.ParrySuccess, OnParrySuccess);
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.EndOfParry, OnEndOfParry);
    }

    public bool CanParryAttack(PlayerBaseAttackLogic attackLogic)
    {
        return m_ParryState == EParryState.Startup;
    }

    void OnParrySuccess(BaseEventParameters baseParams)
    {
        IncreaseSuperGauge(m_Config.m_SuperGaugeParrySuccessBonus);
        m_ParryState = EParryState.Counter;
        m_HasTouched = true; // Set it to true in order to allow unblock attack and cancelling this one by another one if needed (parry success is considered as a regular attack hit)
        if(PlayerGuardCrushTriggerAttackLogic.GetTriggerPointStatus(GetPlayerIndex()) == PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus.Inactive)
        {
            PlayerGuardCrushTriggerAttackLogic.SetTriggerPointStatus(m_InfoComponent, PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus.Active); // Successfully parrying a hit activate the trigger
        }

        m_Animator.Play(K_PARRY_COUNTER_ANIM, 0, 0);

        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "ParrySuccess: Freeze time");
        m_TimeScaleManager.FreezeTime(m_Animator);
    }

    void OnEndOfParry(BaseEventParameters baseParams)
    {
        m_ParryState = EParryState.Whiff;
    }
}
