﻿using UnityEngine;
using System.Collections;

public abstract class BaseEventParameters
{
    public abstract EPlayerEvent GetEventType();
}

#region ATTACK
public class BlockAttackEventParameters : BaseEventParameters
{
    public EAnimationAttackName m_CurrentAttack;

    public BlockAttackEventParameters(EAnimationAttackName currentAttack)
    {
        m_CurrentAttack = currentAttack;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.BlockAttack; }
}

public class UnblockAttackEventParameters : BaseEventParameters
{
    public EAnimationAttackName m_AttackToUnblock;
    public UnblockAttackAnimEventConfig m_Config;

    public UnblockAttackEventParameters(EAnimationAttackName attackToUnblock, UnblockAttackAnimEventConfig config)
    {
        m_AttackToUnblock = attackToUnblock;
        m_Config = config;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.UnblockAttack; }
}

public class SyncGrabbedPositionEventParameters : BaseEventParameters
{
    public Transform m_GrabHook;

    public SyncGrabbedPositionEventParameters(Transform grabHook)
    {
        m_GrabHook = grabHook;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.SyncGrabbedPosition; }
}

public class AttackLaunchedEventParameters : BaseEventParameters
{
    public PlayerBaseAttackLogic m_AttackLogic;

    public AttackLaunchedEventParameters(PlayerBaseAttackLogic attackLogic)
    {
        m_AttackLogic = attackLogic;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.AttackLaunched; }
}

public class EndOfAttackEventParameters : BaseEventParameters
{
    public EAnimationAttackName m_Attack;

    public EndOfAttackEventParameters(EAnimationAttackName attack)
    {
        m_Attack = attack;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.EndOfAttack; }
}

public class ProjectileSpawnedEventParameters : BaseEventParameters
{
    public ProjectileComponent m_Projectile;

    public ProjectileSpawnedEventParameters(ProjectileComponent projectile)
    {
        m_Projectile = projectile;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.ProjectileSpawned; }
}

public class ProjectileDestroyedEventParameters : ProjectileSpawnedEventParameters
{
    public ProjectileDestroyedEventParameters(ProjectileComponent projectile) : base(projectile)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.ProjectileDestroyed; }
}

public class HitNotificationEventParameters : BaseEventParameters
{
    public EHitNotificationType m_HitNotificationType;

    public HitNotificationEventParameters(EHitNotificationType hitNotif)
    {
        m_HitNotificationType = hitNotif;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.HitNotification; }
}
#endregion

#region HEALTH
public class HitEventParameters : BaseEventParameters
{
    public PlayerBaseAttackLogic m_AttackLogic;

    public HitEventParameters(PlayerBaseAttackLogic attackLogic)
    {
        m_AttackLogic = attackLogic;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.Hit; }
}

public class GrabTryEventParameters : HitEventParameters
{
    public GrabTryEventParameters(PlayerBaseAttackLogic attackLogic) : base(attackLogic)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.GrabTry; }
}

public class GrabTouchedEventParameters : HitEventParameters
{
    public GrabTouchedEventParameters(PlayerBaseAttackLogic attackLogic) : base(attackLogic)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.GrabTouched; }
}

public class GrabBlockedEventParameters : HitEventParameters
{
    public GrabBlockedEventParameters(PlayerBaseAttackLogic attackLogic) : base(attackLogic)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.GrabBlocked; }
}

public class GrabbedEventParameters : HitEventParameters
{
    public GrabbedEventParameters(PlayerBaseAttackLogic attackLogic) : base(attackLogic)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.Grabbed; }
}

public class DamageTakenEventParameters : BaseEventParameters
{
    public GameObject m_Victim;
    public PlayerBaseAttackLogic m_AttackLogic;
    public EAttackResult m_AttackResult;
    public uint m_DamageTaken;
    public float m_HealthRatio;
    public bool m_IsAlreadyHitStunned;
    public EHitNotificationType m_HitNotificationType;

    public DamageTakenEventParameters(GameObject victim, PlayerBaseAttackLogic attackLogic, EAttackResult attackResult, bool isAlreadyHitStunned, uint damage, float healthRatio, EHitNotificationType hitNotif)
    {
        m_Victim = victim;
        m_AttackLogic = attackLogic;
        m_AttackResult = attackResult;
        m_DamageTaken = damage;
        m_HealthRatio = healthRatio;
        m_IsAlreadyHitStunned = isAlreadyHitStunned;
        m_HitNotificationType = hitNotif;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.DamageTaken; }
}

public class DeathEventParameters : BaseEventParameters
{
    public string m_PlayerTag;

    public DeathEventParameters(string playerTag)
    {
        m_PlayerTag = playerTag;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.OnDeath; }
}

public abstract class StunEventParameters : BaseEventParameters
{
    public EStunType m_StunType;
    public bool m_StunByGrabAttack;

    public StunEventParameters(EStunType stunType, bool stunByGrabAttack)
    {
        m_StunType = stunType;
        m_StunByGrabAttack = stunByGrabAttack;
    }
}

public class StunBeginEventParameters : StunEventParameters
{

    public StunBeginEventParameters(EStunType stunType, bool stunByGrabAttack) : base(stunType, stunByGrabAttack)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.StunBegin; }
}

public class StunEndEventParameters : StunEventParameters
{

    public StunEndEventParameters(EStunType stunType, bool stunByGrabAttack) : base(stunType, stunByGrabAttack)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.StunEnd; }
}

public class RefillHPEventParameters : BaseEventParameters
{

    public RefillHPEventParameters()
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.OnRefillHP; }
}

public class ProximityBoxParameters : BaseEventParameters
{
    public bool m_OnEnter = false;
    public Collider2D m_Collider;

    public ProximityBoxParameters(bool onEnter, Collider2D collider)
    {
        m_OnEnter = onEnter;
        m_Collider = collider;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.ProximityBox; }
}
#endregion

#region MOVEMENT
public class BlockMovementEventParameters : BaseEventParameters
{
    public EAnimationAttackName m_CurrentAttack;

    public BlockMovementEventParameters(EAnimationAttackName currentAttack)
    {
        m_CurrentAttack = currentAttack;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.BlockMovement; }
}

public class UnblockMovementEventParameters : BlockMovementEventParameters
{
    public UnblockMovementEventParameters(EAnimationAttackName currentAttack) : base(currentAttack)
    {
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.UnblockMovement; }
}
#endregion

#region ROUND
public class EndOfRoundAnimationEventParameters : BaseEventParameters
{
    public EPlayer m_Player;

    public EndOfRoundAnimationEventParameters(EPlayer player)
    {
        m_Player = player;
    }

    public override EPlayerEvent GetEventType() { return EPlayerEvent.EndOfRoundAnimation; }
}
#endregion
