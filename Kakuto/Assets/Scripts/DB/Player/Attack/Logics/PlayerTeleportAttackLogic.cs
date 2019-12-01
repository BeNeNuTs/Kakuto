using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeleportAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerTeleportAttackConfig m_Config;

    private ProjectileComponent m_CurrentProjectile;

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
        m_Animator.Play(m_Attack.m_AnimationAttackName.ToString(), 0, 0);
        m_Owner.transform.position = m_CurrentProjectile.transform.position + m_Config.m_TeleportOffset;
        m_CurrentProjectile.DestroyProjectile();
    }

    private void OnProjectileSpawned(ProjectileComponent projectile)
    {
        m_CurrentProjectile = projectile;
    }

    private void OnProjectileDestroyed(ProjectileComponent projectile)
    {
        m_CurrentProjectile = null;
        if (m_CurrentProjectile != null && m_CurrentProjectile != projectile)
        {
            Debug.LogError("Trying to destroy a projectile which is not the current one : Current " + m_CurrentProjectile + " Destroyed : " + projectile);
        }
    }
}
