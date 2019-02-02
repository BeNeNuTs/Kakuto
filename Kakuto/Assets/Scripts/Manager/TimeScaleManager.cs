using UnityEngine;
using System.Collections;

public static class TimeScaleManager
{
    private static int m_PendingTimeScale = 0;

    public static void StartTimeScale(float amount, float duration)
    {
        m_PendingTimeScale++;

        Time.timeScale = amount;
        Invoker.InvokeDelayed(StopTimeScale, duration);
    }

    public static void StopTimeScale()
    {
        m_PendingTimeScale--;
        if (m_PendingTimeScale <= 0)
        {
            Time.timeScale = 1.0f;
            if(m_PendingTimeScale < 0)
            {
                Debug.LogError("PendingTimeScale should not be inferior to 0.");
            }
        }
    }
}
