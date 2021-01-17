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

    [Serializable]
    public class HighlightInfo
    {
        public GameObject m_SelectedGameObject;
        public Image m_HighlightImage;
    }

    private Button m_CurrentHighlightedButton;
    private HighlightInfo m_CurrentHighlight = null;

    private float m_LastDpadNavigationUpdatedTime = 0f;
    private bool m_ActiveDpadNavigationRepeatDelay = false;
    private bool m_DpadNavigationRepeatDelayChecked = false;

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
        if(EventSystem.current != null)
        {
            GameObject selectedGO = EventSystem.current.currentSelectedGameObject;
            if (selectedGO != null)
            {
                Button selectedButton = selectedGO.GetComponent<Button>();
                if (selectedButton != null && selectedButton != m_CurrentHighlightedButton)
                {
                    m_CurrentHighlightedButton = selectedButton;
                }
            }
            else
            {
                if (menuData.m_ButtonList.Contains(m_CurrentHighlightedButton))
                {
                    m_CurrentHighlightedButton?.Select();
                }
                else
                {
                    menuData.m_DefaultHighlightedButton?.Select();
                    m_CurrentHighlightedButton = menuData.m_DefaultHighlightedButton;
                }
            }
        }
    }

    protected void UpdateHighlightedGameObject(HighlightInfo[] highlightInfos)
    {
        if (EventSystem.current != null)
        {
            GameObject selectedGO = EventSystem.current.currentSelectedGameObject;
            if (selectedGO != null)
            {
                if(m_CurrentHighlight == null || selectedGO != m_CurrentHighlight.m_SelectedGameObject)
                {
                    bool newHighlightFound = false;
                    for (int i = 0; i < highlightInfos.Length; i++)
                    {
                        if (selectedGO == highlightInfos[i].m_SelectedGameObject)
                        {
                            if(m_CurrentHighlight != null)
                            {
                                m_CurrentHighlight.m_HighlightImage.enabled = false;
                            }
                            m_CurrentHighlight = highlightInfos[i];
                            m_CurrentHighlight.m_HighlightImage.enabled = true;
                            newHighlightFound = true;
                        }
                    }

                    if(!newHighlightFound)
                    {
                        if (m_CurrentHighlight != null)
                        {
                            m_CurrentHighlight.m_HighlightImage.enabled = false;
                            m_CurrentHighlight = null;
                        }
                    }
                }
            }
            else if (m_CurrentHighlight != null)
            {
                m_CurrentHighlight.m_HighlightImage.enabled = false;
                m_CurrentHighlight = null;
            }
        }
    }

    protected void UpdateButtonClick()
    {
        if (InputManager.GetSubmitInput(out EPlayer submitInputPlayer))
        {
            EventSystem.current?.currentSelectedGameObject?.GetComponent<ISubmitHandler>()?.OnSubmit(new BaseEventData(EventSystem.current));
        }
    }

    protected void UpdateDpadNavigation()
    {
        EventSystem currentEventSystem = EventSystem.current;
        if (currentEventSystem != null)
        {
            float unscaledTime = Time.unscaledTime;
            if(m_DpadNavigationRepeatDelayChecked || !m_ActiveDpadNavigationRepeatDelay || unscaledTime > m_LastDpadNavigationUpdatedTime + UIConfig.Instance.m_DpadNavigationInputRepeatDelay)
            {
                if (unscaledTime > m_LastDpadNavigationUpdatedTime + UIConfig.Instance.DpadNavigationInputPerSecond)
                {
                    GameObject selectedGO = currentEventSystem.currentSelectedGameObject;
                    if (selectedGO != null)
                    {
                        if (GamePadManager.GetAnyPlayerDpadDirection(out EPlayer player, out float horizontalAxis, out float verticalAxis))
                        {
                            Selectable selectableGO = selectedGO.GetComponent<Selectable>();
                            if (selectableGO != null)
                            {
                                if (horizontalAxis < 0f)
                                {
                                    selectableGO.FindSelectableOnLeft()?.Select();
                                }
                                else if (horizontalAxis > 0f)
                                {
                                    selectableGO.FindSelectableOnRight()?.Select();
                                }
                                else if (verticalAxis < 0f)
                                {
                                    selectableGO.FindSelectableOnDown()?.Select();
                                }
                                else if (verticalAxis > 0f)
                                {
                                    selectableGO.FindSelectableOnUp()?.Select();
                                }
                                m_LastDpadNavigationUpdatedTime = Time.unscaledTime;
                                if(m_ActiveDpadNavigationRepeatDelay)
                                    m_DpadNavigationRepeatDelayChecked = true;
                                else
                                    m_ActiveDpadNavigationRepeatDelay = true;
                            }
                        }
                        else
                        {
                            m_ActiveDpadNavigationRepeatDelay = false;
                            m_DpadNavigationRepeatDelayChecked = false;
                        }
                    }
                }
            }
            else if (!GamePadManager.GetAnyPlayerDpadDirection(out EPlayer player, out float horizontalAxis, out float verticalAxis))
            {
                m_ActiveDpadNavigationRepeatDelay = false;
                m_DpadNavigationRepeatDelayChecked = false;
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
        GameManager.Instance.GetSubManager<GameFlowSubGameManager>(ESubManager.GameFlow).LoadScene(sceneName, false);
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
