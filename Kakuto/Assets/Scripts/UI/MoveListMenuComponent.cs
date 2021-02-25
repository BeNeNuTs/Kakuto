using UnityEngine;
using UnityEngine.UI;

public class MoveListMenuComponent : MenuComponent
{
#pragma warning disable 0649
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private float m_ScrollSpeed;
    [SerializeField] private Image m_ScrollArrowUp;
    [SerializeField] private float m_ScrollArrowUpActivationTheshold = 1f;
    [SerializeField] private Image m_ScrollArrowDown;
    [SerializeField] private float m_ScrollArrowDownActivationTheshold = 0f;
#pragma warning restore 0649

    protected override void OnUpdate_Internal()
    {
        base.OnUpdate_Internal();
        if(GamePadManager.GetJumpInput((int)EPlayer.Player1) || GamePadManager.GetJumpInput((int)EPlayer.Player2))
        {
            UpdateScrollView(false);
        }
        else if (GamePadManager.GetCrouchInput((int)EPlayer.Player1) || GamePadManager.GetCrouchInput((int)EPlayer.Player2))
        {
            UpdateScrollView(true);
        }
    }

    private void UpdateScrollView(bool moveUp)
    {
        if(moveUp)
            m_ScrollRect.verticalNormalizedPosition -= m_ScrollSpeed * Time.unscaledDeltaTime;
        else
            m_ScrollRect.verticalNormalizedPosition += m_ScrollSpeed * Time.unscaledDeltaTime;

        m_ScrollArrowUp.enabled = m_ScrollRect.verticalNormalizedPosition < m_ScrollArrowUpActivationTheshold;
        m_ScrollArrowDown.enabled = m_ScrollRect.verticalNormalizedPosition > m_ScrollArrowDownActivationTheshold;
    }
}
