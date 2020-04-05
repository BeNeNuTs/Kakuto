using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private Dictionary<ESubManager, SubGameManagerBase> m_SubManagers;

    private List<GameObject> m_Players = new List<GameObject>();

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
            { ESubManager.FrameRate, new FrameRateSubGameManager() },
            { ESubManager.Round, new RoundSubGameManager() },
            { ESubManager.OutOfBounds, new OutOfBoundsSubGameManager() },
            { ESubManager.CameraMultiTargets, new CameraMultiTargetsSubGameManager() }
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

    public void RegisterPlayer(GameObject player)
    {
        m_Players.Add(player);
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.OnPlayerRegistered(player);
        }
    }

    public List<GameObject> GetPlayers()
    {
        return m_Players;
    }

    public GameObject GetPlayer(EPlayer player)
    {
        return m_Players.Find(p => p.tag == player.ToString());
    }

    public T GetPlayerComponent<T>(EPlayer player) where T : Behaviour
    {
        GameObject playerGO = m_Players.Find(p => p.tag == player.ToString());
        if(playerGO)
        {
            return playerGO.GetComponentInChildren<T>();
        }
        return null;
    }

    public void UnregisterPlayer(GameObject player)
    {
        m_Players.Remove(player);
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.OnPlayerUnregistered(player);
        }
    }
}
