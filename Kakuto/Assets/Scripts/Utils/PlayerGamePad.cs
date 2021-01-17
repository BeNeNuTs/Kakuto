using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EGamePadType
{
    Xbox,
    PS4,
    Invalid
}

public enum EDirection
{
    Down,
    DownRight,
    Right,
    UpRight,
    Up,
    UpLeft,
    Left,
    DownLeft,

    Invalid
}

public class PlayerGamePad
{
    public static readonly int K_GAMEPAD_BUTTON = 6;
    public static readonly Dictionary<EInputKey, EInputKey> K_DEFAULT_INPUT_MAPPING = new Dictionary<EInputKey, EInputKey>
    {
        { EInputKey.A, EInputKey.A },
        { EInputKey.B, EInputKey.B },
        { EInputKey.X, EInputKey.X },
        { EInputKey.Y, EInputKey.Y },
        { EInputKey.LB, EInputKey.LB },
        { EInputKey.RB, EInputKey.RB },
        { EInputKey.LT, EInputKey.LT },
        { EInputKey.RT, EInputKey.RT }
    };

    public Dictionary<EInputKey, EInputKey> m_InputMapping = new Dictionary<EInputKey, EInputKey>(K_DEFAULT_INPUT_MAPPING);

    public int m_LastUpdate = -1;

    public int m_PlayerIndex = -1;
    private int m_GamePadIndex = -1;

    private EGamePadType m_GamePadType = EGamePadType.Invalid;
    public EGamePadType GamePadType
    {
        get => m_GamePadType;
        set
        {
            if(m_GamePadType != value)
            {
                m_GamePadType = value;
                OnGamePadTypeChanged?.Invoke(value);
            }
        }
    }
    public Action<EGamePadType> OnGamePadTypeChanged;

    public float m_LastXAxis = 0f;
    public float m_LastYAxis = 0f;

    public float m_CurrentLTAxis = -1f;
    public float m_LastLTAxis = -1f;
    public float m_CurrentRTAxis = -1f;
    public float m_LastRTAxis = -1f;

    public bool m_Up = false;
    public bool m_Down = false;
    public bool m_Left = false;
    public bool m_Right = false;

    public EDirection m_LastDirection = EDirection.Invalid;
    public EDirection m_CurrentDirection = EDirection.Invalid;

    public PlayerGamePad(int _m_PlayerIndex)
    {
        m_PlayerIndex = _m_PlayerIndex;
        LoadInputMapping();
    }

    private void LoadInputMapping()
    {
        string inputMappingStr = PlayerPrefs.GetString("Player" + (m_PlayerIndex + 1) + "InputMapping");
        if (string.IsNullOrEmpty(inputMappingStr))
        {
            m_InputMapping = new Dictionary<EInputKey, EInputKey>(K_DEFAULT_INPUT_MAPPING);
            return;
        }

        string[] inputsKeyValue = inputMappingStr.Split('|');
        for(int i = 0; i < inputsKeyValue.Length; i++)
        {
            string[] inputKeyValue = inputsKeyValue[i].Split(';');
            EInputKey inputKey = GameInput.ConvertInputStringToKey(inputKeyValue[0]);
            EInputKey inputValue = GameInput.ConvertInputStringToKey(inputKeyValue[1]);

            if(m_InputMapping.ContainsKey(inputKey))
            {
                m_InputMapping[inputKey] = inputValue;
            }
        }
    }

    public void SetInputMapping(EInputKey key, EInputKey newValue)
    {
        if (m_InputMapping.ContainsKey(key))
        {
            m_InputMapping[key] = newValue;
            SaveInputMapping();
        }
    }

    public EInputKey GetInputMapping(EInputKey value)
    {
        foreach(KeyValuePair<EInputKey, EInputKey> keyValue in m_InputMapping)
        {
            if(keyValue.Value == value)
            {
                return keyValue.Key;
            }
        }

        return EInputKey.Invalid;
    }

    public void ResetInputMapping()
    {
        m_InputMapping = new Dictionary<EInputKey, EInputKey>(K_DEFAULT_INPUT_MAPPING);
        SaveInputMapping();
    }

    private void SaveInputMapping()
    {
        var pairs = m_InputMapping.Select(x => string.Format("{0}{1}{2}", x.Key, ';', x.Value));
        string inputMappingStr = string.Join("|", pairs);
        PlayerPrefs.SetString("Player" + (m_PlayerIndex + 1) + "InputMapping", inputMappingStr);
    }

    public void ResetGamePadIndex()
    {
        m_GamePadIndex = -1;
        GamePadType = EGamePadType.Invalid;
    }

    public bool IsGamePadIndexValid()
    {
        return m_GamePadIndex >= 0;
    }

    public int GetJoystickNum()
    {
        return m_GamePadIndex + 1; // For m_GamePadIndex 0 we should looking for joystickNum 1
    }

    public bool NeedUpdate()
    {
        return m_LastUpdate != Time.frameCount;
    }

