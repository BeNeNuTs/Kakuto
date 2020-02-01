using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum EInputKey
{
    Down,
    DownRight,
    Right,
    UpRight,
    Up,
    UpLeft,
    Left,
    DownLeft,
    A,
    B,
    X,
    Y,
    RB,
    LB,

    Invalid
}

public class GameInput
{
    private readonly EInputKey m_InputKey = EInputKey.Invalid;
    private readonly string m_InputString = "";

    public GameInput(GameInput input)
    {
        m_InputKey = input.GetInputKey();
        m_InputString = input.GetInputString();
    }

    public GameInput(EInputKey key)
    {
        m_InputKey = key;
        m_InputString = ConvertInputKeyToString(key);
    }

    public GameInput(string str)
    {
        m_InputString = str;
        m_InputKey = ConvertInputStringToKey(str);
    }

    public static string ConvertInputKeyToString(EInputKey key)
    {
        string inputString = "";
        switch (key)
        {
            case EInputKey.Down:
                inputString = "↓";
                break;
            case EInputKey.DownRight:
                inputString = "↘";
                break;
            case EInputKey.Right:
                inputString = "→";
                break;
            case EInputKey.UpRight:
                inputString = "↗";
                break;
            case EInputKey.Up:
                inputString = "↑";
                break;
            case EInputKey.UpLeft:
                inputString = "↖";
                break;
            case EInputKey.Left:
                inputString = "←";
                break;
            case EInputKey.DownLeft:
                inputString = "↙";
                break;

            case EInputKey.A:
            case EInputKey.B:
            case EInputKey.X:
            case EInputKey.Y:
            case EInputKey.RB:
            case EInputKey.LB:
                inputString = key.ToString();
                break;

            case EInputKey.Invalid:
            default:
                inputString = "";
                break;
        }

        return inputString;
    }
    public static EInputKey ConvertInputStringToKey(string str)
    {
        EInputKey inputKey = EInputKey.Invalid;
        switch (str)
        {
            case "↓":
                inputKey = EInputKey.Down;
                break;
            case "↘":
                inputKey = EInputKey.DownRight;
                break;
            case "→":
                inputKey = EInputKey.Right;
                break;
            case "↗":
                inputKey = EInputKey.UpRight;
                break;
            case "↑":
                inputKey = EInputKey.Up;
                break;
            case "↖":
                inputKey = EInputKey.UpLeft;
                break;
            case "←":
                inputKey = EInputKey.Left;
                break;
            case "↙":
                inputKey = EInputKey.DownLeft;
                break;

            case "A":
                inputKey = EInputKey.A;
                break;
            case "B":
                inputKey = EInputKey.B;
                break;
            case "X":
                inputKey = EInputKey.X;
                break;
            case "Y":
                inputKey = EInputKey.Y;
                break;
            case "RB":
                inputKey = EInputKey.RB;
                break;
            case "LB":
                inputKey = EInputKey.LB;
                break;

            default:
                inputKey = EInputKey.Invalid;
                break;
        }

        return inputKey;
    }

    public EInputKey GetInputKey() { return m_InputKey; }
    public string GetInputString() { return m_InputString; }

    public override bool Equals(System.Object other)
    {
        return Equals(other as GameInput);
    }

    public bool Equals(GameInput other)
    {
        return other != null && m_InputKey == other.m_InputKey;
    }

    public override int GetHashCode()
    {
        return (int)m_InputKey;
    }
}

public class TriggeredGameInput : GameInput
{
    private readonly float m_TriggeredTime;
    public float m_Persistency;

    public TriggeredGameInput(GameInput input, float time) : base(input)
    {
        m_TriggeredTime = time;
        m_Persistency = AttackConfig.Instance.m_DefaultInputPersistency;
    }

    public bool IsElapsed(float time)
    {
        return time > (m_TriggeredTime + m_Persistency);
    }

    public void AddPersistency(float persistencyBonus)
    {
        m_Persistency += persistencyBonus;
        m_Persistency = Mathf.Min(m_Persistency, AttackConfig.Instance.m_MaxInputPersistency);
    }
}