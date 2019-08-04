using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileAttackLogic : PlayerNormalAttackLogic
{
    private static readonly string K_PROJECTILE_HOOK = "ProjectileHook";

    private readonly PlayerProjectileAttackConfig m_Config;

    private Transform m_ProjectileHook;

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
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.ProjectileLaunch, OnProjectileLaunch);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.ProjectileLaunch, OnProjectileLaunch);
    }

    private void OnProjectileLaunch(bool dummyBool)
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
        }
    }
}
