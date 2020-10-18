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
    LT,
    RT,

    Invalid
}

[Serializable]
public class GameInputList
{
    [SerializeField]
    private List<GameInput> m_GameInputList = new List<GameInput>();

    public int Count
    {
        get { return m_GameInputList.Count; }
    }

    public void Add(GameInput input)
    {
        m_GameInputList.Add(input);
    }

    public GameInput this[int i]
    {
        get { return m_GameInputList[i]; }
        set { m_GameInputList[i] = value; }
    }
}

[Serializable]
public class GameInput
{
    [SerializeField]
    protected string m_InputString = "";
    [SerializeField]
    protected EInputKey m_InputKey = EInputKey.Invalid;

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
            case EInputKey.RT:
            case EInputKey.LT:
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
            case "RT":
                inputKey = EInputKey.RT;
                break;
            case "LT":
                inputKey = EInputKey.LT;
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

    public override string ToString()
    {
        return m_InputString;
    }
}

public class TriggeredGameInput : GameInput
{
    private readonly int m_TriggeredFrame;
    public int m_Persistency;
    public int m_TimeFreezeFrameCount = 0;

    public TriggeredGameInput(GameInput input, int currentFrame) : base(input)
    {
        m_TriggeredFrame = currentFrame;
        m_Persistency = AttackConfig.Instance.DefaultInputFramesPersistency;
    }

    public bool IsElapsed(int currentFrame)
    {
        return currentFrame >= (m_TriggeredFrame + m_Persistency + m_TimeFreezeFrameCount);
    }

    public void OnTimeFreeze()
    {
        m_TimeFreezeFrameCount++;
    }

    public void AddPersistency(int persistencyFramesBonus)
    {
        m_Persistency += persistencyFramesBonus;
        m_Persistency = (int)Mathf.Min(m_Persistency, AttackConfig.Instance.MaxInputFramesPersistency);
    }

    public void OnSideChanged()
    {
        switch (m_InputKey)
        {
            case EInputKey.DownRight:
                m_InputKey = EInputKey.DownLeft;
                break;
            case EInputKey.Right:
                m_InputKey = EInputKey.Left;
                break;
            case EInputKey.UpRight:
                m_InputKey = EInputKey.UpLeft;
                break;
            case EInputKey.UpLeft:
                m_InputKey = EInputKey.UpRight;
                break;
            case EInputKey.Left:
                m_InputKey = EInputKey.Right;
                break;
            case EInputKey.DownLeft:
                m_InputKey = EInputKey.DownRight;
                break;
        }

        m_InputString = ConvertInputKeyToString(m_InputKey);
    }
}