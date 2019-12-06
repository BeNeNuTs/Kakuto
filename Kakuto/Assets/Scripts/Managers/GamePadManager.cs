using UnityEngine;
using System.Collections;

public static class GamePadManager
{
    private static readonly int K_GAMEPAD_BUTTON = 4;
    private static readonly float K_GAMEPAD_CONNECTION_CHECK = 2f; // Check if gamepads are connected every 2 seconds

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

    private class PlayerGamePad
    {
        public int lastUpdate = -1;

        public float lastXAxis = 0;
        public float lastYAxis = 0;

        public bool up = false;
        public bool down = false;
        public bool left = false;
        public bool right = false;

        public EDirection lastDirection = EDirection.Invalid;
        public EDirection currentDirection = EDirection.Invalid;

        public bool NeedUpdate()
        {
            return lastUpdate != Time.frameCount;
        }

        public void Update()
        {
            lastUpdate = Time.frameCount;
            ComputeCurrentDirection();
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
    private static bool lastGamePadConnectedState = false;

    private static PlayerGamePad[] playerGamePads = { new PlayerGamePad(), new PlayerGamePad() };

    public static float GetHorizontalMovement(int playerIndex)
    {
        Update(playerIndex);
        return Mathf.RoundToInt(playerGamePads[playerIndex].lastXAxis);
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

        if(playerGamePads[playerIndex].currentDirection != EDirection.Invalid)
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

        int joystickNum = playerIndex + 1; // For playerIndex 0 we should looking for joystickNum 1

        for (int i = 0; i < K_GAMEPAD_BUTTON; i++)
        {
            if (Input.GetKeyDown("joystick " + joystickNum + " button " + i))
            {
                inputString += ConvertGamePadButtonAsString(i);
            }
        }

        return inputString;
    }

    public static bool AreGamepadsConnected()
    {
        if(Time.unscaledTime < lastGamePadConnectedCheck + K_GAMEPAD_CONNECTION_CHECK)
        {
            return lastGamePadConnectedState;
        }

        bool gamepadConnected = false;
        foreach (string joystickName in Input.GetJoystickNames())
        {
            if (string.IsNullOrEmpty(joystickName) == false)
            {
                gamepadConnected = true;
                break;
            }
        }

        lastGamePadConnectedCheck = Time.unscaledTime;
        lastGamePadConnectedState = gamepadConnected;
        return gamepadConnected;
    }

    private static void Update(int playerIndex)
    {
        if (playerGamePads[playerIndex].NeedUpdate())
        {
            UpdateHorizontal(playerIndex);
            UpdateVertical(playerIndex);

            playerGamePads[playerIndex].Update();
        }
    }

    private static void UpdateHorizontal(int playerIndex)
    {
        int joystickNum = playerIndex + 1; // For playerIndex 0 we should looking for joystickNum 1

        float horizontalRawAxis = Input.GetAxisRaw("Horizontal" + joystickNum);
        if (horizontalRawAxis == 0)
        {
            horizontalRawAxis = Input.GetAxisRaw("DpadX" + joystickNum);
        }

        if (horizontalRawAxis > 0)
        {
            playerGamePads[playerIndex].right = true;
            playerGamePads[playerIndex].left = false;
        }
        else if (horizontalRawAxis < 0)
        {
            playerGamePads[playerIndex].right = false;
            playerGamePads[playerIndex].left = true;
        }
        else
        {
            playerGamePads[playerIndex].right = false;
            playerGamePads[playerIndex].left = false;
        }

        playerGamePads[playerIndex].lastXAxis = horizontalRawAxis;
    }

    private static void UpdateVertical(int playerIndex)
    {
        int joystickNum = playerIndex + 1; // For playerIndex 0 we should looking for joystickNum 1

        float verticalRawAxis = Input.GetAxisRaw("Vertical" + joystickNum);
        if (verticalRawAxis == 0)
        {
            verticalRawAxis = Input.GetAxisRaw("DpadY" + joystickNum);
        }

        if (verticalRawAxis > 0)
        {
            playerGamePads[playerIndex].up = true;
            playerGamePads[playerIndex].down = false;
        }
        else if (verticalRawAxis < 0)
        {
            playerGamePads[playerIndex].up = false;
            playerGamePads[playerIndex].down = true;
        }
        else
        {
            playerGamePads[playerIndex].up = false;
            playerGamePads[playerIndex].down = false;
        }

        playerGamePads[playerIndex].lastYAxis = verticalRawAxis;
    }

    private static string ConvertGamePadButtonAsString(int buttonIndex)
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
}
