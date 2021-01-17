using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class InputManager
{
    public static float GetHorizontalMovement(int playerIndex)
    {
        float horizontalInput = 0f;
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            horizontalInput = GamePadManager.GetHorizontalMovement(playerIndex);
        }
        else
        {
            if(Input.GetKey("left"))
            {
                horizontalInput = -1f;
            }
            else if(Input.GetKey("right"))
            {
                horizontalInput = 1f;
            }
        }
        return horizontalInput;
    }

    public static bool GetJumpInput(int playerIndex)
    {
        bool isJumping = false;
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            isJumping = GamePadManager.GetJumpInput(playerIndex);
        }
        else
        {
            isJumping = Input.GetKey("up");
        }
        return isJumping;
    }

    public static bool GetCrouchInput(int playerIndex)
    {
        bool isCrouching = false;
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            isCrouching = GamePadManager.GetCrouchInput(playerIndex);
        }
        else
        {
            isCrouching = Input.GetKey("down");
        }
        return isCrouching;
    }

    public static void GetAttackInputList(int playerIndex, bool isLeftSide, ref List<GameInput> attackInputs)
    {
        attackInputs.Clear();
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            GamePadManager.GetAttackInputList(playerIndex, isLeftSide, ref attackInputs);
        }
        else
        {
            bool isLeftKeyDown = Input.GetKeyDown("left");
            bool isRightKeyDown = Input.GetKeyDown("right");
            bool isUpKeyDown = Input.GetKeyDown("up");
            bool isDownKeyDown = Input.GetKeyDown("down");

            if(isLeftKeyDown && !isUpKeyDown && !isDownKeyDown)
            {
                attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.Left : EInputKey.Right));
            }

            if (isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
            {
                attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.Right : EInputKey.Left));
            }

            if (isUpKeyDown)
            {
                if(isLeftKeyDown)
                {
                    attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.UpLeft : EInputKey.UpRight));
                }
                else if(isRightKeyDown)
                {
                    attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.UpRight : EInputKey.UpLeft));
                }
                else
                {
                    attackInputs.Add(new GameInput(EInputKey.Up));
                }
            }

            if (isDownKeyDown)
            {
                if (isLeftKeyDown)
                {
                    attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.DownLeft : EInputKey.DownRight));
                }
                else if (isRightKeyDown)
                {
                    attackInputs.Add(new GameInput((isLeftSide) ? EInputKey.DownRight : EInputKey.DownLeft));
                }
                else
                {
                    attackInputs.Add(new GameInput(EInputKey.Down));
                }
            }

            foreach(char c in Input.inputString)
            {
                attackInputs.Add(new GameInput(c.ToString().ToUpper()));
            }
        }
    }

    public static bool GetStartInput(out EPlayer startInputPlayer)
    {
        bool startInput = false;
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            startInput = GamePadManager.GetAnyPlayerStartInput(out startInputPlayer);
        }
        else
        {
            startInput = Input.GetKeyDown(KeyCode.Return);
            startInputPlayer = EPlayer.Player1;
        }
        return startInput;
    }

    public static bool GetSubmitInput(out EPlayer submitInputPlayer)
    {
        bool submitInput = false;
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            submitInput = GamePadManager.GetAnyPlayerSubmitInput(out submitInputPlayer);
        }
        else
        {
            submitInput = Input.GetKeyDown(KeyCode.Return);
            submitInputPlayer = EPlayer.Player1;
        }
        return submitInput;
    }

    public static bool GetBackInput()
    {
        EPlayer playerBackInput;
        return GetBackInput(out playerBackInput);
    }

    public static bool GetBackInput(out EPlayer backInputPlayer)
    {
        bool backInput = false;
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            backInput = GamePadManager.GetAnyPlayerBackInput(out backInputPlayer);
        }
        else
        {
            backInput = Input.GetKey(KeyCode.Backspace);
            backInputPlayer = EPlayer.Player1;
        }
        return backInput;
    }
}
