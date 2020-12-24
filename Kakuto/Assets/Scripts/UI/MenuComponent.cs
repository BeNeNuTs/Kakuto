using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class MenuComponent : MonoBehaviour
{
    [Serializable]
    protected struct MenuData
    {
#pragma warning disable 0649
        public UnityEngine.Object[] m_ObjectsToDisable;
        public UnityEngine.Object[] m_ObjectsToEnable;

        public List<Button> m_ButtonList;
        public Button m_DefaultHighlightedButton;
#pragma warning restore 0649
    }

    private Button m_CurrentHighlightedButton;

    private void Awake()
    {
        OnAwake_Internal();
    }

    private void OnDestroy()
    {
        OnDestroy_Internal();
    }

    protected virtual void OnAwake_Internal() { }
    protected virtual void OnDestroy_Internal() { }

    private void Update()
    {
        OnUpdate_Internal();
    }

    protected virtual void OnUpdate_Internal() { }

    protected void UpdateCursorVisiblity()
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

    protected void UpdateHighlightedButton(MenuData menuData)
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

    protected void GoToMenu(MenuData data)
    {
        SetActive(data.m_ObjectsToDisable, false);
        SetActive(data.m_ObjectsToEnable, true);

        data.m_DefaultHighlightedButton?.Select();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    protected void SetActive(UnityEngine.Object[] objs, bool active)
    {
        if (objs != null)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                SetActive(objs[i], active);
            }
        }
    }

    protected void SetActive(UnityEngine.Object obj, bool active)
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
