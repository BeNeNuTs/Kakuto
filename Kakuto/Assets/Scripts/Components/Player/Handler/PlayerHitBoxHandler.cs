using UnityEngine;

public class PlayerHitBoxHandler : PlayerGizmoBoxColliderDrawer
{
    public PlayerAttackComponent m_AttackComponent;
    public Collider2D m_Collider;

    PlayerBaseAttackLogic m_CurrentAttack;

    protected override void Awake_Internal()
    {
#if UNITY_EDITOR
        if (m_AttackComponent == null)
        {
            KakutoDebug.LogError("Missing AttackComponent in " + this);
        }
#endif

        m_CurrentAttack = null;
        RegisterListeners();
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

    void OnAttackLaunched(BaseEventParameters baseParams)
    {
        AttackLaunchedEventParameters attackLaunchedParams = (AttackLaunchedEventParameters)baseParams;
        m_CurrentAttack = attackLaunchedParams.m_AttackLogic;
    }

    void OnEndOfAttack(BaseEventParameters baseParams)
    {
        EndOfAttackEventParameters endOfAttackEvent = (EndOfAttackEventParameters)baseParams;
        if (m_AttackComponent.GetCurrentAttackLogic() == null || m_AttackComponent.CheckIsCurrentAttack(endOfAttackEvent.m_Attack))
        {
            m_CurrentAttack = null;
        }
    }

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
        if (m_Collider.isActiveAndEnabled && m_CurrentAttack != null)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
            {
#if UNITY_EDITOR || DEBUG_DISPLAY
                if (!collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
                {
                    KakutoDebug.LogError("HitBox has collided with something else than HurtBox !");
                }
#endif
                m_CurrentAttack.OnHandleCollision(true, true, m_Collider, collision);
            }
        }
    }

    protected override bool CanDrawCollider() { return base.CanDrawCollider() && m_CurrentAttack != null; }
}
