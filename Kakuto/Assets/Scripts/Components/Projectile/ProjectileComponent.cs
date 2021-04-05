using UnityEngine;

public enum EProjectileType
{
    Normal,
    GuardCrush,
    Super
}

[RequireComponent(typeof(BoxCollider2D))]
public class ProjectileComponent : PlayerGizmoBoxColliderDrawer
{
    public Collider2D m_Collider;
    public Rigidbody2D m_Rigidbody;
    public SpriteRenderer m_SpriteRenderer;
    public Animator m_Animator;
    public AudioSource m_AudioSource;

    private PlayerProjectileAttackLogic m_Logic;
    private PlayerProjectileAttackConfig m_Config;
    private string m_PlayerTag = "Unknown";
    private ProjectileSFX m_ProjectileSFX;
    private EProjectileType m_ProjectileType;

    private float m_LifeTime = 0f;

    private int m_KeepConstantSpeedUntilFrame = 0;
    private bool m_DestructionRequested = false;

    private static readonly string K_ANIM_ANGLE_INT = "Angle";
    private static readonly string K_ANIM_ISSUPER_BOOL = "IsSuper";
    private static readonly string K_ANIM_ISGUARDCRUSH_BOOL = "IsGuardCrush";
    private static readonly string K_ANIM_DESTRUCTION_REQUESTED_TRIGGER = "DestructionRequested";

    private static readonly string K_GROUND_TAG = "Ground";

    public void OnInit(PlayerProjectileAttackLogic logic, PlayerProjectileAttackConfig config)
    {
        m_Logic = logic;
        m_Config = config;
        m_PlayerTag = logic.GetOwner().tag;
        m_ProjectileSFX = AttackConfig.Instance.m_ProjectileSFX;
        m_ProjectileType = GetProjectileType();

        m_Animator.SetInteger(K_ANIM_ANGLE_INT, Mathf.FloorToInt(m_Config.m_ProjectileAngle));
        m_Animator.SetBool(K_ANIM_ISSUPER_BOOL, m_Logic.IsASuper());
        m_Animator.SetBool(K_ANIM_ISGUARDCRUSH_BOOL, m_Logic.IsGuardCrush());

        InitPalette();
        PlaySFX(EProjectileSFXType.Movement);

        Utils.GetPlayerEventManager(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileSpawned, new ProjectileSpawnedEventParameters(this));

        GamePauseMenuComponent.IsInPauseChanged += IsInPauseChanged;
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

    protected override void Update()
    {
        base.Update();

        if(!m_SpriteRenderer.isVisible)
        {
            DestroyProjectile();
        }

        if (m_KeepConstantSpeedUntilFrame <= Time.frameCount)
        {
            m_LifeTime += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if(!HasDestructionBeenRequested())
        {
            float speed = m_Config.m_ProjectileConstantSpeedAfterHit;
            if (m_KeepConstantSpeedUntilFrame <= Time.frameCount)
            {
                speed = m_Config.m_ProjectileSpeedOverTime.Evaluate(m_LifeTime);
            }

            Vector3 moveDirection = m_Rigidbody.transform.right * m_Rigidbody.transform.localScale.x;
            m_Rigidbody.MovePosition(m_Rigidbody.transform.position + moveDirection * speed * Time.fixedDeltaTime);
        }
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
        if (isActiveAndEnabled && m_Collider.isActiveAndEnabled)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(m_PlayerTag)) && collision.gameObject != gameObject) // Collision with an enemy player
            {
#if UNITY_EDITOR || DEBUG_DISPLAY
                if (!collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
                {
                    KakutoDebug.LogError("Projectile has collided with something else than HurtBox !");
                }
#endif

                if (m_Logic != null)
                {
                    m_Logic.OnHandleCollision(true, true, m_Collider, collision);
                    PlaySFX(EProjectileSFXType.Impact);
                    if (m_Config.m_ApplyConstantSpeedOnPlayerHit)
                    {
                        m_KeepConstantSpeedUntilFrame = Time.frameCount + m_Config.FramesToKeepProjectileAtConstantSpeed;
                    }
                        
                    if (m_Logic.GetCurrentHitCount() >= m_Logic.GetMaxHitCount())
                    {
                        RequestProjectileDestruction();
                    }
                }
            }
            else if (collision.CompareTag(gameObject.tag) && collision.gameObject != gameObject) // Collision with another projectile
            {
                ProjectileComponent collisionProjectile = collision.gameObject.GetComponent<ProjectileComponent>();
                if(collisionProjectile != null && collisionProjectile.GetLogic().GetOwner().CompareTag(Utils.GetEnemyTag(m_PlayerTag))) // Collision with an enemy projectile
                {
                    m_Logic.OnHandleCollision(false, false, m_Collider, collision);
                    PlaySFX(EProjectileSFXType.Impact);
                    if (m_Config.m_ApplyConstantSpeedOnProjectileHit)
                    {
                        m_KeepConstantSpeedUntilFrame = Time.frameCount + m_Config.FramesToKeepProjectileAtConstantSpeed;
                    }

                    if (m_Logic.GetCurrentHitCount() >= m_Logic.GetMaxHitCount())
                    {
                        RequestProjectileDestruction();
                    }
                }
            }
            else if (collision.CompareTag(K_GROUND_TAG)) // Collision with Ground
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
            m_Animator.SetTrigger(K_ANIM_DESTRUCTION_REQUESTED_TRIGGER);
            m_DestructionRequested = true;

            PlaySFX(EProjectileSFXType.Destroy);
        }
    }

    public bool HasDestructionBeenRequested()
    {
        return m_DestructionRequested;
    }

    public void OnEndOfDestructionAnim()
    {
        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        Utils.GetPlayerEventManager(m_PlayerTag).TriggerEvent(EPlayerEvent.ProjectileDestroyed, new ProjectileDestroyedEventParameters(this));
        m_Rigidbody.gameObject.SetActive(false);
        m_Collider.enabled = false;
        Destroy(m_Rigidbody.gameObject);
    }

    private void OnDestroy()
    {
        m_AudioSource.Stop();
        GamePauseMenuComponent.IsInPauseChanged += IsInPauseChanged;
    }

    public PlayerProjectileAttackLogic GetLogic() { return m_Logic; }
    public PlayerProjectileAttackConfig GetConfig() { return m_Config; }
    public string GetPlayerTag() { return m_PlayerTag; }

    private EProjectileType GetProjectileType()
    {
        if(m_Logic.IsASuper())
        {
            return EProjectileType.Super;
        }

        if (m_Logic.IsGuardCrush())
        {
            return EProjectileType.GuardCrush;
        }

        return EProjectileType.Normal;
    }

    private void PlaySFX(EProjectileSFXType projectileSFXType)
    {
        AudioEntry sfxEntry = m_ProjectileSFX.GetSFX(m_ProjectileType, projectileSFXType);
        if(sfxEntry != null)
        {
            m_AudioSource.clip = sfxEntry.m_Clip;
            m_AudioSource.volume = sfxEntry.m_Volume;
            m_AudioSource.loop = projectileSFXType == EProjectileSFXType.Movement;
            m_AudioSource.Play();
        }
    }

    private void IsInPauseChanged(bool isInPause)
    {
        if (isInPause)
            m_AudioSource.Pause();
        else
            m_AudioSource.UnPause();
    }
}
