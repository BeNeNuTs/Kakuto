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
            isJumping = Input.GetKeyDown("up");
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
            isCrouching= Input.GetKey("down");
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
            if (Input.GetKeyDown("left"))
                inputString += (isLeftSide) ? '←' : '→';
            if (Input.GetKeyDown("right"))
                inputString += (isLeftSide) ? '→' : '←';
            if (Input.GetKeyDown("up"))
                inputString += '↑';
            if (Input.GetKeyDown("down"))
                inputString += '↓';

            inputString += Input.inputString.ToUpper();
        }

        return inputString;
    }
}
