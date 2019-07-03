using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class InputManager
{
    public static float GetHorizontalMovement(int playerIndex)
    {
        float horizontalInput = 0f;
        if (GamePadManager.AreGamepadsConnected())
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
        if (GamePadManager.AreGamepadsConnected())
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
        if (GamePadManager.AreGamepadsConnected())
        {
            isCrouching = GamePadManager.GetCrouchInput(playerIndex);
        }
        else
        {
            isCrouching = Input.GetKey("down");
        }
        return isCrouching;
    }

    public static string GetAttackInputString(int playerIndex, bool isLeftSide)
    {
        string inputString = "";
        if (GamePadManager.AreGamepadsConnected())
        {
            inputString = GamePadManager.GetAttackInputString(playerIndex, isLeftSide);
        }
        else
        {
            bool isLeftKeyDown = Input.GetKeyDown("left");
            bool isRightKeyDown = Input.GetKeyDown("right");
            bool isUpKeyDown = Input.GetKeyDown("up");
            bool isDownKeyDown = Input.GetKeyDown("down");

            if(isLeftKeyDown && !isUpKeyDown && !isDownKeyDown)
            {
                inputString += (isLeftSide) ? '←' : '→';
            }

            if (isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
            {
                inputString += (isLeftSide) ? '→' : '←';
            }

            if (isUpKeyDown)
            {
                if(isLeftKeyDown)
                {
                    inputString += (isLeftSide) ? '↖' : '↗';
                }
                else if(isRightKeyDown)
                {
                    inputString += (isLeftSide) ? '↗' : '↖';
                }
                else
                {
                    inputString += '↑';
                }
            }

            if (isDownKeyDown)
            {
                if (isLeftKeyDown)
                {
                    inputString += (isLeftSide) ? '↙' : '↘';
                }
                else if (isRightKeyDown)
                {
                    inputString += (isLeftSide) ? '↘' : '↙';
                }
                else
                {
                    inputString += '↓';
                }
            }

            inputString += Input.inputString.ToUpper();
        }

        return inputString;
    }
}
