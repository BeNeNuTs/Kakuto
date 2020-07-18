using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ESubManager
{
    FrameRate,
    Round,
    OutOfBounds,
    CameraMultiTargets,
    PlayerSpriteSortingOrder
}

public abstract class SubGameManagerBase
{
    private bool m_IsActive = true;

    public void SetActive(bool active) { m_IsActive = active; }

    public virtual void Init() { }
    public virtual void LateUpdate() { }
    public virtual void Shutdown() { }

    public virtual void OnPlayerRegistered(GameObject player) { }
    public virtual void OnPlayerUnregistered(GameObject player) { }
    public virtual void OnPlayersRegistered() { }
}
