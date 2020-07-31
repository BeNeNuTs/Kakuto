using UnityEngine;

public enum EGrabPhase
{
    Startup,
    Grabbed,
    Missed, 
    Blocked
}

public class PlayerGrabAttackLogic : PlayerBaseAttackLogic
{
    private readonly PlayerGrabAttackConfig m_Config;

    private static readonly string K_GRAB_MISS_ANIM = "GrabMissed";
    private static readonly string K_GRAB_CANCEL_ANIM = "GrabCancelled";
    private static readonly string K_GRAB_BLOCK_ANIM = "BlockGrab";
    private static readonly string K_GRAB_HIT_ANIM = "HitGrab";
    private static readonly string K_GRAB_HOOK = "GrabHook";

    private static readonly int K_MAX_LAST_FRAME_TO_GRAB = 2;

    private bool m_GrabTouchedEnemy = false;
    private int m_LastGrabTouchedFrameCount = 0;
    private Transform m_GrabHook;

    private EGrabPhase m_GrabPhase = EGrabPhase.Startup;

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
        base.OnAttackLaunched();

        m_GrabTouchedEnemy = false;
        m_LastGrabTouchedFrameCount = 0;
        m_GrabPhase = EGrabPhase.Startup;
        IncreaseSuperGauge(m_Config.m_SuperGaugeBaseBonus);

        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StartListening(EPlayerEvent.GrabTouched, OnGrabTouched);
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StartListening(EPlayerEvent.GrabBlocked, OnGrabBlocked);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StartListening(EPlayerEvent.EndOfGrab, OnEndOfGrab);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StartListening(EPlayerEvent.ApplyGrabDamages, OnApplyGrabDamages);
        Utils.GetPlayerEventManager<bool>(m_Owner).StartListening(EPlayerEvent.SyncGrabPosition, OnSyncGrabPosition);
    }

    public override string GetAnimationAttackName()
    {
        return m_Attack.m_AnimationAttackName.ToString() + m_Config.m_GrabType.ToString();
    }

    public override uint GetHitDamage(EAttackResult attackResult) { return (uint)Mathf.Max(m_Config.m_Damage * GetDamageRatio(), 1f); }

    public override float GetStunGaugeHitAmount() { return m_Config.m_StunGaugeHitAmount; }

    public override bool CanAttackBeBlocked(bool isCrouching)
    {
        return !isCrouching;
    }

    public override string GetBlockAnimName(EPlayerStance playerStance, EStunAnimState state)
    {
        string blockAnimName = K_GRAB_BLOCK_ANIM;
        return blockAnimName;
    }

    public override string GetHitAnimName(EPlayerStance playerStance, EStunAnimState state)
    {
        string hitAnimName = K_GRAB_HIT_ANIM;
        hitAnimName += m_Config.m_GrabType.ToString();
        return hitAnimName;
    }

    public EGrabPhase GetGrabPhase()
    {
        return m_GrabPhase;
    }

    public override void OnAttackStopped()
    {
        base.OnAttackStopped();

        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StopListening(EPlayerEvent.GrabTouched, OnGrabTouched);
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(m_Owner).StopListening(EPlayerEvent.GrabBlocked, OnGrabBlocked);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StopListening(EPlayerEvent.EndOfGrab, OnEndOfGrab);
        Utils.GetPlayerEventManager<EAnimationAttackName>(m_Owner).StopListening(EPlayerEvent.ApplyGrabDamages, OnApplyGrabDamages);
        Utils.GetPlayerEventManager<bool>(m_Owner).StopListening(EPlayerEvent.SyncGrabPosition, OnSyncGrabPosition);

        m_GrabTouchedEnemy = false;
        m_LastGrabTouchedFrameCount = 0;
        Utils.IgnorePushBoxLayerCollision(false);
    }

    void OnGrabTouched(PlayerBaseAttackLogic attackLogic)
    {
        if(this == attackLogic && m_GrabPhase == EGrabPhase.Startup)
        {
            ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "On grab touched");

            m_GrabTouchedEnemy = true;
            m_LastGrabTouchedFrameCount = Time.frameCount;
        }
    }

    void OnGrabBlocked(PlayerBaseAttackLogic attackLogic)
    {
        if (this == attackLogic && m_GrabPhase == EGrabPhase.Startup)
        {
            ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "On grab blocked");

            m_GrabPhase = EGrabPhase.Blocked;
            IncreaseSuperGauge(m_Config.m_SuperGaugeBlockBonus);
            m_Animator.Play(K_GRAB_CANCEL_ANIM, 0, 0);
        }
    }

    void OnEndOfGrab(EAnimationAttackName attackName)
    {
        ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "End of grab phase");

        if (m_Attack.m_AnimationAttackName == attackName && m_GrabPhase == EGrabPhase.Startup)
        {
            ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "Grab touched enemy : " + m_GrabTouchedEnemy + ", Last grab touched frame count : " + m_LastGrabTouchedFrameCount);

            if (!m_GrabTouchedEnemy || (Time.frameCount - m_LastGrabTouchedFrameCount) > K_MAX_LAST_FRAME_TO_GRAB)
            {
                ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "On grab missed");

                m_GrabPhase = EGrabPhase.Missed;
                m_Animator.Play(K_GRAB_MISS_ANIM, 0, 0);
            }
            else
            {
                ChronicleManager.AddChronicle(m_Owner, EChronicleCategory.Attack, "On grab enemy");

                m_GrabPhase = EGrabPhase.Grabbed;

                Utils.IgnorePushBoxLayerCollision();
                OnSyncGrabPosition();

                //Launch grabbed event
                Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(m_Owner).TriggerEvent(EPlayerEvent.Grabbed, this);
            }
        }
    }

    void OnApplyGrabDamages(EAnimationAttackName attackName)
    {
        if (m_Attack.m_AnimationAttackName == attackName && m_GrabPhase == EGrabPhase.Grabbed)
        {
            IncreaseSuperGauge(m_Config.m_SuperGaugeHitBonus);

            //Launch hit event
            Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(m_Owner).TriggerEvent(EPlayerEvent.Hit, this);
        }
    }

    void OnSyncGrabPosition(bool syncGrab = true)
    {
        // If GrabHook is out of bounds
        float grabHookDistanceOutOfBorder = GameManager.Instance.GetSubManager<OutOfBoundsSubGameManager>(ESubManager.OutOfBounds).GetDistanceOutOfBorder(m_GrabHook.position);
        if (grabHookDistanceOutOfBorder < 0f || grabHookDistanceOutOfBorder > 0f)
        {
            //We need to move back the attacker in order to sync the grab attack position
            Vector3 ownerPosition = m_Owner.transform.root.position;
            ownerPosition.x -= grabHookDistanceOutOfBorder;
            m_Owner.transform.root.position = ownerPosition;
        }

        // Send event to the enemy
        Utils.GetEnemyEventManager<Transform>(m_Owner).TriggerEvent(EPlayerEvent.SyncGrabPosition, m_GrabHook);
    }
}
