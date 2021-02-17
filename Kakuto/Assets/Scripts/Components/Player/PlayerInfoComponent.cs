using UnityEngine;

public class PlayerInfoComponent : MonoBehaviour
{
    private static readonly string K_PALETTE_TEX_PROPERTY = "_PaletteTex";

    public PlayerInfoConfig m_InfoConfig;
    public EPalette m_InitialPalette = EPalette.Default;

    private SpriteRenderer m_SpriteRenderer;
    private EPalette m_DefaultPalette = EPalette.Default;
    private EPalette m_CurrentPalette = EPalette.Default;
    private int m_PlayerIndex = -1;
    private PlayerSettings m_Settings = null;

    void Awake()
    {
        InitPlayerTags();
        InitPlayerIndex();
        InitPlayerSettings();

        m_DefaultPalette = m_InitialPalette;
        m_CurrentPalette = m_DefaultPalette;
        InitPalette();

        GameManager.Instance.RegisterPlayer(gameObject);

#if DEBUG_DISPLAY || UNITY_EDITOR
        PlayerAnimationHelper.CheckAnimationsNaming(gameObject);
#endif
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
        if(m_SpriteRenderer == null)
        {
            m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        if(m_SpriteRenderer != null)
        {
            InitWithCurrentPalette(m_SpriteRenderer.material);
        }
    }

    public void InitWithCurrentPalette(Material material)
    {
        if (material != null && material.HasProperty(K_PALETTE_TEX_PROPERTY))
        {
            material.SetTexture(K_PALETTE_TEX_PROPERTY, GetCurrentPalette());
        }
    }

    public int GetPlayerIndex()
    {
        return m_PlayerIndex;
    }

    public EPlayer GetPlayerEnum()
    {
        return (EPlayer)m_PlayerIndex;
    }

    public PlayerSettings GetPlayerSettings()
    {
        return m_Settings;
    }

    public void SetDefaultAndCurrentPalette(EPalette palette)
    {
        m_DefaultPalette = palette;
        SetCurrentPalette(palette);
    }

    public void ResetDefaultAndCurrentPalette()
    {
        m_DefaultPalette = m_InitialPalette;
        ResetCurrentPalette();
    }

    public Texture GetCurrentPalette()
    {
        return m_InfoConfig.m_Palettes[(int)m_CurrentPalette].m_PaletteSprite.texture;
    }

    public void SetCurrentPalette(EPalette palette)
    {
        m_CurrentPalette = palette;
        InitPalette();
    }

    public void ResetCurrentPalette()
    {
        m_CurrentPalette = m_DefaultPalette;
        InitPalette();
    }
}