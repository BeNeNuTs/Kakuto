using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;
using UnityEditor;

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

#if UNITY_EDITOR
    // Add a menu item named "Do Something" to MyMenu in the menu bar.
    [MenuItem("Kakuto/Clear saved input mapping")]
    static void ClearSavedInputMapping()
    {
        PlayerPrefs.SetString("Player1InputMapping", "");
        PlayerPrefs.SetString("Player2InputMapping", "");
        Debug.Log("Input mapping cleared!");
    }
#endif


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

    public static void GetAttackInputList(int playerIndex, bool isLeftSide, ref List<GameInput> attackInputs)
    {
        Profiler.BeginSample("GamePadManager.GetAttackInputList");

        Update(playerIndex);

        if (m_PlayerGamePads[playerIndex].IsGamePadIndexValid())
        {
            if (m_PlayerGamePads[playerIndex].m_CurrentDirection != EDirection.Invalid)
            {
                if (m_PlayerGamePads[playerIndex].m_LastDirection != m_PlayerGamePads[playerIndex].m_CurrentDirection)
                {
                    switch (m_PlayerGamePads[playerIndex].m_CurrentDirection)
                    {
                        case EDirection.Down:
                            attackInputs.Add(new GameInput(EInputKey.Down));
                            break;
                        case EDirection.DownRight:
                            attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.DownRight : EInputKey.DownLeft));
                            break;
                        case EDirection.Right:
                            attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.Right : EInputKey.Left));
                            break;
                        case EDirection.UpRight:
                            attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.UpRight : EInputKey.UpLeft));
                            break;
                        case EDirection.Up:
                            attackInputs.Add(new GameInput(EInputKey.Up));
                            break;
                        case EDirection.UpLeft:
                            attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.UpLeft : EInputKey.UpRight));
                            break;
                        case EDirection.Left:
                            attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.Left : EInputKey.Right));
                            break;
                        case EDirection.DownLeft:
                            attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.DownLeft : EInputKey.DownRight));
                            break;
                        default:
                            break;
                    }
                }
            }

            List<EInputKey> inputKeysDown = m_PlayerGamePads[playerIndex].GetInputKeysDown();
            foreach (EInputKey inputKey in inputKeysDown)
            {
                attackInputs.Add(new GameInput(inputKey));
            }
        }

        Profiler.EndSample();
    }

    public static bool GetAnyPlayerStartInput(out EPlayer startInputPlayer)
    {
        startInputPlayer = EPlayer.Player1;
        for (int i = 0; i < m_PlayerGamePads.Length; i++)
        {
            Update(i);
            if (m_PlayerGamePads[i].GetStartInput())
            {
                startInputPlayer = (i == 0) ? EPlayer.Player1 : EPlayer.Player2;
                return true;
            }
        }

        return false;
    }

    public static bool GetAnyPlayerSubmitInput(out EPlayer submitInputPlayer)
    {
        submitInputPlayer = EPlayer.Player1;
        for (int i = 0; i < m_PlayerGamePads.Length; i++)
        {
            Update(i);
            if (m_PlayerGamePads[i].GetSubmitInput())
            {
                submitInputPlayer = (i == 0) ? EPlayer.Player1 : EPlayer.Player2;
                return true;
            }
        }

        return false;
    }

    public static bool GetAnyPlayerBackInput(out EPlayer backInputPlayer)
    {
        backInputPlayer = EPlayer.Player1;
        for (int i = 0; i < m_PlayerGamePads.Length; i++)
        {
            Update(i);
            if (m_PlayerGamePads[i].GetBackInput())
            {
                backInputPlayer = (i == 0) ? EPlayer.Player1 : EPlayer.Player2;
                return true;
            }
        }

        return false;
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

    public static int GetJoystickNum(int playerIndex)
    {
        return m_PlayerGamePads[playerIndex].GetJoystickNum();
    }

    public static EGamePadType GetPlayerGamepadType(int playerIndex, Action<EGamePadType> onGamePadTypeChanged = null)
    {
        if(onGamePadTypeChanged != null)
            m_PlayerGamePads[playerIndex].OnGamePadTypeChanged += onGamePadTypeChanged;
        return m_PlayerGamePads[playerIndex].GamePadType;
    }

    public static List<GameInput> GetPlayerGamepadInput(int playerIndex)
    {
        Update(playerIndex);

        List<GameInput> playerGamepadInputList = new List<GameInput>();
        if (m_PlayerGamePads[playerIndex].IsGamePadIndexValid())
        {
            List<EInputKey> inputKeysDown = m_PlayerGamePads[playerIndex].GetInputKeysDown(false);
            foreach (EInputKey inputKey in inputKeysDown)
            {
                playerGamepadInputList.Add(new GameInput(inputKey));
            }
        }

        return playerGamepadInputList;
    }

    public static EInputKey GetPlayerGamepadInputMapping(int playerIndex, EInputKey value)
    {
        return m_PlayerGamePads[playerIndex].GetInputMapping(value);
    }

    public static void SetPlayerGamepadInputMapping(int playerIndex, EInputKey key, EInputKey newValue)
    {
        m_PlayerGamePads[playerIndex].SetInputMapping(key, newValue);
    }

    private static void Update(int playerIndex)
    {
        Profiler.BeginSample("GamePadManager.GetAttackInputList");
        if (m_PlayerGamePads[playerIndex].NeedUpdate())
        {
            m_PlayerGamePads[playerIndex].Update();
        }
        Profiler.EndSample();
    }
}
