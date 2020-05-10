﻿using UnityEngine;

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

#if DEVELOPMENT_BUILD || UNITY_EDITOR
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
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if(spriteRenderer != null)
        {
            InitWithCurrentPalette(spriteRenderer.material);
        }
    }

    public void InitWithCurrentPalette(Material material)
    {
        if (material != null && material.HasProperty("_PaletteTex"))
        {
            material.SetTexture("_PaletteTex", GetCurrentPalette());
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