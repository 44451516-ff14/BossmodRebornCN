namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

public enum OID : uint
{
    Boss = 0x32BF, // R3.64, 4th Legion Blackburn
    ForthLegionInfantry = 0x32C0, // R1.0
    TerminusEst = 0x32C1, // R1.0
    ForthLegionAugur1 = 0x32C8, // R0.55
    ForthLegionAugur2 = 0x33E2, // R0.5
    StormborneZirnitra = 0x32C7, // R2.8
    WaveborneZirnitra = 0x32C6, // R2.8
    FlameborneZirnitra = 0x32C5, // R2.8
    TowerVisual1 = 0x1EB213, // R0.5
    TowerVisual2 = 0x1EB214, // R0.5
    TamedCrow = 0x32CA, // R4.32
    ForthLegionBeastmaster = 0x32CF, // R0.5
    TamedAlkonost = 0x32CB, // R7.5
    VorticalOrb = 0x32CC, // R0.5
    TamedAlkonostsShadow = 0x32CE, // R3.75-7.5
    FourthLegionHoplomachus = 0x3254, // R0.5
    Helper2 = 0x32CD, // R0.5-3.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBlackburn = 25126, // Boss->player, no cast, single-target
    AutoAttackAdd1 = 25129, // ForthLegionInfantry->player, no cast, single-target
    AutoAttackAdd2 = 25128, // FlameborneZirnitra/StormborneZirnitra/WaveborneZirnitra->player, no cast, single-target
    Fire = 25127, // ForthLegionAugur1->player, no cast, single-target
    AutoAttackCrow = 6498, // TamedCrow->player, no cast, single-target
    AutoAttackAlkonost = 6499, // TamedAlkonost->player, no cast, single-target

    SuppressiveMagitekRaysVisual = 24338, // Boss->self, 5.0s cast, single-target
    SuppressiveMagitekRays = 24339, // Helper->self, 5.6s cast, ???

    AntiPersonnelMissile = 24333, // Boss->self, 10.0s cast, single-target
    BallisticImpact = 24334, // Helper->self, no cast, range 24 width 24 rect

    ReadOrdersFieldSupport = 24332, // Boss->self, 3.0s cast, single-target
    TerminusEstVisual = 24323, // ForthLegionInfantry->self, no cast, single-target
    TerminusEst = 24327, // TerminusEst->self, 5.0s cast, range 50 width 8 rect
    CeruleumExplosion = 24326, // ForthLegionInfantry->self, 10.0s cast, range 60 circle, add enrage

    Analysis = 24335, // Boss->self, 3.0s cast, single-target
    SurfaceMissileVisual = 24336, // Boss->self, 3.0s cast, single-target
    SurfaceMissile = 24337, // Helper->location, 3.0s cast, range 6 circle

    SanctifiedQuakeIIIVisual = 24351, // ForthLegionAugur1->self, 5.0s cast, single-target
    SanctifiedQuakeIII = 24352, // Helper->self, 5.6s cast, ???
    VoidCall = 24350, // ForthLegionAugur1->self, 3.0s cast, single-target

    TurbineVisual = 24340, // FlameborneZirnitra->self, 5.0s cast, range 40 circle
    Turbine = 24341, // Helper->self, 5.0s cast, ???, knockback 15, away from source

    FlamingCyclone = 24345, // StormborneZirnitra->self, 7.5s cast, range 10 circle
    SeventyFourDegrees = 24343, // WaveborneZirnitra->player, 8.0s cast, range 4-8 donut

    PyroplexyVisual = 24347, // ForthLegionAugur1->self, 12.0s cast, single-target
    Pyroplexy = 24348, // Helper->location, no cast, range 4 circle
    Pyroclysm = 24349, // Helper->location, no cast, range 40 circle, tower fail

    StormcallVisual = 24358, // TamedAlkonost->self, 3.0s cast, single-target
    Stormcall = 24359, // Helper2/VorticalOrb->self, 2.0s cast, range 35 circle

    WindVisual = 24355, // Helper->self, 8.0s cast, range 60 width 60 rect
    NorthWindVisual = 24353, // TamedCrow->self, 8.0s cast, single-target
    SouthWindVisual = 24354, // TamedCrow->self, 8.0s cast, single-target
    SouthWind = 24815, // Helper->self, no cast, ???, knockback 40, dir forward
    NorthWind = 24814, // Helper->self, no cast, ???, knockback 40, dir forward

    PainStorm = 24363, // TamedAlkonost->self, 6.0s cast, range 35 130-degree cone
    FrigidPulse = 24362, // TamedAlkonost->self, 6.0s cast, range 8-25 donut
    PainStormShadow = 24366, // TamedAlkonostsShadow->self, 11.0s cast, range 35 130-degree cone
    FrigidPulseShadow = 24365, // TamedAlkonostsShadow->self, 11.0s cast, range 8-25 donut
    PainfulGust = 24364, // TamedAlkonost->self, 6.0s cast, range 20 circle
    PainfulGustShadow = 24367, // TamedAlkonostsShadow->self, 11.0s cast, range 20 circle
    ShadowsCast = 24369, // TamedAlkonostsShadow->self, 6.0s cast, single-target
    Foreshadowing = 24368, // TamedAlkonost->self, 11.0s cast, single-target

    NihilitysSongVisual = 24360, // TamedAlkonost->self, 5.0s cast, single-target
    NihilitysSong = 24361, // Helper->self, 5.6s cast, ???
    BroadsideBarrage = 24357, // TamedCrow->self, 5.0s cast, range 40 width 40 rect
}

public enum SID : uint
{
    RightUnseen = 1707, // none->player, extra=0xE9
    LeftUnseen = 1708, // none->player, extra=0xEA
    BackUnseen = 1709 // none->player, extra=0xE8
}

public enum IconID : uint
{
    BallisticImpact = 261, // Helper->self
    SeventyFourDegrees = 288 // player->self
}

public enum TetherID : uint
{
    Stormcall = 4 // VorticalOrb/TamedAlkonostsShadow1->TamedAlkonost
}
