using System.Collections;
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
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StartListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StopListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
    }

    void OnAttackLaunched(PlayerBaseAttackLogic attackLogic)
    {
        m_CurrentAttack = attackLogic;
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
        if(m_Collider.isActiveAndEnabled)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
            {
                if (collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
                {
                    if (m_CurrentAttack != null)
                    {
                        m_CurrentAttack.OnHandleCollision(true);
                        if (m_CurrentAttack.GetCurrentHitCount() >= m_CurrentAttack.GetMaxHitCount())
                        {
                            m_Collider.enabled = false;
                        }
                    }
                }
            }
        }
    }
}
