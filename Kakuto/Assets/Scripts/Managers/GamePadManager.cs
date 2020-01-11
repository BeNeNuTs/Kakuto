using UnityEngine;
using System.Collections;

public enum EGamePadConnectedState
{
    Connected,
    Disconnected
}

public static class GamePadManager
{
    private static readonly int K_GAMEPAD_BUTTON = 4;
    private static readonly float K_GAMEPAD_CONNECTION_CHECK = 1f; // Check if gamepads are connected every second

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

    public enum EGamePadType
    {
        Xbox,
        PS4,
        Invalid
    }

    private class PlayerGamePad
    {
        public int lastUpdate = -1;

        public int playerIndex = -1;
        private int gamePadIndex = -1;

        public EGamePadType gamePadType = EGamePadType.Invalid;

        public float lastXAxis = 0;
        public float lastYAxis = 0;

        public bool up = false;
        public bool down = false;
        public bool left = false;
        public bool right = false;

        public EDirection lastDirection = EDirection.Invalid;
        public EDirection currentDirection = EDirection.Invalid;

        public PlayerGamePad(int _playerIndex)
        {
            playerIndex = _playerIndex;
        }

        public void ResetGamePadIndex()
        {
            gamePadIndex = -1;
            gamePadType = EGamePadType.Invalid;
        }

        public bool IsGamePadIndexValid()
        {
            return gamePadIndex >= 0;
        }

        public int GetJoystickNum()
        {
            return gamePadIndex + 1; // For gamePadIndex 0 we should looking for joystickNum 1
        }

        public bool NeedUpdate()
        {
            return lastUpdate != Time.frameCount;
        }

        public void Update()
        {
            UpdateGamePadIndex();

            if (IsGamePadIndexValid())
            {
                UpdateHorizontal();
                UpdateVertical();
                ComputeCurrentDirection();
            }

            lastUpdate = Time.frameCount;
        }

