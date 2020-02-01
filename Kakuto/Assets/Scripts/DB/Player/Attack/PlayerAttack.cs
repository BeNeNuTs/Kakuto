﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[System.Serializable]
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
    public EPlayerStance m_NeededStance = EPlayerStance.Stand;
    public bool m_IsASuper = false;

    // Condition (SuperGauge, AttackRequirement)
    [Separator("Condition")]
    public bool m_HasCondition = false;

    [ConditionalField(true, "m_HasCondition")]
    [Tooltip("The needed amount of the super gauge to triggered this attack")]
    public float m_SuperGaugeAmountNeeded = 0f;

    [ConditionalField(true, "m_HasCondition")]
    public bool m_HasAttackRequirement = false;
    [ConditionalField(true, "m_HasCondition", "m_HasAttackRequirement")]
    public string m_AttackRequired = "";

    // Attack input to trigger (E+Z+A)
    [Separator("Input")]
    [SerializeField,  Tooltip("Allowed inputs are : A B X Y RB LB ← → ↑ ↓ ↖ ↗ ↙ ↘ and AttackName to refer to another attack inputs. (eg : Grab = standLP + standLK)")]
#pragma warning disable 0649
    private List<string> m_InputStringList;
#pragma warning restore 0649
    [SerializeField, ReadOnly]
    public List<string> m_ComputedInputStringList;
#pragma warning disable 0649
    [ReadOnly]
    private List<List<GameInput>> m_ComputedGameInputList;
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
    public float m_TimeScaleAmount = 0.5f;
    [ConditionalField(true, "m_UseTimeScaleEffect")]
    public float m_TimeScaleDuration = 0.1f;
    [ConditionalField(true, "m_UseTimeScaleEffect")]
    [Tooltip("Smooth will gradually smoothing time scale effect from timeScaleAmount to 1 in timeScaleDuration. Instant will set back instantly timeScale to 1 after timeScaleDuration")]
    public ETimeScaleBackToNormal m_TimeScaleBackToNormal = ETimeScaleBackToNormal.Instant;
    /////////////////////////////////////////////////////
    public bool m_UseCameraShakeEffect = false;
    [ConditionalField(true, "m_UseCameraShakeEffect")]
    public float m_CameraShakeAmount = 0.5f;
    [ConditionalField(true, "m_UseCameraShakeEffect")]
    public float m_CameraShakeDuration = 0.1f;

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

        m_ComputedGameInputList = new List<List<GameInput>>();

        // Parse all the computed input list
        foreach (string inputs in m_ComputedInputStringList)
        {
            List<GameInput> gameInputList = new List<GameInput>();

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
    }

    public List<string> GetInputStringList()
    {
#if UNITY_EDITOR
        if(m_ComputedInputStringList.Count == 0)
        {
            Debug.LogError("PlayerAttack " + m_Name + " has empty computed input string list.");
        }
#endif
        return m_ComputedInputStringList;
    }

    public List<List<GameInput>> GetInputList()
    {
#if UNITY_EDITOR
        if(m_ComputedGameInputList.Count == 0)
        {
            Debug.LogError("PlayerAttack " + m_Name + " has empty computed game input list.");
        }
#endif
        return m_ComputedGameInputList;
    }
}