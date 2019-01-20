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

    void Awake()
    {
        m_MovementComponent = GetComponent<PlayerMovementComponent>();
        m_Anim = GetComponentInChildren<Animator>();
        m_TriggeredInputsList = new List<TriggeredInput>();

        RegisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<string>(gameObject).StartListening(EPlayerEvent.EndOfAttack, EndOfAttack);
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<string>(gameObject).StopListening(EPlayerEvent.EndOfAttack, EndOfAttack);
    }

    void Update()
    {
        UpdateTriggerInputList();
        UpdateTriggerInputString();
    }

    void UpdateTriggerInputList()
    {
        float currentTime = Time.unscaledTime;

        if (Input.GetKeyDown("left"))
            m_TriggeredInputsList.Add(new TriggeredInput('←', currentTime));
        if (Input.GetKeyDown("right"))
            m_TriggeredInputsList.Add(new TriggeredInput('→', currentTime));
        if (Input.GetKeyDown("up"))
            m_TriggeredInputsList.Add(new TriggeredInput('↑', currentTime));
        if (Input.GetKeyDown("down"))
            m_TriggeredInputsList.Add(new TriggeredInput('↓', currentTime));

        foreach (char c in Input.inputString)
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

    void LateUpdate()
    {
        UpdateAttack();
    }

    void UpdateAttack()
    {
        if (m_TriggeredInputsList.Count > 0)
        {
            int inputIndex, attackInputIndex;
            foreach (PlayerAttack attack in m_AttackConfig.m_AttackList)
            {
                if (CheckAttackCondition(attack) && CanAttackBeTriggered(attack, out inputIndex, out attackInputIndex))
                {
                    LaunchAttack(attack, inputIndex, attackInputIndex);
                    break;
                }
            }
        }
    }

    bool CheckAttackCondition(PlayerAttack attack)
    {
        bool conditionIsValid = true;

        if (attack.m_HasCondition)
        {
            if (m_MovementComponent != null)
            {
                conditionIsValid &= attack.m_ShouldBeCrouched == m_MovementComponent.IsCrouching();
                conditionIsValid &= attack.m_ShouldBeInTheAir == m_MovementComponent.IsJumping();
            }

            if (attack.m_HasAttackRequirement)
            {
                conditionIsValid &= (m_CurrentAttack != null && m_CurrentAttack.m_Name == attack.m_CurrentAttackName);
            }
        }

        return conditionIsValid;
    }

    bool CanAttackBeTriggered(PlayerAttack attack, out int inputIndex, out int attackInputIndex)
    {
        inputIndex = -1;
        attackInputIndex = -1;
        for (int i = 0; i < attack.m_InputStringList.Count; i++)
        {
            int index = m_TriggeredInputsString.IndexOf(attack.m_InputStringList[i]);
            if (index != -1)
            {
                inputIndex = index;
                attackInputIndex = i;
                return true;
            }
        }
        return false;
    }

    void LaunchAttack(PlayerAttack attack, int inputIndex, int attackInputIndex)
    {
        //Remove inputString from triggered input to be sure to not trigger this attack twice with the same input
        m_TriggeredInputsList.RemoveRange(inputIndex, attack.m_InputStringList[attackInputIndex].Length);

        m_Anim.Play(attack.m_Name);

        m_CurrentAttack = attack;

        Utils.GetPlayerEventManager<PlayerAttack>(gameObject).TriggerEvent(EPlayerEvent.AttackLaunched, m_CurrentAttack);
    }

    public void EndOfAttack(string attackName)
    {
        if (m_CurrentAttack == null)
        {
            Debug.LogError("There is no current attack");
            return;
        }

        if (m_CurrentAttack.m_Name != attackName)
        {
            Debug.LogError("Trying to EndOfAttack " + attackName + " but current attack is " + m_CurrentAttack.m_Name);
            return;
        }

        m_CurrentAttack = null;
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
