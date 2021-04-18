using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public static AudioSubGameManager m_AudioManager;

    private Button m_CurrentHighlightedButton;
    private Button m_LastValidHighlightedButton;
    private HighlightInfo m_CurrentHighlight = null;

    private float m_LastDpadNavigationUpdatedTime = 0f;
    private bool m_ActiveDpadNavigationRepeatDelay = false;
    private bool m_DpadNavigationRepeatDelayChecked = false;

    protected virtual void Awake()
    {
        if(m_AudioManager == null)
            m_AudioManager = GameManager.Instance.GetSubManager<AudioSubGameManager>(ESubManager.Audio);
    }

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
                if (selectedButton != m_CurrentHighlightedButton)
                {
                    if (selectedButton != null && 
                        ((m_CurrentHighlightedButton == null && menuData.m_ButtonList.Contains(selectedButton) && menuData.m_ButtonList.Contains(m_LastValidHighlightedButton)) ||
                        menuData.m_ButtonList.Contains(m_CurrentHighlightedButton)))
                    {
                        m_AudioManager.PlayUISFX(EUISFXType.Navigation);
                    }

                    if(m_CurrentHighlightedButton != null)
                    {
                        m_LastValidHighlightedButton = m_CurrentHighlightedButton;
                    }
                    m_CurrentHighlightedButton = selectedButton;
                }
            }
            else
            {
                if (menuData.m_ButtonList.Contains(m_CurrentHighlightedButton))
                {
                    m_CurrentHighlightedButton?.Select();
                }
                else if (menuData.m_ButtonList.Contains(m_LastValidHighlightedButton))
                {
                    m_CurrentHighlightedButton = m_LastValidHighlightedButton;
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
                    bool needNavigationSFX = false;
                    for (int i = 0; i < highlightInfos.Length; i++)
                    {
                        if (selectedGO == highlightInfos[i].m_SelectedGameObject)
                        {
                            if(m_CurrentHighlight != null)
                            {
                                m_CurrentHighlight.m_HighlightImage.enabled = false;
                            }
                            needNavigationSFX = m_CurrentHighlight == null || m_CurrentHighlight.m_HighlightImage != highlightInfos[i].m_HighlightImage;

                            m_CurrentHighlight = highlightInfos[i];
                            m_CurrentHighlight.m_HighlightImage.enabled = true;
                            newHighlightFound = true;
                        }
                    }

                    if(!newHighlightFound)
                    {
                        if (m_CurrentHighlight != null)
                        {
                            if(m_CurrentHighlight.m_HighlightImage != null)
                                m_CurrentHighlight.m_HighlightImage.enabled = false;
                            m_CurrentHighlight = null;
                        }
                    }
                    else if(needNavigationSFX)
                    {
                        m_AudioManager.PlayUISFX(EUISFXType.Navigation);
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
            GameObject selectedGameObject = EventSystem.current?.currentSelectedGameObject;
            if(selectedGameObject != null)
            {
                selectedGameObject.GetComponent<ISubmitHandler>()?.OnSubmit(new BaseEventData(EventSystem.current));
                Button button = selectedGameObject.GetComponent<Button>();
                if(button != null && button.onClick != null && button.onClick.GetPersistentEventCount() > 0)
                {
                    m_AudioManager.PlayUISFX(EUISFXType.Confirm);
                }
                else if(selectedGameObject.GetComponent<Toggle>() != null)
                {
                    m_AudioManager.PlayUISFX(EUISFXType.Toggle);
                }
            }
        }
    }

    protected void UpdateDpadNavigation()
    {
        CheckPlayerDpadDirectionWithDelay(UpdateSelectable);
    }

    private void UpdateSelectable(EPlayer player, float horizontalAxis, float verticalAxis)
    {
        EventSystem currentEventSystem = EventSystem.current;
        if (currentEventSystem != null)
        {
            GameObject selectedGO = currentEventSystem.currentSelectedGameObject;
            if (selectedGO != null)
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
                }
            }
        }
    }

    protected void CheckPlayerDpadDirectionWithDelay(Action<EPlayer, float, float> onPlayerDpadDirectionTriggered)
    {
        float unscaledTime = Time.unscaledTime;
        if (m_DpadNavigationRepeatDelayChecked || !m_ActiveDpadNavigationRepeatDelay || unscaledTime > m_LastDpadNavigationUpdatedTime + UIConfig.Instance.m_DpadNavigationInputRepeatDelay)
        {
            if (unscaledTime > m_LastDpadNavigationUpdatedTime + UIConfig.Instance.DpadNavigationInputPerSecond)
            {
                if (GamePadManager.GetAnyPlayerDpadDirection(out EPlayer player, out float horizontalAxis, out float verticalAxis))
                {
                    onPlayerDpadDirectionTriggered.Invoke(player, horizontalAxis, verticalAxis);

                    m_LastDpadNavigationUpdatedTime = Time.unscaledTime;
                    if (m_ActiveDpadNavigationRepeatDelay)
                        m_DpadNavigationRepeatDelayChecked = true;
                    else
                        m_ActiveDpadNavigationRepeatDelay = true;
                }
                else
                {
                    m_ActiveDpadNavigationRepeatDelay = false;
                    m_DpadNavigationRepeatDelayChecked = false;
                }
            }
        }
        else if (!GamePadManager.GetAnyPlayerDpadDirection(out EPlayer player, out float horizontalAxis, out float verticalAxis))
        {
            m_ActiveDpadNavigationRepeatDelay = false;
            m_DpadNavigationRepeatDelayChecked = false;
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

    public Button GetCurrentHighlightedButton() { return m_CurrentHighlightedButton; }
    public HighlightInfo GetCurrentHighlight() { return m_CurrentHighlight; }
}
