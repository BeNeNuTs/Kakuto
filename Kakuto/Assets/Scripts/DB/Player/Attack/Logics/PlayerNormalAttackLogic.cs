using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNormalAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerNormalAttackConfig m_Config;

    private uint m_CurrentHitCount = 0;
    private float m_LastHitCountTimeStamp = 0f;

    public PlayerNormalAttackLogic(PlayerNormalAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        m_CurrentHitCount = 0;
        m_LastHitCountTimeStamp = 0f;
    }

    public override void OnHandleCollision(bool triggerHitEvent)
    {
        base.OnHandleCollision(triggerHitEvent);
        if (m_LastHitCountTimeStamp == 0f || Time.time > m_LastHitCountTimeStamp + GetDelayBetweenHits())
        {
            if (m_CurrentHitCount < GetMaxHitCount())
            {
                m_CurrentHitCount++;
                m_LastHitCountTimeStamp = Time.time;
                if(triggerHitEvent)
                {
                    Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(m_Owner).TriggerEvent(EPlayerEvent.Hit, this);
                }
            }
        }
    }

    protected override void OnEnemyTakesDamage(DamageTakenInfo damageTakenInfo)
    {
        base.OnEnemyTakesDamage(damageTakenInfo);
        IncreaseSuperGauge(damageTakenInfo.m_IsAttackBlocked ? m_Config.m_SuperGaugeBlockBonus : m_Config.m_SuperGaugeHitBonus);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        // If this attack ends without hitting the enemy, increase the super gauge amount with the whiff value
        if (!HasHit())
        {
            IncreaseSuperGauge(m_Config.m_SuperGaugeWhiffBonus);
        }

        m_CurrentHitCount = 0;
        m_LastHitCountTimeStamp = 0f;
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
        float damage = m_Config.m_Damage;
        if (isAttackBlocked)
        {
            damage = m_Config.m_CheapDamage;
        }

        return (uint)(damage * GetDamageRatio());
    }

    public override uint GetCurrentHitCount() { return m_CurrentHitCount; }
    public override uint GetMaxHitCount() { return m_Config.m_MaxHitCount; }
    public override float GetDelayBetweenHits() { return m_Config.m_DelayBetweenHits; }
    public override bool IsHitKO() { return m_Config.m_HitKO; }

    public override bool CanStun() { return true; }
    public override float GetStunDuration(bool isAttackBlocked)
    {
        return (isAttackBlocked) ? m_Config.BlockStun : m_Config.HitStun;
    }
    public override float GetStunGaugeHitAmount() { return m_Config.m_StunGaugeHitAmount; }

    // Pushback can be applied only on last hit
    public override bool CanPushBack() { return GetCurrentHitCount() >= GetMaxHitCount(); }
    public override float GetPushBackForce(bool isAttackBlocked)
    {
        return (isAttackBlocked) ? m_Config.m_BlockPushBack : m_Config.m_HitPushBack;
    }
    public override float GetAttackerPushBackForce(bool isAttackBlocked, bool enemyIsInACorner)
    {
        if ((isAttackBlocked && m_Config.m_ApplyAttackerBlockPushBack) || (!isAttackBlocked && m_Config.m_ApplyAttackerHitPushBack))
        {
            EAttackerPushBackCondition condition = (isAttackBlocked) ? m_Config.m_AttackerBlockPushBackCondition : m_Config.m_AttackerHitPushBackCondition;
            float pushBack = (isAttackBlocked) ? m_Config.m_AttackerBlockPushBack : m_Config.m_AttackerHitPushBack;

            switch (condition)
            {
                case EAttackerPushBackCondition.Always:
                    return pushBack;
                case EAttackerPushBackCondition.OnlyIfEnemyIsInACorner:
                    if (enemyIsInACorner)
                    {
                        return pushBack;
                    }
                    break;
                default:
                    return 0f;
            }
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
