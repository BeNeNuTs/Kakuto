using UnityEngine;

public class BackgroundCamera : MonoBehaviour
{
    public Camera m_BackgroundCamera;
    public Camera m_MainCamera;

    public float m_FOVMultiplier = 1.0f;

    private float m_BackgroundCamDefaultFOV;
    private float m_MainCamDefaultFOV;

    public void Start()
    {
        m_BackgroundCamDefaultFOV = m_BackgroundCamera.fieldOfView;
        m_MainCamDefaultFOV = m_MainCamera.orthographicSize;
    }

    public void Update()
    {
        float deltaFOV = m_MainCamera.orthographicSize - m_MainCamDefaultFOV;
        m_BackgroundCamera.fieldOfView = m_BackgroundCamDefaultFOV + deltaFOV * m_FOVMultiplier;
    }
}
