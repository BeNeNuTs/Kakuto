using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamePauseMenuComponent : MenuComponent
{
    protected enum EMenuState
    {
        Disabled,
        PauseMenu,
        Options,
        TrainingOptions,
        QuitToMainMenuConfirmation
    }

    protected struct PlayerInfo
    {
        public Animator m_Animator;
        bool m_AnimatorUpdateUnscaled;
        public PlayerMovementComponent m_MovementComponent;
        bool m_MovementWasEnabled;
        public PlayerAttackComponent m_AttackComponent;
        bool m_AttackWasEnabled;

        public void OnPauseGame()
        {
            m_AnimatorUpdateUnscaled = m_Animator.updateMode == AnimatorUpdateMode.UnscaledTime;
            m_Animator.updateMode = AnimatorUpdateMode.Normal;

            m_MovementWasEnabled = m_MovementComponent.enabled;
            m_MovementComponent.enabled = false;
            m_AttackWasEnabled = m_AttackComponent.enabled;
            m_AttackComponent.enabled = false;
        }

        public void OnUnpauseGame()
        {
            if (m_AnimatorUpdateUnscaled)
            {
                m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            m_MovementComponent.enabled = m_AttackWasEnabled;
            m_AttackComponent.enabled = m_AttackWasEnabled;
        }
    }

#pragma warning disable 0649
    [Header("Pause")]
    [SerializeField] private MenuData m_GoToPauseMenuData;
    [SerializeField] private Sprite m_Player1PauseSprite;
    [SerializeField] private Sprite m_Player2PauseSprite;
    [SerializeField] private Image m_PauseImage;
    [SerializeField] private MenuData m_QuitPauseMenuData;

    [Header("Options")]
    [SerializeField] private Button m_OptionButton;
    [SerializeField] private MenuData m_GoToOptionsData;

    [Header("Quit")]
    [SerializeField] private Button m_QuitToMainMenuButton;
    [SerializeField] private MenuData m_QuiToMainMenuConfirmationData;
#pragma warning restore 0649

    public static bool m_IsInPause = false;

    protected EMenuState m_MenuState = EMenuState.Disabled;
    protected EPlayer m_PausePlayer = EPlayer.Player1;

    protected PlayerInfo[] m_PlayerInfos = new PlayerInfo[2];

    protected TimeScaleSubGameManager m_TimeScaleManager;

    protected override void OnAwake_Internal()
    {
        GameManager.Instance.AddOnPlayerRegisteredCallback(OnPlayerRegistered, EPlayer.Player1);
        GameManager.Instance.AddOnPlayerRegisteredCallback(OnPlayerRegistered, EPlayer.Player2);

        m_TimeScaleManager = GameManager.Instance.GetSubManager<TimeScaleSubGameManager>(ESubManager.TimeScale);
    }

    protected override void OnDestroy_Internal()
    {
        GameManager.Instance?.RemoveOnPlayerRegisteredCallback(OnPlayerRegistered, EPlayer.Player1);
        GameManager.Instance?.RemoveOnPlayerRegisteredCallback(OnPlayerRegistered, EPlayer.Player2);
    }

    private void OnPlayerRegistered(GameObject player)
    {
        int playerIndex = 0;
        if (player.CompareTag(Player.Player1))
        {
            playerIndex = 0;
            GameManager.Instance.RemoveOnPlayerRegisteredCallback(OnPlayerRegistered, EPlayer.Player1);
        }
        else
        {
            playerIndex = 1;
            GameManager.Instance.RemoveOnPlayerRegisteredCallback(OnPlayerRegistered, EPlayer.Player2);
        }
        m_PlayerInfos[playerIndex].m_Animator = player.GetComponentInChildren<Animator>();
        m_PlayerInfos[playerIndex].m_MovementComponent = player.GetComponent<PlayerMovementComponent>();
        m_PlayerInfos[playerIndex].m_AttackComponent = player.GetComponent<PlayerAttackComponent>();
    }

    protected override void OnUpdate_Internal()
    {
        UpdateCursorVisiblity();

        if (m_MenuState == EMenuState.Disabled)
        {
            if (InputManager.GetStartInput(out m_PausePlayer))
            {
                m_PauseImage.sprite = (m_PausePlayer == EPlayer.Player1) ? m_Player1PauseSprite : m_Player2PauseSprite;
                GoToPauseMenu();
                return;
            }
        }
        else
        {
            if (InputManager.GetStartInput(out EPlayer startInputPlayer))
            {
                if (startInputPlayer == m_PausePlayer)
                {
                    DisablePauseMenu();
                    return;
                }
            }

            switch (m_MenuState)
            {
                case EMenuState.PauseMenu:
                    UpdateHighlightedButton(m_GoToPauseMenuData);
                    UpdateButtonClick();
                    UpdateDpadNavigation();
                    if (InputManager.GetBackInput(out EPlayer backInputPlayer))
                    {
                        if (backInputPlayer == m_PausePlayer)
                        {
                            DisablePauseMenu();
                        }
                    }
                    break;
                case EMenuState.Options:
                    UpdateHighlightedButton(m_GoToOptionsData);
                    if (!InputListener.m_IsListeningInput)
                    {
                        UpdateButtonClick();
                        UpdateDpadNavigation();
                        if (InputManager.GetBackInput())
                        {
                            GoToPauseMenu();
                        }
                    }
                    break;
                case EMenuState.QuitToMainMenuConfirmation:
                    UpdateHighlightedButton(m_QuiToMainMenuConfirmationData);
                    UpdateButtonClick();
                    UpdateDpadNavigation();
                    if (InputManager.GetBackInput())
                    {
                        GoToPauseMenu();
                    }
                    break;
            }
        }
    }

    protected void PauseGame()
    {
        if (!m_TimeScaleManager.IsTimeFrozen)
        {
            m_TimeScaleManager.FreezeTime();
            m_PlayerInfos[0].OnPauseGame();
            m_PlayerInfos[1].OnPauseGame();
            m_IsInPause = true;
        }
    }

    protected void UnpauseGame()
    {
        if (m_TimeScaleManager.IsTimeFrozen)
        {
            m_PlayerInfos[0].OnUnpauseGame();
            m_PlayerInfos[1].OnUnpauseGame();
            m_TimeScaleManager.UnfreezeTime();
            m_IsInPause = false;
        }
    }

    public void GoToPauseMenu()
    {
        GoToMenu(m_GoToPauseMenuData);
        PauseGame();

        switch (m_MenuState)
        {
            case EMenuState.Options:
                m_OptionButton?.Select();
                break;
            case EMenuState.TrainingOptions:
                OnGoToPauseMenuFromTrainingOptions();
                break;
            case EMenuState.QuitToMainMenuConfirmation:
                m_QuitToMainMenuButton?.Select();
                break;
        }
        m_MenuState = EMenuState.PauseMenu;
    }

    protected virtual void OnGoToPauseMenuFromTrainingOptions() { }

    public void DisplayQuitToMainMenuConfirmation()
    {
        GoToMenu(m_QuiToMainMenuConfirmationData);
        m_MenuState = EMenuState.QuitToMainMenuConfirmation;
    }

    public void DisablePauseMenu()
    {
        GoToMenu(m_QuitPauseMenuData);
        EventSystem.current.SetSelectedGameObject(null);
        UnpauseGame();
        m_MenuState = EMenuState.Disabled;
    }

    public void GoToOptionsMenu()
    {
        GoToMenu(m_GoToOptionsData);
        m_MenuState = EMenuState.Options;
    }
}
