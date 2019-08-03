using UnityEngine;
using UnityEditor;

public class CreateAttackConfig
{
    [MenuItem("Assets/Create/Data/Resources/Attack/AttackConfig")]
    public static AttackConfig Create()
    {
        AttackConfig asset = ScriptableObject.CreateInstance<AttackConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Resources/Attack/AttackConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}