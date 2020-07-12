using System;
using UnityEngine;
using System.Collections.Generic;

public enum EPalette
{
    Default,
    EX,
    Palette1,
    Palette2,
    Palette3

    // Please update K_MAX_PALETTE if new palettes need to be added
}

[Serializable]
public class Palette
{
    [SerializeField, ReadOnly]
    private string m_PaletteName;
    [ReadOnly]
    public EPalette m_PaletteType;
    public Sprite m_PaletteSprite;

    public void OnValidate()
    {
        m_PaletteName = m_PaletteType.ToString();
    }
}

[CreateAssetMenu(fileName = "PlayerInfoConfig", menuName = "Data/Player/PlayerInfoConfig", order = 0)]
public class PlayerInfoConfig : ScriptableObject
{
    private static readonly int K_MAX_PALETTE = 5;

    public string m_PlayerName = "Player";
    public Sprite m_PlayerIcon;

    public List<Palette> m_Palettes;

    void OnValidate()
    {
        if(m_Palettes == null)
        {
            m_Palettes = new List<Palette>();
        }

        if(m_Palettes.Count > K_MAX_PALETTE)
        {
            m_Palettes.RemoveRange(K_MAX_PALETTE, m_Palettes.Count - K_MAX_PALETTE);
        }

        while(m_Palettes.Count < K_MAX_PALETTE)
        {
            m_Palettes.Add(new Palette());
        }

        for(int i = 0; i < K_MAX_PALETTE; i++)
        {
            m_Palettes[i].m_PaletteType = (EPalette)i;
            m_Palettes[i].OnValidate();
        }
    }
}