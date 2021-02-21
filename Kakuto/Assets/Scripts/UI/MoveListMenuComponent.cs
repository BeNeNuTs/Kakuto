using UnityEngine;
using UnityEngine.UI;

public class MoveListMenuComponent : MenuComponent
{
#pragma warning disable 0649
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private float m_ScrollSpeed;
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
            m_ScrollRect.verticalNormalizedPosition -= m_ScrollSpeed * Time.deltaTime;
        else
            m_ScrollRect.verticalNormalizedPosition += m_ScrollSpeed * Time.deltaTime;
    }
}
