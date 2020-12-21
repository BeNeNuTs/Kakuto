using UnityEngine;
using UnityEngine.SceneManagement;

public class OutOfBoundsSubGameManager : SubGameManagerBase
{
    public Camera MainCamera { get; private set; }

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
        MainCamera = Camera.main;
    }

    public override void LateUpdate()
    {
        foreach (GameObject player in GameManager.Instance.GetPlayers())
        {
            Vector3 playerPos = player.transform.root.position;
            playerPos.x = Mathf.Clamp(playerPos.x, GetLeftBorderOffset(), GetRightBorderOffset());
            player.transform.root.position = playerPos;
        }
    }

    public float GetDistanceOutOfBorder(Vector3 position)
    {
        float distanceOutOfLeftCorner = position.x - GetLeftBorderOffset();
        if(distanceOutOfLeftCorner < 0f)
        {
            return distanceOutOfLeftCorner;
        }

        float distanceToRightCorner = position.x - GetRightBorderOffset();
        if (distanceToRightCorner > 0f)
        {
            return distanceToRightCorner;
        }

        return 0f;
    }

    public bool IsInACorner(GameObject gameObject)
    {
        return IsInACorner(gameObject.transform.root.position, out float leftOffset, out float rightOffset, out bool leftBorder);
    }

    public bool IsInACorner(Vector3 position, out float leftOffset, out float rightOffset, out bool leftBorder)
    {
        float distanceToClosestBorder = GetDistanceToClosestBorder(position, out leftOffset, out rightOffset, out leftBorder);

        float maxDistanceToCorner = GameConfig.Instance.m_MaxDistanceToBeConsideredInACorner;
        return distanceToClosestBorder < maxDistanceToCorner;
    }

    private float GetDistanceToClosestBorder(Vector3 position, out float leftOffset, out float rightOffset, out bool leftBorder)
    {
        leftOffset = GetLeftBorderOffset();
        rightOffset = GetRightBorderOffset();

        float distanceToLeftCorner = position.x - leftOffset;
        float distanceToRightCorner = rightOffset - position.x;

        leftBorder = distanceToLeftCorner < distanceToRightCorner;
        return Mathf.Min(distanceToLeftCorner, distanceToRightCorner);
    }

    private float GetLeftBorderOffset()
    {
        return MainCamera.ScreenToWorldPoint(Vector3.zero).x + GameConfig.Instance.m_BoundsOffset;
    }

    private float GetRightBorderOffset()
    {
        return MainCamera.ScreenToWorldPoint(new Vector3(MainCamera.pixelWidth, 0f)).x - GameConfig.Instance.m_BoundsOffset;
    }
}
