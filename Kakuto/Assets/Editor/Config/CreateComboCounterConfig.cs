using UnityEngine;
using UnityEditor;

public class CreateComboCounterConfig
{
    [MenuItem("Assets/Create/Data/Resources/UI/ComboCounterConfig")]
    public static ComboCounterConfig Create()
    {
        ComboCounterConfig asset = ScriptableObject.CreateInstance<ComboCounterConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/UI/ComboCounterConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}
