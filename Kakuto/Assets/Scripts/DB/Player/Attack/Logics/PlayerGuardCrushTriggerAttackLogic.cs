using System;
using UnityEngine;

public class PlayerGuardCrushTriggerAttackLogic : PlayerBaseAttackLogic
{
    public enum ETriggerPointStatus
    {
        Inactive,
        Active,
        Triggered
    }

    private static readonly ETriggerPointStatus[] m_TriggerPointStatus = { ETriggerPointStatus.Inactive, ETriggerPointStatus.Inactive };
    public static Action<ETriggerPointStatus>[] OnTriggerPointStatusChanged = { null, null };

    private readonly PlayerGuardCrushTriggerAttackConfig m_Config;

    public PlayerGuardCrushTriggerAttackLogic(PlayerGuardCrushTriggerAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnInit(PlayerAttackComponent playerAttackComponent, PlayerAttack attack)
    {
        base.OnInit(playerAttackComponent, attack);

        bool triggerAlwaysActive = m_InfoComponent.GetPlayerSettings().TriggerPointAlwaysActive;
        SetTriggerPointStatus(m_InfoComponent, (triggerAlwaysActive) ? ETriggerPointStatus.Active : ETriggerPointStatus.Inactive);

        m_InfoComponent.GetPlayerSettings().OnTriggerPointAlwaysActiveChanged += OnTriggerPointAlwaysActiveChanged;
    }

    public override bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = base.EvaluateConditions(currentAttackLogic);

        if(conditionIsValid)
        {
            conditionIsValid &= GetTriggerPointStatus(m_InfoComponent.GetPlayerIndex()) == ETriggerPointStatus.Active && CheckConditionsInternal();
        }

        return conditionIsValid;
    }

    private bool CheckConditionsInternal()
    {
        switch (m_Config.m_GuardCrushType)
        {
            case PlayerGuardCrushTriggerAttackConfig.EGuardCrushType.NextNonSuperProjectile:
                return !PlayerProjectileAttackLogic.IsNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex());
            default:
                return false;
        }
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        TriggerGuardCrushEffect();
    }

    private void TriggerGuardCrushEffect()
    {
        switch (m_Config.m_GuardCrushType)
        {
            case PlayerGuardCrushTriggerAttackConfig.EGuardCrushType.NextNonSuperProjectile:
                PlayerProjectileAttackLogic.SetNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex(), true);
                break;
            default:
                break;
        }

        SetTriggerPointStatus(m_InfoComponent, ETriggerPointStatus.Triggered); // Set trigger point in triggered status as it has been used
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        if (m_TriggerPointStatus[m_InfoComponent.GetPlayerIndex()] == ETriggerPointStatus.Triggered)
        {
            m_InfoComponent.SetDefaultAndCurrentPalette(EPalette.Trigger);
        }
    }

    public override void OnShutdown()
    {
        base.OnShutdown();

        SetTriggerPointStatus(m_InfoComponent, ETriggerPointStatus.Inactive); // Reset trigger point at the end of each round
        m_InfoComponent.GetPlayerSettings().OnTriggerPointAlwaysActiveChanged -= OnTriggerPointAlwaysActiveChanged;
        GamePauseMenuComponent.IsInPauseChanged -= ActiveTriggerPointAfterPause;
    }

    public static void SetTriggerPointStatus(PlayerInfoComponent infoComponent, ETriggerPointStatus status)
    {
        if(status == ETriggerPointStatus.Inactive)
        {
            if (m_TriggerPointStatus[infoComponent.GetPlayerIndex()] == ETriggerPointStatus.Triggered)
            {
                infoComponent.ResetDefaultAndCurrentPalette();
            }

            if (infoComponent.GetPlayerSettings().TriggerPointAlwaysActive)
            {
                status = ETriggerPointStatus.Active;
            }
        }

        m_TriggerPointStatus[infoComponent.GetPlayerIndex()] = status;
        OnTriggerPointStatusChanged[infoComponent.GetPlayerIndex()]?.Invoke(status);
    }

    public static ETriggerPointStatus GetTriggerPointStatus(int playerIndex)
    {
        return m_TriggerPointStatus[playerIndex];
    }

    private void OnTriggerPointAlwaysActiveChanged(bool triggerPointAlwaysActive)
    {
        if(GetTriggerPointStatus(m_InfoComponent.GetPlayerIndex()) == ETriggerPointStatus.Inactive)
        {
            if (triggerPointAlwaysActive)
            {
                GamePauseMenuComponent.IsInPauseChanged += ActiveTriggerPointAfterPause;
            }
            else
            {
                GamePauseMenuComponent.IsInPauseChanged -= ActiveTriggerPointAfterPause;
            }
        }
    }

    private void ActiveTriggerPointAfterPause(bool _isInPause)
    {
        if (!_isInPause)
        {
            SetTriggerPointStatus(m_InfoComponent, ETriggerPointStatus.Active);
            GamePauseMenuComponent.IsInPauseChanged -= ActiveTriggerPointAfterPause;
        }
    }
}
