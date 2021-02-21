using UnityEngine;

public class PlayerTeleportAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerTeleportAttackConfig m_Config;

    private Rigidbody2D m_Rigidbody;
    private ProjectileComponent m_CurrentProjectile;
    private Vector3 m_LastProjectilePosition;
    private bool m_IsAirProjectile;
    private bool m_TeleportRequested = false;

    public PlayerTeleportAttackLogic(PlayerTeleportAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnInit(GameObject owner, PlayerAttack attack)
    {
        base.OnInit(owner, attack);
        m_Rigidbody = m_Owner.GetComponent<Rigidbody2D>();
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);
    }

    public override void OnShutdown()
    {
        base.OnShutdown();
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);
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
        //If there is a projectile in the scene and this is not a super attack AND destruction has not been requested yet 
        //then the projectile is valid
        bool isAValidProjectile = false;
        if(m_CurrentProjectile != null)
        {
            PlayerProjectileAttackLogic projectileLogic = m_CurrentProjectile.GetLogic();
            isAValidProjectile = !projectileLogic.IsASuper() && !m_CurrentProjectile.HasDestructionBeenRequested();
        }
        return isAValidProjectile;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();

        m_LastProjectilePosition = m_CurrentProjectile.transform.position;
        m_IsAirProjectile = m_CurrentProjectile.GetConfig().m_ProjectileAngle > 0f;
        m_TeleportRequested = false;

        m_CurrentProjectile.RequestProjectileDestruction();
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.TriggerTeleport, OnTriggerTeleportRequested);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.TriggerTeleport, OnTriggerTeleportRequested);
    }

    private void OnTriggerTeleportRequested(BaseEventParameters baseParams)
    {
        Vector3 projectilePosition = m_LastProjectilePosition;
        if(m_CurrentProjectile != null)
        {
            projectilePosition = m_CurrentProjectile.transform.position;
        }
        m_Owner.transform.position = projectilePosition + m_Config.m_TeleportOffset;
        m_Rigidbody.position = m_Owner.transform.position;
        if(m_IsAirProjectile)
        {
            int frame = Time.frameCount;
            Vector2 velocityToApply = m_Config.m_FinalTeleportAirVelocity;
            if (!m_MovementComponent.IsFacingRight())
            {
                velocityToApply.x *= -1f;
            }
            m_Rigidbody.velocity = velocityToApply;
        }
        m_TeleportRequested = true;
    }

    private void OnProjectileSpawned(BaseEventParameters baseParams)
    {
        ProjectileSpawnedEventParameters projectileSpawnedParams = (ProjectileSpawnedEventParameters)baseParams;
        m_CurrentProjectile = projectileSpawnedParams.m_Projectile;
    }

    private void OnProjectileDestroyed(BaseEventParameters baseParams)
    {
        ProjectileDestroyedEventParameters projectileDestroyedParams = (ProjectileDestroyedEventParameters)baseParams;
        ProjectileComponent destroyedProjectile = projectileDestroyedParams.m_Projectile;

        if (m_CurrentProjectile != null && m_CurrentProjectile == destroyedProjectile)
        {
            // If the attack's launched and teleport has not been requested yet
            if (m_AttackLaunched && !m_TeleportRequested)
            {
                // Save the last know projectile position
                m_LastProjectilePosition = m_CurrentProjectile.transform.position;
            }
            m_CurrentProjectile = null;
        }
    }
}
