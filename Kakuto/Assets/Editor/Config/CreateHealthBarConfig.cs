using UnityEngine;
using UnityEditor;

public class CreateHealthBarConfig
{
    [MenuItem("Assets/Create/Data/Resources/UI/HealthBarConfig")]
    public static HealthBarConfig Create()
    {
        HealthBarConfig asset = ScriptableObject.CreateInstance<HealthBarConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/UI/HealthBarConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}
