using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[Serializable]
public class PlayerSettings
{
    public PlayerSettings(string playerName)
    {
        SetName(playerName);
    }

    public void SetName(string playerName)
    {
        m_Name = playerName;
    }

    [SerializeField, ReadOnly]
    private string m_Name;

    [Separator("Attack")]
    public bool m_AttackEnabled;
    public bool m_SuperGaugeAlwaysFilled;

    [Separator("Movement")]
    public bool m_IsStatic;

    [Separator("Health")]
    public bool m_DisplayDamageTaken;

    [ConditionalField(false, "m_IsBlockingAllAttacksAfterHitStun")]
    public bool m_IsBlockingAllAttacks;
    [ConditionalField(false, "m_IsBlockingAllAttacks")]
    public bool m_IsBlockingAllAttacksAfterHitStun;
    [Min(0), ConditionalField(true, "m_IsBlockingAllAttacksAfterHitStun")]
    public float m_BlockingAttacksDuration;

    public bool m_IsInvincible;
    public bool m_IsImmuneToStunGauge;
}

[Serializable]
public class SceneSettings
{
    [Scene]
    public string m_Scene;

    public PlayerSettings[] m_PlayerSettings;
}

public class ScenesConfig : ScriptableObject
{
    static ScenesConfig s_Instance = null;
    public static ScenesConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<ScenesConfig>("Game/ScenesConfig");
            return s_Instance;
        }
    }

#pragma warning disable 0649
    [SerializeField]
    private List<SceneSettings> m_SceneSettings;
#pragma warning restore 0649

    void OnValidate()
    {
        foreach(SceneSettings sceneSettings in m_SceneSettings)
        {
            if (sceneSettings.m_PlayerSettings.Length != 2)
            {
                Array.Resize(ref sceneSettings.m_PlayerSettings, 2);
                sceneSettings.m_PlayerSettings[0] = new PlayerSettings(Player.Player1);
                sceneSettings.m_PlayerSettings[1] = new PlayerSettings(Player.Player2);
            }

            sceneSettings.m_PlayerSettings[0].SetName(Player.Player1);
            sceneSettings.m_PlayerSettings[1].SetName(Player.Player2);
        }
    }

    public static PlayerSettings GetPlayerSettings(GameObject player)
    {
        return GetPlayerSettings(player.tag);
    }

    public static PlayerSettings GetPlayerSettings(EPlayer player)
    {
        return GetPlayerSettings(player.ToString());
    }

    public static PlayerSettings GetPlayerSettings(string tag)
    {
        int playerIndex = -1;
        switch (tag)
        {
            case "Player1":
                playerIndex = 0;
                break;
            case "Player2":
                playerIndex = 1;
                break;
        }

        return GetPlayerSettings(playerIndex);
    }

    public static PlayerSettings GetPlayerSettings(int playerIndex)
    {
        foreach (SceneSettings sceneSettings in Instance.m_SceneSettings)
        {
            if (sceneSettings.m_Scene == SceneManager.GetActiveScene().name)
            {
                return sceneSettings.m_PlayerSettings[playerIndex];
            }
        }

        Debug.LogError("Player settings not found for player " + playerIndex + " in the current scene : " + SceneManager.GetActiveScene().name);
        return null;
    }
}