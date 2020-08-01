using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    private static readonly string K_FX_HOOK = "FXHook";

    private Rigidbody2D m_Rigidbody;
    private Transform m_FXHook;

    private SpriteRenderer m_UIBackground;
    private SpriteRenderer m_UIMaskedBackground;
    private SpriteMask m_UIBackgroundMask;

    private void Start()
    {
        m_Rigidbody = GetComponentInParent<Rigidbody2D>();
        m_FXHook = gameObject.transform.Find(K_FX_HOOK);
#if UNITY_EDITOR
        if (m_FXHook == null)
        {
            Debug.LogError(K_FX_HOOK + " can't be found on " + gameObject);
        }
#endif
        m_UIBackground = GameObject.FindGameObjectWithTag("UIBackground")?.GetComponent<SpriteRenderer>();
        m_UIMaskedBackground = GameObject.FindGameObjectWithTag("UIMaskedBackground")?.GetComponent<SpriteRenderer>();
        m_UIBackgroundMask = GameObject.FindGameObjectWithTag("UIBackgroundMask")?.GetComponent<SpriteMask>();
#if UNITY_EDITOR
        if (m_UIBackground == null || m_UIMaskedBackground == null || m_UIBackgroundMask == null)
        {
            Debug.LogError("UIBackground elements can't be found");
        }
#endif
    }

    public void BlockAttack()
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if (behaviour)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block attack");
            Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.BlockAttack, behaviour.m_AnimationAttackName);
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block attack : Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
    }

    public void UnblockAttack(UnblockAttackAnimEventConfig param)
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if(behaviour)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock attack");
            UnblockAttackAnimEvent unblockAnimEvent = new UnblockAttackAnimEvent(behaviour.m_AnimationAttackName, param);
            Utils.GetPlayerEventManager<UnblockAttackAnimEvent>(gameObject).TriggerEvent(EPlayerEvent.UnblockAttack, unblockAnimEvent);
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock attack : Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
    }

    public void BlockMovement()
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if (behaviour)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block movement");
            Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.BlockMovement, behaviour.m_AnimationAttackName);
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Block movement : Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
    }

    public void UnblockMovement()
    {
        PlayerAttackAnimationStateMachineBehavior behaviour = Utils.GetCurrentBehaviour<PlayerAttackAnimationStateMachineBehavior>(gameObject);
        if (behaviour)
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock movement");
            Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.UnblockMovement, behaviour.m_AnimationAttackName);
        }
        else
        {
            ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unblock movement : Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
            Debug.LogError("Unable to find PlayerAttackAnimationStateMachineBehavior from current animation state info.");
        }
    }

    public void EndOfParry()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "End of parry");
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.EndOfParry, EAnimationAttackName.Parry);
    }

    public void EndOfGrab()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "End of grab");
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.EndOfGrab, EAnimationAttackName.Grab);
    }

    public void ApplyGrabDamages()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Apply grab damages");
        Utils.GetPlayerEventManager<EAnimationAttackName>(gameObject).TriggerEvent(EPlayerEvent.ApplyGrabDamages, EAnimationAttackName.Grab);
    }

    public void TriggerProjectile()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger projectile");
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.TriggerProjectile, true);
    }

    public void StopMovement()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Stop movement");
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.StopMovement, true);
    }

    public void TriggerJumpImpulse()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger jump impulse");
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.TriggerJumpImpulse, true);
    }

    public void ApplyDashImpulse()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Apply dash impulse");
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.ApplyDashImpulse, true);
    }

    public void FreezeTime()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Freeze time");
        TimeManager.FreezeTime();
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        else
        {
            Debug.LogError("Animator component not found");
        }
    }

    public void UnfreezeTime()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Unfreeze time");
        TimeManager.UnfreezeTime();
        Animator animator = GetComponent<Animator>();
        if(animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.Normal;
        }
        else
        {
            Debug.LogError("Animator component not found");
        }
    }

    public void TriggerBackgroundEffect(BackgroundEffectAnimEventConfig backgroundEffectConfig)
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger background effect");

        m_UIBackground.enabled = true;
        m_UIBackground.color = backgroundEffectConfig.m_BackgroundColor;

        if(backgroundEffectConfig.m_UseMask)
        {
            m_UIMaskedBackground.enabled = true;
            m_UIMaskedBackground.color = backgroundEffectConfig.m_MaskedBackgroundColor;

            m_UIBackgroundMask.enabled = true;
            m_UIBackgroundMask.sprite = backgroundEffectConfig.m_Mask;
            m_UIBackgroundMask.transform.position = m_FXHook.position;
        }
    }

    public void RestoreBackground()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Restore background");

        m_UIBackground.enabled = false;
        m_UIMaskedBackground.enabled = false;
        m_UIBackgroundMask.enabled = false;
    }

    public void SyncGrabPosition()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Sync grab position");
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.SyncGrabPosition, true);
    }

    public void TriggerTeleport()
    {
        ChronicleManager.AddChronicle(gameObject, EChronicleCategory.Animation, "Trigger teleport");
        Utils.GetPlayerEventManager<bool>(gameObject).TriggerEvent(EPlayerEvent.TriggerTeleport, true);
    }

    public void SetXVelocity(float xVelocity)
    {
        m_Rigidbody.velocity = new Vector2(xVelocity, m_Rigidbody.velocity.y);
    }

    public void SetYVelocity(float yVelocity)
    {
        m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, yVelocity);
    }

    public void SpawnFX(GameObject fx)
    {
        GameObject fxInstance = Instantiate(fx, m_FXHook.transform.position, m_FXHook.transform.localRotation);
        fxInstance.transform.localScale = m_FXHook.lossyScale;
    }
}
