﻿using UnityEngine;
using UnityEngine.UI;

public class TrainingPauseMenuComponent : GamePauseMenuComponent
{
#pragma warning disable 0649
    [Header("Training options")]
    [SerializeField] private Button m_TrainingOptionsButton;
    [SerializeField] private MenuData m_GoToTrainingOptionsMenuData;
#pragma warning restore 0649

    protected override void Update()
    {
        base.Update();

        if(m_MenuState == EMenuState.TrainingOptions)
        {
            UpdateHighlightedButton(m_GoToTrainingOptionsMenuData);
            if (!InputListener.m_IsListeningInput)
            {
                UpdateButtonClick();
                UpdateDpadNavigation();
                if (InputManager.GetBackInput())
                {
                    GoToPauseMenu();
                    m_AudioManager.PlayUISFX(EUISFXType.Back);
                }
            }
        }
    }

    public void GoToTrainingOptionsMenu()
    {
        GoToMenu(m_GoToTrainingOptionsMenuData);
        m_MenuState = EMenuState.TrainingOptions;
    }

    protected override void OnGoToPauseMenuFromTrainingOptions()
    {
        m_TrainingOptionsButton?.Select();
    }
}
