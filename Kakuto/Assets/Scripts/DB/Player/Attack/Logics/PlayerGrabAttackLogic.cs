using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerGrabAttackConfig m_Config;

    private static readonly string K_GRAB_MISS_ANIM = "GrabMissed";
    private static readonly string K_GRAB_CANCEL_ANIM = "GrabCancelled";

    private bool m_GrabHitEnemy = false;

    public PlayerGrabAttackLogic(PlayerGrabAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        Utils.GetPlayerEventManager<PlayerAttack>(m_Owner).StartListening(EPlayerEvent.GrabHit, OnGrabHit);
        Utils.GetPlayerEventManager<PlayerAttack>(m_Owner).StartListening(EPlayerEvent.GrabBlocked, OnGrabBlocked);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StartListening(EPlayerEvent.EndOfGrab, OnEndOfGrab);
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();
        Utils.GetPlayerEventManager<PlayerAttack>(m_Owner).StopListening(EPlayerEvent.GrabHit, OnGrabHit);
        Utils.GetPlayerEventManager<PlayerAttack>(m_Owner).StopListening(EPlayerEvent.GrabBlocked, OnGrabBlocked);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StopListening(EPlayerEvent.EndOfGrab, OnEndOfGrab);

        m_GrabHitEnemy = false;
    }

    void OnGrabHit(PlayerAttack attack)
    {
        if(m_Attack == attack)
        {
            m_GrabHitEnemy = true;
        }
    }

    void OnGrabBlocked(PlayerAttack attack)
    {
        if (m_Attack == attack)
        {
            m_Animator.Play(K_GRAB_CANCEL_ANIM);
        }
    }

    void OnEndOfGrab(EAnimationAttackName attackName)
    {
        if (m_Attack.m_AnimationAttackName == attackName)
        {
            if(!m_GrabHitEnemy)
            {
                m_Animator.Play(K_GRAB_MISS_ANIM);
            }
        }
    }
}
