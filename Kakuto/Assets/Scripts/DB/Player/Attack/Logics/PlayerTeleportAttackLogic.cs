using UnityEngine;

public class PlayerTeleportAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerTeleportAttackConfig m_Config;

    private ProjectileComponent m_CurrentProjectile;
    private Vector3 m_LastProjectilePosition;
    private bool m_TeleportRequested = false;

    public PlayerTeleportAttackLogic(PlayerTeleportAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnInit(GameObject owner, PlayerAttack attack)
    {
        base.OnInit(owner, attack);
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
            conditionIsValid &= ConditionIsValid();
        }

        return conditionIsValid;
    }

    private bool ConditionIsValid()
    {
        switch (m_Config.m_TeleportCondition)
        {
            case PlayerTeleportAttackConfig.ETeleportCondition.LastNonSuperProjectile:
                return IsAValidNonSuperProjectile();
            default:
                return false;
        }
    }

    private bool IsAValidNonSuperProjectile()
    {
        //If there is a projectile in the scene and this is not a super attack, then the projectile is valid
        bool isAValidProjectile = false;
        if(m_CurrentProjectile != null)
        {
            PlayerProjectileAttackLogic projectileLogic = m_CurrentProjectile.GetLogic();
            isAValidProjectile = !projectileLogic.IsASuper();
        }
        return isAValidProjectile;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();

        m_LastProjectilePosition = m_CurrentProjectile.transform.position;
        m_TeleportRequested = false;

        m_CurrentProjectile.RequestProjectileDestruction();
        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.TriggerTeleport, OnTriggerTeleportRequested);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.TriggerTeleport, OnTriggerTeleportRequested);
    }

    private void OnTriggerTeleportRequested(bool dummy)
    {
        Vector3 projectilePosition = m_LastProjectilePosition;
        if(m_CurrentProjectile != null)
        {
            projectilePosition = m_CurrentProjectile.transform.position;
        }
        m_Owner.transform.position = projectilePosition + m_Config.m_TeleportOffset;
        m_TeleportRequested = true;
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

        // If the attack's launched and teleport has not been requested yet
        if(m_AttackLaunched && !m_TeleportRequested)
        {
            // Save the last know projectile position
            m_LastProjectilePosition = m_CurrentProjectile.transform.position;
        }
        m_CurrentProjectile = null;
    }
}
