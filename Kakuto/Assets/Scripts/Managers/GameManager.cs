using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [Tooltip("Time to wait in seconds before restarting level after a player death")]
    public float m_TimeToWaitBetweenRounds = 5f;

    private void Awake()
    {
        RegisterListeners();
    }

    void OnDestroy()
    {
        UnregisterListeners();
    }

    void RegisterListeners()
    {
        Utils.GetPlayerEventManager<bool>(Player.Player1).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager<bool>(Player.Player2).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);
    }

    void UnregisterListeners()
    {
        Utils.GetPlayerEventManager<bool>(Player.Player1).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager<bool>(Player.Player2).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
    }

    private void OnPlayerDeath(bool dummyBool)
    {
        Invoke("RestartLevel", m_TimeToWaitBetweenRounds);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
