using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundSubGameManager : SubGameManagerBase
{
    public override void Init()
    {
        base.Init();
        Utils.GetPlayerEventManager<bool>(Player.Player1).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager<bool>(Player.Player2).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        Utils.GetPlayerEventManager<bool>(Player.Player1).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager<bool>(Player.Player2).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
    }

    private void OnPlayerDeath(bool dummyBool)
    {
        Invoker.InvokeDelayed(RestartLevel, GameConfig.Instance.m_TimeToWaitBetweenRounds);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
