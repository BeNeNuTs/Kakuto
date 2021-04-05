using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EPlayer2TrainingMode
{
    Dummy,
    Player
}

public enum EGuardTrainingMode
{
    None,
    AfterHit,
    Always
}

public enum ETrainingOption
{
    Player2Mode,
    StanceMode,
    GuardMode,
    InfiniteSuperGauge,
    InfiniteTriggerPoint,
    ImmuneToStun,
    DisplayDamage,
    DisplayInputs,
    DisplayHitboxes

    // Please update K_MAX_TRAINING_OPTIONS & K_DEFAULT_TRAINING_OPTIONS
}

public class TrainingOptionListener : MonoBehaviour
{
    public static readonly int K_MAX_TRAINING_OPTIONS = 9;

    public ETrainingOption m_TrainingOption;
    public bool m_IsModeOption = false;
    [ConditionalField(false, "m_IsModeOption")]
    public Toggle m_Toggle;
    [ConditionalField(true, "m_IsModeOption")]
    public Selectable m_Selectable;
    [ConditionalField(true, "m_IsModeOption")]
    public Image m_HighlightImage;
    [ConditionalField(true, "m_IsModeOption")]
    public GameObject m_LeftArrow;
    [ConditionalField(true, "m_IsModeOption")]
    public GameObject m_RightArrow;

    [ConditionalField(true, "m_IsModeOption")]
    public TextMeshProUGUI m_ModeText;
    public string[] m_ModeOptions;

    public static Action<ETrainingOption, int> OnValueChangedCallback;

    private int m_CurrentValue = 0;

    public void Init(int value)
    {
        m_CurrentValue = value;
        if(m_IsModeOption)
        {
            m_ModeText.text = m_ModeOptions[m_CurrentValue];
        }
        else
        {
            m_Toggle.isOn = m_CurrentValue == 1;
        }
    }

    private void Update()
    {
        if(m_IsModeOption && m_HighlightImage.enabled)
        {
            UpdateCurrentMode();
        }
    }

    private void UpdateCurrentMode()
    {
        if (EventSystem.current != null)
        {
            GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (currentSelectedGameObject == m_LeftArrow)
            {
                m_CurrentValue--;
                OnModeChanged();
            }
            else if (currentSelectedGameObject == m_RightArrow)
            {
                m_CurrentValue++;
                OnModeChanged();
            }
        }
    }

    private void OnModeChanged()
    {
        if(m_CurrentValue < 0)
        {
            m_CurrentValue = m_ModeOptions.Length - 1;
        }
        else
        {
            m_CurrentValue = m_CurrentValue % m_ModeOptions.Length;
        }
        m_ModeText.text = m_ModeOptions[m_CurrentValue];
        m_Selectable.Select();
        MenuComponent.m_AudioManager.PlayUISFX(EUISFXType.Navigation);

        OnValueChangedCallback?.Invoke(m_TrainingOption, m_CurrentValue);
    }

    public void OnValueChanged(bool value)
    {
        if(!m_IsModeOption)
        {
            m_CurrentValue = value ? 1 : 0;
            OnValueChangedCallback?.Invoke(m_TrainingOption, m_CurrentValue);
        }
    }
}
