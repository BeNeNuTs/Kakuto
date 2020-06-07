using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class PlayerAnimationRootMotionHandler : MonoBehaviour
{
    private CharacterController2D m_CharacterController2D;
    private PlayerMovementComponent m_PlayerMovementComponent;
    private Rigidbody2D m_Rigidbody;

    private float m_GravityScale = 2.25f;

#if UNITY_EDITOR
    private bool m_LastUpdateCalled = false;
#endif

    private bool m_OriginalRootPositionSetted = false;
    private Vector3 m_OriginalRootPosition = Vector3.zero;

    [SerializeField, ReadOnly]
    private bool m_RootMotionEnabled = false;
    public bool RootMotionEnabled
    {
        set
        {
            m_RootMotionEnabled = value;
        }
        get { return m_RootMotionEnabled; }
    }

    [SerializeField, ReadOnly]
    private Vector3 m_RootMotion;
    public Vector3 RootMotion
    {
        set
        {
            m_RootMotion = value;
        }
        get
        {
            Vector3 finalRootMotion = m_RootMotion;
            finalRootMotion.x *= transform.lossyScale.x;
            return finalRootMotion;
        }
    }

    private void Awake()
    {
        m_CharacterController2D = GetComponentInParent<CharacterController2D>();
        m_PlayerMovementComponent = GetComponentInParent<PlayerMovementComponent>();
        m_Rigidbody = GetComponentInParent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if(CanUpdate())
        {
            UpdateRootPosition();

#if UNITY_EDITOR
            m_LastUpdateCalled = false;
#endif
        }
#if UNITY_EDITOR
        else
        {
            if(!m_LastUpdateCalled)
            {
                UpdateRootPosition();
                m_LastUpdateCalled = true;
            }
        }
#endif
    }

    private bool CanUpdate()
    {
#if UNITY_EDITOR
        return (AnimationMode.InAnimationMode() && IsSelected()) || Application.isPlaying;
#else
        return true;
#endif
    }

#if UNITY_EDITOR
    private bool IsSelected()
    {
        GameObject selectedGO = Selection.activeGameObject;
        if(selectedGO != null)
        {
            if (selectedGO == gameObject)
            {
                return true;
            }

            return selectedGO.transform.IsChildOf(gameObject.transform);
        }

        return false;
    }
#endif

    private void UpdateRootPosition()
    {
        if(RootMotionEnabled)
        {
            if (!m_OriginalRootPositionSetted)
            {
                m_OriginalRootPosition = transform.root.position;
                m_OriginalRootPositionSetted = true;

                if (Application.isPlaying)
                {
                    m_CharacterController2D.enabled = false;
                    m_PlayerMovementComponent.enabled = false;
                    m_GravityScale = m_Rigidbody.gravityScale;
                    m_Rigidbody.gravityScale = 0f;
                }
            }

            Vector3 newPosToReach = m_OriginalRootPosition + RootMotion;
            transform.root.position = newPosToReach;
        }
        else
        {
            if (m_OriginalRootPositionSetted)
            {
                if(Application.isPlaying)
                {
                    m_CharacterController2D.enabled = true;
                    m_PlayerMovementComponent.enabled = true;

                    if (m_GravityScale == 0f)
                    {
                        Debug.LogError("Stored gravity scale of " + transform.root.gameObject.name + " is zero !!");
                        m_GravityScale = 2.25f;
                    }
                    m_Rigidbody.gravityScale = m_GravityScale;
                }
                else
                {
                    transform.root.position = m_OriginalRootPosition;
                }

                m_OriginalRootPosition = Vector3.zero;
                m_OriginalRootPositionSetted = false;
            }
        }
    }
}
