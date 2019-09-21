using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private List<SubGameManagerBase> m_SubManagers;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        GameManager gameManager = Create();
        gameManager.CreateSubManagers();
        gameManager.InitSubManagers();
    }

    protected override void OnShutdown()
    {
        base.OnShutdown();
        ShutdownSubManagers();
        DeleteSubManagers();
    }

    private void CreateSubManagers()
    {
        m_SubManagers = new List<SubGameManagerBase>
        {
            new RoundSubGameManager()
        };
    }

    private void DeleteSubManagers()
    {
        m_SubManagers.Clear();
    }

    private void InitSubManagers()
    {
        foreach(SubGameManagerBase subManager in m_SubManagers)
        {
            subManager.Init();
        }
    }

    private void ShutdownSubManagers()
    {
        foreach (SubGameManagerBase subManager in m_SubManagers)
        {
            subManager.Shutdown();
        }
    }
}
