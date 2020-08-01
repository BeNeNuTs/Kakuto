using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundEffectAnimEventConfig", menuName = "Data/Player/Attacks/AnimEventParameters/BackgroundEffectAnimEventConfig", order = 0)]
public class BackgroundEffectAnimEventConfig : ScriptableObject
{
    public Color m_BackgroundColor;

    public bool m_UseMask = false;
    [ConditionalField(true, "m_UseMask")]
    public Color m_MaskedBackgroundColor;
    [ConditionalField(true, "m_UseMask")]
    public Sprite m_Mask;
}