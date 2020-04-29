using UnityEngine;
using UnityEditor;

public class CreateScenesConfig
{
    [MenuItem("Assets/Create/Data/Resources/Game/ScenesConfig")]
    public static ScenesConfig Create()
    {
        ScenesConfig asset = ScriptableObject.CreateInstance<ScenesConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/Game/ScenesConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}