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
    public int m_LastUpdate = -1;

    public int m_PlayerIndex = -1;
    private int m_GamePadIndex = -1;

    public EGamePadType m_GamePadType = EGamePadType.Invalid;

    public float m_LastXAxis = 0;
    public float m_LastYAxis = 0;

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
        Updatem_GamePadIndex();

        if (IsGamePadIndexValid())
        {
            UpdateHorizontal();
            UpdateVertical();
            ComputeCurrentDirection();
        }

        m_LastUpdate = Time.frameCount;
    }

    private void Updatem_GamePadIndex()
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
}