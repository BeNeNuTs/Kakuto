using UnityEngine;
using System.Collections;
using UnityEditor;

public class ComboCounterConfig : ScriptableObject
{
    static ComboCounterConfig s_Instance = null;
    public static ComboCounterConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<ComboCounterConfig>("UI/ComboCounterConfig");
            return s_Instance;
        }
    }

    [Tooltip("After how much time the hit combo disppear when the combo chain has been broken")]
    public float m_TimeToDisappearAfterComboBreak = 2.0f;
}