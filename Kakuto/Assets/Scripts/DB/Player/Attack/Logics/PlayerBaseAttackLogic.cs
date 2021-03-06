﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStunAnimState
{
    In,
    Loop,
    Out
}

public class PlayerBaseAttackLogic
{
    protected GameObject m_Owner;
    protected PlayerAttack m_Attack;
    protected Animator m_Animator;
    protected PlayerMovementComponent m_MovementComponent;
    protected PlayerAttackComponent m_AttackComponent;
    protected PlayerInfoComponent m_InfoComponent;
    protected AudioSubGameManager m_AudioManager;

    protected bool m_HasTouched = false;
    protected float m_DamageRatio = 1f;
    protected bool m_DamageRatioComputed = false;

    protected bool m_AttackLaunched = false;

    private IEnumerator m_CurrentCrossupCoroutine = null;

    public virtual void OnInit(PlayerAttackComponent playerAttackComponent, PlayerAttack attack)
    {
        m_Owner = playerAttackComponent.gameObject;
        m_Attack = attack;
        m_Animator = playerAttackComponent.m_Animator;
        m_MovementComponent = playerAttackComponent.m_MovementComponent;
        m_AttackComponent = playerAttackComponent;
        m_InfoComponent = playerAttackComponent.m_InfoComponent;
        m_AudioManager = playerAttackComponent.m_AudioManager;
    }

    public virtual void OnShutdown()
    {
        if (m_CurrentCrossupCoroutine != null)
        {
            m_AttackComponent.StopCoroutine(m_CurrentCrossupCoroutine);
        }
    }

    public virtual bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = true;

        PlayerAttack attack = GetAttack();
        if (m_MovementComponent != null)
        {
            conditionIsValid &= (attack.m_NeededStanceList.Contains(m_MovementComponent.GetCurrentStance()));
        }

        if (attack.m_HasCondition)
        {
            if(attack.m_SuperGaugeAmountNeeded > 0f)
            {
                PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
                if(superGaugeSC != null)
                {
                    conditionIsValid &= superGaugeSC.GetCurrentGaugeValue() >= attack.m_SuperGaugeAmountNeeded;
                }
            }

            if (attack.m_HasAttackRequirement)
            {
                conditionIsValid &= (currentAttackLogic != null && currentAttackLogic.GetAttack().m_AnimationAttackName == attack.m_AttackRequired);
            }

            if (attack.m_HasMovementRequirement)
            {
                conditionIsValid &= attack.m_MovementRequiredList.Contains(m_MovementComponent.GetMovingDirection());
            }
        }

