using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundSubGameManager : SubGameManagerBase
{
    public enum ELastRoundWinner
    {
        Player1,
        Player2,
        Both
    }

    enum ERoundNotif
    {
        Round1,
        Round2,
        FinalRound,
        DoubleKO,
        KO,
        Perfect,
        TimeOver
    }

    private static readonly string K_ROUND_ENTRY_ANIM = "RoundEntry";
    private static readonly string K_ROUND_WON_ANIM = "RoundWon";
    private static readonly string K_ROUND_LOST_ANIM = "RoundLost";
    private static readonly string K_STANDIDLE_TAG = "StandIdle";

    private Scene m_RoundScene;
    public static Action OnRoundVictoryCounterChanged;
    public static Action OnRoundBegin;
    public static Action<ELastRoundWinner> OnRoundOver;

    public RoundComponent m_RoundComponent;

    private IEnumerator m_RoundBeginCoroutine = null;
    private IEnumerator m_OnPlayerDeathCoroutine = null;
    private IEnumerator m_PlayWonAndLostRoundAnimCoroutine = null;
    private IEnumerator m_RestartRoundCoroutine = null;

    private readonly uint[] m_PlayersRoundVictoryCounter = { 0, 0 };
    private bool m_LastRoundTimeOver = false;
    private ELastRoundWinner m_LastRoundWinner = ELastRoundWinner.Both;
    private readonly float[] m_PlayersEndRoundHPPercentage = { 0f, 0f };

    private bool m_RoundIsBegin = false;
    private bool m_RoundIsOver = false;

    private bool[] m_PlayerEndOfRoundAnimationFinished = { false, false };

    private readonly float[] m_PlayersSuperGaugeValues = { 0, 0 };

    private GameFlowSubGameManager m_GameFlowManager;
    private TimeScaleSubGameManager m_TimeManager;
    private GameConfig m_GameConfig;
    private AttackConfig m_AttackConfig;

    public override void Init()
    {
        base.Init();
        Utils.GetPlayerEventManager(Player.Player1).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager(Player.Player2).StartListening(EPlayerEvent.OnDeath, OnPlayerDeath);

        m_GameFlowManager = GameManager.Instance.GetSubManager<GameFlowSubGameManager>(ESubManager.GameFlow);
        m_TimeManager = GameManager.Instance.GetSubManager<TimeScaleSubGameManager>(ESubManager.TimeScale);
        m_GameConfig = GameConfig.Instance;
        m_AttackConfig = AttackConfig.Instance;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        Utils.GetPlayerEventManager(Player.Player1).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
        Utils.GetPlayerEventManager(Player.Player2).StopListening(EPlayerEvent.OnDeath, OnPlayerDeath);
    }

    public override void OnSceneUnloaded(Scene unloadedScene)
    {
        if(m_RoundScene.IsValid())
        {
            if (m_RoundScene == unloadedScene && m_RoundScene.name.CompareTo(m_GameFlowManager.GetLoadingScene()) != 0)
            {
                if (m_RoundBeginCoroutine != null)
                {
                    GameManager.Instance.StopCoroutine(m_RoundBeginCoroutine);
                    m_RoundBeginCoroutine = null;
                }
                if (m_OnPlayerDeathCoroutine != null)
                {
                    GameManager.Instance.StopCoroutine(m_OnPlayerDeathCoroutine);
                    m_OnPlayerDeathCoroutine = null;
                }
                if (m_PlayWonAndLostRoundAnimCoroutine != null)
                {
                    GameManager.Instance.StopCoroutine(m_PlayWonAndLostRoundAnimCoroutine);
                    m_PlayWonAndLostRoundAnimCoroutine = null;
                }
                if (m_RestartRoundCoroutine != null)
                {
                    GameManager.Instance.StopCoroutine(m_RestartRoundCoroutine);
                    m_RestartRoundCoroutine = null;
                }

                ResetPlayersRoundVictoryCounter();
                ResetPlayersSuperGaugeValues();

                m_RoundIsBegin = false;
                m_RoundIsOver = false;
                m_RoundScene = default;
            }
        }
    }

    public override void OnPlayersRegistered()
    {
        foreach(GameObject player in GameManager.Instance.GetPlayers())
        {
            EnablePlayer(player, false);
            player.GetComponentInChildren<Animator>().Play(K_ROUND_ENTRY_ANIM, 0, 0);
        }

        m_RoundScene = SceneManager.GetActiveScene();
        m_RoundBeginCoroutine = OnRoundBegin_Internal();
        GameManager.Instance.StartCoroutine(m_RoundBeginCoroutine);
    }

    private void EnablePlayer(GameObject player, bool enable)
    {
        player.GetComponent<PlayerAttackComponent>().enabled = enable;
        player.GetComponent<PlayerMovementComponent>().enabled = enable;
    }

    private IEnumerator OnRoundBegin_Internal()
    {
        yield return new WaitForEndOfFrame(); // Wait for end of frame in order to let the animator play the K_ROUND_ENTRY_ANIM

        float startRoundUIDelay = 0f;
        if (ScenesConfig.GetUISettings().m_IsCounterEnabled)
        {
            startRoundUIDelay = m_GameConfig.m_StartRoundUIDelay;
            yield return new WaitForSeconds(startRoundUIDelay);
            PlayStartRoundNotification();

            yield return new WaitForSeconds(m_GameConfig.m_RoundEntryTime - startRoundUIDelay);
        }

        while (!ArePlayersReady())
        {
            // Player's not ready yet, wait one more second
            yield return new WaitForSeconds(1f);
        }

        foreach (GameObject player in GameManager.Instance.GetPlayers())
        {
            EnablePlayer(player, true);
        }

        m_RoundIsBegin = true;
        m_LastRoundTimeOver = false;
        OnRoundBegin?.Invoke();
    }

    private void OnPlayerDeath(BaseEventParameters baseParams)
    {
        if (m_OnPlayerDeathCoroutine != null)
        {
            GameManager.Instance.StopCoroutine(m_OnPlayerDeathCoroutine);
            m_OnPlayerDeathCoroutine = null;
        }

        m_OnPlayerDeathCoroutine = OnPlayerDeathCoroutine();
        GameManager.Instance.StartCoroutine(m_OnPlayerDeathCoroutine);
    }

    private IEnumerator OnPlayerDeathCoroutine()
    {
        yield return new WaitForEndOfFrame();
        OnRoundOver_Internal();
    }

    public void OnTimerOver()
    {
        m_LastRoundTimeOver = true;
        OnRoundOver_Internal();
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
        if (!m_RoundIsOver)
        {
            PlayerHealthComponent player1HealthComp = GameManager.Instance.GetPlayerComponent<PlayerHealthComponent>(EPlayer.Player1);
            m_PlayersEndRoundHPPercentage[0] = player1HealthComp.GetHPPercentage();

            PlayerHealthComponent player2HealthComp = GameManager.Instance.GetPlayerComponent<PlayerHealthComponent>(EPlayer.Player2);
            m_PlayersEndRoundHPPercentage[1] = player2HealthComp.GetHPPercentage();

            if (m_PlayersEndRoundHPPercentage[0] > m_PlayersEndRoundHPPercentage[1])
            {
                UpdateRoundVictoryCounter(ELastRoundWinner.Player1);
            }
            else if (m_PlayersEndRoundHPPercentage[1] > m_PlayersEndRoundHPPercentage[0])
            {
                UpdateRoundVictoryCounter(ELastRoundWinner.Player2);
            }
            else
            {
                uint maxRoundsToWin = m_GameConfig.m_MaxRoundsToWin;
                if (GetPlayerRoundVictoryCounter(EPlayer.Player1) < maxRoundsToWin - 1 && GetPlayerRoundVictoryCounter(EPlayer.Player2) < maxRoundsToWin - 1)
                {
                    UpdateRoundVictoryCounter(ELastRoundWinner.Both);
                }
                m_LastRoundWinner = ELastRoundWinner.Both;
            }

            m_RoundIsOver = true;
            OnRoundOver?.Invoke(m_LastRoundWinner);

            m_PlayWonAndLostRoundAnimCoroutine = PlayWonAndLostRoundAnimation();
            GameManager.Instance.StartCoroutine(m_PlayWonAndLostRoundAnimCoroutine);
        }
    }

    private bool ArePlayersReady()
    {
        foreach (GameObject player in GameManager.Instance.GetPlayers())
        {
            PlayerHealthComponent playerHealth = player.GetComponent<PlayerHealthComponent>();
            if (playerHealth && !playerHealth.IsDead())
            {
                Animator playerAnimator = player.GetComponentInChildren<Animator>();
                AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
                if (playerAnimator != null && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag(K_STANDIDLE_TAG))
                {
                    // If player's not dead and not playing StandIdle animation, then he's not ready to play end round animations
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerator PlayWonAndLostRoundAnimation()
    {
        // Freeze time in order to play KO notif
        m_TimeManager.FreezeTime();
        
        PlayEndRoundNotification();

        // Wait 2 frames in order to be sure that round anim has been played correctly
        yield return null;
        yield return new WaitForEndOfFrame();

        // Wait for a certain normalized time before playing won / lost player anim
        while (m_RoundComponent.m_RoundNotifAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < m_GameConfig.m_NormalizedTimeToWaitBeforeEndRoundAnimations)
        {
            yield return null;
        }

        // Unfreeze time and start death time scale params, for final slow motion
        m_TimeManager.UnfreezeTime();
        m_TimeManager.StartTimeScale(AttackConfig.Instance.m_OnDeathTimeScaleParams);

        while(!ArePlayersReady())
        {
            // Player's not ready yet, wait one more second
            yield return new WaitForSeconds(1f);
        }

        // Be sure to unfreeze time here as DeathTimeParams has a certain duration but players can be ready immediately
        // And we don't want to play won/lost anim in slow mo
        m_TimeManager.UnfreezeTime();

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
        m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player2] = !player2PlayedAnim;

        if(m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player1] && m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player2])
        {
            m_RestartRoundCoroutine = RestartRound_Internal();
            GameManager.Instance.StartCoroutine(m_RestartRoundCoroutine);
        }
        else
        {
            if (player1PlayedAnim)
            {
                Utils.GetPlayerEventManager(GameManager.Instance.GetPlayer(EPlayer.Player1)).StartListening(EPlayerEvent.EndOfRoundAnimation, EndOfPlayerRoundAnimation);
            }

            if (player2PlayedAnim)
            {
                Utils.GetPlayerEventManager(GameManager.Instance.GetPlayer(EPlayer.Player2)).StartListening(EPlayerEvent.EndOfRoundAnimation, EndOfPlayerRoundAnimation);
            }
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

    private void EndOfPlayerRoundAnimation(BaseEventParameters baseParams)
    {
        EndOfRoundAnimationEventParameters endOfRoundAnimParams = (EndOfRoundAnimationEventParameters)baseParams;
        EPlayer player = endOfRoundAnimParams.m_Player;

        m_PlayerEndOfRoundAnimationFinished[(int)player] = true;
        Utils.GetPlayerEventManager(GameManager.Instance.GetPlayer(player)).StopListening(EPlayerEvent.EndOfRoundAnimation, EndOfPlayerRoundAnimation);

        if (m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player1] && m_PlayerEndOfRoundAnimationFinished[(int)EPlayer.Player2])
        {
            m_RestartRoundCoroutine = RestartRound_Internal();
            GameManager.Instance.StartCoroutine(m_RestartRoundCoroutine);
        }
    }

    private IEnumerator RestartRound_Internal()
    {
        yield return new WaitForSeconds(m_GameConfig.m_TimeToWaitAfterEndRoundAnimations);
        m_RoundIsBegin = false;
        m_RoundIsOver = false;

        uint maxRoundsToWin = m_GameConfig.m_MaxRoundsToWin;
        if (GetPlayerRoundVictoryCounter(EPlayer.Player1) >= maxRoundsToWin || GetPlayerRoundVictoryCounter(EPlayer.Player2) >= maxRoundsToWin)
        {
            m_RoundComponent.DisplayEndRoundButtons(m_LastRoundWinner);
            m_GameConfig.m_DuckMusicSnapshot.TransitionTo(1f);
        }
        else
        {
            SetPlayersSuperGaugeValues();
            GameManager.Instance.GetSubManager<GameFlowSubGameManager>(ESubManager.GameFlow).LoadScene(SceneManager.GetActiveScene().name, true);
        }
    }

    public void ReplayRound()
    {
        ResetPlayersRoundVictoryCounter();
        ResetPlayersSuperGaugeValues();
        GameManager.Instance.GetSubManager<GameFlowSubGameManager>(ESubManager.GameFlow).LoadScene(SceneManager.GetActiveScene().name, true);
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

    public float GetPlayerSuperGaugeValue(EPlayer player)
    {
        return m_PlayersSuperGaugeValues[(int)player];
    }

    void SetPlayersSuperGaugeValues()
    {
        m_PlayersSuperGaugeValues[(int)EPlayer.Player1] = GameManager.Instance.GetPlayer(EPlayer.Player1).GetComponent<PlayerAttackComponent>().GetSuperGaugeSubComponent().GetCurrentGaugeValue();
        m_PlayersSuperGaugeValues[(int)EPlayer.Player2] = GameManager.Instance.GetPlayer(EPlayer.Player2).GetComponent<PlayerAttackComponent>().GetSuperGaugeSubComponent().GetCurrentGaugeValue();
    }

    void ResetPlayersSuperGaugeValues()
    {
        m_PlayersSuperGaugeValues[(int)EPlayer.Player1] = 0;
        m_PlayersSuperGaugeValues[(int)EPlayer.Player2] = 0;
    }

    public void RegisterRoundComponent(RoundComponent roundComponent)
    {
        m_RoundComponent = roundComponent;
    }

    void PlayStartRoundNotification()
    {
        uint player1VictoryCounter = GetPlayerRoundVictoryCounter(EPlayer.Player1);
        uint player2VictoryCounter = GetPlayerRoundVictoryCounter(EPlayer.Player2);
        ERoundNotif roundNotif = ERoundNotif.Round1;
        uint maxRoundToWin = m_GameConfig.m_MaxRoundsToWin;
        if (player1VictoryCounter >= maxRoundToWin - 1 && player2VictoryCounter >= maxRoundToWin - 1)
        {
            roundNotif = ERoundNotif.FinalRound;
        }
        else if (player1VictoryCounter > 0 || player2VictoryCounter > 0)
        {
            roundNotif = ERoundNotif.Round2;
        }
        PlayRoundNotification(true, roundNotif);
    }

    void PlayEndRoundNotification()
    {
        if(m_LastRoundTimeOver)
        {
            PlayRoundNotification(false, ERoundNotif.TimeOver);
        }
        else
        {
            if(m_LastRoundWinner == ELastRoundWinner.Player1 || m_LastRoundWinner == ELastRoundWinner.Player2)
            {
                bool isPerfect = m_PlayersEndRoundHPPercentage[m_LastRoundWinner == ELastRoundWinner.Player1 ? 0 : 1] == 1f;
                PlayRoundNotification(false, isPerfect ? ERoundNotif.Perfect : ERoundNotif.KO);
            }
            else
            {
                PlayRoundNotification(false, ERoundNotif.DoubleKO);
            }
        }
    }

    void PlayRoundNotification(bool startRound, ERoundNotif roundNotif)
    {
        string animToPlay = "Round" + ((startRound) ? "Start_" : "End_") + roundNotif.ToString();
        m_RoundComponent.m_RoundNotifAnimator.Play(animToPlay, 0, 0);
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
