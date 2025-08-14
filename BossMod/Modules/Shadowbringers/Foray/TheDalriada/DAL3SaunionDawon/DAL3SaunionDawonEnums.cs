namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

public enum OID : uint
{
    Boss = 0x31DD, // R7.875
    Dawon = 0x31DE, // R6.9
    VerdantPlume = 0x31E1, // R0.5
    VermilionFlame = 0x31E0, // R0.75
    SpiralPattern2 = 0x1EB217, // R0.5
    SpiralPattern1 = 0x1EB216, // R0.5
    FrigidPulseJump = 0x1EB1E7, // R0.5
    FireBrandJump = 0x1EB1E8, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    AutoAttackDawon = 6497, // Dawon->player, no cast, single-target
    Teleport = 24023, // Boss->location, no cast, single-target

    AetherialDistribution1 = 25060, // Boss->Dawon, no cast, single-target
    AetherialDistribution2 = 25061, // Dawon->Boss, no cast, single-target

    AntiPersonnelMissile = 24002, // Helper->players, 5.0s cast, range 6 circle, spread
    Explosion = 24015, // VerdantPlume->self, 1.5s cast, range 3-12 donut
    RawHeat = 24014, // VermilionFlame->self, 1.5s cast, range 10 circle

    Obey = 24009, // Dawon->self, 11.0s cast, single-target
    FrigidPulseJump = 24011, // Dawon->self, no cast, range 12-60 donut
    FireBrand = 24012, // Dawon->self, no cast, range 50 width 14 cross
    SwoopingJump = 24010, // Dawon->location, no cast, range 12 circle, can be ignored, does miniscule damage

    HighPoweredMagitekRay = 24005, // Boss->player, 5.0s cast, range 5 circle, tankbuster
    MagitekHalo = 23989, // Boss->self, 5.0s cast, range 9-60 donut
    FrigidPulse = 24701, // Dawon->self, 5.0s cast, range 12-60 donut
    MagitekCrossray = 23991, // Boss->self, 5.0s cast, range 60 width 19 cross
    SwoopingFrenzy = 24016, // Dawon->location, 4.0s cast, range 12 circle

    MissileCommand = 24001, // Boss->self, 4.0s cast, single-target
    MissileSalvo = 24003, // Helper->players, 5.0s cast, range 6 circle, stack

    MobileCrossrayVisual1 = 23997, // Boss->self, 7.0s cast, single-target
    MobileCrossrayVisual2 = 23998, // Boss->self, 7.0s cast, single-target
    MobileCrossrayVisual3 = 23999, // Boss->self, 7.0s cast, single-target
    MobileCrossrayVisual4 = 24000, // Boss->self, 7.0s cast, single-target
    MobileCrossray = 23992, // Boss->self, no cast, range 60 width 19 cross

    MobileHaloVisual1 = 23993, // Boss->self, 7.0s cast, single-target
    MobileHaloVisual2 = 23994, // Boss->self, 7.0s cast, single-target
    MobileHaloVisual3 = 23995, // Boss->self, 7.0s cast, single-target
    MobileHaloVisual4 = 23996, // Boss->self, 7.0s cast, single-target
    MobileHalo = 23990, // Boss->self, no cast, range 9-60 donut

    PentagustVisual = 24017, // Dawon->self, 5.0s cast, single-target
    Pentagust = 24018, // Helper->self, 5.0s cast, range 50 20-degree cone

    SpiralScourgeVisual1 = 23986, // Boss->self, 7.0s cast, single-target
    SpiralScourgeVisual2 = 23988, // Boss->self, no cast, single-target
    SpiralScourge = 23987, // Helper->location, no cast, range 13 circle

    SurfaceMissile = 24004, // Helper->location, 5.0s cast, range 6 circle

    ToothAndTalon = 24020, // Dawon->player, 5.0s cast, single-target, tankbuster

    TouchdownVisual = 24912, // Helper->self, 6.0s cast, range 60 circle, miniscule phase change raidwide, knockback, 2 casters
    Touchdown1 = 24006, // Dawon->self, 6.0s cast, ???, knockback 30, away from source
    Touchdown2 = 24007, // Helper->self, no cast, ???, knockback 30, away from source

    WildfireWinds1 = 24013, // Dawon->self, 5.0s cast, ???, raidwide, 2 casters
    WildfireWinds2 = 24772 // Helper->self, no cast, ???
}

public enum SID : uint
{
    OneMind = 2553 // none->Boss/Dawon, extra=0x0
}
