using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private Dictionary<ESubManager, SubGameManagerBase> m_SubManagers;

    private List<GameObject> m_Players = new List<GameObject>();

    private static Action<GameObject> OnPlayer1Registered;
    private static Action<GameObject> OnPlayer2Registered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        GameManager gameManager = Create();
        gameManager.CreateSubManagers();
        gameManager.InitSubManagers();
    }

    private void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Start()
    {
        GetSubManager<GameFlowSubGameManager>(ESubManager.GameFlow).LoadOptions();
    }

    private void OnSceneUnloaded(Scene unloadedScene)
    {
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.OnSceneUnloaded(unloadedScene);
        }
    }

    private void Update()
    {
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.Update();
        }
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

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        ChronicleManager.OnShutdown();
    }

    private void CreateSubManagers()
    {
        m_SubManagers = new Dictionary<ESubManager, SubGameManagerBase>
        {
            { ESubManager.FrameRate, new FrameRateSubGameManager() },
            { ESubManager.Round, new RoundSubGameManager() },
            { ESubManager.OutOfBounds, new OutOfBoundsSubGameManager() },
            { ESubManager.CameraMultiTargets, new CameraMultiTargetsSubGameManager() },
            { ESubManager.PlayerSpriteSortingOrder, new PlayerSpriteSortingOrderSubGameManager() },
            { ESubManager.FX, new FXSubGameManager() },
            { ESubManager.Audio, new AudioSubGameManager() },
            { ESubManager.TimeScale, new TimeScaleSubGameManager() },
            { ESubManager.GameFlow, new GameFlowSubGameManager() }
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
            KakutoDebug.LogError("GameManager::RegisterPlayer - Trying to register an invalid player " + player);
            return;
        }

        m_Players.Add(player);
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.OnPlayerRegistered(player);
        }
        GetOnPlayerRegisteredCallback(player.tag)?.Invoke(player);

        if(ArePlayersRegistered())
        {
            foreach (SubGameManagerBase subManager in m_SubManagers.Values)
            {
                subManager.OnPlayersRegistered();
            }
        }
    }

    public bool ArePlayersRegistered()
    {
        return m_Players.Count == 2;
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
            KakutoDebug.LogError("GameManager::UnregisterPlayer - Trying to unregister an invalid player " + player);
            return;
        }

        m_Players.Remove(player);
        foreach (SubGameManagerBase subManager in m_SubManagers.Values)
        {
            subManager.OnPlayerUnregistered(player);
        }
    }

    public void AddOnPlayerRegisteredCallback(Action<GameObject> method, EPlayer player)
    {
        GetOnPlayerRegisteredCallback(player) += method;

        GameObject playerGO = GetPlayer(player);
        if (playerGO != null)
        {
            method?.Invoke(playerGO);
        }
    }

    public void RemoveOnPlayerRegisteredCallback(Action<GameObject> method, EPlayer player)
    {
        GetOnPlayerRegisteredCallback(player) -= method;
    }

    private ref Action<GameObject> GetOnPlayerRegisteredCallback(EPlayer player)
    {
        return ref GetOnPlayerRegisteredCallback(player.ToString());
    }

    private ref Action<GameObject> GetOnPlayerRegisteredCallback(string playerTag)
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
