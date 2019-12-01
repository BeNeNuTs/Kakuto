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

    private Rigidbody2D m_Rigidbody;
    private SpriteRenderer m_SpriteRenderer;
    private uint m_CurrentHitCount = 0;
    private float m_LastHitCountTimeStamp = 0f;
    private float m_LastVisibilityCheckTimeStamp = 0f;
    private float m_LifeTime = 0f;

    private OutOfBoundsSubGameManager m_OutOfBoundsSubManager;

    public void OnInit(PlayerProjectileAttackLogic logic, PlayerProjectileAttackConfig config)
    {
        m_Logic = logic;
        m_Config = config;
        m_PlayerTag = logic.GetOwner().tag;

        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        m_OutOfBoundsSubManager = GameManager.Instance.GetSubManager<OutOfBoundsSubGameManager>(ESubManager.OutOfBounds);

        Utils.GetPlayerEventManager<ProjectileComponent>(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileSpawned, this);
    }

    void Update()
    {
        if(Time.time > m_LastVisibilityCheckTimeStamp + K_VISIBILITY_CHECK_DELAY)
        {
            if(!m_OutOfBoundsSubManager.IsVisibleFromMainCamera(m_SpriteRenderer))
            {
                DestroyProjectile();
            }

            m_LastVisibilityCheckTimeStamp = Time.time;
        }

        m_LifeTime += Time.deltaTime;
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = transform.right * transform.localScale.x;
        m_Rigidbody.MovePosition(transform.position + moveDirection * m_Config.m_ProjectileSpeedOverTime.Evaluate(m_LifeTime) * Time.fixedDeltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    void HandleCollision(Collider2D collision)
    {
        if (collision.CompareTag(Utils.GetEnemyTag(m_PlayerTag)) && collision.gameObject != gameObject) // Collision with an enemy player
        {
            if (collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
            {
                if (m_LastHitCountTimeStamp == 0f || Time.time > m_LastHitCountTimeStamp + m_Config.m_DelayBetweenHits)
                {
                    if (m_CurrentHitCount < m_Config.m_MaxHitCount)
                    {
                        m_CurrentHitCount++;
                        m_LastHitCountTimeStamp = Time.time;
                        Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(m_PlayerTag).TriggerEvent(EPlayerEvent.Hit, m_Logic);
                    }

                    if (m_CurrentHitCount >= m_Config.m_MaxHitCount)
                    {
                        DestroyProjectile();
                    }
                }
            }
        }
        else if (collision.CompareTag(gameObject.tag) && collision.gameObject != gameObject) // Collision with another projectile
        {
            DestroyProjectile();
        }
        else if (collision.CompareTag("Ground")) // Collision with Ground
        {
            DestroyProjectile();
        }
    }

    public void DestroyProjectile()
    {
        Utils.GetPlayerEventManager<ProjectileComponent>(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileDestroyed, this);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public PlayerProjectileAttackLogic GetLogic() { return m_Logic; }
    public PlayerProjectileAttackConfig GetConfig() { return m_Config; }
}