        private void UpdateGamePadIndex()
        {
            string[] joystickNames = Input.GetJoystickNames();

            if (IsGamePadIndexValid())
            {
                if (gamePadIndex >= joystickNames.Length || string.IsNullOrEmpty(joystickNames[gamePadIndex]))
                {
                    ResetGamePadIndex();
                }
            }

            // gamePadIndex is not valid
            if (!IsGamePadIndexValid())
            {
                int nbValidGamePadFound = 0;
                for (int i = 0; i < joystickNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(joystickNames[i]) == false)
                    {
                        if (nbValidGamePadFound == playerIndex)
                        {
                            gamePadIndex = i;
                            gamePadType = FindGamePadTypeFromName(joystickNames[i]);
                            break;
                        }
                        nbValidGamePadFound++;
                        if (nbValidGamePadFound > playerIndex)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private EGamePadType FindGamePadTypeFromName(string joystickName)
        {
            if (joystickName.ToUpper().Contains("PS4"))
            {
                return EGamePadType.PS4;
            }
            else
            {
                return EGamePadType.Xbox;
            }
        }

        private void UpdateHorizontal()
        {
            float horizontalRawAxis = Input.GetAxisRaw("Horizontal" + GetJoystickNum());
            if (horizontalRawAxis == 0)
            {
                horizontalRawAxis = Input.GetAxisRaw("DpadX" + GetJoystickNum() + "_" + gamePadType.ToString());
            }

            if (horizontalRawAxis > 0)
            {
                right = true;
                left = false;
            }
            else if (horizontalRawAxis < 0)
            {
                right = false;
                left = true;
            }
            else
            {
                right = false;
                left = false;
            }

            lastXAxis = horizontalRawAxis;
        }

        private void UpdateVertical()
        {
            float verticalRawAxis = Input.GetAxisRaw("Vertical" + GetJoystickNum());
            if (verticalRawAxis == 0)
            {
                verticalRawAxis = Input.GetAxisRaw("DpadY" + GetJoystickNum() + "_" + gamePadType.ToString());
            }

            if (verticalRawAxis > 0)
            {
                up = true;
                down = false;
            }
            else if (verticalRawAxis < 0)
            {
                up = false;
                down = true;
            }
            else
            {
                up = false;
                down = false;
            }

            lastYAxis = verticalRawAxis;
        }

        private void ComputeCurrentDirection()
        {
            lastDirection = currentDirection;

            if (up)
            {
                if (left)
                {
                    currentDirection = EDirection.UpLeft;
                }
                else if (right)
                {
                    currentDirection = EDirection.UpRight;
                }
                else
                {
                    currentDirection = EDirection.Up;
                }
            }
            else if (down)
            {
                if (left)
                {
                    currentDirection = EDirection.DownLeft;
                }
                else if (right)
                {
                    currentDirection = EDirection.DownRight;
                }
                else
                {
                    currentDirection = EDirection.Down;
                }
            }
            else
            {
                if (left)
                {
                    currentDirection = EDirection.Left;
                }
                else if (right)
                {
                    currentDirection = EDirection.Right;
                }
                else
                {
                    currentDirection = EDirection.Invalid;
                }
            }
        }
    }

    private static float lastGamePadConnectedCheck = -1;
    private static EGamePadConnectedState lastGamePadConnectedState = EGamePadConnectedState.Disconnected;
    private static int lastNbGamePadConnected = 0;

    private static PlayerGamePad[] playerGamePads = { new PlayerGamePad(0), new PlayerGamePad(1) };

    public static float GetHorizontalMovement(int playerIndex)
    {
        Update(playerIndex);
        float lastXAxis = playerGamePads[playerIndex].lastXAxis;
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
        return (playerGamePads[playerIndex].lastYAxis > 0f);
    }

    public static bool GetCrouchInput(int playerIndex)
    {
        Update(playerIndex);
        return (playerGamePads[playerIndex].lastYAxis < 0f);
    }

    public static string GetAttackInputString(int playerIndex, bool isLeftSide)
    {
        Update(playerIndex);

        string inputString = "";

        if (playerGamePads[playerIndex].IsGamePadIndexValid())
        {
            if (playerGamePads[playerIndex].currentDirection != EDirection.Invalid)
            {
                if (playerGamePads[playerIndex].lastDirection != playerGamePads[playerIndex].currentDirection)
                {
                    switch (playerGamePads[playerIndex].currentDirection)
                    {
                        case EDirection.Down:
                            inputString += '↓';
                            break;
                        case EDirection.DownRight:
                            inputString += (isLeftSide) ? '↘' : '↙';
                            break;
                        case EDirection.Right:
                            inputString += (isLeftSide) ? '→' : '←';
                            break;
                        case EDirection.UpRight:
                            inputString += (isLeftSide) ? '↗' : '↖';
                            break;
                        case EDirection.Up:
                            inputString += '↑';
                            break;
                        case EDirection.UpLeft:
                            inputString += (isLeftSide) ? '↖' : '↗';
                            break;
                        case EDirection.Left:
                            inputString += (isLeftSide) ? '←' : '→';
                            break;
                        case EDirection.DownLeft:
                            inputString += (isLeftSide) ? '↙' : '↘';
                            break;
                        default:
                            break;
                    }
                }
            }

            int joystickNum = playerGamePads[playerIndex].GetJoystickNum();
            EGamePadType gamePadType = playerGamePads[playerIndex].gamePadType;
            for (int i = 0; i < K_GAMEPAD_BUTTON; i++)
            {
                if (Input.GetKeyDown("joystick " + joystickNum + " button " + i))
                {
                    inputString += ConvertGamePadButtonAsString(i, gamePadType);
                }
            }
        }

        return inputString;
    }

    public static EGamePadConnectedState UpdateGamePadsState()
    {
        if (Time.unscaledTime < lastGamePadConnectedCheck + K_GAMEPAD_CONNECTION_CHECK)
        {
            return lastGamePadConnectedState;
        }

        EGamePadConnectedState gamepadConnectedState = EGamePadConnectedState.Disconnected;
        int nbGamePadsConnected = 0;
        foreach (string joystickName in Input.GetJoystickNames())
        {
            if (string.IsNullOrEmpty(joystickName) == false)
            {
                gamepadConnectedState = EGamePadConnectedState.Connected;
                nbGamePadsConnected++;
            }
        }

        if (lastGamePadConnectedState != gamepadConnectedState || lastNbGamePadConnected != nbGamePadsConnected)
        {
            foreach (PlayerGamePad playerGamePad in playerGamePads)
            {
                playerGamePad.ResetGamePadIndex();
            }
        }

        lastGamePadConnectedState = gamepadConnectedState;
        lastNbGamePadConnected = nbGamePadsConnected;
        lastGamePadConnectedCheck = Time.unscaledTime;

        return gamepadConnectedState;
    }

    private static void Update(int playerIndex)
    {
        if (playerGamePads[playerIndex].NeedUpdate())
        {
            playerGamePads[playerIndex].Update();
        }
    }

    private static string ConvertGamePadButtonAsString(int buttonIndex, EGamePadType gamePadType)
    {
        switch (gamePadType)
        {
            case EGamePadType.Xbox:
                return ConvertXboxGamePadButtonAsString(buttonIndex);
            case EGamePadType.PS4:
                return ConvertPS4GamePadButtonAsString(buttonIndex);
            case EGamePadType.Invalid:
            default:
                return "ERROR";
        }
    }

    private static string ConvertXboxGamePadButtonAsString(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0:
                return "A";
            case 1:
                return "B";
            case 2:
                return "X";
            case 3:
                return "Y";
            default:
                return "ERROR";
        }
    }

    private static string ConvertPS4GamePadButtonAsString(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0:
                return "X";
            case 1:
                return "A";
            case 2:
                return "B";
            case 3:
                return "Y";
            default:
                return "ERROR";
        }
    }
}
