using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuComponent : MenuComponent
{
#pragma warning disable 0649
    [Serializable]
    class PlayerControlInfo
    {
        public TextMeshProUGUI m_XboxConsoleText;
        public TextMeshProUGUI m_PS4ConsoleText;
        public InputListener[] m_InputListeners;
    }

    [SerializeField] private Button[] m_OptionButtons;
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_ControlsHighlightInfo;

    [SerializeField] private PlayerControlInfo m_Player1ControlsMapping;
    [SerializeField] private PlayerControlInfo m_Player2ControlsMapping;

    public float m_WrongPlayerFeedbackDuration = 0.5f;
    public float m_WrongPlayerFeedbackSpeed = 1f;
#pragma warning restore 0649

    private IEnumerator m_WrongPlayerFeedbackCoroutine;

    protected void Awake()
    {
        InputListener.OnInputChanged += OnPlayerInputChanged;
    }

    protected void OnDestroy()
    {
        InputListener.OnInputChanged -= OnPlayerInputChanged;
    }

    protected void OnEnable()
    {
        for (int i = 0; i < m_OptionButtons.Length; i++)
        {
            Navigation buttonNavigation = m_OptionButtons[i].navigation;
            buttonNavigation.selectOnDown = m_DefaultSelectable;
            m_OptionButtons[i].navigation = buttonNavigation;
        }
        UpdateHighlightedGameObject(m_ControlsHighlightInfo);

        EGamePadType playerGamepadType = GamePadManager.GetPlayerGamepadType((int)EPlayer.Player1, OnPlayer1GamepadTypeChanged);
        UpdateInputForGamepadType(playerGamepadType, m_Player1ControlsMapping);

        playerGamepadType = GamePadManager.GetPlayerGamepadType((int)EPlayer.Player2, OnPlayer2GamepadTypeChanged);
        UpdateInputForGamepadType(playerGamepadType, m_Player2ControlsMapping);
    }

    private void OnPlayer1GamepadTypeChanged(EGamePadType newGamePadType)
    {
        UpdateInputForGamepadType(newGamePadType, m_Player1ControlsMapping);
    }

    private void OnPlayer2GamepadTypeChanged(EGamePadType newGamePadType)
    {
        UpdateInputForGamepadType(newGamePadType, m_Player2ControlsMapping);
    }

    private void UpdateInputForGamepadType(EGamePadType gamePadType, PlayerControlInfo playerControlInfos)
    {
        bool isXboxGamepad = gamePadType == EGamePadType.Xbox;

        playerControlInfos.m_PS4ConsoleText.enabled = !isXboxGamepad;
        playerControlInfos.m_XboxConsoleText.enabled = isXboxGamepad;

        for(int i = 0; i < playerControlInfos.m_InputListeners.Length; i++)
        {
            playerControlInfos.m_InputListeners[i].UpdateInputForGamepadType(gamePadType);
        }
    }

    protected void Update()
    {
        UpdateHighlightedGameObject(m_ControlsHighlightInfo);
    }

    public void ResetPlayerInputMapping(int playerIndex)
    {
        // Check if it's the correct player's trying to update input
        List<GameInput> gamepadInputList = GamePadManager.GetPlayerGamepadInput(playerIndex);
        if (gamepadInputList.Exists(x => x.GetInputKey() == EInputKey.A))
        {
            GamePadManager.ResetPlayerGamepadInputMapping(playerIndex);
            PlayerControlInfo playerControlInfo = playerIndex == 0 ? m_Player1ControlsMapping : m_Player2ControlsMapping;
            for (int i = 0; i < playerControlInfo.m_InputListeners.Length; i++)
            {
                playerControlInfo.m_InputListeners[i].ResetInputMapping();
            }
        }
        else
        {
            DisplayWrongPlayerFeedback();
        }
    }

    public void DisplayWrongPlayerFeedback()
    {
        if (m_WrongPlayerFeedbackCoroutine != null)
        {
            StopCoroutine(m_WrongPlayerFeedbackCoroutine);
        }
        m_WrongPlayerFeedbackCoroutine = WrongPlayerFeedback_Coroutine();
        StartCoroutine(m_WrongPlayerFeedbackCoroutine);
    }

    private IEnumerator WrongPlayerFeedback_Coroutine()
    {
        Image highlightImage = null;
        HighlightInfo highlightInfo = GetCurrentHighlight();
        if (highlightInfo != null)
        {
            highlightImage = highlightInfo.m_HighlightImage;
        }

        if (highlightImage != null)
        {
            float startingTime = Time.unscaledTime;
            while (Time.unscaledTime < startingTime + m_WrongPlayerFeedbackDuration)
            {
                if (Mathf.Sin((Time.unscaledTime - startingTime) * m_WrongPlayerFeedbackSpeed) > 0f)
                {
                    highlightImage.color = Color.red;
                }
                else
                {
                    highlightImage.color = Color.white;
                }
                yield return null;
            }

            highlightImage.color = Color.white;
        }
    }

    private void OnPlayerInputChanged(InputListener inputListener)
    {
        PlayerControlInfo playerControlInfos = inputListener.m_Player == EPlayer.Player1 ? m_Player1ControlsMapping : m_Player2ControlsMapping;
        List<EInputKey> unassignedInputs = new List<EInputKey>(PlayerGamePad.K_ASSIGNABLE_INPUTS);
        for (int i = 0; i < playerControlInfos.m_InputListeners.Length; i++)
        {
            if(playerControlInfos.m_InputListeners[i] != inputListener && playerControlInfos.m_InputListeners[i].m_CurrentInputKey == inputListener.m_CurrentInputKey)
            {
                EGamePadType playerGamepadType = GamePadManager.GetPlayerGamepadType((int)inputListener.m_Player);

                playerControlInfos.m_InputListeners[i].m_CurrentInputKey = inputListener.m_OldInputKey;
                playerControlInfos.m_InputListeners[i].UpdateInputForGamepadType(playerGamepadType);

                GamePadManager.SetPlayerGamepadInputMapping((int)inputListener.m_Player, inputListener.m_OldInputKey, playerControlInfos.m_InputListeners[i].m_DefaultInputKey);
            }

            unassignedInputs.Remove(playerControlInfos.m_InputListeners[i].m_CurrentInputKey);
        }

#if UNITY_EDITOR || DEBUG_DISPLAY
        if(unassignedInputs.Count > 1)
        {
            KakutoDebug.LogError("There should be only one unassigned remaining input after OnPlayerInputChanged");
        }
#endif

        for(int i = 0; i < unassignedInputs.Count; i++)
        {
            GamePadManager.SetPlayerGamepadInputMapping((int)inputListener.m_Player, unassignedInputs[i], PlayerGamePad.K_DEFAULT_UNASSIGNED_INPUT);
        }
    }
}
