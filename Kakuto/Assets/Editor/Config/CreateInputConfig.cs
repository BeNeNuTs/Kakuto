using UnityEngine;
using UnityEditor;

public class CreateInputConfig
{
    [MenuItem("Assets/Create/Data/Resources/Game/InputConfig")]
    public static InputConfig Create()
    {
        InputConfig asset = ScriptableObject.CreateInstance<InputConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/Game/InputConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}