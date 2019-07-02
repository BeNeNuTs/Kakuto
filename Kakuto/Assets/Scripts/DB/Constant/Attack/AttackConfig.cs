using UnityEngine;
using System.Collections;
using UnityEditor;

public class AttackConfig : ScriptableObject
{
    static AttackConfig s_Instance = null;
    public static AttackConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = AssetDatabase.LoadAssetAtPath<AttackConfig>("Assets/Data/Constant/Attack/AttackConfig.asset");
            return s_Instance;
        }
    }

    [Tooltip("How much time each input will be kept before being deleted")]
    public float m_InputPersistency = 2.0f;

    [Tooltip("How many input can be stacked before being deleted")]
    public uint m_MaxInputs = 10;

    [Tooltip("Each time an input is triggered, we're going to wait x frames before evaluating the sequence to find out if there is an attack matching with it.")]
    public uint m_FramesToWaitBeforeEvaluatingAttacks = 5;

    [Tooltip("Max frames to wait before evaluating attacks, even if we're still trying to wait due to FramesToWaitBeforeEvaluatingAttacks (in case of button mashing, we have to try to trigger an attack anyway each x frames).")]
    public float m_MaxFramesToWaitBeforeEvaluatingAttacks = 24;
}

public class CreateAttackConfig
{
    [MenuItem("Assets/Create/Data/Constant/Attack/AttackConfig")]
    public static AttackConfig Create()
    {
        AttackConfig asset = ScriptableObject.CreateInstance<AttackConfig>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/Constant/Attack/AttackConfig.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }
}