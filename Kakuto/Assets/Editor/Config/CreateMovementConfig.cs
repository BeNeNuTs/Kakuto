using UnityEngine;
using UnityEditor;

public class CreateMovementConfig
{
    [MenuItem("Assets/Create/Data/Resources/Movement/MovementConfig")]
    public static MovementConfig Create()
    {
        MovementConfig asset = ScriptableObject.CreateInstance<MovementConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/Movement/MovementConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}