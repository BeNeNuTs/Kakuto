﻿using System.Collections.Generic;
using UnityEngine;

public class ProjectileProximityBoxHandler : PlayerGizmoBoxColliderDrawer
{
    public ProjectileComponent m_ProjectileComponent;
    public Collider2D m_Collider;

    List<Collider2D> m_HurtBoxesDetected = new List<Collider2D>();
    bool m_EnemyTakesDamages = false;

    void Start()
    {
        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetEnemyEventManager(m_ProjectileComponent.GetPlayerTag()).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamages);
    }

    void OnDestroy()
    {
        UnregisterListeners();

        if(m_HurtBoxesDetected.Count > 0)
        {
            m_HurtBoxesDetected.Clear();
            Utils.GetEnemyEventManager(m_ProjectileComponent.GetPlayerTag()).TriggerEvent(EPlayerEvent.ProximityBox, new ProximityBoxParameters(false, m_Collider));
        }
    }

    void UnregisterListeners()
    {
        Utils.GetEnemyEventManager(m_ProjectileComponent.GetPlayerTag()).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamages);
    }

    void OnEnemyTakesDamages(BaseEventParameters baseParams)
    {
        if (m_HurtBoxesDetected.Count > 0)
        {
            DamageTakenEventParameters damageTakenEventParameters = (DamageTakenEventParameters)baseParams;
            if (damageTakenEventParameters.m_AttackLogic == m_ProjectileComponent.GetLogic())
            {
                Utils.GetEnemyEventManager(m_ProjectileComponent.GetPlayerTag()).TriggerEvent(EPlayerEvent.ProximityBox, new ProximityBoxParameters(false, m_Collider));
                m_EnemyTakesDamages = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_HurtBoxesDetected.Add(collision);
        if (m_HurtBoxesDetected.Count == 1)
        {
            HandleCollision(collision, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_HurtBoxesDetected.Remove(collision);
        if (m_HurtBoxesDetected.Count == 0)
        {
            HandleCollision(collision, false);
        }
    }

    private void HandleCollision(Collider2D collision, bool onEnter)
    {
        if ((!m_EnemyTakesDamages && m_Collider.isActiveAndEnabled) || !onEnter)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(m_ProjectileComponent.GetPlayerTag())) && collision.gameObject != gameObject)
            {
#if UNITY_EDITOR || DEBUG_DISPLAY
                if (!collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
                {
                    KakutoDebug.LogError("ProximityBox has collided with something else than HurtBox !");
                }
#endif
                Utils.GetEnemyEventManager(m_ProjectileComponent.GetPlayerTag()).TriggerEvent(EPlayerEvent.ProximityBox, new ProximityBoxParameters(onEnter, m_Collider));
            }
        }
    }

    protected override bool CanDrawCollider() { return base.CanDrawCollider() && !m_EnemyTakesDamages; }
}
