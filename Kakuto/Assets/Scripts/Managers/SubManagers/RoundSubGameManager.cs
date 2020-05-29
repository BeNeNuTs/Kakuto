using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RoundSubGameManager : SubGameManagerBase
{
    enum ELastRoundWinner
    {
        Player1,
        Player2,
        Both
    }

    private static readonly string K_ROUND_ENTRY_ANIM = "RoundEntry";
    private static readonly string K_ROUND_WON_ANIM = "RoundWon";
    private static readonly string K_ROUND_LOST_ANIM = "RoundLost";

    public static event UnityAction OnRoundVictoryCounterChanged;
    public static event UnityAction OnRoundBegin;
    public static event UnityAction OnRoundOver;

    private readonly uint[] m_PlayersRoundVictoryCounter = { 0, 0 };
    private ELastRoundWinner m_LastRoundWinner = ELastRoundWinner.Both;

    private bool m_RoundIsBegin = false;
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

    public override void OnPlayersReady()
    {
        base.OnPlayersReady();
        foreach(GameObject player in GameManager.Instance.GetPlayers())
        {
            EnablePlayer(player, false);
            player.GetComponentInChildren<Animator>().Play(K_ROUND_ENTRY_ANIM, 0, 0);
        }

        Invoker.InvokeDelayed(OnRoundBegin_Internal, GameConfig.Instance.m_RoundEntryTime);
    }

    private void EnablePlayer(GameObject player, bool enable)
    {
        player.GetComponent<PlayerAttackComponent>().enabled = enable;
        player.GetComponent<PlayerMovementComponent>().enabled = enable;
    }

    private void OnRoundBegin_Internal()
    {
        foreach (GameObject player in GameManager.Instance.GetPlayers())
        {
            EnablePlayer(player, true);
        }

        m_RoundIsBegin = true;
        OnRoundBegin?.Invoke();
    }

    private void OnPlayerDeath(string deadPlayerTag)
    {
        if(!m_RoundIsOver)
        {
            if (deadPlayerTag == Player.Player1)
            {
                UpdateRoundVictoryCounter(ELastRoundWinner.Player2);
            }
            else
            {
                UpdateRoundVictoryCounter(ELastRoundWinner.Player1);
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
                UpdateRoundVictoryCounter(ELastRoundWinner.Player1);
            }
            else if(player2HealthComp.GetHPPercentage() > player1HealthComp.GetHPPercentage())
            {
                UpdateRoundVictoryCounter(ELastRoundWinner.Player2);
            }
            else
            {
                uint maxRoundsToWin = GameConfig.Instance.m_MaxRoundsToWin;
                if (GetPlayerRoundVictoryCounter(EPlayer.Player1) < maxRoundsToWin - 1 && GetPlayerRoundVictoryCounter(EPlayer.Player2) < maxRoundsToWin - 1)
                {
                    UpdateRoundVictoryCounter(ELastRoundWinner.Both);
                }
                m_LastRoundWinner = ELastRoundWinner.Both;
            }

            OnRoundOver_Internal();
        }
    }

    private void UpdateRoundVictoryCounter(ELastRoundWinner roundWinner)
    {
        switch (roundWinner)
        {
            case ELastRoundWinner.Player1:
            case ELastRoundWinner.Player2:
                m_PlayersRoundVictoryCounter[(int)roundWinner]++;
                break;
            case ELastRoundWinner.Both:
                m_PlayersRoundVictoryCounter[(int)EPlayer.Player1]++;
                m_PlayersRoundVictoryCounter[(int)EPlayer.Player2]++;
                break;
        }
        m_LastRoundWinner = roundWinner;
        
        OnRoundVictoryCounterChanged?.Invoke();
    }

    private void OnRoundOver_Internal()
    {
        m_RoundIsOver = true;
        OnRoundOver?.Invoke();

        PlayWonAndLostRoundAnimation();

        Invoker.InvokeDelayed(RestartRound, GameConfig.Instance.m_TimeToWaitBetweenRounds);
    }

    private void PlayWonAndLostRoundAnimation()
    {
        switch (m_LastRoundWinner)
        {
            case ELastRoundWinner.Player1:
                PlayWonRoundAnimation(EPlayer.Player1);
                PlayLostRoundAnimation(EPlayer.Player2);
                break;
            case ELastRoundWinner.Player2:
                PlayWonRoundAnimation(EPlayer.Player2);
                PlayLostRoundAnimation(EPlayer.Player1);
                break;
            case ELastRoundWinner.Both:
                PlayLostRoundAnimation(EPlayer.Player1);
                PlayLostRoundAnimation(EPlayer.Player2);
                break;
            default:
                break;
        }
    }

    private void PlayWonRoundAnimation(EPlayer wonPlayer)
    {
        GameManager.Instance.GetPlayerComponent<Animator>(wonPlayer).Play(K_ROUND_WON_ANIM, 0, 0);
    }

    private void PlayLostRoundAnimation(EPlayer lostPlayer)
    {
        if (!GameManager.Instance.GetPlayerComponent<PlayerHealthComponent>(lostPlayer).IsDead())
        {
            GameManager.Instance.GetPlayerComponent<Animator>(lostPlayer).Play(K_ROUND_LOST_ANIM, 0, 0);
        }
    }

    private void RestartRound()
    {
        uint maxRoundsToWin = GameConfig.Instance.m_MaxRoundsToWin;
        if (GetPlayerRoundVictoryCounter(EPlayer.Player1) >= maxRoundsToWin || GetPlayerRoundVictoryCounter(EPlayer.Player2) >= maxRoundsToWin)
        {
            ResetPlayersRoundVictoryCounter();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        m_RoundIsBegin = false;
        m_RoundIsOver = false;
    }

    public uint GetPlayerRoundVictoryCounter(EPlayer player)
    {
        return m_PlayersRoundVictoryCounter[(int)player];
    }

    void ResetPlayersRoundVictoryCounter()
    {
        m_PlayersRoundVictoryCounter[(int)EPlayer.Player1] = 0;
        m_PlayersRoundVictoryCounter[(int)EPlayer.Player2] = 0;
    }

    public bool IsRoundOver()
    {
        return m_RoundIsOver;
    }

    public bool IsRoundBegin()
    {
        return m_RoundIsBegin;
    }
}
