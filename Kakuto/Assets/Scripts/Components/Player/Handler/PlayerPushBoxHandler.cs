using UnityEngine;

public class PlayerPushBoxHandler : PlayerGizmoBoxColliderDrawer
{
    public PlayerAttackComponent m_AttackComponent;

    PlayerBaseAttackLogic m_CurrentAttack;
    Collider2D m_Collider;

    private void Awake()
    {
#if UNITY_EDITOR
        if(m_AttackComponent == null)
        {
            Debug.LogError("Missing AttackComponent in " + this);
        }
#endif

        m_CurrentAttack = null;
        m_Collider = GetComponent<Collider2D>();
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

        m_CurrentAttack = null;
        if(attackLaunchedParams.m_AttackLogic.NeedPushBoxCollisionCallback())
        {
            m_CurrentAttack = attackLaunchedParams.m_AttackLogic;
        }   
    }

    void OnEndOfAttack(BaseEventParameters baseParams)
    {
        EndOfAttackEventParameters endOfAttackEvent = (EndOfAttackEventParameters)baseParams;
        if(m_AttackComponent.CheckIsCurrentAttack(endOfAttackEvent.m_Attack))
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
                if (collision.collider.GetComponent<PlayerPushBoxHandler>())
                {
                    m_CurrentAttack.OnHandlePushBoxCollision(collision);
                }
            }
        }
    }
}
