using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class RoundComponent : MenuComponent
{
    public TextMeshProUGUI m_TimerText;
    public Image[] m_Player1VictoryRounds;
    public Image[] m_Player2VictoryRounds;
    public Animator m_RoundNotifAnimator;
    public GameObject m_EndRoundButtons;
    public Button m_DefaultSelectedButton;
    public Image m_P1Wins;
    public Image m_P2Wins;

    private float m_InitTimestamp = 0.0f;
    private uint m_RemaningTime = uint.MaxValue;
    private bool m_TimerOverNotified = false;

    private RoundSubGameManager m_RoundSubGameManager;
    private UISettings m_UISettings;

    protected void Awake()
    {
        m_RoundSubGameManager = GameManager.Instance.GetSubManager<RoundSubGameManager>(ESubManager.Round);
        m_RoundSubGameManager.RegisterRoundComponent(this);
        RoundSubGameManager.OnRoundVictoryCounterChanged += UpdateVictoryCounters;
        RoundSubGameManager.OnRoundBegin += OnRoundBegin;

        m_UISettings = ScenesConfig.GetUISettings();
    }

    protected void Start()
    {
        m_InitTimestamp = Time.time;
        UpdateRemainingTimeText();
        UpdateVictoryCounters();
    }

    protected void OnDestroy()
    {
        RoundSubGameManager.OnRoundVictoryCounterChanged -= UpdateVictoryCounters;
        RoundSubGameManager.OnRoundBegin -= OnRoundBegin;
    }

    void OnRoundBegin()
    {
        m_InitTimestamp = Time.time;
        UpdateRemainingTimeText();
    }

    void UpdateVictoryCounters()
    {
        if(ScenesConfig.GetUISettings().m_IsCounterEnabled)
        {
            uint player1VictoryCounter = m_RoundSubGameManager.GetPlayerRoundVictoryCounter(EPlayer.Player1);
            for (int i = 0; i < m_Player1VictoryRounds.Length; i++)
            {
                m_Player1VictoryRounds[i].enabled = (player1VictoryCounter > i);
            }

            uint player2VictoryCounter = m_RoundSubGameManager.GetPlayerRoundVictoryCounter(EPlayer.Player2);
            for (int i = 0; i < m_Player2VictoryRounds.Length; i++)
            {
                m_Player2VictoryRounds[i].enabled = (player2VictoryCounter > i);
            }
        }
    }

    void UpdateRemainingTimeText()
    {
        m_RemaningTime = (uint)Mathf.Max(0f, GameConfig.Instance.m_RoundDuration - (Time.time - m_InitTimestamp));
        m_TimerText.SetText(m_RemaningTime.ToString());
    }

    protected void Update()
    {
        if(m_UISettings.m_IsTimerEnabled && m_RoundSubGameManager.IsRoundBegin() && !m_RoundSubGameManager.IsRoundOver())
        {
            UpdateRemainingTimeText();
        }
        else if(m_EndRoundButtons.activeSelf)
        {
            if(EventSystem.current != null)
            {
                GameObject selectedGO = EventSystem.current.currentSelectedGameObject;
                if (selectedGO == null)
                {
                    m_DefaultSelectedButton.Select();
                }
                UpdateButtonClick();
                UpdateDpadNavigation();
            }
        }
    }

    protected void LateUpdate()
    {
        if(m_RemaningTime == 0 && !m_TimerOverNotified)
        {
            m_RoundSubGameManager.OnTimerOver();
            m_TimerOverNotified = true;
        }
    }

    public uint GetRemainingTime()
    {
        return m_RemaningTime;
    }

    public void DisplayEndRoundButtons(RoundSubGameManager.ELastRoundWinner winner)
    {
        bool p1wins = winner == RoundSubGameManager.ELastRoundWinner.Player1;
        m_P1Wins.enabled = p1wins;
        m_P2Wins.enabled = !p1wins;

        m_EndRoundButtons.SetActive(true);
        m_DefaultSelectedButton.Select();
    }

    public void ReplayRound()
    {
        m_RoundSubGameManager.ReplayRound();
    }

    public void BackToMainMenu()
    {
        GameManager.Instance.GetSubManager<GameFlowSubGameManager>(ESubManager.GameFlow).LoadScene("Menu", false);
    }
}
