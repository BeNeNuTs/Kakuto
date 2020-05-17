
using UnityEngine;

public class PlayerDashAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerDashAttackConfig m_Config;

    protected Rigidbody2D m_Rigidbody;
    private float m_OriginalMass = 1f;

    public PlayerDashAttackLogic(PlayerDashAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnInit(GameObject owner, PlayerAttack attack)
    {
        base.OnInit(owner, attack);
        m_Rigidbody = owner.GetComponent<Rigidbody2D>();
        m_OriginalMass = m_Rigidbody.mass;
    }

    protected override string GetAnimationAttackName()
    {
        return m_Attack.m_AnimationAttackName.ToString() + m_Config.m_DashType.ToString();
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();

        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.ApplyDashImpulse, ApplyDashImpulse);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        m_Rigidbody.mass = m_OriginalMass;
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.ApplyDashImpulse, ApplyDashImpulse);
    }

    private void ApplyDashImpulse(bool dummy)
    {
        m_Rigidbody.mass *= m_Config.m_MassMultiplier;

        switch (m_Config.m_DashType)
        {
            case EDashType.Forward:
                m_MovementComponent.PushForward(m_Config.m_Impulse);
                break;
            case EDashType.Backward:
                m_MovementComponent.PushBack(m_Config.m_Impulse);
                break;
            default:
                break;
        }
    }


    public override bool NeedPushBoxCollisionCallback() { return true; }
    public override void OnHandlePushBoxCollision(Collision2D collision)
    {
        base.OnHandlePushBoxCollision(collision);

        // If enemy is in a corner 
        if (GameManager.Instance.GetSubManager<OutOfBoundsSubGameManager>(ESubManager.OutOfBounds).IsInACorner(collision.gameObject))
        {
            // to avoid passing through, stop movement
            Utils.GetPlayerEventManager<bool>(m_Owner).TriggerEvent(EPlayerEvent.StopMovement, true);
        }
    }
}
