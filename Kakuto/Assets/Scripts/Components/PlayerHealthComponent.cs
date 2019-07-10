using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthComponent : MonoBehaviour
{
    public PlayerHealthConfig m_HealthConfig;

    private uint m_HP;
    private PlayerAttackComponent m_AttackComponent;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    private bool m_IsStunned = false;
    private float m_StunTimer = 0.0f;

    [Header("Debug")]
    [Space]

    public bool m_DisplayDamageTaken = false;
    [ConditionalField(true, "m_DisplayDamageTaken")]
    public GameObject m_DamageTakenUIPrefab;
    [ConditionalField(true, "m_DisplayDamageTaken")]
    public Transform m_DamageTakenParent;

    [ConditionalField(false, "m_IsBlockingAllAttacksAfterHitStun")]
    public bool m_IsBlockingAllAttacks = false;
    [ConditionalField(false, "m_IsBlockingAllAttacks")]
    public bool m_IsBlockingAllAttacksAfterHitStun = false;
    [ConditionalField(true, "m_IsBlockingAllAttacksAfterHitStun")]
    public float m_BlockingAttacksDuration = 1.0f;
    private float m_BlockingAttacksTimer = 0.0f;

    public bool m_IsInvincible = false;

    private void Awake()
    {
        m_HP = m_HealthConfig.m_MaxHP;
        m_AttackComponent = GetComponent<PlayerAttackComponent>();
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_Anim = GetComponentInChildren<Animator>();

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).StartListening(EPlayerEvent.Hit, OnHit);
        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).StartListening(EPlayerEvent.Grab, OnGrab);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).StopListening(EPlayerEvent.Hit, OnHit);
        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).StopListening(EPlayerEvent.Grab, OnGrab);
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
        if (m_IsStunned)
        {
            if (Time.unscaledTime > m_StunTimer)
            {
                StopStun();
            }
        }

        // DEBUG /////////////////////////////////////
        if (m_IsBlockingAllAttacksAfterHitStun)
        {
            if (m_IsBlockingAllAttacks && m_BlockingAttacksTimer > 0.0f)
            {
                if (Time.unscaledTime > m_BlockingAttacksTimer)
                {
                    StopBlockingAttacks();
                }
            }
        }
        //////////////////////////////////////////////
    }

    public bool IsDead()
    {
        return m_HP == 0;
    }

    void OnGrab(PlayerAttack attack)
    {
        if (IsDead())
        {
            return;
        }

        if(CanBlockAttack(attack))
        {
            Utils.GetEnemyEventManager<PlayerAttack>(gameObject).TriggerEvent(EPlayerEvent.GrabBlocked, attack);
        }
    }

    void OnHit(PlayerAttack attack)
    {
        if (IsDead())
        {
            return;
        }

        uint hitDamage = 0;
        bool isAttackBlocked = false;
        GetHitInfo(attack, out hitDamage, out isAttackBlocked);
        ApplyDamage(attack, hitDamage, isAttackBlocked);
    }

    private void GetHitInfo(PlayerAttack attack, out uint hitDamage, out bool isAttackBlocked)
    {
        if (CanBlockAttack(attack))
        {
            hitDamage = attack.m_CheapDamage;
            isAttackBlocked = true;
        }
        else
        {
            hitDamage = attack.m_Damage;
            isAttackBlocked = false;
        }
    }

    private bool CanBlockAttack(PlayerAttack attack)
    {
        // DEBUG ///////////////////////////////////
        if (m_IsBlockingAllAttacks)
        {
            return true;
        }
        ////////////////////////////////////////////

        bool canBlockAttack = true;
        if(m_AttackComponent)
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
            switch (attack.m_AttackType)
            {
                case EAttackType.Low:
                    canBlockAttack &= isCrouching;
                    break;
                case EAttackType.Mid:
                case EAttackType.Overhead:
                    canBlockAttack &= !isCrouching;
                    break;
            }
        }

        return canBlockAttack;
    }

    private void ApplyDamage(PlayerAttack attack, uint damage, bool isAttackBlocked)
    {
        if (damage >= m_HP)
        {
            m_HP = (uint)(m_IsInvincible ? 1 : 0);
        }
        else
        {
            m_HP -= damage;
        }

        OnDamageTaken(attack, damage, isAttackBlocked);
    }

    private void OnDamageTaken(PlayerAttack attack, uint damage, bool isAttackBlocked)
    {
        Debug.Log("Player : " + gameObject.name + " HP : " + m_HP + " damage taken : " + damage + " attack blocked : " + isAttackBlocked);
        Utils.GetPlayerEventManager<float>(gameObject).TriggerEvent(EPlayerEvent.DamageTaken, (float)m_HP / (float)m_HealthConfig.m_MaxHP);

        if (IsDead())
        {
            OnDeath();
        }
        else
        {
            TriggerEffects(attack, damage, isAttackBlocked);
            PlayDamageTakenAnim(attack, isAttackBlocked);
        }

        // DEBUG /////////////////////////////////////
        if (damage > 0 && m_DisplayDamageTaken)
        {
            DisplayDamageTakenUI(damage);
        }
        /////////////////////////////////////////////
    }

    private void TriggerEffects(PlayerAttack attack, uint damage, bool isAttackBlocked)
    {
        float stunDuration = (isAttackBlocked) ? attack.m_BlockStun : attack.m_HitStun;
        if (stunDuration > 0)
        {
            StartStun(stunDuration);
        }

        if(attack.m_UseTimeScaleEffect)
        {
            TimeManager.StartTimeScale(attack.m_TimeScaleAmount, attack.m_TimeScaleDuration, attack.m_TimeScaleBackToNormal);
        }

        if(attack.m_UseCameraShakeEffect)
        {
            CameraShakeManager.Shake(attack.m_CameraShakeAmount, attack.m_CameraShakeDuration);
        }

        float pushBackForce = (isAttackBlocked) ? attack.m_BlockPushBack : attack.m_HitPushBack;
        if (pushBackForce > 0.0f && m_MovementComponent)
        {
            m_MovementComponent.PushBack(pushBackForce);
        }
    }

    private void StartStun(float stunDuration)
    {
        m_StunTimer = Time.unscaledTime + stunDuration;
        if(m_IsStunned == false)
        {
            m_IsStunned = true;
            Utils.GetPlayerEventManager<float>(gameObject).TriggerEvent(EPlayerEvent.StunBegin, m_StunTimer);

            Debug.Log("Player : " + gameObject.name + " is stunned during " + stunDuration + " seconds");
        }
    }

    private void StopStun()
    {
        m_IsStunned = false;
        m_StunTimer = 0;
        Utils.GetPlayerEventManager<float>(gameObject).TriggerEvent(EPlayerEvent.StunEnd, m_StunTimer);

        // DEBUG ///////////////////////////////////
        if (m_IsBlockingAllAttacksAfterHitStun)
        {
            StartBlockingAttacks();
        }
        ////////////////////////////////////////////
        else
        {
            m_Anim.SetTrigger("OnStunEnd");
            Debug.Log("Player : " + gameObject.name + " is no more stunned");
        }
    }

    // DEBUG ///////////////////////////////////
    private void StartBlockingAttacks()
    {
        m_BlockingAttacksTimer = Time.unscaledTime + m_BlockingAttacksDuration;
        m_IsBlockingAllAttacks = true;

        PlayBlockAnimation();

        Debug.Log("Player : " + gameObject.name + " will block all attacks during " + m_BlockingAttacksDuration + " seconds");
    }

    private void StopBlockingAttacks()
    {
        m_BlockingAttacksTimer = 0.0f;
        m_IsBlockingAllAttacks = false;

        m_Anim.SetTrigger("OnStunEnd"); // To trigger end of blocking animation

        Debug.Log("Player : " + gameObject.name + " doesn't block attacks anymore");
    }
    ////////////////////////////////////////////

    private void PlayDamageTakenAnim(PlayerAttack attack, bool isAttackBlocked)
    {
        if(isAttackBlocked)
        {
            PlayBlockAnimation();
        }
        else
        {
            PlayHitAnimation(attack);
        }
    }

    private void PlayBlockAnimation()
    {
        //Play block anim
        string blockAnimName = GetPlayerBlockAnimName();
        m_Anim.Play(blockAnimName);
    }

    private string GetPlayerBlockAnimName()
    {
        string blockAnimName = "Block";

        EPlayerStance playerStance = m_MovementComponent.GetCurrentStance();
        blockAnimName += playerStance.ToString();

        if(playerStance == EPlayerStance.Jump)
        {
            Debug.LogError("A player can't block an attack while jumping.");
        }

        blockAnimName += "_In"; //Play the In animation

        return blockAnimName;
    }

    private void PlayHitAnimation(PlayerAttack attack)
    {
        //Play hit anim
        string hitAnimName = GetPlayerHitAnimName(attack);
        m_Anim.Play(hitAnimName);
    }

    private string GetPlayerHitAnimName(PlayerAttack attack)
    {
        string hitAnimName = "Hit";

        EPlayerStance playerStance = m_MovementComponent.GetCurrentStance();
        hitAnimName += playerStance.ToString();

        switch (playerStance)
        {
            case EPlayerStance.Stand:
                hitAnimName += attack.m_HitHeight.ToString();
                hitAnimName += attack.m_HitStrength.ToString();
                break;
            case EPlayerStance.Crouch:
                hitAnimName += "Low"; // Crouch hit is necessarily low
                hitAnimName += attack.m_HitStrength.ToString();
                break;
            case EPlayerStance.Jump:
                // Jump hit doesn't need hit height / strength
                break;
        }

        hitAnimName += "_In"; //Play the In animation

        return hitAnimName;
    }

    private void OnDeath()
    {
        m_Anim.SetTrigger("OnDeath");
    }

    // DEBUG /////////////////////////////////////
    private void DisplayDamageTakenUI(uint damage)
    {
        //DamageTakenUIInstance will be automatically destroyed
        GameObject damageTakenUI = Instantiate(m_DamageTakenUIPrefab, Vector3.zero, Quaternion.identity);
        damageTakenUI.transform.SetParent(m_DamageTakenParent);
        damageTakenUI.transform.localPosition = Vector3.zero;
        damageTakenUI.GetComponentInChildren<Text>().text = "-" + damage.ToString();
    }
    /////////////////////////////////////////////
}
