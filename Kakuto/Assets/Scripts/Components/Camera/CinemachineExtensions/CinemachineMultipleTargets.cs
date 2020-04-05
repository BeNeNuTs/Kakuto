using UnityEngine;
using System.Collections.Generic;
using Cinemachine.Utility;
using System;

namespace Cinemachine
{
    /// <summary>
    /// An add-on module for Cinemachine Virtual Camera that post-processes
    /// the final position of the follow point of the camera. It will determine the virtual
    /// camera's follow point position according to the center of the boundingBox of targets and manage the zoom ratio.
    /// according to the distance between targets
    /// </summary>
    [SaveDuringPlay]
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    public class CinemachineMultipleTargets : CinemachineExtension
    {
        [Range(1f, 2f)]
        public float m_MinOrthographicSize = 1.2f;
        [Range(0.1f, 5f)]
        public float m_MinDistanceBetweenTargets = 2.6f;
        [Range(1f, 2f)]
        public float m_MaxOrthographicSize = 1.4f;
        [Range(0.1f, 5f)]
        public float m_MaxDistanceBetweenTargets = 3.5f;

        [Range(0.1f, 20f)]
        public float m_OrthographicDamping = 1f;

        private float m_PreviousOrthographicSize = 1.2f;

        private List<Transform> m_Targets = new List<Transform>();
        private Vector3 m_Offset = Vector3.zero;

        private Bounds m_TargetsBounds;

        void Start()
        {
            ComputeOffset();
            m_PreviousOrthographicSize = VirtualCamera.State.Lens.OrthographicSize;
        }

        void ComputeOffset()
        {
            //Need to detect ground
            GameObject ground = GameObject.FindGameObjectWithTag("Ground");
            if (ground != null)
            {
                float groundYPos = ground.transform.position.y;
                BoxCollider2D groundCollider = ground.GetComponent<BoxCollider2D>();
                if (groundCollider != null)
                {
                    groundYPos += groundCollider.offset.y;
                    groundYPos += groundCollider.size.y / 2f;
                }

                m_Offset = Vector3.up * -groundYPos;
            }
            else
            {
                Debug.LogError("Ground doesn't found.");
            }
        }

        public void AddTarget(Transform target)
        {
            m_Targets.Add(target);
        }

        public void RemoveTarget(Transform target)
        {
            m_Targets.Remove(target);
        }

        void UpdateBounds()
        {
            if (m_Targets != null && m_Targets.Count > 0)
            {
                m_TargetsBounds = new Bounds(m_Targets[0] ? m_Targets[0].position : Vector3.zero, Vector3.zero);
                foreach (Transform target in m_Targets)
                {
                    if (target != null)
                    {
                        m_TargetsBounds.Encapsulate(target.position);
                    }
                }
            }
        }

        Vector3 GetFinalPosition()
        {
            if (m_Targets != null && m_Targets.Count > 0)
            {
                Vector3 centerPosition = m_TargetsBounds.center + m_Offset;
                Vector3 finalPosition = new Vector3(centerPosition.x, centerPosition.y, centerPosition.z);
                return finalPosition.Round(2);
            }

            return Vector3.zero;
        }

        float GetFinalOrthographicSize()
        {
            float wantedZoom = Mathf.Lerp(m_MinOrthographicSize, m_MaxOrthographicSize, GetHorizontalDistanceRatio());
            float finalZoom = Mathf.Lerp(m_PreviousOrthographicSize, wantedZoom, Time.deltaTime * m_OrthographicDamping);
            m_PreviousOrthographicSize = finalZoom;

            return finalZoom;
        }

        float GetHorizontalDistanceRatio()
        {
            float distance = GetGreatestHorizontalDistance();
            return (distance - m_MinDistanceBetweenTargets) / (m_MaxDistanceBetweenTargets - m_MinDistanceBetweenTargets);
        }

        float GetGreatestHorizontalDistance()
        {
            if (m_Targets != null && m_Targets.Count > 0)
            {
                return m_TargetsBounds.size.x;
            }

            return 0f;
        }

        /// <summary>Callback to to the camera confining</summary>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            // Move the body before the Aim is calculated
            if (stage == CinemachineCore.Stage.Body)
            {
                UpdateBounds();

                if (vcam.Follow != null)
                {
                    vcam.Follow.position = GetFinalPosition();
                }

                LensSettings newLensSettings = state.Lens;
                newLensSettings.OrthographicSize = GetFinalOrthographicSize();
                state.Lens = newLensSettings;
            }
        }
    }
}