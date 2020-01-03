using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BakeableScriptableObject : ScriptableObject
{
#if UNITY_EDITOR
    public abstract void BakeData();
    public abstract void ResetBakeData();
#endif
}
