using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNormalAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerNormalAttackConfig m_Config;

    public PlayerNormalAttackLogic(PlayerNormalAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        m_Animator.Play(m_Attack.m_AnimationAttackName.ToString(), 0, 0);
    }

    public override bool CanBlockAttack(bool isCrouching)
    {
        switch (m_Config.m_AttackType)
        {
            case EAttackType.Low:
                return isCrouching;
            case EAttackType.Mid:
                return true;
            case EAttackType.Overhead:
                return !isCrouching;
            default:
                return false;
        }
    }
    public override uint GetHitDamage(bool isAttackBlocked)
    {
        return (isAttackBlocked) ? m_Config.m_CheapDamage : m_Attack.m_Damage;
    }

    public override uint GetMaxHitCount() { return m_Config.m_MaxHitCount; }
    public override float GetDelayBetweenHits() { return m_Config.m_DelayBetweenHits; }
    public override bool IsHitKO() { return m_Config.m_HitKO; }

    public override bool CanStun() { return true; }
    public override float GetStunDuration(bool isAttackBlocked)
    {
        return (isAttackBlocked) ? m_Config.BlockStun : m_Config.HitStun;
    }

    public override bool CanPushBack() { return true; }
    public override float GetPushBackForce(bool isAttackBlocked)
    {
        return (isAttackBlocked) ? m_Config.m_BlockPushBack : m_Config.m_HitPushBack;
    }
    public override float GetAttackerPushBackForce(bool enemyIsInACorner)
    {
        switch (m_Config.m_AttackerPushBackCondition)
        {
            case EAttackerPushBackCondition.Always:
                return m_Config.m_AttackerPushBack;
            case EAttackerPushBackCondition.OnlyIfEnemyIsInACorner:
                if(enemyIsInACorner)
                {
                    return m_Config.m_AttackerPushBack;
                }
                break;
            default:
                return 0f;
        }

        return 0f;
    }

    public override bool CanPlayDamageTakenAnim() { return true; }
    public override string GetBlockAnimName(EPlayerStance playerStance)
    {
        string blockAnimName = "Block";

        blockAnimName += playerStance.ToString();

        if (playerStance == EPlayerStance.Jump)
        {
            Debug.LogError("A player can't block an attack while jumping.");
        }

        blockAnimName += "_In"; //Play the In animation

        return blockAnimName;
    }
    public override string GetHitAnimName(EPlayerStance playerStance)
    {
        string hitAnimName = "Hit";

        hitAnimName += playerStance.ToString();

        if(IsHitKO())
        {
            hitAnimName += "KO";
        }
        else
        {
            switch (playerStance)
            {
                case EPlayerStance.Stand:
                    hitAnimName += m_Config.m_HitHeight.ToString();
                    hitAnimName += m_Config.m_HitStrength.ToString();
                    hitAnimName += "_In"; //Play the In animation
                    break;
                case EPlayerStance.Crouch:
                    hitAnimName += "Low"; // Crouch hit is necessarily low
                    hitAnimName += m_Config.m_HitStrength.ToString();
                    hitAnimName += "_In"; //Play the In animation
                    break;
                case EPlayerStance.Jump:
                    // Jump hit doesn't need hit height / strength
                    break;
            }
        }

        return hitAnimName;
    }
}
