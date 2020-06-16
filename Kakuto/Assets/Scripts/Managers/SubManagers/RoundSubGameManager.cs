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

    private bool[] m_PlayerEndOfRoundAnimationFinished = { false, false };

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

        Invoker.InvokeDelayed(PlayWonAndLostRoundAnimation, GameConfig.Instance.m_TimeToWaitBeforeEndRoundAnimations);
    }

    private bool ArePlayersReadyToPlayEndRoundAnimations()
    {
        foreach (GameObject player in GameManager.Instance.GetPlayers())
        {
            PlayerHealthComponent playerHealth = player.GetComponent<PlayerHealthComponent>();
            if (playerHealth && !playerHealth.IsDead())
            {
                Animator playerAnimator = player.GetComponentInChildren<Animator>();
                if (playerAnimator != null && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("StandIdle"))
                {
                    // If player's not dead and not playing StandIdle animation, then he's not ready to play end round animations
                    return false;
                }
            }
        }

        return true;
    }

    private void PlayWonAndLostRoundAnimation()
    {
        if(!ArePlayersReadyToPlayEndRoundAnimations())
        {
            // Player's not ready yet, wait one more second
            Invoker.InvokeDelayed(PlayWonAndLostRoundAnimation, 1f);
            return;
        }

        bool player1PlayedAnim = false;
        bool player2PlayedAnim = false;
        switch (m_LastRoundWinner)
        {
            case ELastRoundWinner.Player1:
                player1PlayedAnim = PlayWonRoundAnimation(EPlayer.Player1);
                player2PlayedAnim = PlayLostRoundAnimation(EPlayer.Player2);
                break;
            case ELastRoundWinner.Player2:
                player1PlayedAnim = PlayLostRoundAnimation(EPlayer.Player1);
                player2PlayedAnim = PlayWonRoundAnimation(EPlayer.Player2);
                break;
            case ELastRoundWinner.Both:
                player1PlayedAnim = PlayLostRoundAnimation(EPlayer.Player1);
                player2PlayedAnim = PlayLostRoundAnimation(EPlayer.Player2);
                break;
            default:
                break;
        }

        m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player1] = !player1PlayedAnim;
        if (player1PlayedAnim)
        {
            Utils.GetPlayerEventManager<EPlayer>(GameManager.Instance.GetPlayer(EPlayer.Player1)).StartListening(EPlayerEvent.EndOfRoundAnimation, EndOfPlayerRoundAnimation);
        }

        m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player2] = !player2PlayedAnim;
        if (player2PlayedAnim)
        {
            Utils.GetPlayerEventManager<EPlayer>(GameManager.Instance.GetPlayer(EPlayer.Player2)).StartListening(EPlayerEvent.EndOfRoundAnimation, EndOfPlayerRoundAnimation);
        }
    }

    private bool PlayWonRoundAnimation(EPlayer wonPlayer)
    {
        GameManager.Instance.GetPlayerComponent<Animator>(wonPlayer).Play(K_ROUND_WON_ANIM, 0, 0);
        return true;
    }

    private bool PlayLostRoundAnimation(EPlayer lostPlayer)
    {
        if (!GameManager.Instance.GetPlayerComponent<PlayerHealthComponent>(lostPlayer).IsDead())
        {
            GameManager.Instance.GetPlayerComponent<Animator>(lostPlayer).Play(K_ROUND_LOST_ANIM, 0, 0);
            return true;
        }

        return false;
    }

    private void EndOfPlayerRoundAnimation(EPlayer player)
    {
        m_PlayerEndOfRoundAnimationFinished[(int)player] = true;
        Utils.GetPlayerEventManager<EPlayer>(GameManager.Instance.GetPlayer(player)).StopListening(EPlayerEvent.EndOfRoundAnimation, EndOfPlayerRoundAnimation);

        if (m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player1] && m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player2])
        {
            RestartRound();
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
