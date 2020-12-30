using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public enum EAttackResult
{
    Hit,
    Blocked,
    Parried
}

public enum EHitNotificationType
{
    None,
    Low,
    Overhead,
    GuardCrush,
    Crossup,
    Counter
}

public class PlayerHealthComponent : MonoBehaviour
{
    public PlayerHealthConfig m_HealthConfig;
    public GameObject m_DamageTakenUIPrefab;
    public Transform m_DamageTakenParent;

    private uint m_HP;

    private PlayerAttackComponent m_AttackComponent;
    private PlayerMovementComponent m_MovementComponent;
    private PlayerInfoComponent m_InfoComponent;
    private Animator m_Anim;

    private PlayerStunInfoSubComponent m_StunInfoSC;
    private PlayerProximityGuardSubComponent m_ProximityGuardSubComponent;
    private TimeScaleSubGameManager m_TimeScaleManager;

    private IEnumerator m_CurrentHitStopCoroutine = null;

    private List<EHitFXType> m_HitFXTypeList = new List<EHitFXType>();

#if UNITY_EDITOR || DEBUG_DISPLAY
    [Separator("Debug")]
    [Space]

    public bool m_DEBUG_BreakOnHit = false;
    public bool m_DEBUG_BreakOnGrabbed = false;
#endif

