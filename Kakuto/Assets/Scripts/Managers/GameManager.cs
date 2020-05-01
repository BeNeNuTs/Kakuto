using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    private Dictionary<ESubManager, SubGameManagerBase> m_SubManagers;

    private List<GameObject> m_Players = new List<GameObject>();

    private static event UnityAction<GameObject> OnPlayer1Registered;
    private static event UnityAction<GameObject> OnPlayer2Registered;

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
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
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
        if(!player.CompareTag(Player.Player1) && !player.CompareTag(Player.Player2))
        {
            Debug.LogError("GameManager::RegisterPlayer - Trying to register an invalid player " + player);
            return;
        }

        m_Players.Add(player);
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.OnPlayerRegistered(player);
        }
        GetOnPlayerRegisteredCallback(player.tag)?.Invoke(player);
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
        if (playerGO)
        {
            return playerGO.GetComponentInChildren<T>();
        }
        return null;
    }

    public void UnregisterPlayer(GameObject player)
    {
        if (!player.CompareTag(Player.Player1) && !player.CompareTag(Player.Player2))
        {
            Debug.LogError("GameManager::UnregisterPlayer - Trying to unregister an invalid player " + player);
            return;
        }

        m_Players.Remove(player);
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.OnPlayerUnregistered(player);
        }
    }

    public void AddOnPlayerRegisteredCallback(UnityAction<GameObject> method, EPlayer player)
    {
        GetOnPlayerRegisteredCallback(player) += method;

        GameObject playerGO = GetPlayer(player);
        if (playerGO != null)
        {
            method?.Invoke(playerGO);
        }
    }

    public void RemoveOnPlayerRegisteredCallback(UnityAction<GameObject> method, EPlayer player)
    {
        GetOnPlayerRegisteredCallback(player) -= method;
    }

    private ref UnityAction<GameObject> GetOnPlayerRegisteredCallback(EPlayer player)
    {
        return ref GetOnPlayerRegisteredCallback(player.ToString());
    }

    private ref UnityAction<GameObject> GetOnPlayerRegisteredCallback(string playerTag)
    {
        if (playerTag.CompareTo(Player.Player1) == 0)
        {
            return ref OnPlayer1Registered;
        }
        else
        {
            return ref OnPlayer2Registered;
        }
    }
}
