﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNormalAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerNormalAttackConfig m_Config;

    private uint m_CurrentHitCount = 0;
    private float m_LastHitCountTimeStamp = 0f;
    private Collider2D m_LastHitCollider;
    private Collider2D m_LastHurtCollider;

    public PlayerNormalAttackLogic(PlayerNormalAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();

        IncreaseSuperGauge(m_Config.m_SuperGaugeBaseBonus);

        m_CurrentHitCount = 0;
        m_LastHitCountTimeStamp = 0f;
        m_LastHitCollider = null;
        m_LastHurtCollider = null;
    }

    public override void OnHandleCollision(bool triggerHitEvent, bool checkHitDelay, Collider2D hitCollider, Collider2D hurtCollider)
    {
        base.OnHandleCollision(triggerHitEvent, checkHitDelay, hitCollider, hurtCollider);
        if (!checkHitDelay || m_LastHitCountTimeStamp == 0f || Time.time > m_LastHitCountTimeStamp + GetDelayBetweenHits())
        {
            if (m_CurrentHitCount < GetMaxHitCount())
            {
                m_CurrentHitCount++;
                m_LastHitCountTimeStamp = Time.time;
                m_LastHitCollider = hitCollider;
                m_LastHurtCollider = hurtCollider;

                ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "On handle collision | Hit count : " + GetCurrentHitCount() + ", Max hit count : " + GetMaxHitCount());

                if (triggerHitEvent)
                {
                    Utils.GetEnemyEventManager(m_Owner).TriggerEvent(EPlayerEvent.Hit, new HitEventParameters(this));
                }
            }
        }
    }

    protected override void OnEnemyTakesDamage(BaseEventParameters baseParams)
    {
        DamageTakenEventParameters damageTakenInfo = (DamageTakenEventParameters)baseParams;

        if (this == damageTakenInfo.m_AttackLogic)
        {
            base.OnEnemyTakesDamage(damageTakenInfo);

            if(damageTakenInfo.m_AttackResult == EAttackResult.Hit)
            {
                IncreaseSuperGauge(m_Config.m_SuperGaugeHitBonus);
            }
            else if(damageTakenInfo.m_AttackResult == EAttackResult.Blocked)
            {
                IncreaseSuperGauge(m_Config.m_SuperGaugeBlockBonus);
            }
        }
    }

    public override bool CanAttackBeBlocked(bool isCrouching)
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
                return base.CanAttackBeBlocked(isCrouching);
        }
    }
    public override uint GetHitDamage(EAttackResult attackResult)
    {
        float damage = 0f;
        switch (attackResult)
        {
            case EAttackResult.Hit:
                damage = m_Config.m_Damage;
                break;
            case EAttackResult.Blocked:
                damage = m_Config.m_CheapDamage;
                break;
            case EAttackResult.Parried:
            default:
                damage = 0f;
                break;
        }

        return (uint)Mathf.Max(damage * GetDamageRatio(), 1f);
    }

    public override uint GetCurrentHitCount() { return m_CurrentHitCount; }
    public override uint GetMaxHitCount() { return m_Config.m_MaxHitCount; }
    public override float GetDelayBetweenHits() { return m_Config.m_DelayBetweenHits; }
    public override bool IsHitKO() { return m_Config.m_HitKO; }

    public override bool CanStunOnDamage() { return true; }
    public override float GetStunDuration(EAttackResult attackResult)
    {
        switch (attackResult)
        {
            case EAttackResult.Hit:
                return m_Config.HitStun;
            case EAttackResult.Blocked:
                return m_Config.BlockStun;
            case EAttackResult.Parried:
            default:
                return 0f;
        }
    }
    public override float GetStunGaugeHitAmount() { return m_Config.m_StunGaugeHitAmount; }

    // Pushback can be applied only on last hit
    public override bool CanPushBack() { return GetCurrentHitCount() >= GetMaxHitCount(); }
    public override float GetPushBackForce(EAttackResult attackResult)
    {
        switch (attackResult)
        {
            case EAttackResult.Hit:
                return m_Config.m_HitPushBack;
            case EAttackResult.Blocked:
                return m_Config.m_BlockPushBack;
            case EAttackResult.Parried:
            default:
                return 0f;
        }
    }
    public override float GetAttackerPushBackForce(EAttackResult attackResult, bool enemyIsInACorner)
    {
        GetAttackerPushbackInfo(attackResult, out bool applyPushBack, out EAttackerPushBackCondition pushBackCondition, out float pushBackForce);
        if (applyPushBack)
        {
            switch (pushBackCondition)
            {
                case EAttackerPushBackCondition.Always:
                    return pushBackForce;
                case EAttackerPushBackCondition.OnlyIfEnemyIsInACorner:
                    if (enemyIsInACorner)
                    {
                        return pushBackForce;
                    }
                    break;
                default:
                    return 0f;
            }
        }

        return 0f;
    }

    private void GetAttackerPushbackInfo(EAttackResult attackResult, out bool applyPushBack, out EAttackerPushBackCondition pushBackCondition, out float pushBackForce)
    {
        switch (attackResult)
        {
            case EAttackResult.Hit:
                applyPushBack = m_Config.m_ApplyAttackerHitPushBack;
                pushBackCondition = m_Config.m_AttackerHitPushBackCondition;
                pushBackForce = m_Config.m_AttackerHitPushBack;
                break;
            case EAttackResult.Blocked:
                applyPushBack = m_Config.m_ApplyAttackerBlockPushBack;
                pushBackCondition = m_Config.m_AttackerBlockPushBackCondition;
                pushBackForce = m_Config.m_AttackerBlockPushBack;
                break;
            case EAttackResult.Parried:
            default:
                applyPushBack = false;
                pushBackCondition = EAttackerPushBackCondition.Always;
                pushBackForce = 0f;
                break;
        }
    }

    public override bool CanPlayDamageTakenAnim() { return true; }
    public override string GetBlockAnimName(EPlayerStance playerStance, EStunAnimState state)
    {
        string blockAnimName = "Block";

        blockAnimName += playerStance.ToString();

        if (playerStance == EPlayerStance.Jump)
        {
            KakutoDebug.LogError("A player can't block an attack while jumping.");
        }

        blockAnimName += "_" + state.ToString();

        return blockAnimName;
    }
    public override string GetHitAnimName(EPlayerStance playerStance, EStunAnimState state)
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
                    hitAnimName += "_" + state.ToString();
                    break;
                case EPlayerStance.Crouch:
                    hitAnimName += "Low"; // Crouch hit is necessarily low
                    hitAnimName += m_Config.m_HitStrength.ToString();
                    hitAnimName += "_" + state.ToString();
                    break;
                case EPlayerStance.Jump:
                    // Jump hit doesn't need hit height / strength
                    break;
            }
        }

        return hitAnimName;
    }

    public override Collider2D GetLastHitCollider()
    {
        return m_LastHitCollider;
    }

    public override Collider2D GetLastHurtCollider()
    {
        return m_LastHurtCollider;
    }

    public override bool GetLastHitPoint(out Vector3 hitPoint)
    {
        if(m_LastHitCollider != null && m_LastHurtCollider != null)
        {
            hitPoint = (m_LastHitCollider.bounds.center + m_LastHurtCollider.bounds.center) / 2.0f;
            return true;
        }

        return base.GetLastHitPoint(out hitPoint);
    }

    public override EHitNotificationType GetHitNotificationType(EAttackResult attackResult, bool isInBlockingStance, bool isCrouching, bool isFacingRight, PlayerAttackComponent victimAttackComponent)
    {
        EHitNotificationType hitType = base.GetHitNotificationType(attackResult, isInBlockingStance, isCrouching, isFacingRight, victimAttackComponent);
        if(hitType == EHitNotificationType.None)
        {
            EAttackType attackType = m_Config.m_AttackType;
            switch (attackType)
            {
                case EAttackType.Low:
                    hitType = EHitNotificationType.Low;
                    break;

                case EAttackType.Overhead:
                    if (!GetAttack().m_NeededStanceList.Contains(EPlayerStance.Jump))
                    {
                        hitType = EHitNotificationType.Overhead;
                    }
                    break;
            }
        }

        return hitType;
    }

    public override void GetHitFX(EAttackResult attackResult, EHitNotificationType hitNotifType, ref List<EHitFXType> hitFXList)
    {
        base.GetHitFX(attackResult, hitNotifType, ref hitFXList);
        if(hitFXList.Count == 0)
        {
            switch (attackResult)
            {
                case EAttackResult.Hit:
                    switch (m_Config.m_HitStrength)
                    {
                        case EHitStrength.Weak:
                            hitFXList.Add(EHitFXType.LightHit);
                            return;
                        case EHitStrength.Strong:
                            hitFXList.Add(EHitFXType.HeavyHit);
                            return;
                    }
                    break;
            }
        }
    }

    public override bool GetHitSFX(EAttackResult attackResult, EHitNotificationType hitNotifType, ref EAttackSFXType hitSFXType)
    {
        bool baseResult = base.GetHitSFX(attackResult, hitNotifType, ref hitSFXType);
        if (!baseResult)
        {
            switch (attackResult)
            {
                case EAttackResult.Hit:
                    switch (m_Config.m_HitStrength)
                    {
                        case EHitStrength.Weak:
                            hitSFXType = (hitNotifType == EHitNotificationType.Counter) ? EAttackSFXType.Counter_Hit_Light : EAttackSFXType.Hit_Light;
                            break;
                        case EHitStrength.Strong:
                            hitSFXType = (hitNotifType == EHitNotificationType.Counter) ? EAttackSFXType.Counter_Hit_Heavy : EAttackSFXType.Hit_Heavy;
                            break;
                    }
                    return true;
            }
        }

        return baseResult;
    }

    public PlayerNormalAttackConfig GetNormalAttackConfig()
    {
        return m_Config;
    }
}
