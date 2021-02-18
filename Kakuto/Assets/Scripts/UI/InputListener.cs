using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputListener : MonoBehaviour
{
    public static bool m_IsListeningInput = false;
    public static Action<InputListener> OnInputChanged;

    public EPlayer m_Player;

    public EInputKey m_DefaultInputKey;
    public Image m_XboxImageInput;
    public Image m_PS4ImageInput;
    public TextMeshProUGUI m_PressAKeyText;
    public Button m_InputButton;
    public ControlsMenuComponent m_ControlMenuComponent;

    [NonSerialized] public EInputKey m_OldInputKey = EInputKey.Invalid;
    [NonSerialized] public EInputKey m_CurrentInputKey = EInputKey.Invalid;

    private EventSystem m_CurrentEventSystem;

    public void UpdateInputForGamepadType(EGamePadType gamePadType)
    {
        bool isXboxGamepad = gamePadType == EGamePadType.Xbox;

        UpdateInputSprite();
        m_PS4ImageInput.enabled = !isXboxGamepad;
        m_XboxImageInput.enabled = isXboxGamepad;
    }

    public void ResetInputMapping()
    {
        m_CurrentInputKey = EInputKey.Invalid;
        UpdateInputSprite();
    }

    private void UpdateInputSprite()
    {
        if (m_CurrentInputKey == EInputKey.Invalid)
        {
            m_CurrentInputKey = GamePadManager.GetPlayerGamepadInputMapping((int)m_Player, m_DefaultInputKey);
            if (m_CurrentInputKey == EInputKey.Invalid)
                m_CurrentInputKey = m_DefaultInputKey;
        }

        InputUIInfo associatedInputUIInfo = UIConfig.Instance.GetAssociatedInputUIInfo(m_CurrentInputKey);
        if (associatedInputUIInfo != null)
        {
            m_PS4ImageInput.sprite = associatedInputUIInfo.m_PS4Sprite;
            m_XboxImageInput.sprite = associatedInputUIInfo.m_XboxSprite;
        }
    }

    public void StartListeningInput()
    {
        // Check if it's the correct player's trying to update input
        List<GameInput> gamepadInputList = GamePadManager.GetPlayerGamepadInput((int)m_Player);
        if(gamepadInputList.Exists(x => x.GetInputKey() == EInputKey.A))
        {
            m_IsListeningInput = true;
            m_OldInputKey = m_CurrentInputKey;
            m_CurrentEventSystem = EventSystem.current;
            m_CurrentEventSystem.enabled = false;
            m_XboxImageInput.enabled = false;
            m_PS4ImageInput.enabled = false;
            m_PressAKeyText.enabled = true;
            StartCoroutine(ListenInput_Coroutine());
        }
        else
        {
            m_ControlMenuComponent.DisplayWrongPlayerFeedback();
        }
    }

    private IEnumerator ListenInput_Coroutine()
    {
        // Skip the first frame as we're just triggering A input to start listening
        yield return null;

        bool validInputReceived = false;
        while (!validInputReceived)
        {
            List<GameInput> gamepadInputList = GamePadManager.GetPlayerGamepadInput((int)m_Player);
            if (HasValidInput(gamepadInputList, out EInputKey validInput))
            {
                m_CurrentInputKey = validInput;
                StopListeningInput();
                yield break;
            }
            yield return null;
        }
    }

    private void StopListeningInput()
    {
        UpdateInputSprite();
        GamePadManager.SetPlayerGamepadInputMapping((int)m_Player, m_CurrentInputKey, m_DefaultInputKey);
        m_CurrentEventSystem.enabled = true;

        bool isXboxGamepad = GamePadManager.GetPlayerGamepadType((int)m_Player) == EGamePadType.Xbox;
        m_XboxImageInput.enabled = isXboxGamepad;
        m_PS4ImageInput.enabled = !isXboxGamepad;
        m_PressAKeyText.enabled = false;
        m_InputButton.Select();
        m_IsListeningInput = false;
        OnInputChanged?.Invoke(this);
    }

    private bool HasValidInput(List<GameInput> gameInputs, out EInputKey validInput)
    {
        validInput = EInputKey.Invalid;
        for(int i = 0; i < gameInputs.Count; i++)
        {
            if(PlayerGamePad.K_DEFAULT_INPUT_MAPPING.ContainsKey(gameInputs[i].GetInputKey()))
            {
                validInput = gameInputs[i].GetInputKey();
                return true;
            }
        }

        return false;
    }
}
