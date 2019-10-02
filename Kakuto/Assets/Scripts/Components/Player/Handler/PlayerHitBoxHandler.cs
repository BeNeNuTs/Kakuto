using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBoxHandler : PlayerGizmoBoxColliderDrawer
{
    PlayerBaseAttackLogic m_CurrentAttack;
    Collider2D m_Collider;

    private uint m_CurrentHitCount = 0;
    private float m_LastHitCountTimeStamp = 0f;

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
        m_CurrentHitCount = 0;
        m_LastHitCountTimeStamp = 0f;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
        {
            if(collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
            {
                if(m_CurrentAttack != null)
                {
                    if (m_LastHitCountTimeStamp == 0f || Time.time > m_LastHitCountTimeStamp + m_CurrentAttack.GetDelayBetweenHits())
                    {
                        if(m_CurrentHitCount < m_CurrentAttack.GetMaxHitCount())
                        {
                            m_CurrentHitCount++;
                            m_LastHitCountTimeStamp = Time.time;
                            Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.Hit, m_CurrentAttack);
                        }
                        else
                        {
                            m_Collider.enabled = false;
                        }
                    }
                }
            }
        }
    }
}
