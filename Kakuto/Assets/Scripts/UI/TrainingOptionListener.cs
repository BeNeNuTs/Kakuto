using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrainingOptionListener : MonoBehaviour
{
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
    }

    public ETrainingOption m_TrainingOption;
    public bool m_IsModeOption = false;
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

    public static Action<ETrainingOption, bool, int> OnValueChangedCallback;

    private int m_CurrentValue = 0;

    private void OnEnable()
    {
        //Load info
    }

    private void Update()
    {
        if(m_IsModeOption && m_HighlightImage.enabled)
        {
            UpdatecCurrentMode();
        }
    }

    private void UpdatecCurrentMode()
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

        OnValueChangedCallback?.Invoke(m_TrainingOption, m_IsModeOption, m_CurrentValue);
    }

    public void OnValueChanged(bool value)
    {
        if(!m_IsModeOption)
        {
            m_CurrentValue = value ? 1 : 0;
            OnValueChangedCallback?.Invoke(m_TrainingOption, m_IsModeOption, m_CurrentValue);
        }
    }
}
