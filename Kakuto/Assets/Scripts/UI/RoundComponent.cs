using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundComponent : MonoBehaviour
{
    public bool m_IsTimerEnabled = true;
    public bool m_IsCounterEnabled = true;

    public TextMeshProUGUI m_TimerText;
    public TextMeshProUGUI m_CounterText;

    private float m_InitTimestamp = 0.0f;
    private uint m_RemaningTime = uint.MaxValue;
    private bool m_TimerOverNotified = false;

    private RoundSubGameManager m_RoundSubGameManager;

    private void Start()
    {
        m_InitTimestamp = Time.unscaledTime;
        m_RoundSubGameManager = GameManager.Instance.GetSubManager<RoundSubGameManager>(ESubManager.Round);

        UpdateCounterText();

        RoundSubGameManager.OnRoundVictoryCounterChanged += UpdateCounterText;
    }

    void UpdateCounterText()
    {
        m_CounterText.SetText(m_RoundSubGameManager.GetPlayerRoundVictoryCounter(EPlayer.Player1) + " - " + m_RoundSubGameManager.GetPlayerRoundVictoryCounter(EPlayer.Player2));
    }

    void Update()
    {
        if(m_IsTimerEnabled && !m_RoundSubGameManager.IsRoundOver())
        {
            m_RemaningTime = (uint)Mathf.Max(0f, GameConfig.Instance.m_RoundDuration - (Time.unscaledTime - m_InitTimestamp));
            m_TimerText.SetText(m_RemaningTime.ToString());
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
