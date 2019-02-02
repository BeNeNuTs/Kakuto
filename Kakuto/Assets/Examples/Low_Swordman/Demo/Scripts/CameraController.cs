using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float m_SmoothValue = 2.0f;

    private Vector3 m_TargetPosition;

    void Start()
    {
        m_TargetPosition = transform.position;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, m_TargetPosition, Time.deltaTime * m_SmoothValue);
    }
}
