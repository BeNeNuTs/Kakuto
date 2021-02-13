using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TrainingOptionsMenuComponent : MenuComponent
{
    public static readonly int[] K_DEFAULT_TRAINING_OPTIONS = new int[]
    {
        0,
        0,
        0,
        1,
        1,
        1,
        1,
        1,
        1
    };

#pragma warning disable 0649
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_TrainingOptionsHighlightInfo;
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private float[] m_ScrollViewNormalizedPosition;
    [SerializeField] private TrainingOptionListener[] m_TrainingOptionListeners;
#pragma warning restore 0649

#if UNITY_EDITOR
    private static PlayerSettings m_InitialPlayer1Settings;
    private static PlayerSettings m_InitialPlayer2Settings;
#endif

    private static PlayerSettings m_Player1Settings;
    private static PlayerSettings m_Player2Settings;
    private static DebugSettings m_DebugSettings;

    private static readonly int[] m_TrainingOptions = new int[TrainingOptionListener.K_MAX_TRAINING_OPTIONS];

#if UNITY_EDITOR
    [MenuItem("Kakuto/Clear saved training options")]
    static void ClearSavedTrainingOptions()
    {
        PlayerPrefs.SetString("TrainingOptions", "");
        Debug.Log("Training options cleared!");
    }
#endif

    private void Awake()
    {
        TrainingOptionListener.OnValueChangedCallback += OnTrainingOptionChanged;
        InitTrainingOptionListeners();
    }
    
    private void InitTrainingOptionListeners()
    {
        for (int i = 0; i < m_TrainingOptions.Length; i++)
        {
            m_TrainingOptionListeners[i].Init(m_TrainingOptions[i]);
        }
    }

    private static void CopyPlayerSettings(PlayerSettings from, PlayerSettings to)
    {
        to.m_AttackEnabled = from.m_AttackEnabled;
        to.SuperGaugeAlwaysFilled = from.SuperGaugeAlwaysFilled;
        to.TriggerPointAlwaysActive = from.TriggerPointAlwaysActive;
        to.m_IsStatic = from.m_IsStatic;
        to.m_DisplayDamageTaken = from.m_DisplayDamageTaken;
        to.m_IsBlockingAllAttacks = from.m_IsBlockingAllAttacks;
        to.m_IsBlockingAllAttacksAfterHitStun = from.m_IsBlockingAllAttacksAfterHitStun;
        to.m_BlockingAttacksDuration = from.m_BlockingAttacksDuration;
        to.m_IsInvincible = from.m_IsInvincible;
        to.m_IsImmuneToStunGauge = from.m_IsImmuneToStunGauge;
    }

    public static void LoadTrainingOptions()
    {
        m_Player1Settings = ScenesConfig.GetPlayerSettings(0, "TrainingMap");
        m_Player2Settings = ScenesConfig.GetPlayerSettings(1, "TrainingMap");
        m_DebugSettings = ScenesConfig.GetDebugSettings("TrainingMap");

#if UNITY_EDITOR
        m_InitialPlayer1Settings = new PlayerSettings(Player.Player1);
        CopyPlayerSettings(m_Player1Settings, m_InitialPlayer1Settings);

        m_InitialPlayer2Settings = new PlayerSettings(Player.Player2);
        CopyPlayerSettings(m_Player2Settings, m_InitialPlayer2Settings);
#endif

        string trainingOptionsStr = PlayerPrefs.GetString("TrainingOptions");
        if (string.IsNullOrEmpty(trainingOptionsStr))
        {
            K_DEFAULT_TRAINING_OPTIONS.CopyTo(m_TrainingOptions, 0);
            return;
        }

        if(trainingOptionsStr.Length != m_TrainingOptions.Length)
        {
            Debug.LogError("LoadTrainingOptions failed");
            return;
        }

        for (int i = 0; i < trainingOptionsStr.Length; i++)
        {
            int loadedValue = int.Parse(trainingOptionsStr[i].ToString());
            UpdateTrainingOptions((ETrainingOption)i, loadedValue);
        }
    }

    private static void SaveTrainingOptions()
    {
        string trainingOptionsStr = "";
        for(int i = 0; i < m_TrainingOptions.Length; i++)
        {
            trainingOptionsStr += m_TrainingOptions[i].ToString();
        }

        PlayerPrefs.SetString("TrainingOptions", trainingOptionsStr);
    }

    private void OnDestroy()
    {
        TrainingOptionListener.OnValueChangedCallback -= OnTrainingOptionChanged;

#if UNITY_EDITOR
        CopyPlayerSettings(m_InitialPlayer1Settings, m_Player1Settings);
        CopyPlayerSettings(m_InitialPlayer2Settings, m_Player2Settings);
#endif
    }

    public void OnEnable()
    {
        m_DefaultSelectable.Select();
        UpdateHighlightedGameObject(m_TrainingOptionsHighlightInfo);
    }

    protected override void OnUpdate_Internal()
    {
        base.OnUpdate_Internal();
        UpdateHighlightedGameObject(m_TrainingOptionsHighlightInfo);
        UpdateScrollView();
    }

    protected void UpdateScrollView()
    {
        bool highlightImageEnabledFound = false;
        for (int i = 0; i < m_TrainingOptionsHighlightInfo.Length; i++)
        {
            if (m_TrainingOptionsHighlightInfo[i].m_HighlightImage.enabled)
            {
                m_ScrollRect.verticalNormalizedPosition = m_ScrollViewNormalizedPosition[i];
                highlightImageEnabledFound = true;
                break;
            }
        }

        if(!highlightImageEnabledFound)
        {
            m_DefaultSelectable.Select();
        }
    }

    private void OnTrainingOptionChanged(ETrainingOption trainingOption, int currentValue)
    {
        UpdateTrainingOptions(trainingOption, currentValue);
        SaveTrainingOptions();
    }

    private static void UpdateTrainingOptions(ETrainingOption trainingOption, int currentValue)
    {
        m_TrainingOptions[(int)trainingOption] = currentValue;

        switch (trainingOption)
        {
            case ETrainingOption.Player2Mode:
                switch ((EPlayer2TrainingMode)currentValue)
                {
                    case EPlayer2TrainingMode.Dummy:
                        m_Player2Settings.m_AttackEnabled = false;
                        m_Player2Settings.m_IsStatic = true;
                        break;
                    case EPlayer2TrainingMode.Player:
                        m_Player2Settings.m_AttackEnabled = true;
                        m_Player2Settings.m_IsStatic = false;
                        break;
                    default:
                        Debug.LogError("Player2TrainingMode isn't valid.");
                        break;
                }
                break;
            case ETrainingOption.StanceMode:
                m_Player2Settings.m_DefaultStance = (EPlayerStance)currentValue;
                break;
            case ETrainingOption.GuardMode:
                switch ((EGuardTrainingMode)currentValue)
                {
                    case EGuardTrainingMode.None:
                        m_Player2Settings.m_IsBlockingAllAttacks = false;
                        m_Player2Settings.m_IsBlockingAllAttacksAfterHitStun = false;
                        break;
                    case EGuardTrainingMode.AfterHit:
                        m_Player2Settings.m_IsBlockingAllAttacks = false;
                        m_Player2Settings.m_IsBlockingAllAttacksAfterHitStun = true;
                        break;
                    case EGuardTrainingMode.Always:
                        m_Player2Settings.m_IsBlockingAllAttacks = true;
                        m_Player2Settings.m_IsBlockingAllAttacksAfterHitStun = false;
                        break;
                    default:
                        Debug.LogError("GuardTrainingMode isn't valid.");
                        break;
                }
                break;
            case ETrainingOption.InfiniteSuperGauge:
                m_Player1Settings.SuperGaugeAlwaysFilled = m_Player2Settings.SuperGaugeAlwaysFilled = currentValue == 0 ? false : true;
                break;
            case ETrainingOption.InfiniteTriggerPoint:
                m_Player1Settings.TriggerPointAlwaysActive = m_Player2Settings.TriggerPointAlwaysActive = currentValue == 0 ? false : true;
                break;
            case ETrainingOption.ImmuneToStun:
                m_Player1Settings.m_IsImmuneToStunGauge = m_Player2Settings.m_IsImmuneToStunGauge = currentValue == 0 ? false : true;
                break;
            case ETrainingOption.DisplayDamage:
                m_Player1Settings.m_DisplayDamageTaken = m_Player2Settings.m_DisplayDamageTaken = currentValue == 0 ? false : true;
                break;
            case ETrainingOption.DisplayInputs:
                m_DebugSettings.m_DisplayInputsInfo = m_DebugSettings.m_DisplayAttacksInfo = currentValue == 0 ? false : true;
                break;
            case ETrainingOption.DisplayHitboxes:
                m_DebugSettings.m_DisplayBoxColliders = currentValue == 0 ? false : true;
                break;
        }
    }
}
