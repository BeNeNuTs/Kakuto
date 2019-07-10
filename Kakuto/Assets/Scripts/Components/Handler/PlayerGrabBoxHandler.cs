using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabBoxHandler : MonoBehaviour {

    PlayerAttackComponent m_PlayerAttackComponent;

    private void Awake()
    {
        m_PlayerAttackComponent = GetComponentInParent<PlayerAttackComponent>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
        {
            if (collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
            {
                Utils.GetPlayerEventManager<PlayerAttack>(gameObject).TriggerEvent(EPlayerEvent.GrabHit, m_PlayerAttackComponent.GetCurrentAttack());
                Utils.GetEnemyEventManager<PlayerAttack>(gameObject).TriggerEvent(EPlayerEvent.Grab, m_PlayerAttackComponent.GetCurrentAttack());
            }
        }
    }
}
