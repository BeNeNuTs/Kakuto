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
    [SerializeField] private bool m_SuperGaugeAlwaysFilled;
    public bool SuperGaugeAlwaysFilled
    {
        get => m_SuperGaugeAlwaysFilled;
        set
        {
            if(m_SuperGaugeAlwaysFilled != value)
            {
                m_SuperGaugeAlwaysFilled = value;
                OnSuperGaugeAlwaysFilledChanged?.Invoke(value);
            }
        }
    }
    public Action<bool> OnSuperGaugeAlwaysFilledChanged;
    [SerializeField] private bool m_TriggerPointAlwaysActive;
    public bool TriggerPointAlwaysActive
    {
        get => m_TriggerPointAlwaysActive;
        set
        {
            if (m_TriggerPointAlwaysActive != value)
            {
                m_TriggerPointAlwaysActive = value;
                OnTriggerPointAlwaysActiveChanged?.Invoke(value);
            }
        }
    }
    public Action<bool> OnTriggerPointAlwaysActiveChanged;

    [Separator("Movement")]
    public bool m_IsStatic;
    [ConditionalField(true, nameof(m_IsStatic))]
    public EPlayerStance m_DefaultStance = EPlayerStance.Stand;

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
public class UISettings
{
    [Separator("Round")]
    public bool m_IsTimerEnabled;
    public bool m_IsCounterEnabled;
}

[Serializable]
public class DebugSettings
{
    public bool m_DisplayInputsInfo;
    public bool m_DisplayAttacksInfo;
    public bool m_DisplayFPS;
    public bool m_DisplayFrameCounter;
    public bool m_DisplayAnimationInfo;
    public bool m_DisplayAttackTriggerInfo;
    public bool m_DisplayGamepadInfo;
    public bool m_DisplayBoxColliders;
}

[Serializable]
public class SceneSettings
{
    [Scene]
    public string m_Scene;

    public PlayerSettings[] m_PlayerSettings;
    public UISettings m_UISettings;
    public DebugSettings m_DebugSettings;
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
            case Player.Player1:
                playerIndex = 0;
                break;
            case Player.Player2:
                playerIndex = 1;
                break;
        }

        return GetPlayerSettings(playerIndex);
    }

    public static SceneSettings GetSceneSettings(string sceneName)
    {
        foreach (SceneSettings sceneSettings in Instance.m_SceneSettings)
        {
            if (sceneSettings.m_Scene == sceneName)
            {
                return sceneSettings;
            }
        }

        KakutoDebug.LogError("Scene settings not found for the current scene : " + sceneName);
        return null;
    }

    public static PlayerSettings GetPlayerSettings(int playerIndex)
    {
        return GetPlayerSettings(playerIndex, SceneManager.GetActiveScene().name);
    }

    public static PlayerSettings GetPlayerSettings(int playerIndex, string sceneName)
    {
        SceneSettings sceneSettings = GetSceneSettings(sceneName);
        if (sceneSettings != null)
        {
            return sceneSettings.m_PlayerSettings[playerIndex];
        }

        KakutoDebug.LogError("Player settings not found for player " + playerIndex + " in the current scene : " + sceneName);
        return null;
    }

    public static UISettings GetUISettings()
    {
        return GetUISettings(SceneManager.GetActiveScene().name);
    }

    public static UISettings GetUISettings(string sceneName)
    {
        SceneSettings sceneSettings = GetSceneSettings(sceneName);
        if (sceneSettings != null)
        {
            return sceneSettings.m_UISettings;
        }

        KakutoDebug.LogError("UI settings not found in the current scene : " + sceneName);
        return null;
    }

    public static DebugSettings GetDebugSettings()
    {
        return GetDebugSettings(SceneManager.GetActiveScene().name);
    }

    public static DebugSettings GetDebugSettings(string sceneName)
    {
        SceneSettings sceneSettings = GetSceneSettings(sceneName);
        if (sceneSettings != null)
        {
            return sceneSettings.m_DebugSettings;
        }

        KakutoDebug.LogError("UI settings not found in the current scene : " + sceneName);
        return null;
    }
}