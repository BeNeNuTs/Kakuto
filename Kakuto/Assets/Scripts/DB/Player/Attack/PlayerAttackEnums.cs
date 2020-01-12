public enum EAnimationAttackName
{
    StandLP,
    StandHP,
    StandLK,
    StandHK,

    CrouchLP,
    CrouchHP,
    CrouchLK,
    CrouchHK,

    JumpLP,
    JumpHP,
    JumpLK,
    JumpHK,

    Grab,
    Parry,
    Dash,

    Special01,
    Special02,
    Special03,
    Special04,
    Special05,
    Special06,
    Special07,
    Special08,
    Special09,
    Special10,

    // Add enum at the end to avoid shifting all enum data
}

public enum EAttackType
{
    Low,
    Mid,
    Overhead
}

public enum EPlayerStance
{
    Stand,
    Crouch,
    Jump
}

public enum EHitHeight
{
    Low,
    High
}

public enum EHitStrength
{
    Weak,
    Strong
}

public enum ETimeScaleBackToNormal
{
    Smooth,
    Instant
}

public enum EAttackerPushBackCondition
{
    Always,
    OnlyIfEnemyIsInACorner
}