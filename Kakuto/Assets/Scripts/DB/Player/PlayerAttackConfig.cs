using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public enum EAnimationAttackName
{
    StandLP,
    StandHP,
    StandLK,
    StandHK,

    CrouchLP,
    CrouchHP,
    CrouchLK,
    CrouchHK,

    JumpLP,
    JumpHP,
    JumpLK,
    JumpHK,
}

public enum EAttackType
{
    Low,
    Mid,
    Overhead
}

public enum EPlayerStance
{
    Stand,
    Crouch,
    Jump
}

public enum EHitHeight
{
    Low,
    High
}

public enum EHitStrength
{
    Weak,
    Strong
}

public enum ETimeScaleBackToNormal
{
    Smooth,
    Instant
}

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
    public EAttackType m_AttackType;

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
    [Range(0, 100)]
    public uint m_CheapDamage = 2;
    /////////////////////////////////////////////////////
    public EHitHeight m_HitHeight = EHitHeight.Low;
    public EHitStrength m_HitStrength = EHitStrength.Weak;
    /////////////////////////////////////////////////////
    public float m_HitStun = 2.0f;
    public float m_BlockStun = 1.0f;
    /////////////////////////////////////////////////////
    [Tooltip("The force of the push back if this attack hit")]
    public float m_HitPushBack = 0.0f;
    [Tooltip("The force of the push back if this attack is blocked")]
    public float m_BlockPushBack = 0.0f;
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

    public List<string> GetRawInputStringList()
    {
        return m_InputStringList;
    }

    public void ResetComputedInputString()
    {
        m_ComputedInputStringList.Clear();
        m_IsInputStringProcessing = false;
        m_IsInputStringComputed = false;
    }

    public int AddComputedInputString(string inputString)
    {
        m_ComputedInputStringList.Add(inputString);
        return m_ComputedInputStringList.Count-1;
    }

    public void ReplaceComputedInputString(int index, string oldString, string newString)
    {
        if (index >= 0 && index < m_ComputedInputStringList.Count)
        {
            m_ComputedInputStringList[index] = m_ComputedInputStringList[index].Replace(oldString, newString);
        }
    }

    public bool IsInputStringProcessing()
    {
        return m_IsInputStringProcessing;
    }

    public bool IsInputStringComputed()
    {
        return m_IsInputStringComputed;
    }

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

[CreateAssetMenu(fileName = "PlayerAttackConfig", menuName = "Data/Player/PlayerAttackConfig", order = 0)]
public class PlayerAttackConfig : ScriptableObject
{
    [Tooltip("Contain the attack list of this player")]
    public List<PlayerAttack> m_AttackList;

    [Tooltip("How much time each input will be kept before being deleted")]
    public float m_InputPersistency = 2.0f;

    [Tooltip("How many input can be stacked before being deleted")]
    public uint m_MaxInputs = 10;

    [ButtonAttribute("ComputeInputStringList", "Compute inputs", "Allow to compute final inputs according to Input string list.", false, false)]
    public bool m_ComputeInputs = false;

    [ButtonAttribute("SortAttackList", "Sort Attack List", "Allow to sort the attack list by input in order to avoid conflict.", false, false)]
    public bool m_SortAttackList = false;

    public void Init()
    {
        ComputeInputStringList();
        SortAttackList();
    }

    private void ComputeInputStringList()
    {
        foreach (PlayerAttack attack in m_AttackList)
        {
            attack.ResetComputedInputString();
        }

        foreach (PlayerAttack attack in m_AttackList)
        {
            if (attack.IsInputStringComputed() == false)
            {
                ComputeInputStringList_Internal(attack);
            }   
        }
    }
    
