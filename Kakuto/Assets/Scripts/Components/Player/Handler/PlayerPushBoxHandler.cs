using UnityEngine;

public class PlayerPushBoxHandler : PlayerGizmoBoxColliderDrawer
{
    PlayerBaseAttackLogic m_CurrentAttack;
    Collider2D m_Collider;

    private void Awake()
    {
        m_CurrentAttack = null;
        m_Collider = GetComponent<Collider2D>();
        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StartListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.EndOfAttack, OnEndOfAttack);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StopListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.EndOfAttack, OnEndOfAttack);
    }

    void OnAttackLaunched(PlayerBaseAttackLogic attackLogic)
    {
        m_CurrentAttack = null;
        if(attackLogic.NeedPushBoxCollisionCallback())
        {
            m_CurrentAttack = attackLogic;
        }   
    }

    void OnEndOfAttack(EAnimationAttackName attackName)
    {
        m_CurrentAttack = null;
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
