using UnityEngine;
using System.IO;

public enum EChronicleCategory
{
    Internal,
    Movement,
    Attack,
    Health,
    Stun,
    Animation
}

public static class ChronicleManager
{
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
        if (!HasValidTag(owner))
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

        Debug.LogError(Path.GetFullPath(GetFilePath(playerTag)));

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
}
