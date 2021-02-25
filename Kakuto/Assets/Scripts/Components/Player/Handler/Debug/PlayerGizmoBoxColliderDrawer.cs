using System;
using UnityEngine;

public class PlayerGizmoBoxColliderDrawer : MonoBehaviour
{
    public bool m_DrawCollider = true;
    [ConditionalField(true, nameof(m_DrawCollider))]
    public Color m_ColliderColor = Color.red;

    private DebugSettings m_DebugSettings;
    private BoxCollider2D m_BoxCollider;

    protected virtual void Awake()
    {
        if(m_DrawCollider)
        {
            m_DebugSettings = ScenesConfig.GetDebugSettings();
            m_BoxCollider = GetComponent<BoxCollider2D>();
            if (m_BoxCollider == null)
            {
                m_DrawCollider = false;
            }
        }
    }

    protected virtual void Update()
    {
        if (CanDrawCollider())
        {
            Bounds bounds = m_BoxCollider.bounds;
            Quaternion rotation = transform.rotation;
            rotation *= Quaternion.Euler(Vector3.right * 90f);
            GLDebug.DrawSquare(bounds.center, rotation, new Vector3(m_BoxCollider.size.x, 0f, m_BoxCollider.size.y), m_ColliderColor);
        }
    }

    protected virtual bool CanDrawCollider() { return m_DrawCollider && m_BoxCollider.enabled && m_DebugSettings.m_DisplayBoxColliders; }
}
