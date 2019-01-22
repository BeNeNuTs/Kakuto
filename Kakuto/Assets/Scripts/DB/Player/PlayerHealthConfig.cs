using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "PlayerHealthConfig", menuName = "Data/Player/PlayerHealthConfig", order = 0)]
public class PlayerHealthConfig : ScriptableObject
{
    [Range(0,100)]
    public uint m_MaxHP = 100;
}