    private void Awake()
    {
        m_HP = m_HealthConfig.m_MaxHP;
        m_AttackComponent = GetComponent<PlayerAttackComponent>();
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_InfoComponent = GetComponent<PlayerInfoComponent>();

        m_Anim = GetComponentInChildren<Animator>();

        m_StunInfoSC = new PlayerStunInfoSubComponent(m_InfoComponent, m_MovementComponent, m_Anim);
        m_ProximityGuardSubComponent = new PlayerProximityGuardSubComponent(this, m_MovementComponent, m_Anim);

        m_TimeScaleManager = GameManager.Instance.GetSubManager<TimeScaleSubGameManager>(ESubManager.TimeScale);

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.Hit, OnHit);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.GrabTry, OnGrabTry);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.SyncGrabbedPosition, OnSyncGrabbedPosition);
        Utils.GetPlayerEventManager(gameObject).StartListening(EPlayerEvent.Grabbed, OnGrabbed);

        RoundSubGameManager.OnRoundOver += OnRoundOver;
    }

    void OnDestroy()
    {
        UnregisterListeners();
        m_StunInfoSC.OnDestroy();
        m_ProximityGuardSubComponent.OnDestroy();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.Hit, OnHit);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.GrabTry, OnGrabTry);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.SyncGrabbedPosition, OnSyncGrabbedPosition);
        Utils.GetPlayerEventManager(gameObject).StopListening(EPlayerEvent.Grabbed, OnGrabbed);

        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void Update()
    {
        if(IsDead())
        {
            return;
        }

        m_StunInfoSC.Update();
        m_ProximityGuardSubComponent.Update();
    }

    public bool IsDead()
    {
        return m_HP == 0;
    }

    void OnGrabTry(BaseEventParameters baseParams)
    {
        if (IsDead())
        {
            return;
        }

        GrabTryEventParameters grabTryParams = (GrabTryEventParameters)baseParams;
        PlayerBaseAttackLogic grabAttackLogic = grabTryParams.m_AttackLogic;

        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "On grab try");

        if (CanBlockGrabAttack(grabAttackLogic))
        {
            // Here, both players are currently playing grab attack
            // But one is the attacker, and the second the defender
            // Only the defender can trigger GrabBlocked event and start a block animation
            // If the player who's trying to grab is the first one to have triggered the grab attack, he's the attacker, so we can block it
            if (IsGrabAttacker(grabAttackLogic))
            {
                ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "On grab blocked");

                Utils.GetEnemyEventManager(gameObject).TriggerEvent(EPlayerEvent.GrabBlocked, new GrabBlockedEventParameters(grabAttackLogic));
                m_StunInfoSC.StartStun(grabAttackLogic, EAttackResult.Blocked);
                PlayBlockAnimation(grabAttackLogic);
            }
        }
        else if(!m_StunInfoSC.IsHitStunned() && !m_StunInfoSC.IsBlockStunned() && !m_MovementComponent.IsJumping()) // A grab can't touch if player is stunned or is jumping
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "On grab touched");

            Utils.GetEnemyEventManager(gameObject).TriggerEvent(EPlayerEvent.GrabTouched, new GrabTouchedEventParameters(grabAttackLogic));
        }
    }

    private bool CanBlockGrabAttack(PlayerBaseAttackLogic attackLogic)
    {
        // Can't blocked grab attack when stunned
        if (!m_StunInfoSC.IsStunned())
        {
            if (m_AttackComponent)
            {
                // Check if we are playing grab attack as well
                if (m_AttackComponent.GetCurrentAttackLogic() is PlayerGrabAttackLogic grabAttack)
                {
                    return (grabAttack.GetGrabPhase() == EGrabPhase.Startup);
                }
            }
        }

        return false;
    }

    private bool IsGrabAttacker(PlayerBaseAttackLogic grabLogic)
    {
        bool isGrabAttacker = false;
        GameObject grabInstigator = grabLogic.GetOwner();
        Animator grabInstigatorAnim = grabLogic.GetAnimator();
        float currentGrabInstigatorFrame = Utils.GetCurrentAnimFrame(grabInstigatorAnim);
        float currentGrabOwnerFrame = Utils.GetCurrentAnimFrame(m_Anim);

        // If grab instigator has been triggered at exact same frame than owner
        if(currentGrabInstigatorFrame == currentGrabOwnerFrame)
        {
            isGrabAttacker = grabInstigator.CompareTag(Player.Player1);
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "Is " + grabInstigator.tag + " GrabAttacker | Grab attack has been triggered at same frame on both players. Player1 defined as attacker by default");
        }
        else
        {
            isGrabAttacker = currentGrabInstigatorFrame > currentGrabOwnerFrame;
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "Is " + grabInstigator.tag + " GrabAttacker | " + ((isGrabAttacker) ? grabInstigator.tag : gameObject.tag) + " is grab attacker");
        }
        return isGrabAttacker;
    }

    void OnSyncGrabbedPosition(BaseEventParameters baseParams)
    {
        if (IsDead())
        {
            return;
        }

        SyncGrabbedPositionEventParameters syncGrabbedParams = (SyncGrabbedPositionEventParameters)baseParams;

        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "On sync grab position");
        transform.position = syncGrabbedParams.m_GrabHook.position;
    }

    void OnGrabbed(BaseEventParameters baseParams)
    {
        if (IsDead())
        {
            return;
        }

#if UNITY_EDITOR || DEBUG_DISPLAY
        if (m_DEBUG_BreakOnGrabbed)
        {
            Debug.Break();
        }
#endif
        GrabbedEventParameters grabbedParams = (GrabbedEventParameters)baseParams;
        PlayerBaseAttackLogic grabAttackLogic = grabbedParams.m_AttackLogic;

        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "On grabbed by : " + grabAttackLogic.GetAttack().m_Name);
        m_StunInfoSC.StartStun(grabAttackLogic, EAttackResult.Hit);
        PlayHitAnimation(grabAttackLogic);
    }

    void OnHit(BaseEventParameters baseParams)
    {
        Profiler.BeginSample("PlayerHealthComponent.OnHit");
        if (IsDead())
        {
            return;
        }

#if UNITY_EDITOR || DEBUG_DISPLAY
        if(m_DEBUG_BreakOnHit)
        {
            Debug.Break();
        }
#endif
        HitEventParameters hitParams = (HitEventParameters)baseParams;
        PlayerBaseAttackLogic hitAttackLogic = hitParams.m_AttackLogic;

        GetHitInfo(hitAttackLogic, out uint hitDamage, out EAttackResult attackResult, out EHitNotificationType hitNotificationType);
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "On hit by : " + hitAttackLogic.GetAttack().m_Name + ", damage : " + hitDamage + ", result : " + attackResult + ", hitNotif : " + hitNotificationType);

        ApplyDamage(hitAttackLogic, hitDamage, attackResult, hitNotificationType);
        Profiler.EndSample();
    }

    private void GetHitInfo(PlayerBaseAttackLogic attackLogic, out uint hitDamage, out EAttackResult attackResult, out EHitNotificationType hitNotificationType)
    {
        Profiler.BeginSample("PlayerHealthComponent.GetHitInfo");
        hitNotificationType = EHitNotificationType.None;

        if (CanParryAttack(attackLogic))
        {
            attackResult = EAttackResult.Parried;
            hitDamage = 0;
        }
        else if(CanBlockAttack(attackLogic))
        {
            attackResult = EAttackResult.Blocked;
            hitDamage = attackLogic.GetHitDamage(attackResult);
        }
        else
        {
            attackResult = EAttackResult.Hit;
            hitDamage = attackLogic.GetHitDamage(attackResult);
            hitNotificationType = attackLogic.GetHitNotificationType(attackResult, IsInBlockingStance(), m_MovementComponent.IsCrouching(), m_MovementComponent.IsFacingRight(), m_AttackComponent);
        }
        Profiler.EndSample();
    }

    private bool CanParryAttack(PlayerBaseAttackLogic attackLogic)
    {
        if(m_AttackComponent)
        {
            // Check if we are playing parry attack
            if(m_AttackComponent.GetCurrentAttackLogic() is PlayerParryAttackLogic parryAttackLogic && parryAttackLogic.CanParryAttack(attackLogic))
            {
                return attackLogic.CanAttackBeParried();
            }
        }

        return false;
    }

    private bool CanBlockAttack(PlayerBaseAttackLogic attackLogic)
    {
        bool canBlockAttack = IsInBlockingStance();
        if(canBlockAttack && m_MovementComponent)
        {
            //Check if the player is in the right stance for this attack
            bool isCrouching = m_MovementComponent.IsCrouching();
            canBlockAttack &= attackLogic.CanAttackBeBlocked(isCrouching);
        }

        return canBlockAttack;
    }

    public bool IsInBlockingStance()
    {
        if (m_InfoComponent.GetPlayerSettings().m_IsBlockingAllAttacks)
        {
            return true;
        }

        if (m_StunInfoSC.IsBlockStunned())
        {
            return true;
        }

        bool canBlockAttack = true;
        if (m_AttackComponent)
        {
            // Check if we are not attacking
            canBlockAttack &= (m_AttackComponent.GetCurrentAttack() == null);
        }

        if (m_MovementComponent)
        {
            // If he's moving back and not jumping
            canBlockAttack &= (m_MovementComponent.IsMovingBack() && m_MovementComponent.IsJumping() == false);
        }

        return canBlockAttack;
    }

    private void ApplyDamage(PlayerBaseAttackLogic attackLogic, uint damage, EAttackResult attackResult, EHitNotificationType hitNotificationType)
    {
        if (damage >= m_HP)
        {
            m_HP = (uint)(m_InfoComponent.GetPlayerSettings().m_IsInvincible ? 1 : 0);
        }
        else
        {
            m_HP -= damage;
        }

        OnDamageTaken(attackLogic, damage, attackResult, hitNotificationType);
    }

    private void OnDamageTaken(PlayerBaseAttackLogic attackLogic, uint damage, EAttackResult attackResult, EHitNotificationType hitNotificationType)
    {
        Profiler.BeginSample("PlayerHealthComponent.OnDamageTaken");

#if DEBUG_DISPLAY || UNITY_EDITOR
        Debug.Log("Player : " + gameObject.name + " HP : " + m_HP + " damage taken : " + damage + " attack " + attackResult.ToString());
#endif
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "On damage taken : " + damage + ", current HP : " + m_HP);

        DamageTakenEventParameters damageTakenInfo = new DamageTakenEventParameters(gameObject, attackLogic, attackResult, m_StunInfoSC.IsHitStunned(), (float)m_HP / (float)m_HealthConfig.m_MaxHP, hitNotificationType);
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.DamageTaken, damageTakenInfo);

        if (!IsDead() && attackLogic.CanPlayDamageTakenAnim())
        {
            PlayDamageTakenAnim(attackLogic, attackResult);
        }
            
        TriggerEffects(attackLogic, damage, attackResult, hitNotificationType);

        if (damage > 0 && m_InfoComponent.GetPlayerSettings().m_DisplayDamageTaken)
        {
            DisplayDamageTakenUI(damage);
        }

        if (IsDead())
        {
            OnDeath(attackLogic);
        }
        Profiler.EndSample();
    }

    private void TriggerEffects(PlayerBaseAttackLogic attackLogic, uint damage, EAttackResult attackResult, EHitNotificationType hitNotificationType)
    {
        Profiler.BeginSample("PlayerHealthComponent.TriggerEffects");
        PlayerAttack attack = attackLogic.GetAttack();
        bool isDead = IsDead();

        // No stun neither pushback when an attack is parried
        if (attackResult != EAttackResult.Parried)
        {
            if (attackLogic.CanStunOnDamage())
            {
                m_StunInfoSC.StartStun(attackLogic, attackResult);
            }

            // If stun duration is not anim driven, we can set the duration and apply a pushback
            if (!m_StunInfoSC.IsStunDurationAnimDriven())
            {
                if (attackLogic.CanStunOnDamage())
                {
                    float stunDuration = attackLogic.GetStunDuration(attackResult);
                    if (stunDuration > 0f)
                    {
                        m_StunInfoSC.SetStunDuration(attackLogic, stunDuration);
                    }
                }

                if (attackLogic.CanPushBack())
                {
                    float pushBackForce = attackLogic.GetPushBackForce(attackResult);
                    if (pushBackForce > 0.0f && m_MovementComponent)
                    {
                        if (isDead)
                        {
                            pushBackForce *= AttackConfig.Instance.m_OnDeathPushbackMultiplier;
                        }
                        m_MovementComponent.PushBack(pushBackForce);
                    }
                }
            }
        }

        PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
        if(superGaugeSC != null)
        {
            superGaugeSC.IncreaseGaugeValue(AttackConfig.Instance.m_DefenderSuperGaugeBonus);
        }

        if(attackResult == EAttackResult.Hit)
        {
            m_StunInfoSC.IncreaseGaugeValue(attackLogic.GetStunGaugeHitAmount());
        }

        TimeScaleParams timeScaleParams = null;
        if (isDead)
        {
            timeScaleParams = AttackConfig.Instance.m_OnDeathTimeScaleParams;
        }
        else if(attack.m_UseTimeScaleEffect)
        {
            timeScaleParams = attack.m_TimeScaleParams;
        }

        if(timeScaleParams != null)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Health, "StartTimeScale - Amount: " + timeScaleParams.m_TimeScaleAmount + " Duration: " + timeScaleParams.m_TimeScaleDuration);
            m_TimeScaleManager.StartTimeScale(timeScaleParams);
            if (timeScaleParams.m_TimeScaleAmount == 0f)
            {
                TriggerHitStopShake(timeScaleParams.m_TimeScaleDuration);
            }
        }

        if (!attackLogic.GetLastHitPoint(out Vector3 hitPoint))
        {
            hitPoint = attackLogic.GetOwner().transform.position;
        }

        if (attackResult != EAttackResult.Blocked)
        {
            if (attack.m_UseCameraShakeEffect || isDead)
            {
                Vector3 hitDirection = (transform.position - attackLogic.GetOwner().transform.position).normalized;
                CameraShakeParams camShakeParamsToUse = isDead ? AttackConfig.Instance.m_OnDeathCamShakeParams : attack.m_CameraShakeParams;
                CameraShakeManager.GenerateImpulseAt(camShakeParamsToUse, hitPoint, hitDirection);
            }
        }

        TriggerHitFX(attackLogic, hitPoint, attackResult, hitNotificationType);

        Profiler.EndSample();
    }

    private void TriggerHitFX(PlayerBaseAttackLogic attackLogic, Vector3 hitPoint, EAttackResult attackResult, EHitNotificationType hitNotificationType)
    {
        Profiler.BeginSample("PlayerHealthComponent.TriggerHitFX");

        m_HitFXTypeList.Clear();
        attackLogic.GetHitFX(attackResult, hitNotificationType, ref m_HitFXTypeList);
        if (m_HitFXTypeList.Count > 0)
        {
            bool flipHitFX;
            Collider2D lastHitCollider = attackLogic.GetLastHitCollider();
            if (lastHitCollider != null)
            {
                Transform hitOwner = lastHitCollider.transform.root;
                flipHitFX = hitOwner.position.x < transform.position.x;
            }
            else
            {
                flipHitFX = attackLogic.GetOwner().transform.position.x < transform.position.x;
            }

            FXSubGameManager fxSubGameManager = GameManager.Instance.GetSubManager<FXSubGameManager>(ESubManager.FX);
            int playerIndex = m_InfoComponent.GetPlayerIndex();
            for (int i = 0; i < m_HitFXTypeList.Count; i++)
            {
                fxSubGameManager.SpawnHitFX(playerIndex, m_HitFXTypeList[i], hitPoint, Quaternion.identity, flipHitFX);
            }
        }

        Profiler.EndSample();
    }

    private void TriggerHitStopShake(float duration)
    {
        if(m_CurrentHitStopCoroutine != null)
        {
            StopCoroutine(m_CurrentHitStopCoroutine);
        }

        m_CurrentHitStopCoroutine = HitStopShake_Coroutine(duration);
        StartCoroutine(m_CurrentHitStopCoroutine);
    }

    private IEnumerator HitStopShake_Coroutine(float duration)
    {
        float startingTime = Time.unscaledTime;
        Vector3 startingPos = transform.root.position;
        Vector3 offsetPos = Vector3.zero;

        float shakeSpeed = AttackConfig.Instance.m_HitStopShakeSpeed;
        float shakeAmount = AttackConfig.Instance.m_HitStopShakeAmount;

        while (Time.unscaledTime < startingTime + duration)
        {
            offsetPos.x = Mathf.Sin(Time.unscaledTime * shakeSpeed) < 0.0f ? -shakeAmount : shakeAmount;
            transform.root.position = startingPos + offsetPos;
            yield return null;
        }
    }

    private void PlayDamageTakenAnim(PlayerBaseAttackLogic attackLogic, EAttackResult attackResult)
    {
        switch (attackResult)
        {
            case EAttackResult.Hit:
                PlayHitAnimation(attackLogic);
                break;
            case EAttackResult.Blocked:
                PlayBlockAnimation(attackLogic);
                break;
            case EAttackResult.Parried:
                PlayParriedAnimation(attackLogic);
                break;
            default:
                break;
        }
    }

    private void PlayHitAnimation(PlayerBaseAttackLogic attackLogic)
    {
        //Play hit anim
        string hitAnimName = attackLogic.GetHitAnimName(m_MovementComponent.GetCurrentStance(), EStunAnimState.In);
        m_Anim.Play(hitAnimName, 0, 0);
    }

    private void PlayBlockAnimation(PlayerBaseAttackLogic attackLogic)
    {
        //Play block anim
        string blockAnimName = attackLogic.GetBlockAnimName(m_MovementComponent.GetCurrentStance(), EStunAnimState.In);
        m_Anim.Play(blockAnimName, 0, 0);
    }

    private void PlayParriedAnimation(PlayerBaseAttackLogic attackLogic)
    {
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.ParrySuccess);
    }

    private void OnDeath(PlayerBaseAttackLogic attackLogic)
    {
        m_Anim.SetBool("IsDead", true);

        // Only play death animation if we're not killed by grab animation
        // For grab attacks, let the anim end correctly
        if(!(attackLogic is PlayerGrabAttackLogic))
        {
            m_Anim.Play("Death", 0, 0);
        }

        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.OnDeath, new DeathEventParameters(gameObject.tag));
    }

    private void OnRoundOver(RoundSubGameManager.ELastRoundWinner lastRoundWinner)
    {
        UnregisterListeners();
        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    public float GetHPPercentage()
    {
        return (float)m_HP / (float)m_HealthConfig.m_MaxHP;
    }

    public PlayerStunInfoSubComponent GetStunInfoSubComponent()
    {
        return m_StunInfoSC;
    }

    public PlayerProximityGuardSubComponent GetProximityGuardSubComponent()
    {
        return m_ProximityGuardSubComponent;
    }

    private void DisplayDamageTakenUI(uint damage)
    {
        //DamageTakenUIInstance will be automatically destroyed
        GameObject damageTakenUI = Instantiate(m_DamageTakenUIPrefab, Vector3.zero, Quaternion.identity);
        damageTakenUI.transform.SetParent(m_DamageTakenParent);
        damageTakenUI.transform.localPosition = Vector3.zero;
        damageTakenUI.GetComponentInChildren<Text>().text = "-" + damage.ToString();
    }
}
