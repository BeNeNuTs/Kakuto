using UnityEngine;

public class PlayerInfoComponent : MonoBehaviour
{
    private int m_PlayerIndex = -1;
    private PlayerSettings m_PlayerSettings;

    void Awake()
    {
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

    void InitPlayerIndex()
    {
        if(m_PlayerIndex == -1)
        {
            m_PlayerIndex = gameObject.CompareTag(Player.Player1) ? 0 : 1;
        }
    }

    void InitPlayerSettings()
    {
        if(m_PlayerSettings == null)
        {
            m_PlayerSettings = ScenesConfig.GetPlayerSettings(gameObject);
        }
    }

    public int GetPlayerIndex()
    {
        InitPlayerIndex();
        return m_PlayerIndex;
    }

    public PlayerSettings GetPlayerSettings()
    {
        InitPlayerSettings();
        return m_PlayerSettings;
    }
}