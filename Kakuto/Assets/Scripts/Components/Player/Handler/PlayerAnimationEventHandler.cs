using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    public PlayerInfoComponent m_PlayerInfoComponent;
    public PlayerMovementComponent m_PlayerMovementComponent;
    public PlayerShadowComponent m_PlayerShadowComponent;
    public Rigidbody2D m_Rigidbody;
    public Transform m_FXHook;

    private Animator m_Animator;
    private SpriteRenderer m_UIBackground;
    private SpriteRenderer m_UIMaskedBackground;
    private SpriteMask m_UIBackgroundMask;
    private SpriteRenderer m_UIBackgroundMaskDetail;

    private BaseAttackStateMachineBehaviour m_CurrentAttack;
    private TimeScaleSubGameManager m_TimeScaleManager;
    private FXSubGameManager m_FXManager;
    private AudioSubGameManager m_AudioManager;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_TimeScaleManager = GameManager.Instance.GetSubManager<TimeScaleSubGameManager>(ESubManager.TimeScale);
        m_FXManager = GameManager.Instance.GetSubManager<FXSubGameManager>(ESubManager.FX);
        m_AudioManager = GameManager.Instance.GetSubManager<AudioSubGameManager>(ESubManager.Audio);

        m_UIBackground = GameObject.FindGameObjectWithTag("UIBackground")?.GetComponent<SpriteRenderer>();
        m_UIMaskedBackground = GameObject.FindGameObjectWithTag("UIMaskedBackground")?.GetComponent<SpriteRenderer>();
        m_UIBackgroundMask = GameObject.FindGameObjectWithTag("UIBackgroundMask")?.GetComponent<SpriteMask>();
        m_UIBackgroundMaskDetail = GameObject.FindGameObjectWithTag("UIBackgroundMaskDetail")?.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
        if (m_UIBackground == null || m_UIMaskedBackground == null || m_UIBackgroundMask == null)
        {
            KakutoDebug.LogError("UIBackground elements can't be found");
        }
#endif
    }

    public void UpdateCurrentAttack(BaseAttackStateMachineBehaviour currentAttack)
    {
        m_CurrentAttack = currentAttack;
    }

    public void BlockAttack()
    {
        if (m_CurrentAttack)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block attack");
            Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.BlockAttack, new BlockAttackEventParameters(m_CurrentAttack.GetAnimationAttackName()));
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block attack: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
            KakutoDebug.LogError("Block attack: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
        }
    }

    public void UnblockAttack(UnblockAttackAnimEventConfig param)
    {
        if (m_CurrentAttack)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock attack");
            Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.UnblockAttack, new UnblockAttackEventParameters(m_CurrentAttack.GetAnimationAttackName(), param));
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock attack: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
            KakutoDebug.LogError("Unblock attack: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
        }
    }

    public void BlockMovement()
    {
        if (m_CurrentAttack)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block movement");
            Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.BlockMovement, new BlockMovementEventParameters(m_CurrentAttack.GetAnimationAttackName()));
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block movement: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
            KakutoDebug.LogError("Block movement: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
        }
    }

    public void UnblockMovement()
    {
        if (m_CurrentAttack)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock movement");
            Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.UnblockMovement, new UnblockMovementEventParameters(m_CurrentAttack.GetAnimationAttackName()));
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock movement: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
            KakutoDebug.LogError("Unblock movement: Unable to find BaseAttackStateMachineBehaviour from current animation state info.");
        }
    }

    public void EndOfParry()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "End of parry");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.EndOfParry);
    }

    public void EndOfGrab()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "End of grab");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.EndOfGrab);
    }

    public void ApplyGrabDamages()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Apply grab damages");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.ApplyGrabDamages);
    }

    public void TriggerProjectile()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger projectile");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.TriggerProjectile);
    }

    public void StopMovement()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Stop movement");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.StopMovement);
    }

    public void TriggerJumpImpulse()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger jump impulse");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.TriggerJumpImpulse);
    }

    public void ApplyDashImpulse()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Apply dash impulse");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.ApplyDashImpulse);
    }

    public void FreezeTime()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Freeze time");
        m_TimeScaleManager.FreezeTime(m_Animator);
    }

    public void UnfreezeTime()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unfreeze time");
        m_TimeScaleManager.UnfreezeTime(m_Animator);
    }

    public void TriggerBackgroundEffect(BackgroundEffectAnimEventConfig backgroundEffectConfig)
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger background effect");

        m_UIBackground.enabled = true;
        m_UIBackground.color = backgroundEffectConfig.m_BackgroundColor;

        if(backgroundEffectConfig.m_UseMask)
        {
            m_UIBackgroundMask.enabled = true;
            m_UIBackgroundMask.sprite = backgroundEffectConfig.m_Mask;
            m_UIBackgroundMask.transform.position = m_FXHook.position;

            if(backgroundEffectConfig.m_MaskDetail != null)
            {
                m_UIBackgroundMaskDetail.enabled = true;
                m_UIBackgroundMaskDetail.sprite = backgroundEffectConfig.m_MaskDetail;
                m_UIBackgroundMaskDetail.color = backgroundEffectConfig.m_MaskedBackgroundColor;
                m_UIBackgroundMaskDetail.transform.position = m_UIBackgroundMask.transform.position;
            }
            else
            {
                m_UIMaskedBackground.enabled = true;
                m_UIMaskedBackground.color = backgroundEffectConfig.m_MaskedBackgroundColor;
            }
        }
    }

    public void RestoreBackground()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Restore background");

        m_UIBackground.enabled = false;
        m_UIMaskedBackground.enabled = false;
        m_UIBackgroundMask.enabled = false;
        m_UIBackgroundMaskDetail.enabled = false;
    }

    public void SyncGrabPosition()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Sync grab attacker position");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.SyncGrabAttackerPosition);
    }

    public void TriggerTeleport()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger teleport");
        Utils.GetPlayerEventManager(gameObject).TriggerEvent(EPlayerEvent.TriggerTeleport);
    }

    public void SetXVelocity(float xVelocity)
    {
        if (!m_PlayerMovementComponent.IsFacingRight())
        {
            xVelocity *= -1f;       
        }

        m_Rigidbody.velocity = new Vector2(xVelocity, m_Rigidbody.velocity.y);
    }

    public void SetYVelocity(float yVelocity)
    {
        m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, yVelocity);
    }

    public void SpawnHitFX(EHitFXType hitFXType)
    {
        m_FXManager.SpawnHitFX(m_PlayerInfoComponent.GetPlayerIndex(), hitFXType, m_FXHook.position, m_FXHook.localRotation, m_FXHook.lossyScale.x < 0);
    }

    public void SpawnOtherFX(EFXType fxType)
    {
        m_FXManager.SpawnOtherFX(m_PlayerInfoComponent.GetPlayerIndex(), fxType, m_FXHook.position, m_FXHook.localRotation, m_FXHook.lossyScale.x < 0);
    }

    public void PlaySFX(EAnimSFXType animSFXType)
    {
        m_AudioManager.PlayAnimSFX(m_PlayerInfoComponent.GetPlayerIndex(), animSFXType);
    }

    public void DisplayShadow()
    {
        m_PlayerShadowComponent.enabled = true;
    }

    public void HideShadow()
    {
        m_PlayerShadowComponent.enabled = false;
    }

    public void InstantFlip()
    {
        m_PlayerMovementComponent.OnInstantFlip();
    }
}
