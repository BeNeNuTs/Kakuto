using System;
using System.Collections.Generic;
using UnityEngine;

public class ChronicleConfig : ScriptableObject
{
    static ChronicleConfig s_Instance = null;
    public static ChronicleConfig Instance
    {
        get
        {
            if (!s_Instance)
                s_Instance = Resources.Load<ChronicleConfig>("Debug/ChronicleConfig");
            return s_Instance;
        }
    }

    [Serializable]
    public struct ChronicleCategoryToLog
    {
        [ReadOnly]
        public string m_Name;
        public bool m_Log;
    }

    public List<ChronicleCategoryToLog> m_ChronicleCategoryToLog = new List<ChronicleCategoryToLog>();

    void OnValidate()
    {
        if(m_ChronicleCategoryToLog == null)
        {
            m_ChronicleCategoryToLog = new List<ChronicleCategoryToLog>();
        }

        while(m_ChronicleCategoryToLog.Count < ChronicleManager.K_ChronicleCategory_Count)
        {
            m_ChronicleCategoryToLog.Add(new ChronicleCategoryToLog());
        }

        for(int i = 0; i < ChronicleManager.K_ChronicleCategory_Count; i++)
        {
            EChronicleCategory category = (EChronicleCategory)(i);
            ChronicleCategoryToLog chronicleCategoryLog = m_ChronicleCategoryToLog[i];
            chronicleCategoryLog.m_Name = category.ToString() + " - Log : " + m_ChronicleCategoryToLog[i].m_Log;
            m_ChronicleCategoryToLog[i] = chronicleCategoryLog;
        }
    }
}