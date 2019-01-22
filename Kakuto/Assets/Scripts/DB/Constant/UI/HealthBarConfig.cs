using UnityEngine;
using System.Collections;
using UnityEditor;

public class HealthBarConfig : ScriptableObject
{
    static HealthBarConfig s_Instance = null;
    public static HealthBarConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = AssetDatabase.LoadAssetAtPath<HealthBarConfig>("Assets/Data/Constant/UI/HealthBarConfig.asset");
            return s_Instance;
        }
    }

    [Tooltip("Time to fill the health bar")]
    public float m_TimeToFill = 0.2f;

    [Tooltip("Time between HealthBar and HealthBarBackground")]
    public float m_TimeBetweenHealthBar = 0.5f;
}

public class CreateHealthBarConfig
{
    [MenuItem("Assets/Create/Data/Constant/UI/HealthBarConfig")]
    public static HealthBarConfig Create()
    {
        HealthBarConfig asset = ScriptableObject.CreateInstance<HealthBarConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Constant/UI/HealthBarConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}