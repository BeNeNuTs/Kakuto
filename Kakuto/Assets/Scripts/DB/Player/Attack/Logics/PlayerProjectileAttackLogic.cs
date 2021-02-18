using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileAttackLogic : PlayerNormalAttackLogic
{
    private static readonly string K_PROJECTILE_HOOK = "ProjectileHook";
    private static readonly bool[] m_NextNonSuperProjectileIsGuardCrush = { false, false };

    private readonly PlayerProjectileAttackConfig m_Config;

    private Transform m_ProjectileHook;
    private List<ProjectileComponent> m_CurrentProjectiles;
    private ProjectileComponent m_MyProjectile;

    private bool m_IsGuardCrush = false;

    public PlayerProjectileAttackLogic(PlayerProjectileAttackConfig config) : base(config)
    {
        m_Config = config;
    }

    public override void OnInit(GameObject owner, PlayerAttack attack)
    {
        base.OnInit(owner, attack);
        m_ProjectileHook = m_Owner.transform.Find("Model/" + K_PROJECTILE_HOOK);
        m_CurrentProjectiles = new List<ProjectileComponent>();
#if UNITY_EDITOR
        if (m_ProjectileHook == null)
        {
            KakutoDebug.LogError(K_PROJECTILE_HOOK + " can't be found on " + m_Owner);
        }
#endif
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);
    }

    public override void OnShutdown()
    {
        base.OnShutdown();
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);

        SetNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex(), false);
    }

    public override bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = base.EvaluateConditions(currentAttackLogic);
        if(conditionIsValid)
        {
            foreach(ProjectileComponent projectile in m_CurrentProjectiles)
            {
                conditionIsValid &= projectile.GetLogic().IsASuper(); // Condition is valid only is there is no other projectiles than super in the scene
            }
        }
        return conditionIsValid;
    }

    public override void OnAttackLaunched()
    {
        m_IsGuardCrush = !IsASuper() && IsNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex()); // Need to be before base.OnAttackLaunched for GetAnimationAttackName()

        base.OnAttackLaunched();
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.TriggerProjectile, OnTriggerProjectile);
    }

    public override string GetAnimationAttackName()
    {
        if(m_Config.m_UseSpecificGuardCrushAnim && m_IsGuardCrush)
        {
            return m_Config.m_AnimationGuardCrushAttackName.ToString();
        }
        else
        {
           return base.GetAnimationAttackName();
        }
    }

    public override bool CanAttackBeBlocked(bool isCrouching)
    {
        bool canAttackBeBlocked = base.CanAttackBeBlocked(isCrouching);
        if (canAttackBeBlocked)
        {
            canAttackBeBlocked &= !IsGuardCrush();
        }
        return canAttackBeBlocked;
    }

    public override bool CanAttackBeParried()
    {
        bool canAttackBeParried = base.CanAttackBeParried();
        if (canAttackBeParried)
        {
            canAttackBeParried &= !IsGuardCrush();
        }
        return canAttackBeParried;
    }

    protected override bool CanStopListeningEnemyTakesDamage()
    {
        return m_MyProjectile == null;
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.TriggerProjectile, OnTriggerProjectile);
    }

    private void OnProjectileSpawned(BaseEventParameters baseParams)
    {
        ProjectileSpawnedEventParameters projectileSpawnedParams = (ProjectileSpawnedEventParameters)baseParams;
        m_CurrentProjectiles.Add(projectileSpawnedParams.m_Projectile);
    }

    private void OnProjectileDestroyed(BaseEventParameters baseParams)
    {
        ProjectileDestroyedEventParameters projectileDestroyedParams = (ProjectileDestroyedEventParameters)baseParams;
        ProjectileComponent destroyedProjectile = projectileDestroyedParams.m_Projectile;

        if (m_MyProjectile == destroyedProjectile)
        {
            Utils.GetEnemyEventManager(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);
            m_MyProjectile = null;
        }

        if (!m_CurrentProjectiles.Contains(destroyedProjectile))
        {
            KakutoDebug.LogError("Trying to destroy a projectile which is not in the list");
        }

        m_CurrentProjectiles.Remove(destroyedProjectile);
    }

    private void OnTriggerProjectile(BaseEventParameters baseParams)
    {
        float ownerLocalScaleX = m_Owner.transform.localScale.x; 
        GameObject projectile = GameObject.Instantiate(m_Config.m_ProjectilePrefab, m_ProjectileHook.position, Quaternion.AngleAxis(m_Config.m_ProjectileAngle, Vector3.forward * ownerLocalScaleX));
        projectile.transform.localScale = new Vector3(ownerLocalScaleX, projectile.transform.localScale.y, projectile.transform.localScale.z);

        ProjectileComponent projectileComponent = projectile.GetComponentInChildren<ProjectileComponent>();
        if(projectileComponent)
        {
            projectileComponent.OnInit(this, m_Config);
            m_MyProjectile = projectileComponent;
        }
        else
        {
            KakutoDebug.LogError("ProjectileComponent could not be found on " + projectile);
            GameObject.Destroy(projectile);
        }

        if (m_IsGuardCrush)
        {
            SetNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex(), false);
            PlayerGuardCrushTriggerAttackLogic.SetTriggerPointStatus(m_InfoComponent, PlayerGuardCrushTriggerAttackLogic.ETriggerPointStatus.Inactive);
        }
    }

    public bool IsGuardCrush()
    {
        return m_IsGuardCrush;
    }

    private bool IsHitTakenInGuardCrush(bool isInBlockingStance, bool isCrouching)
    {
        return isInBlockingStance && base.CanAttackBeBlocked(isCrouching) && IsGuardCrush();
    }

    public override EHitNotificationType GetHitNotificationType(EAttackResult attackResult, bool isInBlockingStance, bool isCrouching, bool isFacingRight, PlayerAttackComponent victimAttackComponent)
    {
        EHitNotificationType hitType = base.GetHitNotificationType(attackResult, isInBlockingStance, isCrouching, isFacingRight, victimAttackComponent);
        if (hitType == EHitNotificationType.None)
        {
            if(IsHitTakenInGuardCrush(isInBlockingStance, isCrouching))
            {
                hitType = EHitNotificationType.GuardCrush;
            }
        }

        return hitType;
    }

    public override void GetHitFX(EAttackResult attackResult, EHitNotificationType hitNotifType, ref List<EHitFXType> hitFXList)
    {
        switch (attackResult)
        {
            case EAttackResult.Hit:
                if (hitNotifType == EHitNotificationType.GuardCrush)
                {
                    hitFXList.Add(EHitFXType.GuardCrush);
                }
                break;
        }

        base.GetHitFX(attackResult, hitNotifType, ref hitFXList);
    }

    public static void SetNextNonSuperProjectileGuardCrush(int playerIndex, bool active)
    {
        m_NextNonSuperProjectileIsGuardCrush[playerIndex] = active;
    }

    public static bool IsNextNonSuperProjectileGuardCrush(int playerIndex)
    {
        return m_NextNonSuperProjectileIsGuardCrush[playerIndex];
    }
}
