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
                PlayerProjectileAttackLogic.SetNextNonSuperProjectileGuardCrush();
                break;
            default:
                break;
        }
    }
}
