using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabBoxHandler : PlayerGizmoBoxColliderDrawer
{
    public PlayerAttackComponent m_PlayerAttackComponent;
    public Collider2D m_Collider;

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
        if (m_Collider.isActiveAndEnabled)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
            {
#if UNITY_EDITOR || DEBUG_DISPLAY
                if (!collision.gameObject.GetComponent<PlayerGrabHurtBoxHandler>())
                {
                    KakutoDebug.LogError("GrabBox has collided with something else than GrabHurtBox !");
                }
#endif
                Utils.GetEnemyEventManager(gameObject).TriggerEvent(EPlayerEvent.GrabTry, new GrabTryEventParameters(m_PlayerAttackComponent.GetCurrentAttackLogic()));
            }
        }
    }
}
