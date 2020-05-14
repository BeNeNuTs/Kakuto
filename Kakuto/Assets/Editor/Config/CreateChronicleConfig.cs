using UnityEngine;
using UnityEditor;

public class CreateChronicleConfig
{
    [MenuItem("Assets/Create/Data/Resources/Debug/ChronicleConfig")]
    public static ChronicleConfig Create()
    {
        ChronicleConfig asset = ScriptableObject.CreateInstance<ChronicleConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/Debug/ChronicleConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}