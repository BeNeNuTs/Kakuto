using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class PlayerAttack {

    public string m_Name;

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
    public string m_CurrentAttack = "";

    // Attack input to trigger (E+Z+A)
    [Header("Input")]
    public List<string> m_InputStringList;

    // Effect (damage/stun/etc..)
    [Header("Effect")]
    [Range(0,100)]
    public uint m_Damage = 10;
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

public class CreatePlayerAttackConfig
{
    //[MenuItem("Assets/Create/Player Attack Config")]
    public static PlayerAttackConfig Create()
    {
        PlayerAttackConfig asset = ScriptableObject.CreateInstance<PlayerAttackConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Player/Attack/PlayerAttackConfig_New.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}