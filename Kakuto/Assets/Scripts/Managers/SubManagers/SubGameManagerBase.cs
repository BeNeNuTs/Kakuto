using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESubManager
{
    Round
}

public abstract class SubGameManagerBase
{
    private bool m_IsActive = true;

    public void SetActive(bool active) { m_IsActive = active; }

    public virtual void Init() { }
    public virtual void Shutdown() { }
}