    public void Update()
    {
        UpdateGamePadIndex();

        if (IsGamePadIndexValid())
        {
            UpdateHorizontal();
            UpdateVertical();
            ComputeCurrentDirection();
            UpdateTriggers();
        }

        m_LastUpdate = Time.frameCount;
    }

    private void UpdateGamePadIndex()
    {
        string[] joystickNames = Input.GetJoystickNames();

        if (IsGamePadIndexValid())
        {
            if (m_GamePadIndex >= joystickNames.Length || string.IsNullOrEmpty(joystickNames[m_GamePadIndex]))
            {
                ResetGamePadIndex();
            }
        }

        // m_GamePadIndex is not valid
        if (!IsGamePadIndexValid())
        {
            int nbValidGamePadFound = 0;
            for (int i = 0; i < joystickNames.Length; i++)
            {
                if (string.IsNullOrEmpty(joystickNames[i]) == false)
                {
                    if (nbValidGamePadFound == m_PlayerIndex)
                    {
                        m_GamePadIndex = i;
                        GamePadType = FindGamePadTypeFromName(joystickNames[i]);
                        break;
                    }
                    nbValidGamePadFound++;
                    if (nbValidGamePadFound > m_PlayerIndex)
                    {
                        break;
                    }
                }
            }
        }
    }

    private EGamePadType FindGamePadTypeFromName(string joystickName)
    {
        string lowerJoystickName = joystickName.ToLower();
        if (lowerJoystickName.Contains("xbox") || lowerJoystickName.Contains("microsoft"))
        {
            return EGamePadType.Xbox;
        }
        else if (lowerJoystickName.Contains("ps3") || lowerJoystickName.Contains("ps4") || lowerJoystickName.Contains("sony"))
        {
            return EGamePadType.PS4;
        }
        else
        {
            EGamePadType defaultGamepadType = GameConfig.Instance.m_DefaultGamepadType;
            Debug.LogError("Gamepad : " + joystickName + " has not been recognized. Default mapping : " + defaultGamepadType.ToString());
            return defaultGamepadType;
        }
    }

    private void UpdateHorizontal()
    {
        float horizontalRawAxis = Input.GetAxisRaw("Horizontal" + GetJoystickNum());
        if (horizontalRawAxis == 0)
        {
            horizontalRawAxis = Input.GetAxisRaw("DpadX" + GetJoystickNum() + "_" + GamePadType.ToString());
        }

        if (horizontalRawAxis > 0)
        {
            m_Right = true;
            m_Left = false;
        }
        else if (horizontalRawAxis < 0)
        {
            m_Right = false;
            m_Left = true;
        }
        else
        {
            m_Right = false;
            m_Left = false;
        }

        m_LastXAxis = horizontalRawAxis;
    }

    private void UpdateVertical()
    {
        float verticalRawAxis = Input.GetAxisRaw("Vertical" + GetJoystickNum());
        if (verticalRawAxis == 0)
        {
            verticalRawAxis = Input.GetAxisRaw("DpadY" + GetJoystickNum() + "_" + GamePadType.ToString());
        }

        if (verticalRawAxis > 0)
        {
            m_Up = true;
            m_Down = false;
        }
        else if (verticalRawAxis < 0)
        {
            m_Up = false;
            m_Down = true;
        }
        else
        {
            m_Up = false;
            m_Down = false;
        }

        m_LastYAxis = verticalRawAxis;
    }

    private void ComputeCurrentDirection()
    {
        m_LastDirection = m_CurrentDirection;

        if (m_Up)
        {
            if (m_Left)
            {
                m_CurrentDirection = EDirection.UpLeft;
            }
            else if (m_Right)
            {
                m_CurrentDirection = EDirection.UpRight;
            }
            else
            {
                m_CurrentDirection = EDirection.Up;
            }
        }
        else if (m_Down)
        {
            if (m_Left)
            {
                m_CurrentDirection = EDirection.DownLeft;
            }
            else if (m_Right)
            {
                m_CurrentDirection = EDirection.DownRight;
            }
            else
            {
                m_CurrentDirection = EDirection.Down;
            }
        }
        else
        {
            if (m_Left)
            {
                m_CurrentDirection = EDirection.Left;
            }
            else if (m_Right)
            {
                m_CurrentDirection = EDirection.Right;
            }
            else
            {
                m_CurrentDirection = EDirection.Invalid;
            }
        }
    }

    private void UpdateTriggers()
    {
        // Need to check if application is focused due a bug with PS4 gamepads 
        // By default, a released trigger on PS4 gamepads is equal to -1
        // But when application is not focused, Input.GetAxisRaw returns 0 even if the trigger is still released
        if(Application.isFocused)
        {
            int joystickNum = GetJoystickNum();

            m_LastLTAxis = m_CurrentLTAxis;
            m_CurrentLTAxis = Input.GetAxisRaw("LT" + joystickNum + "_" + GamePadType.ToString());

            m_LastRTAxis = m_CurrentRTAxis;
            m_CurrentRTAxis = Input.GetAxisRaw("RT" + joystickNum + "_" + GamePadType.ToString());
        }
    }

