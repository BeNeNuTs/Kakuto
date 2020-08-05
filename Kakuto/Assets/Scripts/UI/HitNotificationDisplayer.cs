using UnityEngine;
using TMPro;
using System.Collections;

public class HitNotificationDisplayer : MonoBehaviour
{
    public EPlayer m_Target;
    public GameObject m_NotificationBackground;
    public TextMeshProUGUI m_NotificationText;

    private IEnumerator m_CurrentDisplayNotifCoroutine = null;

    private void Awake()
    {
        SetNotificationActive(false);
        Utils.GetPlayerEventManager<DamageTakenInfo>(m_Target).StartListening(EPlayerEvent.DamageTaken, OnDamageTaken);
    }

    private void OnDestroy()
    {
        Utils.GetPlayerEventManager<DamageTakenInfo>(m_Target).StopListening(EPlayerEvent.DamageTaken, OnDamageTaken);
    }

    private void OnDamageTaken(DamageTakenInfo damageTakenInfo)
    {
        if(damageTakenInfo.m_HitNotificationType != EHitNotificationType.None)
        {
            if(m_CurrentDisplayNotifCoroutine != null)
            {
                StopCoroutine(m_CurrentDisplayNotifCoroutine);
            }
            m_CurrentDisplayNotifCoroutine = DisplayNotification(damageTakenInfo.m_HitNotificationType);
            StartCoroutine(m_CurrentDisplayNotifCoroutine);
        }
    }

    private void SetNotificationActive(bool active)
    {
        m_NotificationBackground.SetActive(active);
        m_NotificationText.enabled = active;
    }

    private IEnumerator DisplayNotification(EHitNotificationType hitNotifType)
    {
        m_NotificationText.text = hitNotifType.ToString();
        SetNotificationActive(true);

        yield return new WaitForSeconds(UIConfig.Instance.m_MaxHitNotificationDisplayTime);

        SetNotificationActive(false);
    }
}
