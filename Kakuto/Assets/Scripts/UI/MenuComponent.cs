using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
#pragma warning disable 0649
        public UnityEngine.Object[] m_ObjectsToDisable;
        public UnityEngine.Object[] m_ObjectsToEnable;

        public List<Button> m_ButtonList;
        public Button m_DefaultHighlightedButton;
#pragma warning restore 0649
    }

#pragma warning disable 0649
    [SerializeField] private MenuData m_GoToMainMenuData;
    [SerializeField] private MenuData m_GoToOptionsData;
#pragma warning restore 0649

    private EMenuState m_MenuState = EMenuState.TitleScreen;
    private Button m_CurrentHighlightedButton;

    private void Update()
    {
        UpdateCursorVisiblity();

        switch (m_MenuState)
        {
            case EMenuState.TitleScreen:
                if(InputManager.GetStartInput())
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
                break;
        }
    }

    private void UpdateCursorVisiblity()
    {
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateHighlightedButton(MenuData menuData)
    {
        GameObject selectedGO = EventSystem.current.currentSelectedGameObject;
        if(selectedGO != null)
        {
            Button selectedButton = selectedGO.GetComponent<Button>();
            if(selectedButton != null && selectedButton != m_CurrentHighlightedButton)
            {
                m_CurrentHighlightedButton = selectedButton;
            }
        }
        else
        {
            if (menuData.m_ButtonList.Contains(m_CurrentHighlightedButton))
            {
                m_CurrentHighlightedButton.Select();
            }
            else
            {
                menuData.m_DefaultHighlightedButton.Select();
                m_CurrentHighlightedButton = menuData.m_DefaultHighlightedButton;
            }
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

    public void GoToMainMenu()
    {
        GoToMenu(m_GoToMainMenuData);
        m_MenuState = EMenuState.MainMenu;
    }

    public void GoToOptionsMenu()
    {
        GoToMenu(m_GoToOptionsData);
        m_MenuState = EMenuState.Options;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
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
