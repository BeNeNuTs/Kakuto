using UnityEngine;

public class PlayerInfoComponent : MonoBehaviour
{
    public PlayerInfoConfig m_InfoConfig;
    public EPalette m_CurrentPalette = EPalette.Default;

    private int m_PlayerIndex = -1;
    private PlayerSettings m_Settings = null;

    void Awake()
    {
        InitPlayerTags();
        InitPlayerIndex();
        InitPlayerSettings();
        InitPalette();

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
        m_Settings = ScenesConfig.GetPlayerSettings(gameObject);
    }

    void InitPalette()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        InitWithCurrentPalette(spriteRenderer);
    }

    public void InitWithCurrentPalette(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer != null)
        {
            Material mat = spriteRenderer.material;
            if (mat != null && mat.HasProperty("_PaletteTex"))
            {
                mat.SetTexture("_PaletteTex", GetCurrentPalette());
            }
        }
    }

    public int GetPlayerIndex()
    {
        return m_PlayerIndex;
    }

    public PlayerSettings GetPlayerSettings()
    {
        return m_Settings;
    }

    public Texture GetCurrentPalette()
    {
        return m_InfoConfig.m_Palettes[(int)m_CurrentPalette].texture;
    }
}