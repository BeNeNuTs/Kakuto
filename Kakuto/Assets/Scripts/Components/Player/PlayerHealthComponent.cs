using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct DamageTakenInfo
{
    public PlayerBaseAttackLogic m_AttackLogic;
    public bool m_IsAttackBlocked;
    public float m_HealthRatio;
    public bool m_IsAlreadyHitStunned;

    public DamageTakenInfo(PlayerBaseAttackLogic attackLogic, bool isAttackBlocked, bool isAlreadyHitStunned, float healthRatio)
    {
        m_AttackLogic = attackLogic;
        m_IsAttackBlocked = isAttackBlocked;
        m_HealthRatio = healthRatio;
        m_IsAlreadyHitStunned = isAlreadyHitStunned;
    }
}

public class PlayerHealthComponent : MonoBehaviour
{
    public PlayerHealthConfig m_HealthConfig;

    private uint m_HP;

    private PlayerAttackComponent m_AttackComponent;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    private PlayerStunInfoSubComponent m_StunInfoSC;

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

    public bool m_DEBUG_IsInvincible = false;

    private void Awake()
    {
        m_HP = m_HealthConfig.m_MaxHP;
        m_AttackComponent = GetComponent<PlayerAttackComponent>();
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_Anim = GetComponentInChildren<Animator>();

        m_StunInfoSC = new PlayerStunInfoSubComponent(this, m_MovementComponent, m_Anim);

        RegisterListeners();
        GameManager.Instance.RegisterPlayer(gameObject);
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StartListening(EPlayerEvent.Hit, OnHit);
        Utils.GetPlayerEventManager<PlayerBaseAttackLogic>(gameObject).StartListening(EPlayerEvent.GrabTry, OnGrabTry);
        Utils.GetPlayerEventManager<GrabbedInfo>(gameObject).StartListening(EPlayerEvent.Grabbed, OnGrabbed);

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

        RoundSubGameManager.OnRoundOver -= OnRoundOver;
    }

    void Update()
    {
        if(IsDead())
        {
            return;
        }

        m_StunInfoSC.Update();
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
        else if(!m_StunInfoSC.IsStunned() && !m_MovementComponent.IsJumping()) // A grab can't touch if player is stunned or is jumping
        {
            Utils.GetEnemyEventManager<PlayerBaseAttackLogic>(gameObject).TriggerEvent(EPlayerEvent.GrabTouched, attackLogic);
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

        DamageTakenInfo damageTakenInfo = new DamageTakenInfo(attackLogic, isAttackBlocked, m_StunInfoSC.IsHitStunned(), (float)m_HP / (float)m_HealthConfig.m_MaxHP);
        Utils.GetPlayerEventManager<DamageTakenInfo>(gameObject).TriggerEvent(EPlayerEvent.DamageTaken, damageTakenInfo);

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

        // Stun duration and pushback are anim driven for hit air/KO
        if(!m_MovementComponent.IsJumping() && !attackLogic.IsHitKO())
        {
            if (attackLogic.CanStun())
            {
                float stunDuration = attackLogic.GetStunDuration(isAttackBlocked);
                if (stunDuration > 0)
                {
                    m_StunInfoSC.StartStun(stunDuration, attackLogic.IsHitKO(), (isAttackBlocked) ? EStunType.Block : EStunType.Hit, false);
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

        PlayerSuperGaugeSubComponent superGaugeSC = m_AttackComponent.GetSuperGaugeSubComponent();
        if(superGaugeSC != null)
        {
            superGaugeSC.IncreaseGaugeValue(AttackConfig.Instance.m_DefenderSuperGaugeBonus);
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
        m_Anim.Play(blockAnimName, 0, 0);
    }

    private void PlayHitAnimation(PlayerBaseAttackLogic attackLogic)
    {
        //Play hit anim
        string hitAnimName = attackLogic.GetHitAnimName(m_MovementComponent.GetCurrentStance());
        m_Anim.Play(hitAnimName, 0, 0);
    }

    private void OnDeath()
    {
        m_Anim.SetTrigger("OnDeath");
        Utils.GetPlayerEventManager<string>(gameObject).TriggerEvent(EPlayerEvent.OnDeath, gameObject.tag);
    }

    private void OnRoundOver()
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

    // DEBUG /////////////////////////////////////
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
