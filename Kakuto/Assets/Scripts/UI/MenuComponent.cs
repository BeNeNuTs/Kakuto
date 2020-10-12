using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuComponent : MonoBehaviour
{
    enum EMenuState
    {
        TitleScreen,
        MainMenu,
        Options
    }

    [Serializable]
    struct MenuData
    {
        public UnityEngine.Object[] m_ObjectsToDisable;
        public UnityEngine.Object[] m_ObjectsToEnable;

        public Button m_DefaultHighlightedButton;
    }

    [SerializeField] private MenuData m_GoToMainMenuData;
    [SerializeField] private MenuData m_GoToOptionsData;

    private EMenuState m_MenuState = EMenuState.TitleScreen;

    private void Update()
    {
        switch(m_MenuState)
        {
            case EMenuState.TitleScreen:
                if(InputManager.GetStartInput())
                {
                    GoToMenu(m_GoToMainMenuData);
                    m_MenuState = EMenuState.MainMenu;
                }
                break;

            case EMenuState.MainMenu:
                break;
            case EMenuState.Options:
                if (InputManager.GetBackInput())
                {
                    GoToMenu(m_GoToMainMenuData);
                    m_MenuState = EMenuState.MainMenu;
                }
                break;
        }
    }

    private void GoToMenu(MenuData data)
    {
        SetActive(data.m_ObjectsToDisable, false);
        SetActive(data.m_ObjectsToEnable, true);

        data.m_DefaultHighlightedButton.Select();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToOptionsMenu()
    {
        GoToMenu(m_GoToOptionsData);
        m_MenuState = EMenuState.Options;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Break();
#else
        Application.Quit();
#endif
    }

    private void SetActive(UnityEngine.Object[] objs, bool active)
    {
        if (objs != null)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                SetActive(objs[i], active);
            }
        }
    }

    private void SetActive(UnityEngine.Object obj, bool active)
    {
        if (obj is GameObject go)
        {
            go.SetActive(active);
        }
        else if (obj is Behaviour comp)
        {
            comp.enabled = active;
        }
    }
}
