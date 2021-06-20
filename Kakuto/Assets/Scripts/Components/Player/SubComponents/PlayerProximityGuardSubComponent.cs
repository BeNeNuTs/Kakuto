using System.Collections.Generic;
using UnityEngine;

public class PlayerProximityGuardSubComponent : PlayerBaseSubComponent
{
    private static readonly string K_ANIM_ONPROXIMITYEND_TRIGGER = "OnProximityEnd";
    private static readonly string K_ANIM_PROXIMITY = "Proximity";
    private static readonly string K_ANIM_IN = "_In";

    private readonly PlayerHealthComponent m_PlayerHealthComponent;
    private readonly PlayerStunInfoSubComponent m_PlayerStunInfoSubComponent;
    private readonly PlayerMovementComponent m_PlayerMovementComponent;
    private readonly Animator m_Animator;

    private bool m_IsInsideProximityBox;
    private bool m_IsInProximityGuard;

    private List<Collider2D> m_ProximityBoxes = new List<Collider2D>();

    public PlayerProximityGuardSubComponent(PlayerHealthComponent healthComponent, PlayerMovementComponent movementComponent, Animator anim) : base(healthComponent.gameObject)
    {
        m_PlayerHealthComponent = healthComponent;
        m_PlayerStunInfoSubComponent = m_PlayerHealthComponent.GetStunInfoSubComponent();
        m_PlayerMovementComponent = movementComponent;
        m_Animator = anim;
        Utils.GetPlayerEventManager(m_Owner).StartListening(EPlayerEvent.ProximityBox, OnProximityBoxEvent);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Utils.GetPlayerEventManager(m_Owner).StopListening(EPlayerEvent.ProximityBox, OnProximityBoxEvent);
    }

    public void Update()
    {
        if(m_IsInsideProximityBox)
        {
            UpdateProximityGuard();
        }
    }

    private void OnProximityBoxEvent(BaseEventParameters baseEventParameters)
    {
        ProximityBoxParameters proximityBoxParameters = (ProximityBoxParameters)baseEventParameters;
        if(proximityBoxParameters.m_OnEnter)
        {
            m_ProximityBoxes.Add(proximityBoxParameters.m_Collider);
        }
        else
        {
            m_ProximityBoxes.Remove(proximityBoxParameters.m_Collider);
        }
        m_IsInsideProximityBox = proximityBoxParameters.m_OnEnter || m_ProximityBoxes.Count > 0;
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Proximity, "Is inside proximity box: " + m_IsInsideProximityBox);
        UpdateProximityGuard();
    }

    private void UpdateProximityGuard()
    {
        bool wasInProximityGuard = m_IsInProximityGuard;
        m_IsInProximityGuard = CheckProximityGuardConditions();

        if(wasInProximityGuard != m_IsInProximityGuard)
        {
            ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Proximity, (m_IsInProximityGuard) ? "Trigger" : "End" + " proximity guard");
            if (m_IsInProximityGuard)
            {
                TriggerProximityGuard();
            }
            else
            {
                EndProximityGuard();
            }
        }
    }

    private bool CheckProximityGuardConditions()
    {
        return m_IsInsideProximityBox && !m_PlayerStunInfoSubComponent.IsStunned() && m_PlayerHealthComponent.IsInBlockingStance();
    }

    public bool IsInProximityGuard()
    {
        return m_IsInProximityGuard;
    }

    private void TriggerProximityGuard()
    {
        m_Animator.ResetTrigger(K_ANIM_ONPROXIMITYEND_TRIGGER);
        m_Animator.Play(K_ANIM_PROXIMITY + m_PlayerMovementComponent.GetCurrentStance().ToString() + K_ANIM_IN, 0, 0);
        Utils.GetPlayerEventManager(m_Owner).TriggerEvent(EPlayerEvent.StopMovement);
    }

    private void EndProximityGuard()
    {
        m_Animator.SetTrigger(K_ANIM_ONPROXIMITYEND_TRIGGER);
    }
}
