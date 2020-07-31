using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundComponent : MonoBehaviour
{
    public TextMeshProUGUI m_TimerText;
    public Image[] m_Player1VictoryRounds;
    public Image[] m_Player2VictoryRounds;

    private float m_InitTimestamp = 0.0f;
    private uint m_RemaningTime = uint.MaxValue;
    private bool m_TimerOverNotified = false;

    private RoundSubGameManager m_RoundSubGameManager;

    private void Start()
    {
        m_RoundSubGameManager = GameManager.Instance.GetSubManager<RoundSubGameManager>(ESubManager.Round);
        RoundSubGameManager.OnRoundVictoryCounterChanged += UpdateVictoryCounters;
        RoundSubGameManager.OnRoundBegin += OnRoundBegin;

        m_InitTimestamp = Time.unscaledTime;
        UpdateRemainingTimeText();
        UpdateVictoryCounters();
    }

    private void OnDestroy()
    {
        RoundSubGameManager.OnRoundVictoryCounterChanged -= UpdateVictoryCounters;
        RoundSubGameManager.OnRoundBegin -= OnRoundBegin;
    }

    void OnRoundBegin()
    {
        m_InitTimestamp = Time.unscaledTime;
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
        m_RemaningTime = (uint)Mathf.Max(0f, GameConfig.Instance.m_RoundDuration - (Time.unscaledTime - m_InitTimestamp));
        m_TimerText.SetText(m_RemaningTime.ToString());
    }

    void Update()
    {
        if(ScenesConfig.GetUISettings().m_IsTimerEnabled && m_RoundSubGameManager.IsRoundBegin() && !m_RoundSubGameManager.IsRoundOver())
        {
            UpdateRemainingTimeText();
        }
    }

    void LateUpdate()
    {
        if(m_RemaningTime == 0 && !m_TimerOverNotified)
        {
            m_RoundSubGameManager.OnTimerOver();
            m_TimerOverNotified = true;
        }
    }
}
