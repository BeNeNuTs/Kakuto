using UnityEngine;
using System.IO;
using System.Diagnostics;

public enum EChronicleCategory
{
    Internal,
    Movement,
    Attack,
    Health,
    Stun,
    Animation,
    Input,
    Proximity
    // If new category, please update K_ChronicleCategory_Count and ChronicleConfig
}

public static class ChronicleManager
{
    public static uint K_ChronicleCategory_Count = 8;

    private static TextWriter m_WriterP1;
    private static TextWriter m_WriterP2;

#if DEBUG_DISPLAY || UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        ClearAll();
        OpenFiles();

        string text = "Chronicle init | OnSceneLoading";

        AddChronicle(Player.Player1, EChronicleCategory.Internal, text);
        AddChronicle(Player.Player2, EChronicleCategory.Internal, text);
    }
#endif

    public static void OnShutdown()
    {
#if DEBUG_DISPLAY || UNITY_EDITOR
        CloseFiles();
#endif
    }

    [Conditional("DEBUG_DISPLAY"), Conditional("UNITY_EDITOR")]
    public static void AddChronicle(GameObject owner, EChronicleCategory category, string text)
    {
        AddChronicle(owner.tag, category, text);
    }

    static void OpenFiles()
    {
        string filePath = GetFilePath(Player.Player1);
        m_WriterP1 = File.AppendText(filePath);

        filePath = GetFilePath(Player.Player2);
        m_WriterP2 = File.AppendText(filePath);
    }

    static void CloseFiles()
    {
        m_WriterP1.Close();
        m_WriterP2.Close();
    }

    static void AddChronicle(string playerTag, EChronicleCategory category, string text)
    {
        if (!HasValidTag(playerTag) || !HasValidCategoryToLog(category))
        {
            return;
        }

        TextWriter writer = GetWriter(playerTag);
        writer.Write(string.Format("{0,-6}", Time.frameCount));
        writer.Write(" - " + string.Format("{0,-12}", ("[" + category + "] ")));
        writer.Write(text);
        writer.Write(writer.NewLine);
    }

    static void ClearAll()
    {
        File.Delete(GetFilePath(Player.Player1));
        File.Delete(GetFilePath(Player.Player2));
    }

    static string GetFilePath(string playerTag)
    {
        string filePath = playerTag + "Chronicle.txt";
        return filePath;
    }

    static TextWriter GetWriter(string playerTag)
    {
        switch (playerTag)
        {
            case Player.Player1:
                return m_WriterP1;
            case Player.Player2:
                return m_WriterP2;
            default:
                KakutoDebug.LogError("GetWriter on : " + playerTag + " is not allowed");
                return null;
        }
    }

    static bool HasValidTag(string tag)
    {
        switch(tag)
        {
            case Player.Player1:
            case Player.Player2:
                return true;
            default:
                KakutoDebug.LogError("AddChronicle on : " + tag + " is not allowed");
                return false;
        }
    }

    static bool HasValidCategoryToLog(EChronicleCategory category)
    {
        return ChronicleConfig.Instance.m_UseChronicle && ChronicleConfig.Instance.m_ChronicleCategoryToLog[(int)category].m_Log;
    }
}
