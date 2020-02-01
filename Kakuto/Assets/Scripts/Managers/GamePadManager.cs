using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EGamePadsConnectedState
{
    Connected,
    Disconnected
}

public static class GamePadManager
{
    private static readonly int K_GAMEPAD_BUTTON = 6;
    private static readonly float K_GAMEPAD_CONNECTION_CHECK = 1f; // Check if gamepads are connected every second

    private static float m_LastGamePadsConnectedCheck = -1;
    private static EGamePadsConnectedState m_LastgamepadsConnectedState = EGamePadsConnectedState.Disconnected;
    private static int m_LastNbGamePadConnected = 0;

    private static PlayerGamePad[] m_PlayerGamePads = { new PlayerGamePad(0), new PlayerGamePad(1) };

    public static float GetHorizontalMovement(int playerIndex)
    {
        Update(playerIndex);
        float lastXAxis = m_PlayerGamePads[playerIndex].m_LastXAxis;
        if (lastXAxis == 0f)
        {
            return lastXAxis;
        }
        else
        {
            return Mathf.Sign(lastXAxis);
        }      
    }

    public static bool GetJumpInput(int playerIndex)
    {
        Update(playerIndex);
        return (m_PlayerGamePads[playerIndex].m_LastYAxis > 0f);
    }

    public static bool GetCrouchInput(int playerIndex)
    {
        Update(playerIndex);
        return (m_PlayerGamePads[playerIndex].m_LastYAxis < 0f);
    }

    public static List<GameInput> GetAttackInputList(int playerIndex, bool isLeftSide)
    {
        Update(playerIndex);

        List<GameInput> gameInputList = new List<GameInput>();

        if (m_PlayerGamePads[playerIndex].IsGamePadIndexValid())
        {
            if (m_PlayerGamePads[playerIndex].m_CurrentDirection != EDirection.Invalid)
            {
                if (m_PlayerGamePads[playerIndex].m_LastDirection != m_PlayerGamePads[playerIndex].m_CurrentDirection)
                {
                    switch (m_PlayerGamePads[playerIndex].m_CurrentDirection)
                    {
                        case EDirection.Down:
                            gameInputList.Add(new GameInput(EInputKey.Down));
                            break;
                        case EDirection.DownRight:
                            gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.DownRight : EInputKey.DownLeft));
                            break;
                        case EDirection.Right:
                            gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.Right : EInputKey.Left));
                            break;
                        case EDirection.UpRight:
                            gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.UpRight : EInputKey.UpLeft));
                            break;
                        case EDirection.Up:
                            gameInputList.Add(new GameInput(EInputKey.Up));
                            break;
                        case EDirection.UpLeft:
                            gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.UpLeft : EInputKey.UpRight));
                            break;
                        case EDirection.Left:
                            gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.Left : EInputKey.Right));
                            break;
                        case EDirection.DownLeft:
                            gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.DownLeft : EInputKey.DownRight));
                            break;
                        default:
                            break;
                    }
                }
            }

            int joystickNum = m_PlayerGamePads[playerIndex].GetJoystickNum();
            EGamePadType gamePadType = m_PlayerGamePads[playerIndex].m_GamePadType;
            for (int i = 0; i < K_GAMEPAD_BUTTON; i++)
            {
                if (Input.GetKeyDown("joystick " + joystickNum + " button " + i))
                {
                    gameInputList.Add(new GameInput(ConvertGamePadButtonAsKey(i, gamePadType)));
                }
            }
        }

        return gameInputList;
    }

    public static EGamePadsConnectedState UpdateGamePadsState()
    {
        if (Time.unscaledTime < m_LastGamePadsConnectedCheck + K_GAMEPAD_CONNECTION_CHECK)
        {
            return m_LastgamepadsConnectedState;
        }

        EGamePadsConnectedState gamepadsConnectedState = EGamePadsConnectedState.Disconnected;
        int nbGamePadsConnected = 0;
        foreach (string joystickName in Input.GetJoystickNames())
        {
            if (string.IsNullOrEmpty(joystickName) == false)
            {
                gamepadsConnectedState = EGamePadsConnectedState.Connected;
                nbGamePadsConnected++;
            }
        }

        if (m_LastgamepadsConnectedState != gamepadsConnectedState || m_LastNbGamePadConnected != nbGamePadsConnected)
        {
            foreach (PlayerGamePad playerGamePad in m_PlayerGamePads)
            {
                playerGamePad.ResetGamePadIndex();
            }
        }

        m_LastgamepadsConnectedState = gamepadsConnectedState;
        m_LastNbGamePadConnected = nbGamePadsConnected;
        m_LastGamePadsConnectedCheck = Time.unscaledTime;

        return gamepadsConnectedState;
    }

    private static void Update(int playerIndex)
    {
        if (m_PlayerGamePads[playerIndex].NeedUpdate())
        {
            m_PlayerGamePads[playerIndex].Update();
        }
    }

    private static EInputKey ConvertGamePadButtonAsKey(int buttonIndex, EGamePadType gamePadType)
    {
        switch (gamePadType)
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
