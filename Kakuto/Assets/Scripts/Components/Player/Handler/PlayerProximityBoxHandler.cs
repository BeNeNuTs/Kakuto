using System.Collections.Generic;
using UnityEngine;

public class PlayerProximityBoxHandler : PlayerGizmoBoxColliderDrawer
{
    public PlayerAttackComponent m_AttackComponent;

    PlayerBaseAttackLogic m_CurrentAttack;
    Collider2D m_Collider;
    List<Collider2D> m_HurtBoxesDetected = new List<Collider2D>();

    private void Awake()
    {
#if UNITY_EDITOR
        if (m_AttackComponent == null)
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

        Utils.GetEnemyEventManager(gameObject).StartListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamages);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.AttackLaunched, OnAttackLaunched);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.EndOfAttack, OnEndOfAttack);

        Utils.GetEnemyEventManager(gameObject).StopListening(EPlayerEvent.DamageTaken, OnEnemyTakesDamages);
    }

    void OnAttackLaunched(BaseEventParameters baseParams)
    {
        AttackLaunchedEventParameters attackLaunchedParams = (AttackLaunchedEventParameters)baseParams;
        m_CurrentAttack = attackLaunchedParams.m_AttackLogic;
    }

    void OnEndOfAttack(BaseEventParameters baseParams)
    {
        if(m_CurrentAttack != null)
        {
            EndOfAttackEventParameters endOfAttackEvent = (EndOfAttackEventParameters)baseParams;
            if (m_AttackComponent.GetCurrentAttackLogic() == null || m_AttackComponent.CheckIsCurrentAttack(endOfAttackEvent.m_Attack))
            {
                m_CurrentAttack = null;
            }
        }
    }

    void OnEnemyTakesDamages(BaseEventParameters baseParams)
    {
        m_CurrentAttack = null;
        if(m_HurtBoxesDetected.Count > 0)
        {
            Utils.GetEnemyEventManager(gameObject).TriggerEvent(EPlayerEvent.ProximityBox, new ProximityBoxParameters(false));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_HurtBoxesDetected.Add(collision);
        if (m_HurtBoxesDetected.Count == 1)
        {
            HandleCollision(collision, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_HurtBoxesDetected.Remove(collision);
        if (m_HurtBoxesDetected.Count == 0)
        {
            HandleCollision(collision, false);
        }
    }

    private void HandleCollision(Collider2D collision, bool onEnter)
    {
        if ((m_CurrentAttack != null && m_Collider.isActiveAndEnabled) || !onEnter)
        {
            if (collision.CompareTag(Utils.GetEnemyTag(gameObject)) && collision.gameObject != gameObject)
            {
#if UNITY_EDITOR || DEBUG_DISPLAY
                if (!collision.gameObject.GetComponent<PlayerHurtBoxHandler>())
                {
                    Debug.LogError("ProximityBox has collided with something else than HurtBox !");
                }
#endif
                Utils.GetEnemyEventManager(gameObject).TriggerEvent(EPlayerEvent.ProximityBox, new ProximityBoxParameters(onEnter));
            }
        }
    }
}
