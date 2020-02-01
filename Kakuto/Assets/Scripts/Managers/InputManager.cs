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

    public static List<GameInput> GetAttackInputList(int playerIndex, bool isLeftSide)
    {
        List<GameInput> gameInputList = new List<GameInput>();
        if (GamePadManager.UpdateGamePadsState() == EGamePadsConnectedState.Connected)
        {
            gameInputList = GamePadManager.GetAttackInputList(playerIndex, isLeftSide);
        }
        else
        {
            bool isLeftKeyDown = Input.GetKeyDown("left");
            bool isRightKeyDown = Input.GetKeyDown("right");
            bool isUpKeyDown = Input.GetKeyDown("up");
            bool isDownKeyDown = Input.GetKeyDown("down");

            if(isLeftKeyDown && !isUpKeyDown && !isDownKeyDown)
            {
                gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.Left : EInputKey.Right));
            }

            if (isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
            {
                gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.Right : EInputKey.Left));
            }

            if (isUpKeyDown)
            {
                if(isLeftKeyDown)
                {
                    gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.UpLeft : EInputKey.UpRight));
                }
                else if(isRightKeyDown)
                {
                    gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.UpRight : EInputKey.UpLeft));
                }
                else
                {
                    gameInputList.Add(new GameInput(EInputKey.Up));
                }
            }

            if (isDownKeyDown)
            {
                if (isLeftKeyDown)
                {
                    gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.DownLeft : EInputKey.DownRight));
                }
                else if (isRightKeyDown)
                {
                    gameInputList.Add(new GameInput((isLeftSide) ? EInputKey.DownRight : EInputKey.DownLeft));
                }
                else
                {
                    gameInputList.Add(new GameInput(EInputKey.Down));
                }
            }

            foreach(char c in Input.inputString)
            {
                gameInputList.Add(new GameInput(c.ToString().ToUpper()));
            }
        }

        return gameInputList;
    }
}
