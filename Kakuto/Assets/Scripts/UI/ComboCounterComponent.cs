using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboCounterComponent : MonoBehaviour
{
    public EPlayer m_Target;
    public Text m_ComboText;

    private EPlayer m_DamageTakenTarget;
    private uint m_HitCounter = 0;

    private bool m_FreezeHitCounterDisplay = false;
    private float m_FreezeHitCounterDisplayCooldown = 0f;

    private void Awake()
    {
        m_DamageTakenTarget = m_Target == EPlayer.Player1 ? EPlayer.Player2 : EPlayer.Player1;
        Utils.GetPlayerEventManager<DamageTakenInfo>(m_DamageTakenTarget).StartListening(EPlayerEvent.DamageTaken, OnDamageTaken);
        Utils.GetPlayerEventManager<float>(m_DamageTakenTarget).StartListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    private void OnDestroy()
    {
        Utils.GetPlayerEventManager<DamageTakenInfo>(m_DamageTakenTarget).StopListening(EPlayerEvent.DamageTaken, OnDamageTaken);
        Utils.GetPlayerEventManager<float>(m_DamageTakenTarget).StartListening(EPlayerEvent.StunEnd, OnStunEnd);
    }

    private void Update()
    {
        if(!m_FreezeHitCounterDisplay)
        {
            m_ComboText.enabled = m_HitCounter >= 2;
            m_ComboText.text = m_HitCounter + " HIT";
        }
        else
        {
            m_FreezeHitCounterDisplayCooldown -= Time.deltaTime;
            if(m_FreezeHitCounterDisplayCooldown <= 0f)
            {
                m_FreezeHitCounterDisplay = false;
                m_FreezeHitCounterDisplayCooldown = 0f;
            }
        }
    }

    private void OnDamageTaken(DamageTakenInfo damageTakenInfo)
    {
        if(damageTakenInfo.m_IsAlreadyHitStunned || m_HitCounter == 0)
        {
            m_HitCounter++;
        }
    }

    private void OnStunEnd(float stunTimer)
    {
        if(m_ComboText.enabled)
        {
            m_FreezeHitCounterDisplay = true;
            m_FreezeHitCounterDisplayCooldown = ComboCounterConfig.Instance.m_TimeToDisappearAfterComboBreak;
        }
        m_HitCounter = 0;
    }
}
