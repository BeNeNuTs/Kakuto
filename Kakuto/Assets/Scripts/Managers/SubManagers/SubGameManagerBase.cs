using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ESubManager
{
    FrameRate,
    Round,
    OutOfBounds
}

public abstract class SubGameManagerBase
{
    private bool m_IsActive = true;

    public void SetActive(bool active) { m_IsActive = active; }

    public virtual void Init() { }
    public virtual void LateUpdate() { }
    public virtual void Shutdown() { }
}
