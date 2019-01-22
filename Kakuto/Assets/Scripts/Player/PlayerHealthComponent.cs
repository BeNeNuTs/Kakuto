using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthComponent : MonoBehaviour
{
    public PlayerHealthConfig m_HealthConfig;

    [Header("Debug")]
    [Space]
    public bool m_DisplayDamageTaken = false;
    [ConditionalField(true, "m_DisplayDamageTaken")]
    public GameObject m_DamageTakenUIPrefab;
    [ConditionalField(true, "m_DisplayDamageTaken")]
    public Transform m_DamageTakenParent;

    private uint m_HP;
    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    private void Awake()
    {
        m_HP = m_HealthConfig.m_MaxHP;
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_Anim = GetComponentInChildren<Animator>();

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).StartListening(EPlayerEvent.Hit, OnHit);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).StopListening(EPlayerEvent.Hit, OnHit);
    }

    public bool IsDead()
    {
        return m_HP == 0;
    }

    public void OnHit(PlayerAttack attack)
    {
        if (IsDead())
        {
            return;
        }

        uint hitDamage = GetHitDamage(attack);
        if(hitDamage > 0)
        {
            ApplyDamage(hitDamage);
        }
    }

    private uint GetHitDamage(PlayerAttack attack)
    {
        return CanBlockAttack(attack) ? attack.m_CheapDamage : attack.m_Damage;
    }

    private bool CanBlockAttack(PlayerAttack attack)
    {
        bool canBlockAttack = false;
        if (m_MovementComponent)
        {
            //TODO : Change to => Moving at the opposite direction of the attacker
            if (m_MovementComponent.GetHorizontalMoveInput() == 0.0f && m_MovementComponent.IsJumping() == false) // For now, not moving at all
            {
                //Check if the player is in the right stance 
                bool isCrouching = m_MovementComponent.IsCrouching();
                switch (attack.m_AttackType)
                {
                    case EAttackType.Low:
                        canBlockAttack = isCrouching;
                        break;
                    case EAttackType.Mid:
                    case EAttackType.Overhead:
                        canBlockAttack = !isCrouching;
                        break;
                }
            }
        }

        return canBlockAttack;
    }

    private void ApplyDamage(uint damage)
    {
        if (damage > m_HP)
        {
            m_HP = 0;
        }
        else
        {
            m_HP -= damage;
        }

        OnDamageTaken(damage);
    }

    private void OnDamageTaken(uint damage)
    {
        Debug.Log("Player : " + gameObject.name + " HP : " + m_HP + " damage taken : " + damage);
        Utils.GetPlayerEventManager<float>(gameObject).TriggerEvent(EPlayerEvent.DamageTaken, (float)m_HP / (float)m_HealthConfig.m_MaxHP);

        if (IsDead())
        {
            OnDeath();
        }

        if (m_DisplayDamageTaken)
        {
            DisplayDamageTakenUI(damage);
        }
    }

    private void OnDeath()
    {
        m_Anim.SetTrigger("OnDeath");
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
