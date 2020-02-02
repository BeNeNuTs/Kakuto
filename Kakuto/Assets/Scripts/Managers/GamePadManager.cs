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

            List<EInputKey> inputKeysDown = m_PlayerGamePads[playerIndex].GetInputKeysDown();
            foreach (EInputKey inputKey in inputKeysDown)
            {
                gameInputList.Add(new GameInput(inputKey));
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
}
