namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

public enum OID : uint
{
    Kamlanaut = 0x48F8, // R6.0
    KamlanautClone = 0x48F9, // R6.0
    SublimeEstoc = 0x48FA, // R1.0
    ProvingGroundVoidzone = 0x1EBEEF, // R0.5
    Crystal1 = 0x1EBE87, // R0.5
    Crystal2 = 0x1EBE88, // R0.5
    Crystal3 = 0x1EBE85, // R0.5
    Crystal4 = 0x1EBE86, // R0.5
    Crystal5 = 0x1EBE84, // R0.5
    Crystal6 = 0x1EBE89, // R0.5
    Helper2 = 0x49C7, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44876, // Kamlanaut->player, no cast, single-target
    Teleport = 44225, // Kamlanaut->location, no cast, single-target

    EnspiritedSwordplay = 44221, // Kamlanaut->self, 5.0s cast, range 60 circle

    ProvingGround = 45065, // Kamlanaut->self, 3.0s cast, range 5 circle

    SublimeElements = 45066, // Kamlanaut->self, 8.0+1,0s cast, single-target
    SublimeEarthWide = 44186, // Helper->self, 9.0s cast, range 40 100-degree cone
    SublimeEarthNarrow = 44180, // Helper->self, 9.0s cast, range 40 20-degree cone
    SublimeFireWide = 44185, // Helper->self, 9.0s cast, range 40 100-degree cone
    SublimeFireNarrow = 44179, // Helper->self, 9.0s cast, range 40 20-degree cone
    SublimeWindWide = 44190, // Helper->self, 9.0s cast, range 40 100-degree cone
    SublimeWindNarrow = 44184, // Helper->self, 9.0s cast, range 40 20-degree cone
    SublimeWaterWide = 44187, // Helper->self, 9.0s cast, range 40 100-degree cone
    SublimeWaterNarrow = 44181, // Helper->self, 9.0s cast, range 40 20-degree cone
    SublimeLightningWide = 44189, // Helper->self, 9.0s cast, range 40 100-degree cone
    SublimeLightningNarrow = 44183, // Helper->self, 9.0s cast, range 40 20-degree cone
    SublimeIceWide = 44188, // Helper->self, 9.0s cast, range 40 100-degree cone
    SublimeIceNarrow = 44182, // Helper->self, 9.0s cast, range 40 20-degree cone

    ElementalBlade1 = 44177, // Kamlanaut->self, 8.0s cast, single-target
    ElementalBlade2 = 44178, // Kamlanaut->self, 11.0s cast, single-target
    IceBladeWide = 44200, // Helper->self, 9.0s cast, range 80 width 20 rect
    IceBladeNarrow = 44194, // Helper->self, 9.0s cast, range 80 width 5 rect
    LightningBladeWide = 44201, // Helper->self, 9.0s cast, range 80 width 20 rect
    LightningBladeNarrow = 44195, // Helper->self, 9.0s cast, range 80 width 5 rect
    FireBladeWide = 44197, // Helper->self, 9.0s cast, range 80 width 20 rect
    FireBladeNarrow = 44191, // Helper->self, 9.0s cast, range 80 width 5 rect
    EarthBladeWide = 44198, // Helper->self, 9.0s cast, range 80 width 20 rect
    EarthBladeNarrow = 44192, // Helper->self, 9.0s cast, range 80 width 5 rect
    WaterBladeWide = 44199, // Helper->self, 9.0s cast, range 80 width 20 rect
    WaterBladeNarrow = 44193, // Helper->self, 9.0s cast, range 80 width 5 rect
    WindBladeWide = 44202, // Helper->self, 9.0s cast, range 80 width 20 rect
    WindBladeNarrow = 44196, // Helper->self, 9.0s cast, range 80 width 5 rect

    PrincelyBlowVisual = 44219, // Kamlanaut->self, 7.2+0,8s cast, single-target
    PrincelyBlow = 44220, // Helper->self, no cast, range 60 width 10 rect, knockback 30, dir forward

    SublimeEstoc = 44204, // SublimeEstoc->self, 3.0s cast, range 40 width 5 rect
    GreatWheelCircle1 = 44205, // Kamlanaut->self, 3.0s cast, range 10 circle
    GreatWheelCircle2 = 44206, // Kamlanaut->self, 3.0s cast, range 10 circle
    GreatWheelCircle3 = 44207, // Kamlanaut->self, 3.0s cast, range 10 circle
    GreatWheelCircle4 = 44208, // Kamlanaut->self, 3.0s cast, range 10 circle
    GreatWheelCone = 44209, // Helper->self, 5.8s cast, range 80 180-degree cone

    EsotericScrivening = 44210, // Kamlanaut->self, 6.0s cast, single-target
    ShockwaveVisual1 = 44409, // Kamlanaut->self, no cast, single-target
    ShockwaveVisual2 = 44410, // Kamlanaut->self, no cast, single-target
    ShockwaveVisual3 = 41727, // Helper->Kamlanaut, no cast, single-target
    Shockwave = 44211, // Helper->self, 5.2s cast, range 60 circle, raidwide

    TranscendentUnionVisual = 44212, // Kamlanaut->self, 5.0s cast, single-target, raidwide x7
    ElementalEdge = 44289, // Helper->self, no cast, range 60 circle
    TranscendentUnion = 44213, // Helper->self, no cast, range 60 circle, big final raidwide

    EsotericPalisade = 44214, // Kamlanaut->self, 3.0s cast, single-target
    CrystallineResonance = 44215, // Kamlanaut->self, 3.0s cast, single-target
    ElementalResonance = 44216, // Helper->self, 7.0s cast, range 18 circle

    EmpyrealBanishIVVisual = 44907, // Kamlanaut->self, 4.0+1,0s cast, single-target
    EmpyrealBanishIV = 44224, // Helper->players, 5.0s cast, range 5 circle, stack
    EmpyrealBanishIII = 44223, // Helper->players, 5.0s cast, range 5 circle, spread

    IllumedFacet = 44217, // Kamlanaut->self, 3.0s cast, single-target
    IllumedEstoc = 44218, // KamlanautClone->self, 8.0s cast, range 120 width 13 rect
    LightBlade = 44203, // Kamlanaut->self, 3.0s cast, range 120 width 13 rect
    ShieldBash = 44222 // Kamlanaut->self, 7.0s cast, range 60 circle, knockback 30, away from source
}

public enum IconID : uint
{
    PrincelyBlow = 613 // Kamlanaut->player
}
