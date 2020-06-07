using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class ProjectileComponent : MonoBehaviour
{
    private PlayerProjectileAttackLogic m_Logic;
    private PlayerProjectileAttackConfig m_Config;
    private string m_PlayerTag = "Unknown";

    Collider2D m_Collider;
    private Rigidbody2D m_Rigidbody;
    private SpriteRenderer m_SpriteRenderer;
    private Animator m_Animator;
    private float m_LifeTime = 0f;

    private bool m_DestructionRequested = false;

    private OutOfBoundsSubGameManager m_OutOfBoundsSubManager;

    public void OnInit(PlayerProjectileAttackLogic logic, PlayerProjectileAttackConfig config)
    {
        m_Logic = logic;
        m_Config = config;
        m_PlayerTag = logic.GetOwner().tag;

        m_Collider = GetComponent<Collider2D>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_Animator = GetComponent<Animator>();
        m_Animator.SetInteger("Angle", Mathf.FloorToInt(m_Config.m_ProjectileAngle));
        m_Animator.SetBool("IsSuper", m_Logic.IsASuper());
        m_Animator.SetBool("IsGuardCrush", m_Logic.IsGuardCrush());

        m_OutOfBoundsSubManager = GameManager.Instance.GetSubManager<OutOfBoundsSubGameManager>(ESubManager.OutOfBounds);

        InitPalette();

        Utils.GetPlayerEventManager<ProjectileComponent>(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileSpawned, this);
    }

    void InitPalette()
    {
        GameObject owner = m_Logic.GetOwner();
        if (owner != null)
        {
            PlayerInfoComponent infoComp = owner.GetComponent<PlayerInfoComponent>();
            if (infoComp != null)
            {
                infoComp.InitWithCurrentPalette(m_SpriteRenderer.material);
            }
        }
    }

    void Update()
    {
        if(!m_SpriteRenderer.isVisible)
        {
            DestroyProjectile();
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
        if(isActiveAndEnabled && m_Collider.isActiveAndEnabled)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(m_PlayerTag)) && collision.gameObject != gameObject) // Collision with an enemy player
            {
                if (collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
                {
                    if (m_Logic != null)
                    {
                        m_Logic.OnHandleCollision(true, m_Collider);
                        if (m_Logic.GetCurrentHitCount() >= m_Logic.GetMaxHitCount())
                        {
                            RequestProjectileDestruction();
                        }
                    }
                }
            }
            else if (collision.CompareTag(gameObject.tag) && collision.gameObject != gameObject) // Collision with another projectile
            {
                ProjectileComponent collisionProjectile = collision.gameObject.GetComponent<ProjectileComponent>();
                if(collisionProjectile != null && collisionProjectile.GetLogic().GetOwner().CompareTag(Utils.GetEnemyTag(m_PlayerTag))) // Collision with an enemy projectile
                {
                    m_Logic.OnHandleCollision(false, m_Collider);
                    if (m_Logic.GetCurrentHitCount() >= m_Logic.GetMaxHitCount())
                    {
                        RequestProjectileDestruction();
                    }
                }
            }
            else if (collision.CompareTag("Ground")) // Collision with Ground
            {
                RequestProjectileDestruction();
            }
        }
    }

    public void RequestProjectileDestruction()
    {
        if (!m_DestructionRequested)
        {
            m_Collider.enabled = false;
            m_Animator.SetTrigger("DestructionRequested");
            m_DestructionRequested = true;
        }
    }

    public void OnEndOfDestructionAnim()
    {
        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        Utils.GetPlayerEventManager<ProjectileComponent>(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileDestroyed, this);
        gameObject.SetActive(false);
        m_Collider.enabled = false;
        Destroy(gameObject);
    }

    public PlayerProjectileAttackLogic GetLogic() { return m_Logic; }
    public PlayerProjectileAttackConfig GetConfig() { return m_Config; }
}
