using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthComponent : MonoBehaviour
{
    struct StunInfo
    {
        public bool m_IsStunned;
        public bool m_StunnedWhileJumping;
        public bool m_StunnedByHitKO;
        public bool m_IsHitStun;
        public float m_StunTimer;
    }

    public PlayerHealthConfig m_HealthConfig;

    private uint m_HP;
    private PlayerAttackComponent m_AttackComponent;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    private StunInfo m_StunInfo;

    [Header("Debug")]
    [Space]

    public bool m_DEBUG_DisplayDamageTaken = false;
    [ConditionalField(true, "m_DEBUG_DisplayDamageTaken")]
    public GameObject m_DEBUG_DamageTakenUIPrefab;
    [ConditionalField(true, "m_DEBUG_DisplayDamageTaken")]
    public Transform m_DEBUG_DamageTakenParent;

    [ConditionalField(false, "m_DEBUG_IsBlockingAllAttacksAfterHitStun")]
    public bool m_DEBUG_IsBlockingAllAttacks = false;
    [ConditionalField(false, "m_DEBUG_IsBlockingAllAttacks")]
    public bool m_DEBUG_IsBlockingAllAttacksAfterHitStun = false;
    [ConditionalField(true, "m_DEBUG_IsBlockingAllAttacksAfterHitStun")]
    public float m_DEBUG_BlockingAttacksDuration = 1.0f;
    private float m_DEBUG_BlockingAttacksTimer = 0.0f;

    public bool m_DEBUG_IsInvincible = false;

    private void Awake()
    {
        m_HP = m_HealthConfig.m_MaxHP;
        m_AttackComponent = GetComponent<PlayerAttackComponent>();
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_Anim = GetComponentInChildren<Animator>();

        RegisterListeners();
        GameManager.Instance.RegisterPlayer(gameObject);
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StartListening(EPlayerEvent.Hit, OnHit);
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StartListening(EPlayerEvent.GrabTry, OnGrabTry);
        Utils.GetPlayerEventManager<GrabbedInfo>(gameObject).StartListening(EPlayerEvent.Grabbed, OnGrabbed);
        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.GrabStun, OnGrabStun);

        RoundSubGameManager.OnRoundOver += OnRoundOver;
    }

    void OnDestroy()
    {
        UnregisterListeners();

        GameManager gameMgr = GameManager.Instance;
        if (gameMgr)
        {
            gameMgr.UnregisterPlayer(gameObject);
        }
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StopListening(EPlayerEvent.Hit, OnHit);
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StopListening(EPlayerEvent.GrabTry, OnGrabTry);
        Utils.GetPlayerEventManager<GrabbedInfo>(gameObject).StopListening(EPlayerEvent.Grabbed, OnGrabbed);
        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.GrabStun, OnGrabStun);

        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void Update()
    {
        if(IsDead())
        {
            return;
        }

        UpdateStun();
    }

    void UpdateStun()
    {
        if (IsStunned())
        {
            if (Time.unscaledTime > m_StunInfo.m_StunTimer)
            {
                StopStun();
            }
        }

        // DEBUG /////////////////////////////////////
        if (m_DEBUG_IsBlockingAllAttacksAfterHitStun)
        {
            if (m_DEBUG_IsBlockingAllAttacks && m_DEBUG_BlockingAttacksTimer > 0.0f)
            {
                if (Time.unscaledTime > m_DEBUG_BlockingAttacksTimer)
                {
                    DEBUG_StopBlockingAttacks();
                }
            }
        }
        //////////////////////////////////////////////
    }

    public bool IsDead()
    {
        return m_HP == 0;
    }

    void OnGrabTry(PlayerBaseAttackLogic attackLogic)
    {
        if (IsDead())
        {
            return;
        }

        if(CanBlockGrabAttack(attackLogic))
        {
            Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.GrabBlocked, attackLogic);
            PlayBlockAnimation(attackLogic);
        }
        else if(!IsStunned() && !m_MovementComponent.IsJumping()) // A grab can't touch if player is stunned or is jumping
        {
            Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.GrabTouched, attackLogic);
        }
    }

    private bool CanBlockGrabAttack(PlayerBaseAttackLogic attackLogic)
    {
        // Can't blocked grab attack when stunned
        if (!IsStunned())
        {
            if (m_AttackComponent)
            {
                // Check if we are playing grab attack as well
                PlayerAttack attack = m_AttackComponent.GetCurrentAttack();
                if(attack != null)
                {
                    return (attack.m_AnimationAttackName == EAnimationAttackName.Grab);
                }   
            }
        }

        return false;
    }

    void OnGrabbed(GrabbedInfo grabbedInfo)
    {
        if (IsDead())
        {
            return;
        }

        transform.position = grabbedInfo.GetGrabHookPosition();
        PlayHitAnimation(grabbedInfo.GetAttackLogic());
    }

    void OnHit(PlayerBaseAttackLogic attackLogic)
    {
        if (IsDead())
        {
            return;
        }

        GetHitInfo(attackLogic, out uint hitDamage, out bool isAttackBlocked);
        ApplyDamage(attackLogic, hitDamage, isAttackBlocked);
    }

    private void GetHitInfo(PlayerBaseAttackLogic attackLogic, out uint hitDamage, out bool isAttackBlocked)
    {
        isAttackBlocked = CanBlockAttack(attackLogic);
        hitDamage = attackLogic.GetHitDamage(isAttackBlocked);
    }

    private bool CanBlockAttack(PlayerBaseAttackLogic attackLogic)
    {
        // DEBUG ///////////////////////////////////
        if (m_DEBUG_IsBlockingAllAttacks)
        {
            return true;
        }
        ////////////////////////////////////////////

        bool canBlockAttack = true;
        canBlockAttack &= !IsStunned(); // Can't blocked attack when stunned
        if (m_AttackComponent)
        {
            // Check if we are not attacking
            canBlockAttack &= (m_AttackComponent.GetCurrentAttack() == null);
        }

        if(m_MovementComponent)
        {
            // If he's moving back and not jumping
            canBlockAttack &= (m_MovementComponent.IsMovingBack() && m_MovementComponent.IsJumping() == false);
            
            //Check if the player is in the right stance 
            bool isCrouching = m_MovementComponent.IsCrouching();
            canBlockAttack &= attackLogic.CanBlockAttack(isCrouching);
        }

        return canBlockAttack;
    }

    private void ApplyDamage(PlayerBaseAttackLogic attackLogic, uint damage, bool isAttackBlocked)
    {
        if (damage >= m_HP)
        {
            m_HP = (uint)(m_DEBUG_IsInvincible ? 1 : 0);
        }
        else
        {
            m_HP -= damage;
        }

        OnDamageTaken(attackLogic, damage, isAttackBlocked);
    }

    private void OnDamageTaken(PlayerBaseAttackLogic attackLogic, uint damage, bool isAttackBlocked)
    {
        Debug.Log("Player : " + gameObject.name + " HP : " + m_HP + " damage taken : " + damage + " attack blocked : " + isAttackBlocked);
        Utils.GetPlayerEventManager<float>(gameObject).TriggerEvent(EPlayerEvent.DamageTaken, (float)m_HP / (float)m_HealthConfig.m_MaxHP);

        if (IsDead())
        {
            OnDeath();
        }
        else
        {
            TriggerEffects(attackLogic, damage, isAttackBlocked);
            if (attackLogic.CanPlayDamageTakenAnim())
            {
                PlayDamageTakenAnim(attackLogic, isAttackBlocked);
            }
        }

        // DEBUG /////////////////////////////////////
        if (damage > 0 && m_DEBUG_DisplayDamageTaken)
        {
            DEBUG_DisplayDamageTakenUI(damage);
        }
        /////////////////////////////////////////////
    }

    private void TriggerEffects(PlayerBaseAttackLogic attackLogic, uint damage, bool isAttackBlocked)
    {
        PlayerAttack attack = attackLogic.GetAttack();
        if(attackLogic.CanStun())
        {
            float stunDuration = attackLogic.GetStunDuration(isAttackBlocked);
            if (stunDuration > 0)
            {
                StartStun(stunDuration, attackLogic.IsHitKO(), isAttackBlocked);
            }
        }

        if(attack.m_UseTimeScaleEffect)
        {
            TimeManager.StartTimeScale(attack.m_TimeScaleAmount, attack.m_TimeScaleDuration, attack.m_TimeScaleBackToNormal);
        }

        if (!isAttackBlocked)
        {
            if (attack.m_UseCameraShakeEffect)
            {
                CameraShakeManager.Shake(attack.m_CameraShakeAmount, attack.m_CameraShakeDuration);
            }
        }

        if (attackLogic.CanPushBack())
        {
            float pushBackForce = attackLogic.GetPushBackForce(isAttackBlocked);
            if (pushBackForce > 0.0f && m_MovementComponent)
            {
                m_MovementComponent.PushBack(pushBackForce);
            }
        }
    }

    private void OnGrabStun(float stunDuration)
    {
        StartStun(stunDuration, false, false);
    }

    private void StartStun(float stunDuration, bool isHitKO, bool isAttackBlocked)
    {
        m_StunInfo.m_StunTimer = Time.unscaledTime + stunDuration;
        if(!IsStunned())
        {
            m_StunInfo.m_IsStunned = true;
            m_StunInfo.m_StunnedWhileJumping = m_MovementComponent.IsJumping();
            m_StunInfo.m_StunnedByHitKO = isHitKO;
            m_StunInfo.m_IsHitStun = !isAttackBlocked;
            Utils.GetPlayerEventManager<float>(gameObject).TriggerEvent(EPlayerEvent.StunBegin, m_StunInfo.m_StunTimer);
        }
        Debug.Log("Player : " + gameObject.name + " is stunned during " + stunDuration + " seconds");
    }

    public bool IsStunned()
    {
        return m_StunInfo.m_IsStunned;
    }

    private void StopStun()
    {
        bool wasHitStun = m_StunInfo.m_IsHitStun;

        m_StunInfo.m_IsStunned = false;
        m_StunInfo.m_StunnedWhileJumping = false;
        m_StunInfo.m_StunnedByHitKO = false;
        m_StunInfo.m_IsHitStun = false;
        m_StunInfo.m_StunTimer = 0;
        Utils.GetPlayerEventManager<float>(gameObject).TriggerEvent(EPlayerEvent.StunEnd, m_StunInfo.m_StunTimer);

        // DEBUG ///////////////////////////////////
        if (wasHitStun && m_DEBUG_IsBlockingAllAttacksAfterHitStun)
        {
            DEBUG_StartBlockingAttacks();
        }
        ////////////////////////////////////////////
        else
        {
            m_Anim.SetTrigger("OnStunEnd");
            Debug.Log("Player : " + gameObject.name + " is no more stunned");
        }
    }

    public void OnJumping(bool isJumping)
    {
        // If we're just landing and was stunned while jumping AND not stunned by hit KO (took a damage and played a hit animation)
        if(!isJumping && m_StunInfo.m_StunnedWhileJumping && !m_StunInfo.m_StunnedByHitKO)
        {
            // Stop stun on landing
            StopStun();
        }
    }

    private void PlayDamageTakenAnim(PlayerBaseAttackLogic attackLogic, bool isAttackBlocked)
    {
        if(isAttackBlocked)
        {
            PlayBlockAnimation(attackLogic);
        }
        else
        {
            PlayHitAnimation(attackLogic);
        }
    }

    private void PlayBlockAnimation(PlayerBaseAttackLogic attackLogic)
    {
        //Play block anim
        string blockAnimName = attackLogic.GetBlockAnimName(m_MovementComponent.GetCurrentStance());
        m_Anim.Play(blockAnimName);
    }

    private void PlayHitAnimation(PlayerBaseAttackLogic attackLogic)
    {
        //Play hit anim
        string hitAnimName = attackLogic.GetHitAnimName(m_MovementComponent.GetCurrentStance());
        m_Anim.Play(hitAnimName);
    }

    private void OnDeath()
    {
        m_Anim.SetTrigger("OnDeath");
        Utils.GetPlayerEventManager<string>(gameObject).TriggerEvent(EPlayerEvent.OnDeath, gameObject.tag);
    }

    public float GetHPPercentage()
    {
        return (float)m_HP / (float)m_HealthConfig.m_MaxHP;
    }

    private void OnRoundOver()
    {
        UnregisterListeners();
        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    // DEBUG /////////////////////////////////////
    private void DEBUG_StartBlockingAttacks()
    {
        m_DEBUG_BlockingAttacksTimer = Time.unscaledTime + m_DEBUG_BlockingAttacksDuration;
        m_DEBUG_IsBlockingAllAttacks = true;

        m_Anim.Play("BlockStand_In");

        Debug.Log("Player : " + gameObject.name + " will block all attacks during " + m_DEBUG_BlockingAttacksDuration + " seconds");
    }

    private void DEBUG_StopBlockingAttacks()
    {
        m_DEBUG_BlockingAttacksTimer = 0.0f;
        m_DEBUG_IsBlockingAllAttacks = false;

        if(!m_StunInfo.m_IsStunned)
        {
            m_Anim.SetTrigger("OnStunEnd"); // To trigger end of blocking animation
        }

        Debug.Log("Player : " + gameObject.name + " doesn't block attacks anymore");
    }

    private void DEBUG_DisplayDamageTakenUI(uint damage)
    {
        //DamageTakenUIInstance will be automatically destroyed
        GameObject damageTakenUI = Instantiate(m_DEBUG_DamageTakenUIPrefab, Vector3.zero, Quaternion.identity);
        damageTakenUI.transform.SetParent(m_DEBUG_DamageTakenParent);
        damageTakenUI.transform.localPosition = Vector3.zero;
        damageTakenUI.GetComponentInChildren<Text>().text = "-" + damage.ToString();
    }
    /////////////////////////////////////////////
}
