using System.Collections;
using System.Threading;
using UnityEngine;

public class FrameRateSubGameManager : SubGameManagerBase
{
    private float m_CurrentFrameTime;

    public override void Init()
    {
        base.Init();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        m_CurrentFrameTime = Time.realtimeSinceStartup;

        GameManager.Instance.StartCoroutine(WaitForNextFrame());
    }

    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            m_CurrentFrameTime += 1.0f / GameConfig.Instance.m_GameFPS;
            float t = Time.realtimeSinceStartup;
            float sleepTime = m_CurrentFrameTime - t - 0.01f;
            if (sleepTime > 0f)
                Thread.Sleep((int)(sleepTime * 1000));
            while (t < m_CurrentFrameTime)
                t = Time.realtimeSinceStartup;
        }
    }
}
