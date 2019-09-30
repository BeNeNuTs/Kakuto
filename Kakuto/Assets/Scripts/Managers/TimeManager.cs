using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    private static TimeManager m_Instance = null;
    public static TimeManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                GameObject go = new GameObject();
                m_Instance = go.AddComponent<TimeManager>();
                go.name = "_TimeManager";
            }
            return m_Instance;
        }
    }

    private static float K_DEFAULT_FIXED_DELTA_TIME = Time.fixedDeltaTime;

    bool m_TimeScaleInProgress = false;
    float m_TimeScaleTimeStamp = 0f;
    float m_TimeScaleDuration = 2f;
    ETimeScaleBackToNormal m_TimeScaleBackToNormal = ETimeScaleBackToNormal.Instant;

    void Update()
    {
        if (m_TimeScaleInProgress)
        {
            if (m_TimeScaleBackToNormal == ETimeScaleBackToNormal.Smooth)
            {
                Time.timeScale += (1f / m_TimeScaleDuration) * Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
                Time.fixedDeltaTime = Time.timeScale * .02f;
                if(Time.timeScale == 1f)
                {
                    m_TimeScaleInProgress = false;
                }
            }
            else if (m_TimeScaleBackToNormal == ETimeScaleBackToNormal.Instant)
            {
                if (Time.unscaledTime >= m_TimeScaleTimeStamp + m_TimeScaleDuration)
                {
                    Time.timeScale = 1.0f;
                    Time.fixedDeltaTime = K_DEFAULT_FIXED_DELTA_TIME;
                    m_TimeScaleInProgress = false;
                }
            }
        }
    }

    public static void StartTimeScale(float timeScaleFactor, float timeScaleDuration, ETimeScaleBackToNormal timeScaleBackToNormal)
    {
        Time.timeScale = timeScaleFactor;
        Time.fixedDeltaTime = Time.timeScale * K_DEFAULT_FIXED_DELTA_TIME;
        Instance.m_TimeScaleDuration = timeScaleDuration;
        Instance.m_TimeScaleBackToNormal = timeScaleBackToNormal;
        Instance.m_TimeScaleTimeStamp = Time.unscaledTime;
        Instance.m_TimeScaleInProgress = true;
    }
}
