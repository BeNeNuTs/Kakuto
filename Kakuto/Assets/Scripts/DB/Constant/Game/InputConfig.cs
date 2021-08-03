using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class InputConfig : ScriptableObject
{
    private static int K_MAX_GAMEPADS = 16;

    [Separator("Horizontal")]
    public float m_HorizontalGravity = 0f;
    public float m_HorizontalDead = 0.35f;
    public float m_HorizontalSensitivity = 1f;
    public bool m_HorizontalSnap = true;
    public bool m_HorizontalInvert = false;
    private int m_HorizontalAxis = 0;

    [Separator("Vertical")]
    public float m_VerticalGravity = 0f;
    public float m_VerticalDead = 0.35f;
    public float m_VerticalSensitivity = 1f;
    public bool m_VerticalSnap = true;
    public bool m_VerticalInvert = true;
    private int m_VerticalAxis = 1;

    private int m_DpadX_XboxAxis = 5;
    private int m_DpadY_XboxAxis = 6;
    private int m_LT_XboxAxis = 8;
    private int m_RT_XboxAxis = 9;

    private int m_DpadX_PS4Axis = 6;
    private int m_DpadY_PS4Axis = 7;
    private int m_LT_PS4Axis = 3;
    private int m_RT_PS4Axis = 4;

    [ButtonAttribute("GenerateInputs", "Generate inputs", "Allow generate all inputs list.", false, false)]
    [SerializeField] bool m_GenerateInputs = false;

    private string InputManagerPath => "ProjectSettings/InputManager.asset";

#if UNITY_EDITOR
    private void GenerateInputs()
    {
        File.Delete(InputManagerPath);

        TextWriter writer = File.AppendText(InputManagerPath);
        writer.WriteLine("%YAML 1.1");
        writer.WriteLine("%TAG !u! tag:unity3d.com,2011:");
        writer.WriteLine("--- !u!13 &1");
        writer.WriteLine("InputManager:");
        writer.WriteLine("  m_ObjectHideFlags: 0");
        writer.WriteLine("  serializedVersion: 2");
        writer.WriteLine("  m_Axes:");

        WriteInput(writer, "Horizontal", m_HorizontalAxis, "", m_HorizontalGravity, m_HorizontalDead, m_HorizontalSensitivity, m_HorizontalSnap, m_HorizontalInvert);
        WriteInput(writer, "Vertical", m_VerticalAxis, "", m_VerticalGravity, m_VerticalDead, m_VerticalSensitivity, m_VerticalSnap, m_VerticalInvert);

        //Xbox
        WriteInput(writer, "DpadX", m_DpadX_XboxAxis, "_Xbox");
        WriteInput(writer, "DpadY", m_DpadY_XboxAxis, "_Xbox");
        WriteInput(writer, "LT", m_LT_XboxAxis, "_Xbox");
        WriteInput(writer, "RT", m_RT_XboxAxis, "_Xbox");

        //PS4
        WriteInput(writer, "DpadX", m_DpadX_PS4Axis, "_PS4");
        WriteInput(writer, "DpadY", m_DpadY_PS4Axis, "_PS4");
        WriteInput(writer, "LT", m_LT_PS4Axis, "_PS4");
        WriteInput(writer, "RT", m_RT_PS4Axis, "_PS4");

        WriteMandotoryInput(writer, "Horizontal", m_HorizontalGravity, m_HorizontalDead, m_HorizontalSensitivity, m_HorizontalInvert, 2, m_HorizontalAxis);
        WriteMandotoryInput(writer, "Vertical", m_VerticalGravity, m_VerticalDead, m_VerticalSensitivity, m_VerticalInvert, 2, m_VerticalAxis);
        WriteMandotoryInput(writer, "Submit", 1000f, 0.001f, 1000f, false, 0, 0);
        WriteMandotoryInput(writer, "Cancel", 1000f, 0.001f, 1000f, false, 0, 0);

        writer.Close();

        AssetDatabase.Refresh();
        Debug.Log("Inputs generated");
    }

    private static void WriteInput(TextWriter writer, string inputName, int axis, string inputNameSuffix = "", float gravity = 0f, float dead = 0.19f, float sensitivity = 1f, bool snap = false, bool invert = false)
    {
        string gravityStr = gravity.ToString().Replace(",", ".");
        string deadStr = dead.ToString().Replace(",", ".");
        string sensitivityStr = sensitivity.ToString().Replace(",", ".");

        for (int i = 1; i <= K_MAX_GAMEPADS; i++)
        {
            writer.WriteLine("  - serializedVersion: 3");
            writer.WriteLine("    m_Name: " + inputName + i + inputNameSuffix);
            writer.WriteLine("    descriptiveName:");
            writer.WriteLine("    descriptiveNegativeName:");
            writer.WriteLine("    negativeButton:");
            writer.WriteLine("    positiveButton:");
            writer.WriteLine("    altNegativeButton:");
            writer.WriteLine("    altPositiveButton:");
            writer.WriteLine("    gravity: " + gravityStr);
            writer.WriteLine("    dead: " + deadStr);
            writer.WriteLine("    sensitivity: " + sensitivityStr);
            writer.WriteLine("    snap: " + (snap ? "1" : "0"));
            writer.WriteLine("    invert: " + (invert ? "1" : "0"));
            writer.WriteLine("    type: 2");
            writer.WriteLine("    axis: " + axis);
            writer.WriteLine("    joyNum: " + i);
        }
    }

    private static void WriteMandotoryInput(TextWriter writer, string inputName, float gravity, float dead, float sensitivity, bool invert, int type, int axis)
    {
        string gravityStr = gravity.ToString().Replace(",", ".");
        string deadStr = dead.ToString().Replace(",", ".");
        string sensitivityStr = sensitivity.ToString().Replace(",", ".");

        writer.WriteLine("  - serializedVersion: 3");
        writer.WriteLine("    m_Name: " + inputName);
        writer.WriteLine("    descriptiveName:");
        writer.WriteLine("    descriptiveNegativeName:");
        writer.WriteLine("    negativeButton:");
        writer.WriteLine("    positiveButton:");
        writer.WriteLine("    altNegativeButton:");
        writer.WriteLine("    altPositiveButton:");
        writer.WriteLine("    gravity: " + gravityStr);
        writer.WriteLine("    dead: " + deadStr);
        writer.WriteLine("    sensitivity: " + sensitivityStr);
        writer.WriteLine("    snap: 0");
        writer.WriteLine("    invert: " + (invert ? "1" : "0"));
        writer.WriteLine("    type: " + type);
        writer.WriteLine("    axis: " + axis);
        writer.WriteLine("    joyNum: 0");
    }
#endif
}