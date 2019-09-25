using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutOfBoundsSubGameManager : SubGameManagerBase
{
    private Camera m_MainCamera;

    public override void Init()
    {
        base.Init();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_MainCamera = Camera.main;
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        float boundsOffset = GameConfig.Instance.m_BoundsOffset;
        foreach (GameObject player in GameManager.Instance.GetPlayers())
        {
            Vector3 playerScreenPos = m_MainCamera.WorldToScreenPoint(player.transform.root.position);
            playerScreenPos.x = Mathf.Clamp(playerScreenPos.x, boundsOffset, m_MainCamera.pixelWidth - boundsOffset);
            player.transform.root.position = m_MainCamera.ScreenToWorldPoint(playerScreenPos);
        }
    }

    public bool IsVisibleFromMainCamera(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_MainCamera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}
