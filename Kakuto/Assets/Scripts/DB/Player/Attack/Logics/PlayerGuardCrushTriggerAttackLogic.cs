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

    public override void OnInit(GameObject owner, PlayerAttack attack)
    {
        base.OnInit(owner, attack);

        if (m_InfoComponent.GetPlayerSettings().m_TriggerPointAlwaysActive)
        {
            SetTriggerPointStatus(m_InfoComponent, ETriggerPointStatus.Active);
        }
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

    public override void OnShutdown()
    {
        base.OnShutdown();

        SetTriggerPointStatus(m_InfoComponent, ETriggerPointStatus.Inactive); // Reset trigger point at the end of each round
    }

    public static void SetTriggerPointStatus(PlayerInfoComponent infoComponent, ETriggerPointStatus status)
    {
        if (infoComponent.GetPlayerSettings().m_TriggerPointAlwaysActive && status == ETriggerPointStatus.Inactive)
        {
            status = ETriggerPointStatus.Active;
        }

        m_TriggerPointStatus[infoComponent.GetPlayerIndex()] = status;
        OnTriggerPointStatusChanged[infoComponent.GetPlayerIndex()]?.Invoke(status);
    }

    public static ETriggerPointStatus GetTriggerPointStatus(int playerIndex)
    {
        return m_TriggerPointStatus[playerIndex];
    }
}
