using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RoundSubGameManager : SubGameManagerBase
{
    public static event UnityAction OnRoundVictoryCounterChanged;
    public static event UnityAction OnRoundOver;

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

    private void OnPlayerDeath(string deadPlayerTag)
    {
        if(!m_RoundIsOver)
        {
            if (deadPlayerTag == Player.Player1)
            {
                UpdateRoundVictoryCounter(Player.Player2);
            }
            else
            {
                UpdateRoundVictoryCounter(Player.Player1);
            }

            OnRoundOver_Internal();
        }
    }

    public void OnTimerOver()
    {
        if (!m_RoundIsOver)
        {
            PlayerHealthComponent player1HealthComp = GameManager.Instance.GetPlayerComponent<PlayerHealthComponent>(EPlayer.Player1);
            PlayerHealthComponent player2HealthComp = GameManager.Instance.GetPlayerComponent<PlayerHealthComponent>(EPlayer.Player2);

            if(player1HealthComp.GetHPPercentage() > player2HealthComp.GetHPPercentage())
            {
                UpdateRoundVictoryCounter(Player.Player1);
            }
            else if(player2HealthComp.GetHPPercentage() > player1HealthComp.GetHPPercentage())
            {
                UpdateRoundVictoryCounter(Player.Player2);
            }
            else
            {
                uint maxRoundsToWin = GameConfig.Instance.m_MaxRoundsToWin;
                if (m_Player1RoundVictoryCounter < maxRoundsToWin - 1 && m_Player2RoundVictoryCounter < maxRoundsToWin - 1)
                {
                    UpdateRoundVictoryCounter(Player.Player1);
                    UpdateRoundVictoryCounter(Player.Player2);
                }
            }

            OnRoundOver_Internal();
        }
    }

    private void UpdateRoundVictoryCounter(string victoryPlayer)
    {
        if (victoryPlayer == Player.Player1)
        {
            m_Player1RoundVictoryCounter++;
        }
        else
        {
            m_Player2RoundVictoryCounter++;
        }

        OnRoundVictoryCounterChanged.Invoke();
    }

    private void OnRoundOver_Internal()
    {
        Invoker.InvokeDelayed(RestartRound, GameConfig.Instance.m_TimeToWaitBetweenRounds);
        m_RoundIsOver = true;
        OnRoundOver.Invoke();
    }

    private void RestartRound()
    {
        uint maxRoundsToWin = GameConfig.Instance.m_MaxRoundsToWin;
        if (m_Player1RoundVictoryCounter >= maxRoundsToWin || m_Player2RoundVictoryCounter >= maxRoundsToWin)
        {
            m_Player1RoundVictoryCounter = m_Player2RoundVictoryCounter = 0;
        }

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
