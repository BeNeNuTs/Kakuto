using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundComponent : MonoBehaviour
{
    public TextMeshProUGUI m_TimerText;
    public TextMeshProUGUI m_CounterText;

    private float m_InitTimestamp = 0.0f;
    private uint m_RemaningTime = uint.MaxValue;
    private bool m_TimerOverNotified = false;

    private RoundSubGameManager m_RoundSubGameManager;

    private void Start()
    {
        m_RoundSubGameManager = GameManager.Instance.GetSubManager<RoundSubGameManager>(ESubManager.Round);
        RoundSubGameManager.OnRoundVictoryCounterChanged += UpdateCounterText;
        RoundSubGameManager.OnRoundBegin += OnRoundBegin;

        m_InitTimestamp = Time.unscaledTime;
        UpdateRemainingTimeText();
        UpdateCounterText();
    }

    void OnRoundBegin()
    {
        m_InitTimestamp = Time.unscaledTime;
        UpdateRemainingTimeText();
    }

    void UpdateCounterText()
    {
        if(ScenesConfig.GetUISettings().m_IsCounterEnabled)
        {
            m_CounterText.SetText(m_RoundSubGameManager.GetPlayerRoundVictoryCounter(EPlayer.Player1) + " - " + m_RoundSubGameManager.GetPlayerRoundVictoryCounter(EPlayer.Player2));
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
