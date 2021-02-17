using UnityEngine;
using TMPro;
using System.Collections;

public class HitNotificationDisplayer : MonoBehaviour
{
    private static readonly string K_ANIM_ISACTIVE_BOOL = "IsActive";

    public EPlayer m_Target;
    public Animator m_Animator;
    public GameObject m_NotificationBackground;
    public TextMeshProUGUI m_NotificationText;

    private IEnumerator m_CurrentDisplayNotifCoroutine = null;

    private void Awake()
    {
        m_Animator.enabled = false;
        m_NotificationBackground.SetActive(false);
        m_NotificationText.gameObject.SetActive(false);

        Utils.GetPlayerEventManager(m_Target).StartListening(EPlayerEvent.DamageTaken, OnDamageTaken);
        Utils.GetPlayerEventManager(m_Target).StartListening(EPlayerEvent.HitNotification, OnHitNotification);
    }

    private void OnDestroy()
    {
        Utils.GetPlayerEventManager(m_Target).StopListening(EPlayerEvent.DamageTaken, OnDamageTaken);
        Utils.GetPlayerEventManager(m_Target).StopListening(EPlayerEvent.HitNotification, OnHitNotification);
    }

    private void OnDamageTaken(BaseEventParameters baseParams)
    {
        DamageTakenEventParameters damageTakenInfo = (DamageTakenEventParameters)baseParams;
        if (damageTakenInfo.m_HitNotificationType != EHitNotificationType.None)
        {
            TriggerNotification_Internal(damageTakenInfo.m_HitNotificationType);
        }
    }

    private void OnHitNotification(BaseEventParameters baseParams)
    {
        HitNotificationEventParameters hitNotifParams = (HitNotificationEventParameters)baseParams;
        if (hitNotifParams.m_HitNotificationType != EHitNotificationType.None)
        {
            TriggerNotification_Internal(hitNotifParams.m_HitNotificationType);
        }
    }

    private void SetNotificationActive(bool active)
    {
        m_Animator.enabled = true;
        m_Animator.SetBool(K_ANIM_ISACTIVE_BOOL, active);
    }

    private void TriggerNotification_Internal(EHitNotificationType hitNotifType)
    {
        if (m_CurrentDisplayNotifCoroutine != null)
        {
            StopCoroutine(m_CurrentDisplayNotifCoroutine);
        }
        m_CurrentDisplayNotifCoroutine = DisplayNotification(hitNotifType);
        StartCoroutine(m_CurrentDisplayNotifCoroutine);
    }

    private IEnumerator DisplayNotification(EHitNotificationType hitNotifType)
    {
        m_NotificationText.text = hitNotifType.ToString();
        SetNotificationActive(true);

        yield return new WaitForSeconds(UIConfig.Instance.m_MaxHitNotificationDisplayTime);

        SetNotificationActive(false);
    }
}
