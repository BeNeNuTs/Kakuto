using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeScaleSubGameManager : SubGameManagerBase
{
    private static float K_DEFAULT_FIXED_DELTA_TIME = Time.fixedDeltaTime;

    bool m_TimeIsFrozen = false;
    public bool IsTimeFrozen => m_TimeIsFrozen;
    bool m_TimeScaleInProgress = false;
    float m_TimeScaleTimeStamp = 0f;
    float m_TimeScaleDuration = 2f;
    ETimeScaleBackToNormal m_TimeScaleBackToNormal = ETimeScaleBackToNormal.Instant;

    public override void OnSceneUnloaded(Scene unloadedScene)
    {
        UnfreezeTime();
    }

    public override void Update()
    {
        if (m_TimeScaleInProgress)
        {
            if (m_TimeScaleBackToNormal == ETimeScaleBackToNormal.Smooth)
            {
                Time.timeScale += (1f / m_TimeScaleDuration) * Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
                Time.fixedDeltaTime = Time.timeScale * K_DEFAULT_FIXED_DELTA_TIME;
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

    public void StartTimeScale(TimeScaleParams timeScaleParams)
    {
        if(!m_TimeIsFrozen)
        {
            Time.timeScale = timeScaleParams.m_TimeScaleAmount;
            Time.fixedDeltaTime = Time.timeScale * K_DEFAULT_FIXED_DELTA_TIME;
            m_TimeScaleDuration = timeScaleParams.m_TimeScaleDuration;
            m_TimeScaleBackToNormal = timeScaleParams.m_TimeScaleBackToNormal;
            m_TimeScaleTimeStamp = Time.unscaledTime;
            m_TimeScaleInProgress = true;
        }
    }

    public void FreezeTime(Animator unscaledTimeAnimator = null)
    {
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
        m_TimeIsFrozen = true;

        m_TimeScaleInProgress = false; // stop time scale in progress

        if(unscaledTimeAnimator != null)
        {
            unscaledTimeAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    public void UnfreezeTime(Animator unscaledTimeAnimator = null)
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = K_DEFAULT_FIXED_DELTA_TIME;
        m_TimeIsFrozen = false;

        if (unscaledTimeAnimator != null)
        {
            unscaledTimeAnimator.updateMode = AnimatorUpdateMode.Normal;
        }
    }
}
