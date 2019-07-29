using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBoxHandler : MonoBehaviour {

    PlayerAttackComponent m_PlayerAttackComponent;

    private void Awake()
    {
        m_PlayerAttackComponent = GetComponentInParent<PlayerAttackComponent>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
        {
            if(collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
            {
                Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.Hit, m_PlayerAttackComponent.GetCurrentAttackLogic());
            }
        }
    }
}
