using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarComponent : MonoBehaviour
{
    public EPlayer m_Target;
    public Image m_HealthBarBackground;
    public Image m_HealthBar;

    public TextMeshProUGUI m_PlayerName;
    public Image m_PlayerIcon;

    private UIConfig m_UIConfig;

    private void Awake()
    {
        m_UIConfig = UIConfig.Instance;
        GameManager.Instance?.AddOnPlayerRegisteredCallback(OnPlayerRegistered, m_Target);
        Utils.GetPlayerEventManager(m_Target).StartListening(EPlayerEvent.DamageTaken, OnDamageTaken);
    }

    private void OnDestroy()
    {
        Utils.GetPlayerEventManager(m_Target).StopListening(EPlayerEvent.DamageTaken, OnDamageTaken);
        GameManager.Instance?.RemoveOnPlayerRegisteredCallback(OnPlayerRegistered, m_Target);
    }

    private void OnPlayerRegistered(GameObject player)
    {
        PlayerInfoComponent infoComponent = player.GetComponent<PlayerInfoComponent>();
        if(infoComponent != null)
        {
            m_PlayerName.SetText(infoComponent.m_InfoConfig.m_PlayerName);
            m_PlayerIcon.sprite = infoComponent.m_InfoConfig.m_PlayerIcon;

            Material instianciatedMaterial = Instantiate(m_PlayerIcon.material); // By default Image material is shared...
            infoComponent.InitWithCurrentPalette(instianciatedMaterial);
            m_PlayerIcon.material = instianciatedMaterial;
        }
    }

    private void OnDamageTaken(BaseEventParameters baseParams)
    {
        DamageTakenEventParameters damageTakenInfo = (DamageTakenEventParameters)baseParams;

        StopAllCoroutines();

        StartCoroutine(UpdateHealthFill(m_HealthBar, damageTakenInfo.m_HealthRatio, 0.0f));
        StartCoroutine(UpdateHealthFill(m_HealthBarBackground, damageTakenInfo.m_HealthRatio, m_UIConfig.m_TimeBetweenHealthBar));
    }

    IEnumerator UpdateHealthFill(Image imageToUpdate, float healthRatio, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        float currentTimestamp = Time.time;

        float initialFillAmount = imageToUpdate.fillAmount;
        float currentTime = 0.0f;
        float duration = UIConfig.Instance.m_TimeToFillHealthBar;
        while (imageToUpdate.fillAmount != healthRatio)
        {
            imageToUpdate.fillAmount = Mathf.Lerp(initialFillAmount, healthRatio, currentTime);
            currentTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}
