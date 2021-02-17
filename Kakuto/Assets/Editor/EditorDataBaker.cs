#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class EditorDataBaker : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
    {
        List<BakeableScriptableObject> allBakeableObjects = EditorUtils.FindAssetsByType<BakeableScriptableObject>();
        foreach(BakeableScriptableObject bakeableObject in allBakeableObjects)
        {
            bakeableObject.ResetBakeData();
            bakeableObject.BakeData();
        }

        KakutoDebug.Log("Data baking success !");
    }
}
#endif