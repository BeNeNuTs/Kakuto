using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private Dictionary<ESubManager, SubGameManagerBase> m_SubManagers;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        GameManager gameManager = Create();
        gameManager.CreateSubManagers();
        gameManager.InitSubManagers();
    }

    private void LateUpdate()
    {
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.LateUpdate();
        }
    }

    protected override void OnShutdown()
    {
        base.OnShutdown();
        ShutdownSubManagers();
        DeleteSubManagers();
    }

    private void CreateSubManagers()
    {
        m_SubManagers = new Dictionary<ESubManager, SubGameManagerBase>
        {
            { ESubManager.Round, new RoundSubGameManager() },
            { ESubManager.OutOfBounds, new OutOfBoundsSubGameManager() }
        };
    }

    private void DeleteSubManagers()
    {
        m_SubManagers.Clear();
    }

    private void InitSubManagers()
    {
        foreach(SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.Init();
        }
    }

    private void ShutdownSubManagers()
    {
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.Shutdown();
        }
    }

    public T GetSubManager<T>(ESubManager subManager) where T : SubGameManagerBase
    {
        return (T)m_SubManagers[subManager];
    }
}
