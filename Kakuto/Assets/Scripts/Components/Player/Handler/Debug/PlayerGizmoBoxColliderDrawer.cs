﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGizmoBoxColliderDrawer : MonoBehaviour
{
#if UNITY_EDITOR
    public bool d_OverrideGizmo = true;

    [ConditionalField(true, "d_OverrideGizmo")]
    public Color d_GizmoColor = Color.red;

    void OnDrawGizmosSelected()
    {
        if (d_OverrideGizmo)
        {
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider)
            {
                if (boxCollider.enabled)
                {
                    Vector3 offset = boxCollider.offset;
                    Vector3 size = boxCollider.size;

                    Gizmos.color = d_GizmoColor;
                    Gizmos.DrawWireCube(transform.position + offset, size);
                }
            }
            else
            {
                Debug.LogError("No BoxCollider2D found on " + this);
            }
        }
    }
#endif
}
