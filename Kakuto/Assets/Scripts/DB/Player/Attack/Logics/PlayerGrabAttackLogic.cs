using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GrabbedInfo
{
    PlayerBaseAttackLogic m_AttackLogic;
    Transform m_GrabHook;

    public GrabbedInfo(PlayerBaseAttackLogic attackLogic, Transform grabHook)
    {
        m_AttackLogic = attackLogic;
        m_GrabHook = grabHook;
    }

    public PlayerBaseAttackLogic GetAttackLogic() { return m_AttackLogic; }
    public Vector3 GetGrabHookPosition() { return m_GrabHook.position; }
}

public class PlayerGrabAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerGrabAttackConfig m_Config;

    private static readonly string K_GRAB_MISS_ANIM = "GrabMissed";
    private static readonly string K_GRAB_CANCEL_ANIM = "GrabCancelled";
    private static readonly string K_GRAB_BLOCK_ANIM = "BlockGrab";
    private static readonly string K_GRAB_HIT_ANIM = "HitGrab";
    private static readonly string K_GRAB_HOOK = "GrabHook";

    private bool m_GrabTouchedEnemy = false;
    private Transform m_GrabHook;

    public PlayerGrabAttackLogic(PlayerGrabAttackConfig config)
    {
        m_Config = config;
    }

    public override void OnInit(GameObject owner, PlayerAttack attack)
    {
        base.OnInit(owner, attack);
        m_GrabHook = m_Owner.transform.Find("Model/" + K_GRAB_HOOK);
#if UNITY_EDITOR
        if (m_GrabHook == null)
        {
            Debug.LogError(K_GRAB_HOOK + " can't be found on " + m_Owner);
        }
#endif
    }

    public override void OnAttackLaunched()
    {
        m_Animator.Play(m_Attack.m_AnimationAttackName.ToString() + m_Config.m_GrabType.ToString());

        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StartListening(EPlayerEvent.GrabTouched, OnGrabTouched);
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StartListening(EPlayerEvent.GrabBlocked, OnGrabBlocked);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StartListening(EPlayerEvent.EndOfGrab, OnEndOfGrab);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StartListening(EPlayerEvent.ApplyGrabDamages, OnApplyGrabDamages);
    }

    public override bool CanBlockAttack(bool isCrouching)
    {
        return !isCrouching;
    }

    public override string GetBlockAnimName(EPlayerStance playerStance)
    {
        string blockAnimName = K_GRAB_BLOCK_ANIM;
        return blockAnimName;
    }

    public override string GetHitAnimName(EPlayerStance playerStance)
    {
        string hitAnimName = K_GRAB_HIT_ANIM;
        hitAnimName += m_Config.m_GrabType.ToString();
        return hitAnimName;
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StopListening(EPlayerEvent.GrabTouched, OnGrabTouched);
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StopListening(EPlayerEvent.GrabBlocked, OnGrabBlocked);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StopListening(EPlayerEvent.EndOfGrab, OnEndOfGrab);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StopListening(EPlayerEvent.ApplyGrabDamages, OnApplyGrabDamages);

        m_GrabTouchedEnemy = false;
        Utils.IgnorePushBoxLayerCollision(false);
    }

    void OnGrabTouched(PlayerBaseAttackLogic attackLogic)
    {
        if(this == attackLogic)
        {
            m_GrabTouchedEnemy = true;
        }
    }

    void OnGrabBlocked(PlayerBaseAttackLogic attackLogic)
    {
        if (this == attackLogic)
        {
            m_Animator.Play(K_GRAB_CANCEL_ANIM);
        }
    }

    void OnEndOfGrab(EAnimationAttackName attackName)
    {
        if (m_Attack.m_AnimationAttackName == attackName)
        {
            if(!m_GrabTouchedEnemy)
            {
                m_Animator.Play(K_GRAB_MISS_ANIM);
            }
            else
            {
                //Launch grabbed event
                GrabbedInfo grabbedInfo = new GrabbedInfo(this, m_GrabHook);
                Utils.GetEnemyEventManager<GrabbedInfo>(m_Owner).TriggerEvent(EPlayerEvent.Grabbed, grabbedInfo);
                Utils.IgnorePushBoxLayerCollision();
            }
        }
    }

    void OnApplyGrabDamages(EAnimationAttackName attackName)
    {
        if (m_Attack.m_AnimationAttackName == attackName)
        {
            if (m_GrabTouchedEnemy)
            {
                //Launch hit event
                Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(m_Owner).TriggerEvent(EPlayerEvent.Hit, this);
            }
            else
            {
                Debug.LogError("Can't apply grab damages without grabbing him.");
            }
        }
    }
}
