using UnityEngine;
using System.Collections;

public class CameraShakeManager : MonoBehaviour
{
    private static CameraShakeManager m_Instance = null;
    public static CameraShakeManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                GameObject go = new GameObject();
                m_Instance = go.AddComponent<CameraShakeManager>();
                go.name = "_CameraShakeManager";
            }
            return m_Instance;
        }
    }

    private Transform m_CamTransform;

    // How long the object should shake for.
    private float m_ShakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    private float m_ShakeAmount = 0.7f;
    private float m_DecreaseFactor = 1.0f;

    void OnEnable()
    {
        m_CamTransform = Camera.main.transform;
    }

    void Update()
    {
        if (m_ShakeDuration > 0)
        {
            m_CamTransform.localPosition += Random.insideUnitSphere * m_ShakeAmount;
            m_ShakeAmount -= Time.deltaTime * m_DecreaseFactor;
            m_ShakeDuration -= Time.deltaTime;
        }
        else
        {
            m_ShakeDuration = 0f;
        }
    }

    public static void Shake(float shakeAmount, float shakeDuration)
    {
        Instance.m_ShakeAmount = shakeAmount;
        Instance.m_ShakeDuration = shakeDuration;
    }
}