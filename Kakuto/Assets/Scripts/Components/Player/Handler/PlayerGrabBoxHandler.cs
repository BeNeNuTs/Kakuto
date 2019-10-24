using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabBoxHandler : PlayerGizmoBoxColliderDrawer
{
    PlayerAttackComponent m_PlayerAttackComponent;

    private void Awake()
    {
        m_PlayerAttackComponent = GetComponentInParent<PlayerAttackComponent>();
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
        if (collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
        {
            if (collision.gameObject.GetComponent<PlayerGrabHurtBoxHandler>())
            {
                Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.GrabTry, m_PlayerAttackComponent.GetCurrentAttackLogic());
            }
        }
    }
}
