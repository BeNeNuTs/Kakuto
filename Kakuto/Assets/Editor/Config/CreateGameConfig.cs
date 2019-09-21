using UnityEngine;
using UnityEditor;

public class CreateGameConfig
{
    [MenuItem("Assets/Create/Data/Resources/Game/GameConfig")]
    public static GameConfig Create()
    {
        GameConfig asset = ScriptableObject.CreateInstance<GameConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/Game/GameConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}