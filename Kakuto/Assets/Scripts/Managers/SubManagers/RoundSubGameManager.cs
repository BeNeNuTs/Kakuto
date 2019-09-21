using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundSubGameManager : SubGameManagerBase
{
    private uint m_Player1RoundVictoryCounter = 0;
    private uint m_Player2RoundVictoryCounter = 0;

    private bool m_RoundIsOver = false;

    public override void Init()
    {
        base.Init();
        Utils.GetPlayerEventManager<string>(Player.Player1).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager<string>(Player.Player2).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        Utils.GetPlayerEventManager<string>(Player.Player1).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager<string>(Player.Player2).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
    }

    private void OnPlayerDeath(string playerTag)
    {
        if(!m_RoundIsOver)
        {
            UpdateRoundVictoryCounter(playerTag);
            Invoker.InvokeDelayed(RestartLevel, GameConfig.Instance.m_TimeToWaitBetweenRounds);
            m_RoundIsOver = true;
        }
    }

    public void OnTimerOver()
    {
        if (!m_RoundIsOver)
        {
            Invoker.InvokeDelayed(RestartLevel, GameConfig.Instance.m_TimeToWaitBetweenRounds);
            m_RoundIsOver = true;
        }
    }

    private void UpdateRoundVictoryCounter(string deadPlayer)
    {
        if (deadPlayer == Player.Player1)
        {
            m_Player2RoundVictoryCounter++;
        }
        else
        {
            m_Player1RoundVictoryCounter++;
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        m_RoundIsOver = false;
    }

    public uint GetPlayerRoundVictoryCounter(EPlayer player)
    {
        switch (player)
        {
            case EPlayer.Player1:
                return m_Player1RoundVictoryCounter;
            case EPlayer.Player2:
                return m_Player2RoundVictoryCounter;
            default:
                return 0;
        }
    }

    public bool IsRoundOver()
    {
        return m_RoundIsOver;
    }
}
