﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBoxHandler : PlayerGizmoBoxColliderDrawer
{
    PlayerBaseAttackLogic m_CurrentAttack;
    Collider2D m_Collider;

    private void Awake()
    {
        m_CurrentAttack = null;
        m_Collider = GetComponent<Collider2D>();
        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.EndOfAttack, OnEndOfAttack);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.EndOfAttack, OnEndOfAttack);
    }

    void OnAttackLaunched(BaseEventParameters baseParams)
    {
        AttackLaunchedEventParameters attackLaunchedParams = (AttackLaunchedEventParameters)baseParams;
        m_CurrentAttack = attackLaunchedParams.m_AttackLogic;
    }

    void OnEndOfAttack(BaseEventParameters baseParams)
    {
        m_CurrentAttack = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collider2D collision)
    {
        if (m_Collider.isActiveAndEnabled && m_CurrentAttack != null)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
            {
                if (collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
                {
                    m_CurrentAttack.OnHandleCollision(true, true, m_Collider, collision);
                    if (m_CurrentAttack.GetCurrentHitCount() >= m_CurrentAttack.GetMaxHitCount())
                    {
                        m_Collider.enabled = false;
                    }
                }
            }
        }
    }
}
