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
        if(collision.CompareTag("Player") && collision.gameObject != gameObject)
        {
            collision.GetComponent<PlayerHealthComponent>().OnHit(m_PlayerAttackComponent.GetCurrentAttack());
        }
    }
}
