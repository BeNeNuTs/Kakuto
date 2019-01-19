using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHandler : MonoBehaviour {

    PlayerAttackComponent m_PlayerAttackComponent;

    private void Awake()
    {
        m_PlayerAttackComponent = GetComponentInParent<PlayerAttackComponent>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
        {
            Utils.GetEnemyEventManager<PlayerAttack>(gameObject).TriggerEvent(EPlayerEvent.Hit, m_PlayerAttackComponent.GetCurrentAttack());
        }
    }
}
