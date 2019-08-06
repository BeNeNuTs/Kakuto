using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class ProjectileComponent : MonoBehaviour
{
    private static float K_VISIBILITY_CHECK_DELAY = 2.0f;

    private PlayerProjectileAttackLogic m_Logic;
    private PlayerProjectileAttackConfig m_Config;
    private string m_PlayerTag = "Unknown";

    private Camera m_MainCamera;
    private Rigidbody2D m_Rigidbody;
    private SpriteRenderer m_SpriteRenderer;
    private uint m_HitCount = 0;
    private float m_LastHitCountTimeStamp = 0f;
    private float m_LastVisibilityCheckTimeStamp = 0f;

    public void OnInit(PlayerProjectileAttackLogic logic, PlayerProjectileAttackConfig config)
    {
        m_Logic = logic;
        m_Config = config;
        m_PlayerTag = logic.GetOwner().tag;

        m_MainCamera = Camera.main;
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Utils.GetPlayerEventManager<GameObject>(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileSpawned, gameObject);
    }

    void Update()
    {
        if(Time.time > m_LastVisibilityCheckTimeStamp + K_VISIBILITY_CHECK_DELAY)
        {
            bool isVisibleFromCamera = Utils.IsVisibleFrom(m_SpriteRenderer, m_MainCamera);
            if(!isVisibleFromCamera)
            {
                DestroyProjectile();
            }

            m_LastVisibilityCheckTimeStamp = Time.time;
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = transform.right * m_Config.m_ProjectileSpeed;
        m_Rigidbody.MovePosition(transform.position + moveDirection * Time.fixedDeltaTime);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(Utils.GetEnemyTag(m_PlayerTag)) && collision.gameObject != gameObject)
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
                        DestroyProjectile();
                    }
                }
            }
        }
    }

    void DestroyProjectile()
    {
        Utils.GetPlayerEventManager<GameObject>(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileDestroyed, gameObject);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
