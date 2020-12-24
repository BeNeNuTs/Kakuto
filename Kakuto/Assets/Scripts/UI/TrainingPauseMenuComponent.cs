using UnityEngine;

public class TrainingPauseMenuComponent : GamePauseMenuComponent
{
#pragma warning disable 0649
    [SerializeField] private MenuData m_GoToTrainingOptionsMenuData;
#pragma warning restore 0649

    protected override void OnUpdate_Internal()
    {
        base.OnUpdate_Internal();
        if(m_MenuState == EMenuState.TrainingOptions)
        {
            UpdateHighlightedButton(m_GoToTrainingOptionsMenuData);
            if (InputManager.GetBackInput())
            {
                GoToPauseMenu();
            }
        }
    }

    public void GoToTrainingOptionsMenu()
    {
        GoToMenu(m_GoToTrainingOptionsMenuData);
        m_MenuState = EMenuState.TrainingOptions;
    }
}
