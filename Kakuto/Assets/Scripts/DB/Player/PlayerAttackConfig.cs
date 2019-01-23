using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public enum EAttackType
{
    Low,
    Mid,
    Overhead
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
    public string m_AnimationName;
    public EAttackType m_AttackType;

    // Condition (Crouch/Jump/etc..)
    [Header("Condition")]
    public bool m_HasCondition = false;

    [ConditionalField(true, "m_HasCondition")]
    public bool m_ShouldBeCrouched = false;
    [ConditionalField(true, "m_HasCondition")]
    public bool m_ShouldBeInTheAir = false;

    [ConditionalField(true, "m_HasCondition")]
    public bool m_HasAttackRequirement = false;
    [ConditionalField(true, "m_HasCondition", "m_HasAttackRequirement")]
    public string m_AttackRequired = "";

    // Attack input to trigger (E+Z+A)
    [Header("Input")]
    public List<string> m_InputStringList;

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
    public float m_HitStun = 2.0f;
    public float m_BlockStun = 1.0f;
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

    [ButtonAttribute("SortAttackList", "Sort Attack List", "Allow to sort the attack list by input in order to avoid conflict.", false, false)]
    public bool m_SortAttackList = false;

    private void SortAttackList()
    {
        m_AttackList.Sort(SortByInput);
        Debug.Log("Attack list sorted !");
    }

    static int SortByInput(PlayerAttack attack1, PlayerAttack attack2)
    {
        if (attack1.m_InputStringList.Count > 0 && attack2.m_InputStringList.Count > 0)
        {
            return attack2.m_InputStringList[0].Length.CompareTo(attack1.m_InputStringList[0].Length);
        }
        Debug.LogError("Attack list contains attack without input");
        return 0;
    }
}