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
    private Vector3 m_PreviousPosToReach = Vector3.zero;

    private static readonly float k_Epsilon = 0.001f;
    private List<Vector3> m_FailedPosToReach;

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
        m_FailedPosToReach = new List<Vector3>();
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
        return (AnimationMode.InAnimationMode() && Selection.activeGameObject == gameObject) || Application.isPlaying;
#else
        return true;
#endif
    }

    private void UpdateRootPosition()
    {
        if(RootMotionEnabled)
        {
            if (!m_OriginalRootPositionSetted)
            {
                m_OriginalRootPosition = transform.root.position;
                m_PreviousPosToReach = m_OriginalRootPosition;
                m_FailedPosToReach.Clear();
                m_OriginalRootPositionSetted = true;

                if (Application.isPlaying)
                {
                    m_CharacterController2D.enabled = false;
                    m_PlayerMovementComponent.enabled = false;
                    m_GravityScale = m_Rigidbody.gravityScale;
                    m_Rigidbody.gravityScale = 0f;
                }
            }

            if(Application.isPlaying)
            {
                // If the transform position is different than the previous pos to reach
                // This means something has blocked the player displacement
                // In this case, store the previousPosToReach in FailedPosToReach in order to keep it in memory
                if(Vector3.SqrMagnitude(transform.root.position - m_PreviousPosToReach) > k_Epsilon)
                {
                    m_FailedPosToReach.Add(m_PreviousPosToReach);
                }
            }

            Vector3 newPosToReach = m_OriginalRootPosition + RootMotion;
            bool transformPositionUpdated = false;

            // If there are some failed pos to reach in the list
            if (m_FailedPosToReach.Count > 0)
            {
                // Try to reach them before trying to reach the new current one
                for(int i = 0; i < m_FailedPosToReach.Count; i++)
                {
                    // If the first one is not reached yet, try to reach it
                    if (Vector3.SqrMagnitude(transform.root.position - m_FailedPosToReach[i]) > k_Epsilon)
                    {
                        transform.root.position = m_FailedPosToReach[i];
                        transformPositionUpdated = true;
                        break;
                    }
                    // If this pos is already reached, remove it
                    else
                    {
                        m_FailedPosToReach.RemoveAt(i);
                        i--;
                    }
                }
            }
            
            // If transform position has not been updated (by a previous pos to reach for example), update it with the new pos to reach
            if(!transformPositionUpdated)
            {
                transform.root.position = newPosToReach;
            }
            
            m_PreviousPosToReach = newPosToReach;
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
                m_PreviousPosToReach = Vector3.zero;
                m_FailedPosToReach.Clear();
                m_OriginalRootPositionSetted = false;
            }
        }
    }
}
