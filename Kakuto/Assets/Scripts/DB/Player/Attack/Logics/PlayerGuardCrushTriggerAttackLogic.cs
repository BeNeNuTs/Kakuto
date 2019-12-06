using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGuardCrushTriggerAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerGuardCrushTriggerAttackConfig m_Config;

    public PlayerGuardCrushTriggerAttackLogic(PlayerGuardCrushTriggerAttackConfig config)
    {
        m_Config = config;
    }

    public override bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = base.EvaluateConditions(currentAttackLogic);

        if(conditionIsValid)
        {
            conditionIsValid &= CheckConditionsInternal();
        }

        return conditionIsValid;
    }

    private bool CheckConditionsInternal()
    {
        switch (m_Config.m_GuardCrushType)
        {
            case PlayerGuardCrushTriggerAttackConfig.EGuardCrushType.NextNonSuperProjectile:
                return !PlayerProjectileAttackLogic.IsNextNonSuperProjectileGuardCrush(m_MovementComponent.GetPlayerIndex());
            default:
                return false;
        }
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        m_Animator.Play(m_Attack.m_AnimationAttackName.ToString(), 0, 0);
        TriggerGuardCrushEffect();
    }

    private void TriggerGuardCrushEffect()
    {
        switch (m_Config.m_GuardCrushType)
        {
            case PlayerGuardCrushTriggerAttackConfig.EGuardCrushType.NextNonSuperProjectile:
                PlayerProjectileAttackLogic.SetNextNonSuperProjectileGuardCrush(m_MovementComponent.GetPlayerIndex(), true);
                break;
            default:
                break;
        }
    }
}
