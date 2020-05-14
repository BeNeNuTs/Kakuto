using UnityEngine;
using System.IO;

public enum EChronicleCategory
{
    Internal,
    Movement,
    Attack,
    Health,
    Stun,
    Animation,
    Input
    // If new category, please update K_ChronicleCategory_Count and ChronicleConfig
}

public static class ChronicleManager
{
    public static uint K_ChronicleCategory_Count = 7;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        ClearAll();

        string text = "Chronicle init | OnSceneLoading";

        AddChronicle(Player.Player1, EChronicleCategory.Internal, text);
        AddChronicle(Player.Player2, EChronicleCategory.Internal, text);
    }
#endif

    public static void AddChronicle(GameObject owner, EChronicleCategory category, string text)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (!HasValidTag(owner) || !HasValidCategoryToLog(category))
        {
            return;
        }

        AddChronicle(owner.tag, category, text);
#endif
    }

    static void AddChronicle(string playerTag, EChronicleCategory category, string text)
    {
        TextWriter writer;
        string filePath = GetFilePath(playerTag);
        writer = File.AppendText(filePath);

        writer.Write(string.Format("{0,-6}", Time.frameCount));
        writer.Write(" - " + string.Format("{0,-12}", ("[" + category + "] ")));
        writer.Write(text);
        writer.Write(writer.NewLine);

        writer.Close();
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

    static bool HasValidTag(GameObject gameObj)
    {
        switch(gameObj.tag)
        {
            case "Player1":
            case "Player2":
                return true;
            default:
                Debug.LogError("AddChronicle on : " + gameObj + " is not allowed");
                return false;
        }
    }

    static bool HasValidCategoryToLog(EChronicleCategory category)
    {
        return ChronicleConfig.Instance.m_ChronicleCategoryToLog[(int)category].m_Log;
    }
}
