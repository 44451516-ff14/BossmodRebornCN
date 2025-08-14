namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

public enum OID : uint
{
    DemonTablet = 0x470A, // R11.0
    TetherHelper = 0x470D, // R1.2
    PortentousCometeor = 0x1EBD76, // R0.5
    AddPortal = 0x1EBD77, // R0.5
    SummonedDemon = 0x470B, // R1.5
    SummonedArchDemon = 0x470C, // R2.0
    GargoyleStatue = 0x1EBD75, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackVisual = 41737, // Boss->self, no cast, single-target
    AutoAttack = 41738, // Helper->player, no cast, single-target, hits 3 highest emnity players
    AutoAttackAdd = 41811, // SummonedArchDemon->player, no cast, single-target
    AbyssalTransfixion = 41743, // SummonedArchDemon/SummonedDemon->player, no cast, single-target

    DemonicDarkIIVisual = 41734, // Boss->self, 5.0s cast, single-target, raidwide
    DemonicDarkII = 41903, // Helper->self, no cast, ???

    RayOfDangersNear = 41715, // Boss->self, 10.0s cast, single-target
    RayOfExpulsionAfar = 41716, // Boss->self, 10.0s cast, single-target
    GravityOfDangersNear = 41706, // Boss->self, 10.0s cast, single-target
    GravityOfExpulsionAfar = 43521, // Boss->self, 10.0s cast, single-target
    DemonographOfDangersNear = 41718, // Boss->self, 10.0s cast, single-target
    DemonographOfExpulsionAfar = 41719, // Boss->self, 10.0s cast, single-target
    CometeorOfDangersNear = 41700, // Boss->self, 10.0s cast, single-target
    CometeorOfExpulsionAfar = 41701, // Boss->self, 10.0s cast, single-target

    Landing1 = 41812, // Helper->self, 10.5s cast, range 6 width 30 rect
    Landing2 = 43293, // Helper->self, 10.5s cast, range 15 width 30 rect
    Landing3 = 41709, // Helper->self, 14.0s cast, range 18 circle
    LandingKB1 = 43794, // Helper->self, 10.5s cast, range 30 width 30 rect, knockback 25, dir forward, 2 casters for both directions to avoid aoe target limit
    LandingKB2 = 43294, // Helper->self, 10.5s cast, range 30 width 30 rect, knockback 25, dir forward
    RayOfIgnorance = 41717, // Helper->self, 10.5s cast, range 30 width 30 rect

    OccultChiselVisual = 41735, // Boss->self, 8.5s cast, single-target, tankbuster
    OccultChisel = 41736, // Helper->player, 8.5s cast, range 5 circle

    RotateRight = 41729, // Boss->self, 8.0s cast, single-target
    RotateLeft = 41730, // Boss->self, 8.0s cast, single-target
    Rotation1 = 41731, // Helper->self, 8.8s cast, range 37 90-degree cone
    Rotation2 = 41732, // Helper->self, 8.8s cast, range 33 width 3 rect
    Rotation3 = 41733, // Helper->self, 8.8s cast, range 33 width 3 rect

    LacunateStreamVisual = 41728, // Boss->self, no cast, single-target
    LacunateStreamFirst = 41725, // Helper->self, 1.0s cast, range 31 width 30 rect
    LacunateStreamRepeat = 41726, // Helper->self, no cast, range 34 width 30 rect

    PortentousComet1 = 41703, // Helper->players, 17.0s cast, range 4 circle, knockback 13, dir north
    PortentousComet2 = 41704, // Helper->players, 17.0s cast, range 4 circle, knockback 13, dir south
    PortentousCometeor = 41702, // Helper->self, 12.0s cast, range 43 circle

    SummonVisual = 41705, // Boss->self, 4.0s cast, single-target
    Summon = 41741, // Boss->self, 10.0s cast, range 36 width 30 rect

    Demonography = 41710, // Boss->self, 4.0s cast, single-target
    EraseGravity = 41707, // Helper->self, 14.0s cast, range 4 circle, gives levitation buff
    RestoreGravity = 41708, // Boss->self, 12.0s cast, single-target
    Explosion1 = 41720, // Helper->self, 15.0s cast, range 4 circle, ground, tower
    Explosion2 = 41713, // Helper->self, 21.0s cast, range 4 circle, flying, tower
    Explosion3 = 41711, // Helper->self, 21.0s cast, range 4 circle, ground, tower
    UnmitigatedExplosionVisual1 = 41721, // Helper->self, no cast, single-target, tower fail
    UnmitigatedExplosion1 = 43035, // Helper->self, no cast, ???
    UnmitigatedExplosionVisual2 = 41714, // Helper->self, no cast, single-target, tower fail
    UnmitigatedExplosion2 = 43034, // Helper->self, no cast, ???
    UnmitigatedExplosionVisual3 = 41712, // Helper->self, no cast, single-target
    UnmitigatedExplosion3 = 43033, // Helper->self, no cast, ???

    EndOfHistoryVisual = 41739, // Boss->self, 10.0s cast, ???, enrage
    EndOfHistory1 = 41813, // Helper->self, no cast, ???
    EndOfHistory2 = 41740 // Boss->self, no cast, ???
}

public enum SID : uint
{
    CraterLater = 4354, // none->player, extra=0x0
    DarkDefenses = 4355 // none->SummonedArchDemon, extra=0x0
}
