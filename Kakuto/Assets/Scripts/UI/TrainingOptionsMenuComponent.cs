using UnityEngine;
using UnityEngine.UI;

public class TrainingOptionsMenuComponent : MenuComponent
{
#pragma warning disable 0649
    [SerializeField] private Selectable m_DefaultSelectable;
    [SerializeField] private HighlightInfo[] m_TrainingOptionsHighlightInfo;
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private float[] m_ScrollViewNormalizedPosition;
#pragma warning restore 0649

    public void OnEnable()
    {
        m_DefaultSelectable.Select();
        UpdateHighlightedGameObject(m_TrainingOptionsHighlightInfo);
    }

    protected override void OnUpdate_Internal()
    {
        base.OnUpdate_Internal();
        UpdateHighlightedGameObject(m_TrainingOptionsHighlightInfo);
        UpdateScrollView();
    }

    protected void UpdateScrollView()
    {
        bool highlightImageEnabledFound = false;
        for (int i = 0; i < m_TrainingOptionsHighlightInfo.Length; i++)
        {
            if (m_TrainingOptionsHighlightInfo[i].m_HighlightImage.enabled)
            {
                m_ScrollRect.verticalNormalizedPosition = m_ScrollViewNormalizedPosition[i];
                highlightImageEnabledFound = true;
                break;
            }
        }

        if(!highlightImageEnabledFound)
        {
            m_DefaultSelectable.Select();
        }
    }
}
