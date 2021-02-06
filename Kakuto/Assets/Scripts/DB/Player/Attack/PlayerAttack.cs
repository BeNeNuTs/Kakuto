using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class PlayerAttack
{
    [Tooltip("The real attack name")]
    public string m_Name;
    [Tooltip("Description of the attack")]
    public string m_Description;

    // Setting (AnimName/AttackType/etc..)
    [Separator("Setting")]

    [SearchableEnum]
    public EAnimationAttackName m_AnimationAttackName;
    public List<EPlayerStance> m_NeededStanceList;

    [Tooltip("If true, stance will be automatically switched to this default stance at first attack frame.")]
    public bool m_UseDefaultStance = true;
    [Tooltip("Which is the attack default stance at the beggining of the animation ?"), ConditionalField(true, "m_UseDefaultStance")]
    public EPlayerStance m_DefaultStance = EPlayerStance.Stand;
    public bool m_IsASuper = false;
    public bool m_IsEXAttack = false;
    [Tooltip("When hitting the enemy, is this attack able to juggle him")]
    public bool m_CanJuggle = true;

    // Condition (SuperGauge, AttackRequirement)
    [Separator("Condition")]
    public bool m_HasCondition = false;

    [ConditionalField(true, "m_HasCondition")]
    [Tooltip("The needed amount of the super gauge to triggered this attack")]
    public float m_SuperGaugeAmountNeeded = 0f;

    [ConditionalField(true, "m_HasCondition")]
    public bool m_HasAttackRequirement = false;
    [ConditionalField(true, "m_HasCondition", "m_HasAttackRequirement")]
    public EAnimationAttackName m_AttackRequired = EAnimationAttackName.StandHP;

    [ConditionalField(true, "m_HasCondition")]
    public bool m_HasMovementRequirement = false;
    [ConditionalField(true, "m_HasCondition", "m_HasMovementRequirement")]
    public List<EMovingDirection> m_MovementRequiredList;

    // Attack input to trigger (E+Z+A)
    [Separator("Input")]
    [SerializeField,  Tooltip("Allowed inputs are : A B X Y RB LB RT LT ← → ↑ ↓ ↖ ↗ ↙ ↘ and AttackName to refer to another attack inputs. (eg : Grab = standLP + standLK)")]
#pragma warning disable 0649
    private List<string> m_InputStringList;
#pragma warning restore 0649
    [SerializeField, ReadOnly]
    public List<string> m_ComputedInputStringList;
#pragma warning disable 0649
    [SerializeField, ReadOnly]
    private List<GameInputList> m_ComputedGameInputList;
#pragma warning restore 0649

    private bool m_IsInputStringProcessing = false;
    private bool m_IsInputStringComputed = false;

    // Effect (damage/stun/etc..)
    [Separator("Player effect")]
    [Tooltip("Once this attack launched, player won't be able to move anymore until UnblockMovement/EndOfAnim is called")]
    public bool m_BlockMovement = false;

    [Separator("Effects")]
    /////////////////////////////////////////////////////
    public bool m_UseTimeScaleEffect = false;
    [ConditionalField(true, "m_UseTimeScaleEffect")]
    public TimeScaleParams m_TimeScaleParams;
    /////////////////////////////////////////////////////
    public bool m_UseCameraShakeEffect = false;
    [ConditionalField(true, "m_UseCameraShakeEffect")]
    public CameraShakeParams m_CameraShakeParams;

    [Separator("Config")]
    public PlayerBaseAttackConfig m_AttackConfig;

    public List<string> GetRawInputStringList(){ return m_InputStringList; }

    public void ResetComputedInputString()
    {
        m_ComputedInputStringList.Clear();
        if(m_ComputedGameInputList != null)
        {
            m_ComputedGameInputList.Clear();
        }
        m_IsInputStringProcessing = false;
        m_IsInputStringComputed = false;
    }

    public void ReplaceComputedInputString(int index, string oldString, string newString)
    {
        if (index >= 0 && index < m_ComputedInputStringList.Count)
        {
            m_ComputedInputStringList[index] = m_ComputedInputStringList[index].Replace(oldString, newString);
        }
    }

    public bool IsInputStringProcessing() { return m_IsInputStringProcessing; }
    public bool IsInputStringComputed() { return m_IsInputStringComputed; }

    public void SetInputStringProcessing()
    {
        m_IsInputStringProcessing = true;
        m_IsInputStringComputed = false;
    }

    public void SetInputStringComputed()
    {
        m_IsInputStringComputed = true;
        m_IsInputStringProcessing = false;

        m_ComputedGameInputList = new List<GameInputList>();

        // Parse all the computed input list
        foreach (string inputs in m_ComputedInputStringList)
        {
            GameInputList gameInputList = new GameInputList();

            string inputToCheck = string.Empty;
            // Parse all single character
            foreach (char c in inputs)
            {
                inputToCheck += c;
                // For each inputToCheck, try to find the matching EInputKey
                foreach (EInputKey inputKey in Enum.GetValues(typeof(EInputKey)))
                {
                    string inputKeyToString = GameInput.ConvertInputKeyToString(inputKey);
                    if(inputToCheck.Equals(inputKeyToString))
                    {
                        gameInputList.Add(new GameInput(inputKey));
                        inputToCheck = string.Empty;
                        break;
                    }
                }
            }

            m_ComputedGameInputList.Add(gameInputList);
        }

        if(m_NeededStanceList.Count == 0)
        {
            Debug.LogError("Needed stance list is empty for attack " + m_Name);
        }
        else
        {
            for(int i = 0; i < m_NeededStanceList.Count; i++)
            {
                for(int j = i+1; j < m_NeededStanceList.Count; j++)
                {
                    if(m_NeededStanceList[i] == m_NeededStanceList[j])
                    {
                        Debug.LogError("Needed stance list contains the stance " + m_NeededStanceList[i] + " twice for attack " + m_Name);
                    }
                }
            }
        }
    }

    public List<string> GetInputStringList()
    {
#if UNITY_EDITOR
        if(m_ComputedGameInputList == null || m_ComputedInputStringList.Count == 0)
        {
            Debug.LogError("PlayerAttack " + m_Name + " has empty computed input string list.");
        }
#endif
        return m_ComputedInputStringList;
    }

    public List<GameInputList> GetInputList()
    {
#if UNITY_EDITOR
        if(m_ComputedGameInputList == null || m_ComputedGameInputList.Count == 0)
        {
            Debug.LogError("PlayerAttack " + m_Name + " has empty computed game input list.");
        }
#endif
        return m_ComputedGameInputList;
    }
}