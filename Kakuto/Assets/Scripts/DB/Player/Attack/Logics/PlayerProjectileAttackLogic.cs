using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileAttackLogic : PlayerNormalAttackLogic
{
    private static readonly string K_PROJECTILE_HOOK = "ProjectileHook";
    private static bool[] m_NextNonSuperProjectileIsGuardCrush = { false, false };

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
        base.OnAttackLaunched();
        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.TriggerProjectile, OnTriggerProjectile);

        m_IsGuardCrush = !IsASuper() && IsNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex());
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
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.TriggerProjectile, OnTriggerProjectile);
    }

    private void OnProjectileSpawned(ProjectileComponent projectile)
    {
        m_CurrentProjectiles.Add(projectile);
    }

    private void OnProjectileDestroyed(ProjectileComponent projectile)
    {
        if(m_MyProjectile == projectile)
        {
            Utils.GetEnemyEventManager<DamageTakenInfo>(m_Owner).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamage);
            m_MyProjectile = null;
        }

        if (!m_CurrentProjectiles.Contains(projectile))
        {
            Debug.LogError("Trying to destroy a projectile which is not in the list");
        }

        m_CurrentProjectiles.Remove(projectile);
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
            m_MyProjectile = projectileComponent;
        }
        else
        {
            Debug.LogError("ProjectileComponent could not be found on " + projectile);
            GameObject.Destroy(projectile);
        }

        if (m_IsGuardCrush)
        {
            SetNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex(), false);
        }
    }

    public bool IsGuardCrush()
    {
        return m_IsGuardCrush;
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
