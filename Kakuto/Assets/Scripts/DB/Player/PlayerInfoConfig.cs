using UnityEngine;
using System.Collections.Generic;

public enum EPalette
{
    Default,
    Palette1,
    Palette2,
    Palette3,

    Count
}

[CreateAssetMenu(fileName = "PlayerInfoConfig", menuName = "Data/Player/PlayerInfoConfig", order = 0)]
public class PlayerInfoConfig : ScriptableObject
{
    public string m_PlayerName = "Player";

    public List<Sprite> m_Palettes;

    void OnValidate()
    {
        if(m_Palettes.Count > (int)EPalette.Count)
        {
            Debug.LogError("Max palettes allowed is " + (int)EPalette.Count);
            m_Palettes.RemoveRange((int)EPalette.Count, m_Palettes.Count - (int)EPalette.Count);
        }
    }
}