    private float GetMinTriggerValue()
    {
        switch (GamePadType)
        {
            case EGamePadType.Xbox:
                return 0f;
            case EGamePadType.PS4:
                return -1f;
            case EGamePadType.Invalid:
            default:
                return 0f;
        }
    }

    public List<EInputKey> GetInputKeysDown(bool useRemapInputs = true)
    {
        List<EInputKey> inputKeysDown = new List<EInputKey>();

        int joystickNum = GetJoystickNum();
        for (int i = 0; i < K_GAMEPAD_BUTTON; i++)
        {
            if (Input.GetKeyDown("joystick " + joystickNum + " button " + i))
            {
                inputKeysDown.Add(ConvertGamePadButtonAsKey(i, useRemapInputs));
            }
        }

        float minTriggerValue = GetMinTriggerValue();
        if (m_CurrentLTAxis > minTriggerValue && m_LastLTAxis == minTriggerValue)
        {
            inputKeysDown.Add(useRemapInputs ? m_InputMapping[EInputKey.LT] : EInputKey.LT);
        }

        if (m_CurrentRTAxis > minTriggerValue && m_LastRTAxis == minTriggerValue)
        {
            inputKeysDown.Add(useRemapInputs ? m_InputMapping[EInputKey.RT] : EInputKey.RT);
        }

        return inputKeysDown;
    }

    public bool GetStartInput()
    {
        int joystickNum = GetJoystickNum();
        switch (GamePadType)
        {
            case EGamePadType.Xbox:
                return Input.GetKeyDown("joystick " + joystickNum + " button 7");
            case EGamePadType.PS4:
                return Input.GetKeyDown("joystick " + joystickNum + " button 9");
            case EGamePadType.Invalid:
            default:
                return false;
        }
    }

    public bool GetSubmitInput()
    {
        int joystickNum = GetJoystickNum();
        switch (GamePadType)
        {
            case EGamePadType.Xbox:
                return Input.GetKeyDown("joystick " + joystickNum + " button 0");
            case EGamePadType.PS4:
                return Input.GetKeyDown("joystick " + joystickNum + " button 1");
            case EGamePadType.Invalid:
            default:
                return false;
        }
    }

    public bool GetBackInput()
    {
        int joystickNum = GetJoystickNum();
        switch (GamePadType)
        {
            case EGamePadType.Xbox:
                return Input.GetKeyDown("joystick " + joystickNum + " button 1");
            case EGamePadType.PS4:
                return Input.GetKeyDown("joystick " + joystickNum + " button 2");
            case EGamePadType.Invalid:
            default:
                return false;
        }
    }

    public EInputKey ConvertGamePadButtonAsKey(int buttonIndex, bool useRemapInputs)
    {
        switch (m_GamePadType)
        {
            case EGamePadType.Xbox:
                return ConvertXboxGamePadButtonAsKey(buttonIndex, useRemapInputs);
            case EGamePadType.PS4:
                return ConvertPS4GamePadButtonAsKey(buttonIndex, useRemapInputs);
            case EGamePadType.Invalid:
            default:
                return EInputKey.Invalid;
        }
    }

    private EInputKey ConvertXboxGamePadButtonAsKey(int buttonIndex, bool useRemapInputs)
    {
        switch (buttonIndex)
        {
            case 0:
                return useRemapInputs ? m_InputMapping[EInputKey.A] : EInputKey.A;
            case 1:
                return useRemapInputs ? m_InputMapping[EInputKey.B] : EInputKey.B;
            case 2:
                return useRemapInputs ? m_InputMapping[EInputKey.X] : EInputKey.X;
            case 3:
                return useRemapInputs ? m_InputMapping[EInputKey.Y] : EInputKey.Y;
            case 4:
                return useRemapInputs ? m_InputMapping[EInputKey.LB] : EInputKey.LB;
            case 5:
                return useRemapInputs ? m_InputMapping[EInputKey.RB] : EInputKey.RB;
            default:
                return EInputKey.Invalid;
        }
    }

    private EInputKey ConvertPS4GamePadButtonAsKey(int buttonIndex, bool useRemapInputs)
    {
        switch (buttonIndex)
        {
            case 0:
                return useRemapInputs ? m_InputMapping[EInputKey.X] : EInputKey.X;
            case 1:
                return useRemapInputs ? m_InputMapping[EInputKey.A] : EInputKey.A;
            case 2:
                return useRemapInputs ? m_InputMapping[EInputKey.B] : EInputKey.B;
            case 3:
                return useRemapInputs ? m_InputMapping[EInputKey.Y] : EInputKey.Y;
            case 4:
                return useRemapInputs ? m_InputMapping[EInputKey.LB] : EInputKey.LB;
            case 5:
                return useRemapInputs ? m_InputMapping[EInputKey.RB] : EInputKey.RB;
            default:
                return EInputKey.Invalid;
        }
    }
}