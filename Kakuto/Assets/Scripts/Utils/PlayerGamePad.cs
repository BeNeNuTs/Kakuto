using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private static readonly int K_GAMEPAD_BUTTON = 6;

    public int m_LastUpdate = -1;

    public int m_PlayerIndex = -1;
    private int m_GamePadIndex = -1;

    public EGamePadType m_GamePadType = EGamePadType.Invalid;

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
    }

    public void ResetGamePadIndex()
    {
        m_GamePadIndex = -1;
        m_GamePadType = EGamePadType.Invalid;
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
                        m_GamePadType = FindGamePadTypeFromName(joystickNames[i]);
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
            horizontalRawAxis = Input.GetAxisRaw("DpadX" + GetJoystickNum() + "_" + m_GamePadType.ToString());
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
            verticalRawAxis = Input.GetAxisRaw("DpadY" + GetJoystickNum() + "_" + m_GamePadType.ToString());
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
            m_CurrentLTAxis = Input.GetAxisRaw("LT" + joystickNum + "_" + m_GamePadType.ToString());

            m_LastRTAxis = m_CurrentRTAxis;
            m_CurrentRTAxis = Input.GetAxisRaw("RT" + joystickNum + "_" + m_GamePadType.ToString());
        }
    }

    private float GetMinTriggerValue()
    {
        switch (m_GamePadType)
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

    public List<EInputKey> GetInputKeysDown()
    {
        List<EInputKey> inputKeysDown = new List<EInputKey>();

        int joystickNum = GetJoystickNum();
        for (int i = 0; i < K_GAMEPAD_BUTTON; i++)
        {
            if (Input.GetKeyDown("joystick " + joystickNum + " button " + i))
            {
                inputKeysDown.Add(ConvertGamePadButtonAsKey(i));
            }
        }

        float minTriggerValue = GetMinTriggerValue();
        if (m_CurrentLTAxis > minTriggerValue && m_LastLTAxis == minTriggerValue)
        {
            inputKeysDown.Add(EInputKey.LT);
        }

        if (m_CurrentRTAxis > minTriggerValue && m_LastRTAxis == minTriggerValue)
        {
            inputKeysDown.Add(EInputKey.RT);
        }

        return inputKeysDown;
    }

    public bool GetStartInput()
    {
        int joystickNum = GetJoystickNum();
        switch (m_GamePadType)
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

    public bool GetBackInput()
    {
        int joystickNum = GetJoystickNum();
        switch (m_GamePadType)
        {
            case EGamePadType.Xbox:
                return Input.GetKey("joystick " + joystickNum + " button 1");
            case EGamePadType.PS4:
                return Input.GetKey("joystick " + joystickNum + " button 2");
            case EGamePadType.Invalid:
            default:
                return false;
        }
    }

    public EInputKey ConvertGamePadButtonAsKey(int buttonIndex)
    {
        switch (m_GamePadType)
        {
            case EGamePadType.Xbox:
                return ConvertXboxGamePadButtonAsKey(buttonIndex);
            case EGamePadType.PS4:
                return ConvertPS4GamePadButtonAsKey(buttonIndex);
            case EGamePadType.Invalid:
            default:
                return EInputKey.Invalid;
        }
    }

    private static EInputKey ConvertXboxGamePadButtonAsKey(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0:
                return EInputKey.A;
            case 1:
                return EInputKey.B;
            case 2:
                return EInputKey.X;
            case 3:
                return EInputKey.Y;
            case 4:
                return EInputKey.LB;
            case 5:
                return EInputKey.RB;
            default:
                return EInputKey.Invalid;
        }
    }

    private static EInputKey ConvertPS4GamePadButtonAsKey(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0:
                return EInputKey.X;
            case 1:
                return EInputKey.A;
            case 2:
                return EInputKey.B;
            case 3:
                return EInputKey.Y;
            case 4:
                return EInputKey.LB;
            case 5:
                return EInputKey.RB;
            default:
                return EInputKey.Invalid;
        }
    }
}