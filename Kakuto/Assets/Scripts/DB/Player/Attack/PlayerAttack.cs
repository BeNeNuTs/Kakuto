using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class PlayerAttack
{
    [Tooltip("The real attack name")]
    public string m_Name;
    [Tooltip("Description of the attack")]
    public string m_Description;

    // Setting (AnimName/AttackType/etc..)
    [Header("Setting")]
    public EAnimationAttackName m_AnimationAttackName;
    public EPlayerStance m_NeededStance = EPlayerStance.Stand;

    // Condition (AttackRequirement)
    [Header("Condition")]
    public bool m_HasCondition = false;

    [ConditionalField(true, "m_HasCondition")]
    public bool m_HasAttackRequirement = false;
    [ConditionalField(true, "m_HasCondition", "m_HasAttackRequirement")]
    public string m_AttackRequired = "";

    // Attack input to trigger (E+Z+A)
    [Header("Input")]
    [SerializeField,  Tooltip("Allowed inputs are : A B X Y ← → ↑ ↓ and AttackName to refer to another attack inputs. (eg : Grab = standLP + standLK)")]
#pragma warning disable 0649
    private List<string> m_InputStringList;
#pragma warning restore 0649
    [SerializeField, ReadOnly]
    public List<string> m_ComputedInputStringList;

    private bool m_IsInputStringProcessing = false;
    private bool m_IsInputStringComputed = false;

    // Effect (damage/stun/etc..)
    [Header("Player effect")]
    [Tooltip("Once this attack launched, player won't be able to attack anymore until UnblockAttack/EndOfAnim is called")]
    public bool m_BlockAttack = false;
    [Tooltip("Once this attack launched, player won't be able to move anymore until UnblockMovement/EndOfAnim is called")]
    public bool m_BlockMovement = false;

    [Header("Enemy effect")]
    [Range(0, 100)]
    public uint m_Damage = 10;
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

    public PlayerBaseAttackConfig m_AttackConfig;

    public List<string> GetRawInputStringList(){ return m_InputStringList; }

    public void ResetComputedInputString()
    {
        m_ComputedInputStringList.Clear();
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
}