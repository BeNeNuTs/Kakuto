using System;
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
#pragma warning restore 0649

    public void Awake()
    {
        InputListener.OnInputChanged += OnPlayerInputChanged;
    }

    public void OnDestroy()
    {
        InputListener.OnInputChanged -= OnPlayerInputChanged;
    }

    public void OnEnable()
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

    protected override void OnUpdate_Internal()
    {
        base.OnUpdate_Internal();
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
    }

    private void OnPlayerInputChanged(InputListener inputListener)
    {
        PlayerControlInfo playerControlInfos = inputListener.m_Player == EPlayer.Player1 ? m_Player1ControlsMapping : m_Player2ControlsMapping;
        for (int i = 0; i < playerControlInfos.m_InputListeners.Length; i++)
        {
            if(playerControlInfos.m_InputListeners[i] != inputListener && playerControlInfos.m_InputListeners[i].m_CurrentInputKey == inputListener.m_CurrentInputKey)
            {
                EGamePadType playerGamepadType = GamePadManager.GetPlayerGamepadType((int)inputListener.m_Player);

                playerControlInfos.m_InputListeners[i].m_CurrentInputKey = inputListener.m_OldInputKey;
                playerControlInfos.m_InputListeners[i].UpdateInputForGamepadType(playerGamepadType);

                GamePadManager.SetPlayerGamepadInputMapping((int)inputListener.m_Player, playerControlInfos.m_InputListeners[i].m_CurrentInputKey, playerControlInfos.m_InputListeners[i].m_DefaultInputKey);
            }
        }
    }
}
