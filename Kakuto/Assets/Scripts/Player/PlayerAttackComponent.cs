using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackComponent : MonoBehaviour
{
    struct TriggeredInput
    {
        private readonly char m_Input;
        private readonly float m_TriggeredTime;

        public TriggeredInput(char input, float time)
        {
            m_Input = input;
            m_TriggeredTime = time;
        }

        public char GetInput() { return m_Input; }

        public bool IsElapsed(float time, float persistency)
        {
            return time > (m_TriggeredTime + persistency);
        }
    }

    public PlayerAttackConfig m_AttackConfig;

    private PlayerMovementComponent m_MovementComponent;
    private Animator m_Anim;

    private List<TriggeredInput> m_TriggeredInputsList;
    private string m_TriggeredInputsString;

    private PlayerAttack m_CurrentAttack;
    private bool m_IsAttackBlocked = false;

    void Awake()
    {
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_Anim = GetComponentInChildren<Animator>();
        m_TriggeredInputsList = new List<TriggeredInput>();

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StartListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StartListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.EndOfAttack, EndOfAttack);
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).StopListening(EPlayerEvent.UnblockAttack, UnblockAttack);

        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunBegin, OnStunBegin);
        Utils.GetPlayerEventManager<float>(gameObject).StopListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    void Update()
    {
        if (m_IsAttackBlocked)
        {
            return;
        }

        UpdateTriggerInputList();
        UpdateTriggerInputString();
    }

    void UpdateTriggerInputList()
    {
        float currentTime = Time.unscaledTime;

        string attackInputs = InputManager.GetAttackInputString(m_MovementComponent.GetPlayerIndex(), m_MovementComponent.IsLeftSide());
        foreach (char c in attackInputs)
        {
            m_TriggeredInputsList.Add(new TriggeredInput(c, currentTime));
        }

        while (m_TriggeredInputsList.Count > m_AttackConfig.m_MaxInputs)
        {
            m_TriggeredInputsList.RemoveAt(0);
        }

        m_TriggeredInputsList.RemoveAll(input => input.IsElapsed(currentTime, m_AttackConfig.m_InputPersistency));
    }

    void UpdateTriggerInputString()
    {
        m_TriggeredInputsString = "";
        foreach (TriggeredInput triggeredInput in m_TriggeredInputsList)
        {
            m_TriggeredInputsString += triggeredInput.GetInput();
        }
    }

    void ClearTriggeredInputs()
    {
        m_TriggeredInputsList.Clear();
        m_TriggeredInputsString = "";
    }

    void LateUpdate()
    {
        if(m_IsAttackBlocked)
        {
            return;
        }

        UpdateAttack();
    }

    void UpdateAttack()
    {
        if (m_TriggeredInputsList.Count > 0)
        {
            foreach (PlayerAttack attack in m_AttackConfig.m_AttackList)
            {
                if (CheckAttackCondition(attack) && CanAttackBeTriggered(attack))
                {
                    LaunchAttack(attack);
                    break;
                }
            }
        }
    }

    bool CheckAttackCondition(PlayerAttack attackToCheck)
    {
        bool conditionIsValid = true;

        if (attackToCheck.m_HasCondition)
        {
            if (m_MovementComponent != null)
            {
                conditionIsValid &= attackToCheck.m_ShouldBeCrouched == m_MovementComponent.IsCrouching();
                conditionIsValid &= attackToCheck.m_ShouldBeInTheAir == m_MovementComponent.IsJumping();
            }

            if (attackToCheck.m_HasAttackRequirement)
            {
                conditionIsValid &= (m_CurrentAttack != null && m_CurrentAttack.m_Name == attackToCheck.m_AttackRequired);
            }
        }

        return conditionIsValid;
    }

    bool CanAttackBeTriggered(PlayerAttack attack)
    {
        for (int i = 0; i < attack.m_InputStringList.Count; i++)
        {
            if (m_TriggeredInputsString.Contains(attack.m_InputStringList[i]))
            {
                return true;
            }
        }
        return false;
    }

    void LaunchAttack(PlayerAttack attack)
    {
        ClearTriggeredInputs();

        m_Anim.Play(attack.m_AnimationAttackName.ToString());
        m_CurrentAttack = attack;

        m_IsAttackBlocked = attack.m_BlockAttack;
        m_MovementComponent.SetMovementBlockedByAttack(attack.m_BlockMovement);

        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).TriggerEvent(EPlayerEvent.AttackLaunched, m_CurrentAttack);
    }

    bool CheckIsCurrentAttack(EAnimationAttackName attackName, string methodName)
    {
        if (m_CurrentAttack == null)
        {
            Debug.LogError("There is no current attack");
            return false;
        }

        if (m_CurrentAttack.m_AnimationAttackName != attackName)
        {
            Debug.LogError("Trying to " + methodName + " " + attackName.ToString() + " but current attack is " + m_CurrentAttack.m_AnimationAttackName.ToString());
            return false;
        }
        return true;
    }

    void EndOfAttack(EAnimationAttackName attackName)
    {
        if(CheckIsCurrentAttack(attackName, "EndOfAttack"))
        {
            if(m_CurrentAttack.m_BlockAttack)
            {
                m_IsAttackBlocked = false;
            }
            
            m_CurrentAttack = null;
        }
    }

    void UnblockAttack(EAnimationAttackName attackName)
    {
        if (CheckIsCurrentAttack(attackName, "UnblockAttack"))
        {
            if(m_IsAttackBlocked == false)
            {
                Debug.LogError("Attack was not blocked by " + attackName);
                return;
            }

            m_IsAttackBlocked = false;
        }
    }

    void OnStunBegin(float stunTimeStamp)
    {
        ClearTriggeredInputs();

        m_CurrentAttack = null;
        m_IsAttackBlocked = true;
    }

    void OnStunEnd(float stunTimeStamp)
    {
        if(m_IsAttackBlocked == false)
        {
            Debug.LogError("Attack was not blocked by stun");
            return;
        }
        m_IsAttackBlocked = false;
    }

    public string GetTriggeredInputString()
    {
        return m_TriggeredInputsString;
    }

    public PlayerAttack GetCurrentAttack()
    {
        return m_CurrentAttack;
    }
}