        return conditionIsValid;
    }

    public virtual void OnAttackLaunched()
    {
        m_Animator.Play(GetAnimationAttackName(), 0, 0);
        m_AudioManager.PlayWhiffSFX(GetPlayerIndex(), m_Attack.m_WhiffSFXType);

        if (m_Attack.m_IsEXAttack)
        {
            m_InfoComponent.SetCurrentPalette(EPalette.EX);
        }

        PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
        if (superGaugeSC != null)
        {
            superGaugeSC.DecreaseGaugeValue(GetAttack().m_SuperGaugeAmountNeeded);
        }

        m_HasTouched = false;
        m_DamageRatio = 1f;
        m_DamageRatioComputed = false;
        Utils.GetEnemyEventManager(m_Owner).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);

        m_AttackLaunched = true;
    }

    public virtual string GetAnimationAttackName()
    {
        return m_Attack.m_AnimationAttackName.ToString();
    }

    public virtual void OnAttackStopped()
    {
        if (m_Attack.m_IsEXAttack)
        {
            m_InfoComponent.ResetCurrentPalette();
        }

        if (CanStopListeningEnemyTakesDamage())
        {
            Utils.GetEnemyEventManager(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);
        }

        m_AttackLaunched = false;
    }

    public virtual bool CanAttackBeBlocked(bool isCrouching) { return true; }
    public virtual bool CanAttackBeParried() { return true; }
    public virtual uint GetHitDamage(EAttackResult attackResult) { return 0; }

    public virtual uint GetCurrentHitCount() { return 0; }
    public virtual uint GetMaxHitCount() { return 1; }
    public virtual float GetDelayBetweenHits() { return 0.1f; }
    public virtual bool IsHitKO() { return false; }

    public virtual bool CanStunOnDamage() { return false; }
    public virtual float GetStunDuration(EAttackResult attackResult) { return 0.0f; }
    public virtual float GetStunGaugeHitAmount() { return 0f; }

    public virtual bool CanPushBack() { return false; }
    public virtual float GetPushBackForce(EAttackResult attackResult) { return 0.0f; }
    public virtual float GetAttackerPushBackForce(EAttackResult attackResult, bool enemyIsInACorner) { return 0.0f; }

    public virtual bool CanPlayDamageTakenAnim() { return false; }
    public virtual string GetBlockAnimName(EPlayerStance playerStance, EStunAnimState state) { return ""; }
    public virtual string GetHitAnimName(EPlayerStance playerStance, EStunAnimState state) { return ""; }

    public virtual void OnHandleCollision(bool triggerHitEvent, bool checkHitDelay, Collider2D hitCollider, Collider2D hurtCollider) { }
    public virtual bool NeedPushBoxCollisionCallback() { return false; }
    public virtual void OnHandlePushBoxCollision(Collision2D collision) { }
    public bool HasTouched() { return m_HasTouched; }

    public GameObject GetOwner() { return m_Owner; }
    public int GetPlayerIndex() { return m_InfoComponent.GetPlayerIndex(); }
    public PlayerAttack GetAttack() { return m_Attack; }
    public Animator GetAnimator() { return m_Animator; }

    public bool IsASuper() { return m_Attack.m_IsASuper; }

    protected virtual bool CanStopListeningEnemyTakesDamage() { return true; }
    protected virtual void OnEnemyTakesDamage(BaseEventParameters baseParams)
    {
        DamageTakenEventParameters damageTakenInfo = (DamageTakenEventParameters)baseParams;
        if (this == damageTakenInfo.m_AttackLogic)
        {
            m_HasTouched = true;
        }
        else if(damageTakenInfo.m_AttackLogic.GetType() != typeof(PlayerProjectileAttackLogic))
        {
            KakutoDebug.LogError("DamageTaken event has been received in " + m_Attack.m_AnimationAttackName + " but damage taken doesn't come from this attack. This attack has not been stopped correctly");
        }
    }

    protected void IncreaseSuperGauge(float amount)
    {
        if (amount > 0f)
        {
            PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
            if (superGaugeSC != null)
            {
                superGaugeSC.IncreaseGaugeValue(amount);
            }
        }
    }

    protected float GetDamageRatio()
    {
        if(!m_DamageRatioComputed)
        {
            PlayerComboCounterSubComponent comboSC = m_AttackComponent.GetComboCounterSubComponent();
            if (comboSC != null)
            {
                uint hitCounter = comboSC.GetComboCounter();
                m_DamageRatio = AttackConfig.Instance.m_DamageScaling.Evaluate(hitCounter);
                m_DamageRatio = Mathf.Clamp(m_DamageRatio, 0f, 1f);
                m_DamageRatioComputed = true;
            }
        }

        return m_DamageRatio;
    }

    public virtual Collider2D GetLastHitCollider()
    {
        return null;
    }

    public virtual Collider2D GetLastHurtCollider()
    {
        return null;
    }

    public virtual bool GetLastHitPoint(out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        return false;
    }

    public virtual EHitNotificationType GetHitNotificationType(EAttackResult attackResult, bool isInBlockingStance, bool isCrouching, bool isFacingRight, PlayerAttackComponent victimAttackComponent)
    {
        if (victimAttackComponent.GetCurrentAttackLogic() != null)
        {
            EAttackState victimAttackState = victimAttackComponent.CurrentAttackState;
            if (victimAttackState == EAttackState.Startup || victimAttackState == EAttackState.Active)
            {
                return EHitNotificationType.Counter;
            }
        }

        if (m_Attack.m_NeededStanceList.Contains(EPlayerStance.Jump) && m_MovementComponent.IsJumping())
        {
            if (m_CurrentCrossupCoroutine != null)
            {
                m_AttackComponent.StopCoroutine(m_CurrentCrossupCoroutine);
            }

            m_CurrentCrossupCoroutine = ValidateCrossup_Coroutine(victimAttackComponent.transform);
            m_AttackComponent.StartCoroutine(m_CurrentCrossupCoroutine);
        }

        return EHitNotificationType.None;
    }

    private IEnumerator ValidateCrossup_Coroutine(Transform victimAttackTransform)
    {
        // Wait for landing
        while(m_MovementComponent.IsJumping())
        {
            yield return null;
        }

        m_MovementComponent.GetOnJumpingXPositions(out float myXPosition, out float enemyXPosition);
        if (myXPosition < enemyXPosition)
        {
            if (m_Owner.transform.position.x > victimAttackTransform.position.x)
            {
                HitNotificationEventParameters hitNotifParams = new HitNotificationEventParameters(EHitNotificationType.Crossup);
                Utils.GetPlayerEventManager(m_Owner).TriggerEvent(EPlayerEvent.HitNotification, hitNotifParams);
            }
        }
        else
        {
            if (m_Owner.transform.position.x < victimAttackTransform.position.x)
            {
                HitNotificationEventParameters hitNotifParams = new HitNotificationEventParameters(EHitNotificationType.Crossup);
                Utils.GetPlayerEventManager(m_Owner).TriggerEvent(EPlayerEvent.HitNotification, hitNotifParams);
            }
        }

    }

    public virtual void GetHitFX(EAttackResult attackResult, EHitNotificationType hitNotifType, ref List<EHitFXType> hitFXList)
    {
        switch (attackResult)
        {
            case EAttackResult.Hit:
                if(hitNotifType == EHitNotificationType.Counter)
                {
                    hitFXList.Add(EHitFXType.Counter);
                    return;
                }

                if(m_Attack.m_AnimationAttackName >= EAnimationAttackName.Special01)
                {
                    hitFXList.Add(EHitFXType.SpecialHit);
                    return;
                }
                break;
            case EAttackResult.Blocked:
                hitFXList.Add(EHitFXType.Block);
                return;

            case EAttackResult.Parried:
                hitFXList.Add(EHitFXType.Parry);
                return;
        }
    }

    public virtual bool GetHitSFX(EAttackResult attackResult, EHitNotificationType hitNotifType, ref EAttackSFXType hitSFXType)
    {
        switch (attackResult)
        {
            case EAttackResult.Hit:
                if(IsHitKO())
                {
                    hitSFXType = EAttackSFXType.Hit_KO;
                    return true;
                }

                if (m_Attack.m_AnimationAttackName >= EAnimationAttackName.Special01)
                {
                    hitSFXType = EAttackSFXType.Hit_Special;
                    return true;
                }
                break;
            case EAttackResult.Blocked:
                hitSFXType = EAttackSFXType.Blocked_Hit;
                return true;
            case EAttackResult.Parried:
                hitSFXType = EAttackSFXType.Parry_Hit;
                return true;
        }

        return false;
    }
}
