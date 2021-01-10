using UnityEngine;
using UnityEngine.UI;

public class MainMenuComponent : MenuComponent
{
    enum EMenuState
    {
        TitleScreen,
        MainMenu,
        Options
    }

#pragma warning disable 0649
    [SerializeField] private MenuData m_GoToMainMenuData;
    [SerializeField] private Button m_OptionsButton;
    [SerializeField] private MenuData m_GoToOptionsData;
    [SerializeField] private HighlightInfo[] m_OptionsHighlightInfo;
#pragma warning restore 0649

    private EMenuState m_MenuState = EMenuState.TitleScreen;

    private void Update()
    {
        UpdateCursorVisiblity();

        switch (m_MenuState)
        {
            case EMenuState.TitleScreen:
                if(InputManager.GetStartInput(out EPlayer startInputPlayer))
                {
                    GoToMainMenu();
                }
                break;

            case EMenuState.MainMenu:
                UpdateHighlightedButton(m_GoToMainMenuData);
                break;
            case EMenuState.Options:
                UpdateHighlightedButton(m_GoToOptionsData);
                if (InputManager.GetBackInput())
                {
                    GoToMainMenu();
                }
                UpdateHighlightedGameObject(m_OptionsHighlightInfo);
                break;
        }
    }

    public void GoToMainMenu()
    {
        GoToMenu(m_GoToMainMenuData);
        if(m_MenuState == EMenuState.Options)
        {
            m_OptionsButton?.Select();
        }
        m_MenuState = EMenuState.MainMenu;
    }

    public void GoToOptionsMenu()
    {
        GoToMenu(m_GoToOptionsData);
        m_MenuState = EMenuState.Options;
    }
}
