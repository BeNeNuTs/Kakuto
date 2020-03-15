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

        foreach (GameObject player in GameManager.Instance.GetPlayers())
        {
            Vector3 playerScreenPos = m_MainCamera.WorldToScreenPoint(player.transform.root.position);
            playerScreenPos.x = Mathf.Clamp(playerScreenPos.x, GetLeftBorderOffset(), GetRightBorderOffset());
            player.transform.root.position = m_MainCamera.ScreenToWorldPoint(playerScreenPos);
        }
    }

    public bool IsVisibleFromMainCamera(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_MainCamera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    public bool IsInACorner(GameObject gameObject)
    {
        Vector3 gameObjectScreenPos = m_MainCamera.WorldToScreenPoint(gameObject.transform.root.position);

        float distanceToLeftCorner = Mathf.Abs(gameObjectScreenPos.x - GetLeftBorderOffset());
        float distanceToRightCorner = Mathf.Abs(gameObjectScreenPos.x - GetRightBorderOffset());

        float maxDistanceToCorner = GameConfig.Instance.m_MaxDistanceToBeConsideredInACorner;
        return (distanceToLeftCorner < maxDistanceToCorner || distanceToRightCorner < maxDistanceToCorner);
    }

    private float GetLeftBorderOffset()
    {
        return GameConfig.Instance.m_BoundsOffset;
    }

    private float GetRightBorderOffset()
    {
        return m_MainCamera.pixelWidth - GameConfig.Instance.m_BoundsOffset;
    }
}
