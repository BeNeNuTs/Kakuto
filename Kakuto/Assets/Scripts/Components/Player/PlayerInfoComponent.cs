using UnityEngine;

public class PlayerInfoComponent : MonoBehaviour
{
    private int m_PlayerIndex = -1;
    private PlayerSettings m_PlayerSettings = null;

    void Awake()
    {
        InitPlayerTags();
        InitPlayerIndex();
        InitPlayerSettings();

        GameManager.Instance.RegisterPlayer(gameObject);
    }

    void OnDestroy()
    {
        GameManager gameMgr = GameManager.Instance;
        if (gameMgr)
        {
            gameMgr.UnregisterPlayer(gameObject);
        }
    }

    void InitPlayerTags()
    {
        string playerTag = transform.root.tag;

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            child.tag = playerTag;
        }
    }

    void InitPlayerIndex()
    {
        m_PlayerIndex = gameObject.CompareTag(Player.Player1) ? 0 : 1;
    }

    void InitPlayerSettings()
    {
        m_PlayerSettings = ScenesConfig.GetPlayerSettings(gameObject);
    }

    public int GetPlayerIndex()
    {
        return m_PlayerIndex;
    }

    public PlayerSettings GetPlayerSettings()
    {
        return m_PlayerSettings;
    }
}