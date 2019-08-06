﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileAttackLogic : PlayerNormalAttackLogic
{
    private static readonly string K_PROJECTILE_HOOK = "ProjectileHook";

    private readonly PlayerProjectileAttackConfig m_Config;

    private Transform m_ProjectileHook;
    private GameObject m_CurrentProjectile;

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
        Utils.GetPlayerEventManager<GameObject>(m_Owner).StartListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager<GameObject>(m_Owner).StartListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);
    }

    public override void OnShutdown()
    {
        base.OnShutdown();
        Utils.GetPlayerEventManager<GameObject>(m_Owner).StopListening(EPlayerEvent.ProjectileSpawned, OnProjectileSpawned);
        Utils.GetPlayerEventManager<GameObject>(m_Owner).StopListening(EPlayerEvent.ProjectileDestroyed, OnProjectileDestroyed);
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

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.TriggerProjectile, OnTriggerProjectile);
    }

    private void OnProjectileSpawned(GameObject projectile)
    {
        m_CurrentProjectile = projectile;
    }

    private void OnProjectileDestroyed(GameObject projectile)
    {
        m_CurrentProjectile = null;
        if (m_CurrentProjectile != null && m_CurrentProjectile != projectile)
        {
            Debug.LogError("Trying to destroy a projectile which is not the current one : Current " + m_CurrentProjectile + " Destroyed : " + projectile);
        }
    }

    private void OnTriggerProjectile(bool dummyBool)
    {
        GameObject projectile = GameObject.Instantiate(m_Config.m_ProjectilePrefab, m_ProjectileHook.position, Quaternion.AngleAxis(m_Config.m_ProjectileAngle, Vector3.forward));
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
}
