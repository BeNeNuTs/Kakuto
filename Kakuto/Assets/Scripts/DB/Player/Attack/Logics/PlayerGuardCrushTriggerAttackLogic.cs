using System;

public class PlayerGuardCrushTriggerAttackLogic : PlayerBaseAttackLogic
{
    private static readonly bool[] m_TriggerPointActive = { false, false };
    public static Action<bool>[] OnTriggerPointStatusChanged = { null, null };

    private readonly PlayerGuardCrushTriggerAttackConfig m_Config;

    public PlayerGuardCrushTriggerAttackLogic(PlayerGuardCrushTriggerAttackConfig config)
    {
        m_Config = config;
    }

    public override bool EvaluateConditions(PlayerBaseAttackLogic currentAttackLogic)
    {
        bool conditionIsValid = base.EvaluateConditions(currentAttackLogic);

        if(conditionIsValid)
        {
            conditionIsValid &= IsTriggerPointActive(m_InfoComponent.GetPlayerIndex()) && CheckConditionsInternal();
        }

        return conditionIsValid;
    }

    private bool CheckConditionsInternal()
    {
        switch (m_Config.m_GuardCrushType)
        {
            case PlayerGuardCrushTriggerAttackConfig.EGuardCrushType.NextNonSuperProjectile:
                return !PlayerProjectileAttackLogic.IsNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex());
            default:
                return false;
        }
    }

    public override void OnAttackLaunched()
    {
        base.OnAttackLaunched();
        TriggerGuardCrushEffect();
    }

    private void TriggerGuardCrushEffect()
    {
        switch (m_Config.m_GuardCrushType)
        {
            case PlayerGuardCrushTriggerAttackConfig.EGuardCrushType.NextNonSuperProjectile:
                PlayerProjectileAttackLogic.SetNextNonSuperProjectileGuardCrush(m_InfoComponent.GetPlayerIndex(), true);
                break;
            default:
                break;
        }

        SetTriggerPointActive(m_InfoComponent.GetPlayerIndex(), false); // Reset trigger point as it has been used
    }

    public override void OnShutdown()
    {
        base.OnShutdown();

        SetTriggerPointActive(m_InfoComponent.GetPlayerIndex(), false); // Reset trigger point at the end of each round
    }

    public static void SetTriggerPointActive(int playerIndex, bool active)
    {
        m_TriggerPointActive[playerIndex] = active;
        OnTriggerPointStatusChanged[playerIndex]?.Invoke(active);
    }

    public static bool IsTriggerPointActive(int playerIndex)
    {
        return m_TriggerPointActive[playerIndex];
    }
}
