using UnityEngine;

public class GamepadDebugDisplay : MonoBehaviour
{
    void DisplaGamepadInfo(string text, TextAnchor anchor)
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle
        {
            alignment = anchor,
            fontSize = h * 2 / 100
        };
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        GUI.Label(new Rect(0, h / 2.0f + style.fontSize * 2.0f, w, style.fontSize), text, style);
    }

    void OnGUI()
    {
        if (ScenesConfig.GetDebugSettings().m_DisplayGamepadInfo)
        {
            int w = Screen.width, h = Screen.height;

            // Display Gamepad Info ///////////////
            {
                DisplaGamepadInfo("Gamepad index : " + GamePadManager.GetJoystickNum(0), TextAnchor.MiddleLeft);
                DisplaGamepadInfo("Gamepad index : " + GamePadManager.GetJoystickNum(1), TextAnchor.MiddleRight);
            }
            //////////////////////////////
        }
    }
}