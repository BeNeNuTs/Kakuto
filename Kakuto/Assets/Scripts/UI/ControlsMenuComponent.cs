using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuComponent : MenuComponent
{
    [SerializeField] private Button[] m_OptionButtons;
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_ControlsHighlightInfo;

    public void OnEnable()
    {
        for (int i = 0; i < m_OptionButtons.Length; i++)
        {
            Navigation buttonNavigation = m_OptionButtons[i].navigation;
            buttonNavigation.selectOnDown = m_DefaultSelectable;
            m_OptionButtons[i].navigation = buttonNavigation;
        }
        UpdateHighlightedGameObject(m_ControlsHighlightInfo);
    }

    protected override void OnUpdate_Internal()
    {
        base.OnUpdate_Internal();
        UpdateHighlightedGameObject(m_ControlsHighlightInfo);
    }
}