    private void ComputeInputStringList_Internal(PlayerAttack attack)
    {
        attack.SetInputStringProcessing();

        var attackNameToInputs = new Dictionary<string, List<string>>();
        List<string> inputsAssociatedToAttack = new List<string>();

        // Find inputs associated to attack ///////////////////////////////////////////////////////
        foreach (string rawInputString in attack.GetRawInputStringList())
        {
            string rawInputStringWithoutEmptySpace = rawInputString.Replace(" ", string.Empty);
            string[] inputList = rawInputStringWithoutEmptySpace.Split('+');

            attack.AddComputedInputString(string.Concat<string>(inputList));

            foreach (string input in inputList)
            {
                // If input is more than 1 character, it's not a normal input
                if(input.Length > 1)
                {
                    string attackName = input;

                    if(attackNameToInputs.ContainsKey(attackName) == false)
                    {
                        foreach (PlayerAttack attack2 in m_AttackList)
                        {
                            if (attackName == attack2.m_Name)
                            {
                                // Catch error cases ///////////////
                                if (attack == attack2)
                                {
                                    Debug.LogError("An input of attack " + attack.m_Name + " is refering itself.");
                                    attack.SetInputStringComputed();
                                    return;
                                }

                                if (attack2.IsInputStringProcessing())
                                {
                                    Debug.LogError("Attacks " + attack.m_Name + " and " + attack2.m_Name + " inputs are refering each other");
                                    attack.SetInputStringComputed();
                                    return;
                                }
                                ///////////////////////////////////

                                if (attack2.IsInputStringComputed() == false)
                                {
                                    ComputeInputStringList_Internal(attack2);
                                }

                                inputsAssociatedToAttack = new List<string>(attack2.GetInputStringList());
                                break;
                            }
                        }

                        if (inputsAssociatedToAttack.Count > 0)
                        {
                            attackNameToInputs.Add(attackName, new List<string>(inputsAssociatedToAttack));
                            inputsAssociatedToAttack.Clear();
                        }
                    }
                }
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////

        // Create X ComputedInputStringList according to the x variations of inputs for each attack
        int nbComputedInputInputStringNeeded = 1;
        foreach(var pair in attackNameToInputs)
        {
            nbComputedInputInputStringNeeded *= pair.Value.Count;
        }

        if (nbComputedInputInputStringNeeded > 1)
        {
            List<string> originalComputedStringList = new List<string>(attack.m_ComputedInputStringList);
            attack.m_ComputedInputStringList.Clear();
            foreach(string originalComputedString in originalComputedStringList)
            {
                for (int i = 0; i < nbComputedInputInputStringNeeded; i++)
                {
                    attack.m_ComputedInputStringList.Add(originalComputedString);
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////

        // Replace recursively attackName by the right inputs //////////////////////////////////////
        if (attackNameToInputs.Count > 0 && attack.m_ComputedInputStringList.Count > 0)
        {
            List<KeyValuePair<string, string>> listKeyValue = new List<KeyValuePair<string, string>>();
            int computedInputStringIndex = 0;
            for(int attackNameToInputsIndex = 0; attackNameToInputsIndex < attackNameToInputs.Count; attackNameToInputsIndex++)
            {
                if (computedInputStringIndex >= attack.m_ComputedInputStringList.Count)
                {
                    break;
                }

                ReplaceAttackNameByInputs(attack, attackNameToInputs, ref attackNameToInputsIndex, ref computedInputStringIndex, listKeyValue);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////

        attack.SetInputStringComputed();
    }

    private void ReplaceAttackNameByInputs(PlayerAttack attack, Dictionary<string, List<string>> attackNameToInputs, ref int attackNameToInputsIndex, ref int computedInputStringIndex, List<KeyValuePair<string, string>> listKeyValue)
    {
        var lastKeyItem = attackNameToInputs.Keys.Last();
        var currentItem = attackNameToInputs.ElementAt(attackNameToInputsIndex);

        var oldKeyValueList = listKeyValue;

        foreach (string input in currentItem.Value)
        {
            var newKeyValueList = new List<KeyValuePair<string, string>>(oldKeyValueList);
            newKeyValueList.Add(new KeyValuePair<string, string>(currentItem.Key, input));
            if (currentItem.Key != lastKeyItem)
            {
                attackNameToInputsIndex++;
                ReplaceAttackNameByInputs(attack, attackNameToInputs, ref attackNameToInputsIndex, ref computedInputStringIndex, newKeyValueList);
            }
            else
            {
                foreach(var pair in newKeyValueList)
                {
                    attack.ReplaceComputedInputString(computedInputStringIndex, pair.Key, pair.Value);
                }
                computedInputStringIndex++;
            }
        }

        attackNameToInputsIndex--;
    }

    private void SortAttackList()
    {
        m_AttackList.Sort(SortByInput);
        Debug.Log("Attack list sorted !");
    }

    static int SortByInput(PlayerAttack attack1, PlayerAttack attack2)
    {
        if (attack1.GetInputStringList().Count > 0 && attack2.GetInputStringList().Count > 0)
        {
            return attack2.GetInputStringList()[0].Length.CompareTo(attack1.GetInputStringList()[0].Length);
        }
        Debug.LogError("Attack list contains attack without input");
        return 0;
    }
}
 