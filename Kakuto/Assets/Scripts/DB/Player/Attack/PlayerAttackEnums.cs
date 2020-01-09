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

    Special1,
    Special2,
    Special3,
    Special4,
    Special5,
    Special6,
    Special7,
    Special8,
    Special9,
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