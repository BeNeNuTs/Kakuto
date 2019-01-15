using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class PlayerAttack {

    public string m_Name;

    // Condition (Crouch/Jump/etc..)
    [Header("Condition")]
    public bool m_ShouldBeCrouched = false;
    public bool m_ShouldBeInTheAir = false;

    // Attack input to trigger (E+Z+A)
    [Header("Input")]
    public List<string> m_InputStringList;

    // Effect (damage/stun/etc..)
    [Header("Effect")]
    [Range(0,100)]
    public int m_Damage = 10;
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
}

public class CreatePlayerAttackConfig
{
    [MenuItem("Assets/Create/Player Attack Config")]
    public static PlayerAttackConfig Create()
    {
        PlayerAttackConfig asset = ScriptableObject.CreateInstance<PlayerAttackConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Player/Attack/PlayerAttackConfig_New.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}