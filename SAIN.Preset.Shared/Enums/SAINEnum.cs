namespace SAIN.Preset.Shared.Enums;

public enum ECombatDecision
{
    None,
    Retreat,
    Search,
    RunAway,
    DogFight,
    SeekCover,
    StandAndShoot,
    ThrowGrenade,
    ShiftCover,
    RushEnemy,
    MoveToEngage,
    ShootDistantEnemy,
    AvoidGrenade,
    Freeze,
    CreepOnEnemy,
    MeleeAttack,
    FightZombies,
    DebugNoDecision,
}

public enum ESelfActionType
{
    None = 0,
    Reload = 1,
    FirstAid = 2,
    Stims = 3,
    Surgery = 4,
}

public enum FriendlyFireStatus
{
    None,
    FriendlyBlock,
    Clear,
}

public enum EHeardFromPeaceBehavior
{
    None,
    Freeze,
    SearchNow,
    Charge,
}

public enum EWeaponClass
{
    Default,
    assaultRifle,
    assaultCarbine,
    machinegun,
    smg,
    pistol,
    marksmanRifle,
    sniperRifle,
    shotgun,
    grenadeLauncher,
    specialWeapon,
}

public enum CoverStatus
{
    None = 0,
    FarFromCover = 1,
    MidRangeToCover = 2,
    CloseToCover = 3,
    InCover = 4,
}

public enum LeanSetting
{
    None = 0,
    Left = 1,
    Right = 2,
}

public enum ESquadDecision
{
    None,
    Surround,
    Retreat,
    Suppress,
    PushSuppressedEnemy,
    BoundingRetreat,
    Regroup,
    SpreadOut,
    HoldPositions,
    Help,
    Search,
    GroupSearch,
}

public enum SAINSoundType
{
    None,
    Generic,
    FootStep,
    Sprint,
    Prone,
    Looting,
    Reload,
    GearSound,
    GrenadePin,
    GrenadeExplosion,
    GrenadeDraw,
    Jump,
    Door,
    DoorBreach,
    Shot,
    SuppressedShot,
    Heal,
    Food,
    Conversation,
    Surgery,
    DryFire,
    TurnSound,
    Breathing,
    Pain,
    Bush,
    BulletImpact,
    Land,
}

public enum ELocation
{
    None = 0,
    Factory = 1,
    FactoryNight = 2,
    Customs = 3,
    GroundZero = 4,
    Reserve = 5,
    Streets = 6,
    Lighthouse = 7,
    Shoreline = 8,
    Labs = 9,
    Woods = 10,
    Interchange = 11,
    Terminal = 12,
    Town = 13,
    Labyrinth = 14,
}

public enum EPathDistance
{
    NoEnemy,
    VeryClose,
    Close,
    Mid,
    Far,
    VeryFar,
}

public enum StyleState
{
    normal,
    onNormal,
    active,
    onActive,
    hover,
    onHover,
    focused,
    onFocused,
}

public enum AILimitSetting
{
    None = 0,
    Far = 1,
    VeryFar = 2,
    Narnia = 3,
}
