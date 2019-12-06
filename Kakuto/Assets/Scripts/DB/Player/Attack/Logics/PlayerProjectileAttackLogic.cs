using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileAttackLogic : PlayerNormalAttackLogic
{
    private static readonly string K_PROJECTILE_HOOK = "ProjectileHook";
    private static bool[] m_NextNonSuperProjectileIsGuardCrush = { false, false };

    private readonly PlayerProjectileAttackConfig m_Config;

    private Transform m_ProjectileHook;
    private ProjectileComponent m_CurrentProjectile;

    public PlayerProjectileAttackLogic(PlayerProjectileAttackConfig config) : base(config)
    {
        m_Config = config;
    }

    public override void OnInit(GameObject owner, PlayerAttack attack)
    {
        base.OnInit(owner, attack);
        m_ProjectileHook = m_Owner.transform.Find("Model/" + K_PROJECTILE_HOOK);
#if UNITY_EDITOR
        if (m_ProjectileHook == null)
        {
            Debug.LogError(K_PROJECTILE_HOOK + " can't be found on " + m_Owner);
        }
#endif
        Utils.GetPlayerEventManager<ProjectileComponent>(m_Owner).StartListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager<ProjectileComponent>(m_Owner).StartListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);
    }

    public override void OnShutdown()
    {
        base.OnShutdown();
        Utils.GetPlayerEventManager<ProjectileComponent>(m_Owner).StopListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager<ProjectileComponent>(m_Owner).StopListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);
    }

    public override bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = base.EvaluateConditions(currentAttackLogic);
        if(conditionIsValid)
        {
            conditionIsValid &= m_CurrentProjectile == null; // Condition is valid only is there is no current projectile in the scene
        }
        return conditionIsValid;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.TriggerProjectile, OnTriggerProjectile);
    }

    public override bool CanBlockAttack(bool isCrouching)
    {
        bool canBlockAttack = base.CanBlockAttack(isCrouching);
        if (canBlockAttack)
        {
            canBlockAttack &= IsASuper() || !IsNextNonSuperProjectileGuardCrush(m_MovementComponent.GetPlayerIndex());
        }
        return canBlockAttack;
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.TriggerProjectile, OnTriggerProjectile);
    }

    private void OnProjectileSpawned(ProjectileComponent projectile)
    {
        m_CurrentProjectile = projectile;
    }

    private void OnProjectileDestroyed(ProjectileComponent projectile)
    {
        if (m_CurrentProjectile != null && m_CurrentProjectile != projectile)
        {
            Debug.LogError("Trying to destroy a projectile which is not the current one : Current " + m_CurrentProjectile + " Destroyed : " + projectile);
        }

        if(!IsASuper())
        {
            SetNextNonSuperProjectileGuardCrush(m_MovementComponent.GetPlayerIndex(), false);
        }
        m_CurrentProjectile = null;
    }

    private void OnTriggerProjectile(bool dummyBool)
    {
        float ownerLocalScaleX = m_Owner.transform.localScale.x; 
        GameObject projectile = GameObject.Instantiate(m_Config.m_ProjectilePrefab, m_ProjectileHook.position, Quaternion.AngleAxis(m_Config.m_ProjectileAngle, Vector3.forward * ownerLocalScaleX));
        projectile.transform.localScale = new Vector3(ownerLocalScaleX, projectile.transform.localScale.y, projectile.transform.localScale.z);

        ProjectileComponent projectileComponent = projectile.GetComponent<ProjectileComponent>();
        if(projectileComponent)
        {
            projectileComponent.OnInit(this, m_Config);
        }
        else
        {
            Debug.LogError("ProjectileComponent could not be found on " + projectile);
            GameObject.Destroy(projectile);
        }
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
