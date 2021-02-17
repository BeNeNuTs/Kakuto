using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class KakutoDebug
{
    [Conditional("DEBUG_DISPLAY"), Conditional("UNITY_EDITOR")]
    public static void Log(string msg) => Debug.Log(msg);
    [Conditional("DEBUG_DISPLAY"), Conditional("UNITY_EDITOR")]
    public static void LogWarning(string msg) => Debug.LogWarning(msg);
    [Conditional("DEBUG_DISPLAY"), Conditional("UNITY_EDITOR")]
    public static void LogError(string msg) => Debug.LogError(msg);
}
