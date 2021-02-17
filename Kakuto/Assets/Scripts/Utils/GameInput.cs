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

public static class GameInputConstants
{
    public const string K_DOWN = "↓";
    public const string K_DOWN_RIGHT = "↘";
    public const string K_RIGHT = "→";
    public const string K_UP_RIGHT = "↗";
    public const string K_UP = "↑";
    public const string K_UP_LEFT = "↖";
    public const string K_LEFT = "←";
    public const string K_DOWN_LEFT = "↙";

    public const string K_A = "A";
    public const string K_B = "B";
    public const string K_X = "X";
    public const string K_Y = "Y";

    public const string K_RB = "RB";
    public const string K_LB = "LB";
    public const string K_RT = "RT";
    public const string K_LT = "LT";
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
                inputString = GameInputConstants.K_DOWN;
                break;
            case EInputKey.DownRight:
                inputString = GameInputConstants.K_DOWN_RIGHT;
                break;
            case EInputKey.Right:
                inputString = GameInputConstants.K_RIGHT;
                break;
            case EInputKey.UpRight:
                inputString = GameInputConstants.K_UP_RIGHT;
                break;
            case EInputKey.Up:
                inputString = GameInputConstants.K_UP;
                break;
            case EInputKey.UpLeft:
                inputString = GameInputConstants.K_UP_LEFT;
                break;
            case EInputKey.Left:
                inputString = GameInputConstants.K_LEFT;
                break;
            case EInputKey.DownLeft:
                inputString = GameInputConstants.K_DOWN_LEFT;
                break;

            case EInputKey.A:
                inputString = GameInputConstants.K_A;
                break;
            case EInputKey.B:
                inputString = GameInputConstants.K_B;
                break;
            case EInputKey.X:
                inputString = GameInputConstants.K_X;
                break;
            case EInputKey.Y:
                inputString = GameInputConstants.K_Y;
                break;
            case EInputKey.RB:
                inputString = GameInputConstants.K_RB;
                break;
            case EInputKey.LB:
                inputString = GameInputConstants.K_LB;
                break;
            case EInputKey.RT:
                inputString = GameInputConstants.K_RT;
                break;
            case EInputKey.LT:
                inputString = GameInputConstants.K_LT;
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
            case GameInputConstants.K_DOWN:
                inputKey = EInputKey.Down;
                break;
            case GameInputConstants.K_DOWN_RIGHT:
                inputKey = EInputKey.DownRight;
                break;
            case GameInputConstants.K_RIGHT:
                inputKey = EInputKey.Right;
                break;
            case GameInputConstants.K_UP_RIGHT:
                inputKey = EInputKey.UpRight;
                break;
            case GameInputConstants.K_UP:
                inputKey = EInputKey.Up;
                break;
            case GameInputConstants.K_UP_LEFT:
                inputKey = EInputKey.UpLeft;
                break;
            case GameInputConstants.K_LEFT:
                inputKey = EInputKey.Left;
                break;
            case GameInputConstants.K_DOWN_LEFT:
                inputKey = EInputKey.DownLeft;
                break;

            case GameInputConstants.K_A:
                inputKey = EInputKey.A;
                break;
            case GameInputConstants.K_B:
                inputKey = EInputKey.B;
                break;
            case GameInputConstants.K_X:
                inputKey = EInputKey.X;
                break;
            case GameInputConstants.K_Y:
                inputKey = EInputKey.Y;
                break;
            case GameInputConstants.K_RB:
                inputKey = EInputKey.RB;
                break;
            case GameInputConstants.K_LB:
                inputKey = EInputKey.LB;
                break;
            case GameInputConstants.K_RT:
                inputKey = EInputKey.RT;
                break;
            case GameInputConstants.K_LT:
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