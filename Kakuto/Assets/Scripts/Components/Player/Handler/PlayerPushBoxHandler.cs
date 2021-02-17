using UnityEngine;

public class PlayerPushBoxHandler : PlayerGizmoBoxColliderDrawer
{
    public PlayerHealthComponent m_HealthComponent;
    public CharacterController2D m_Controller;
    public PlayerAttackComponent m_AttackComponent;

    public Rigidbody2D m_Rigidbody;

    OutOfBoundsSubGameManager m_OOBSubManager;
    PlayerStunInfoSubComponent m_StunInfoSC;
    PlayerBaseAttackLogic m_CurrentAttack;
    Collider2D m_Collider;

    Collider2D[] m_FallingHits = new Collider2D[2];

    protected override void Awake_Internal()
    {
#if UNITY_EDITOR
        if (m_HealthComponent == null)
        {
            KakutoDebug.LogError("Missing HealthComponent in " + this);
        }
        if (m_Controller == null)
        {
            KakutoDebug.LogError("Missing CharacterController2D in " + this);
        }
        if (m_AttackComponent == null)
        {
            KakutoDebug.LogError("Missing AttackComponent in " + this);
        }
        if (m_Rigidbody == null)
        {
            KakutoDebug.LogError("Missing Rigidbody in " + this);
        }
#endif

        m_CurrentAttack = null;
        m_Collider = GetComponent<Collider2D>();
        RegisterListeners();
    }

    private void Start()
    {
        m_StunInfoSC = m_HealthComponent.GetStunInfoSubComponent();
        m_OOBSubManager = GameManager.Instance.GetSubManager<OutOfBoundsSubGameManager>(ESubManager.OutOfBounds);
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.EndOfAttack, OnEndOfAttack);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.EndOfAttack, OnEndOfAttack);
    }

    private void FixedUpdate()
    {
        if(m_Collider.enabled)
        {
            if (m_Controller.IsJumping() && !m_StunInfoSC.IsStunned())
            {
                if (m_Controller.IsJumpApexReached())
                {
                    UpdateFallingTrajectory();
                }
            }
        }
    }

    void UpdateFallingTrajectory()
    {
        float circleRadius = m_Collider.bounds.extents.x;
        int hitCount = Physics2D.OverlapCircleNonAlloc(m_Collider.bounds.center + ((m_Collider.bounds.extents.y + circleRadius) * Vector3.down), circleRadius, m_FallingHits, 1 << gameObject.layer);
        for(int i = 0; i < hitCount; i++)
        {
            if(m_FallingHits[i].attachedRigidbody != m_Collider.attachedRigidbody)
            {
                Vector3 trajectoryDir = (m_Collider.bounds.center.x < m_FallingHits[i].bounds.center.x) ? Vector3.left : Vector3.right;
                m_Rigidbody.velocity = new Vector2(0f, m_Rigidbody.velocity.y);
                Vector2 targetPosition = AdjustPosition(m_FallingHits[i].bounds.center.x, trajectoryDir, m_FallingHits[i]);

                PlayerPushBoxHandler pushBoxhandler = m_FallingHits[i].GetComponent<PlayerPushBoxHandler>();
                if(pushBoxhandler != null)
                {
                    pushBoxhandler.OnEnemyFallingOnMe(m_Collider, targetPosition);
                }
                break;
            }
        }
    }

    public void OnEnemyFallingOnMe(Collider2D other, Vector3 otherTargetPosition)
    {
        if (!m_Controller.IsJumping() && !m_StunInfoSC.IsStunned())
        {
            if(m_OOBSubManager.IsInACorner(otherTargetPosition, out float leftOffset, out float rightOffset, out bool leftBorder))
            {
                // If enemy's trying to go behind me
                if (leftBorder)
                {
                    if (otherTargetPosition.x < m_Controller.transform.position.x)
                    {
                        AdjustPosition(leftOffset, Vector3.right, other);
                    }
                }
                else
                {
                    if (otherTargetPosition.x > m_Controller.transform.position.x)
                    {
                        AdjustPosition(rightOffset, Vector3.left, other);
                    }
                }
            }
        }
    }

    private Vector2 AdjustPosition(float initialXPosition, Vector3 trajectoryDir, Collider2D other)
    {
        float newXPos = initialXPosition + (other.bounds.extents.x * trajectoryDir.x) + (m_Collider.bounds.extents.x * trajectoryDir.x);
        Vector2 targetPosition = new Vector2(newXPos, m_Rigidbody.position.y);

        Vector2 dummyVelocity = Vector2.zero;
        m_Rigidbody.position = Vector2.SmoothDamp(m_Rigidbody.position, targetPosition, ref dummyVelocity, MovementConfig.Instance.m_FallingTrajectorySmoothing);

        return targetPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(MovementConfig.Instance.m_DEBUG_DisplayFallingRaycast)
        {
            if (m_Collider != null && m_Collider.enabled)
            {
                if (m_Controller.IsJumping() && !m_StunInfoSC.IsStunned())
                {
                    if (m_Controller.IsJumpApexReached())
                    {
                        Gizmos.DrawWireSphere(m_Collider.bounds.center + ((m_Collider.bounds.extents.y + m_Collider.bounds.extents.x) * Vector3.down), m_Collider.bounds.extents.x);
                    }
                }
            }
        }
    }
#endif

    void OnAttackLaunched(BaseEventParameters baseParams)
    {
        AttackLaunchedEventParameters attackLaunchedParams = (AttackLaunchedEventParameters)baseParams;

        m_CurrentAttack = null;
        if(attackLaunchedParams.m_AttackLogic.NeedPushBoxCollisionCallback())
        {
            m_CurrentAttack = attackLaunchedParams.m_AttackLogic;
        }   
    }

    void OnEndOfAttack(BaseEventParameters baseParams)
    {
        EndOfAttackEventParameters endOfAttackEvent = (EndOfAttackEventParameters)baseParams;
        if (m_AttackComponent.GetCurrentAttackLogic() == null || m_AttackComponent.CheckIsCurrentAttack(endOfAttackEvent.m_Attack))
        {
            m_CurrentAttack = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision2D collision)
    {
        if (m_Collider.isActiveAndEnabled && m_CurrentAttack != null)
        {
            if (collision.collider.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
            {

#if UNITY_EDITOR || DEBUG_DISPLAY
                if (!collision.collider.GetComponent<PlayerPushBoxHandler>())
                {
                    KakutoDebug.LogError("PushBox has collided with something else than PushBox !");
                }
#endif
                m_CurrentAttack.OnHandlePushBoxCollision(collision);
            }
        }
    }
}
