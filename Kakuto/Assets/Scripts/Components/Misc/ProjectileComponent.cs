using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class ProjectileComponent : MonoBehaviour
{
    private PlayerProjectileAttackLogic m_Logic;
    private PlayerProjectileAttackConfig m_Config;

    private Rigidbody2D m_Rigidbody;
    private uint m_HitCount = 0;
    private float m_LastHitCountTimeStamp = 0f;

    public void OnInit(PlayerProjectileAttackLogic logic, PlayerProjectileAttackConfig config)
    {
        m_Logic = logic;
        m_Config = config;
        gameObject.tag = logic.GetOwner().tag;

        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = transform.right * m_Config.m_ProjectileSpeed;
        m_Rigidbody.MovePosition(transform.position + moveDirection * Time.fixedDeltaTime);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
        {
            if (collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
            {
                if (Time.time > m_LastHitCountTimeStamp + m_Config.m_ProjectileDelayBetweenHits)
                {
                    Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.Hit, m_Logic);
                    m_HitCount++;
                    m_LastHitCountTimeStamp = Time.time;
                    if (m_HitCount >= m_Config.m_ProjectileHitCount)
                    {
                        gameObject.SetActive(false);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
