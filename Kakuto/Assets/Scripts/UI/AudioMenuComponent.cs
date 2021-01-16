using UnityEngine;
using UnityEngine.UI;

public class AudioMenuComponent : MenuComponent
{
#pragma warning disable 0649
    [SerializeField] private Button[] m_OptionButtons;
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_AudioHighlightInfo;
#pragma warning restore 0649

    public void OnEnable()
    {
        for (int i = 0; i < m_OptionButtons.Length; i++)
        {
            Navigation buttonNavigation = m_OptionButtons[i].navigation;
            buttonNavigation.selectOnDown = m_DefaultSelectable;
            m_OptionButtons[i].navigation = buttonNavigation;
        }
        UpdateHighlightedGameObject(m_AudioHighlightInfo);
    }

    protected override void OnUpdate_Internal()
    {
        base.OnUpdate_Internal();
        UpdateHighlightedGameObject(m_AudioHighlightInfo);
    }
}
