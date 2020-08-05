using UnityEngine;
using UnityEditor;

public class CreateUIConfig
{
    [MenuItem("Assets/Create/Data/Resources/UI/UIConfig")]
    public static UIConfig Create()
    {
        UIConfig asset = ScriptableObject.CreateInstance<UIConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/UI/UIConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}
