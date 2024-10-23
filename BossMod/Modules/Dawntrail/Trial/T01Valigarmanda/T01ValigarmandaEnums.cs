namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

public enum OID : uint
{
    Boss = 0x4115, // R20.0
    Valigarmanda1 = 0x417A,
    Valigarmanda2 = 0x4179,
    FeatherOfRuin = 0x4116, // R2.68
    ArcaneSphere1 = 0x4181, // R1.0
    ArcaneSphere2 = 0x4493, // R1.0
    IceBoulder = 0x4117, // R1.26
    FlameKissedBeacon = 0x438B, // R4.8
    GlacialBeacon = 0x438C, // R4.8
    ThunderousBeacon = 0x438A, // R4.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36899, // Boss->player, no cast, single-target

    StranglingCoilVisual = 36159, // Boss->self, 6.5s cast, single-target
    StranglingCoil = 36160, // Helper->self, 7.3s cast, range 8-30 donut 

    SlitheringStrikeVisual = 36157, // Boss->self, 6.5s cast, single-target
    SlitheringStrike = 36158, // Helper->self, 7.3s cast, range 24 180-degree cone, point blank AOE, out of melee range safe

    SusurrantBreathVisual = 36155, // Boss->self, 6.5s cast, single-target
    SusurrantBreath = 36156, // Helper->self, 7.3s cast, range 50 80-degree cone, corners of arena safe

    SkyruinVisual1 = 36161, // Boss->self, 6.0+5.3s cast, single-target
    SkyruinVisual2 = 38338, // Boss->self, 6.0+5.3s cast, single-target
    Skyruin1 = 36162, // Helper->self, 4.5s cast, range 80 circle, raidwide
    Skyruin2 = 36163, // Helper->self, 4.5s cast, range 80 circle, raidwide

    ThunderousBreathVisual = 36175, // Boss->self, 7.0+0.9s cast, single-target
    ThunderousBreath = 36176, // Helper->self, 7.9s cast, range 50 135-degree cone, raidwide, mitigated by standing on levitation pads

    HailOfFeathersVisual1 = 36170, // Boss->self, 4.0+2.0s cast, single-target
    HailOfFeathersVisual2 = 36171, // FeatherOfRuin->self, no cast, single-target
    HailOfFeathers = 36361, // Helper->self, 6.0s cast, range 80 circle, raidwide

    BlightedBoltVisual = 36172, // Boss->self, 7.0+0.8s cast, single-target
    BlightedBolt1 = 36173, // Helper->player, no cast, range 3 circle, player-targeted AOE, mitigated by leaving the levitation tiles
    BlightedBolt2 = 36174, // Helper->FeatherOfRuin, 7.8s cast, range 7 circle, feather-targeted AOE

    ArcaneLightning = 39001, // ArcaneSphere1->self, 1.0s cast, range 50 width 5 rect

    DisasterZoneVisual1 = 36164, // Boss->self, 3.0+0.8s cast, ???
    DisasterZoneVisual2 = 36166, // Boss->self, 3.0+0.8s cast, ???
    DisasterZone1 = 36165, // Helper->self, 3.8s cast, range 80 circle, raidwide
    DisasterZone2 = 36167, // Helper->self, 3.8s cast, range 80 circle, raidwide

    RuinfallVisual = 36186, // Boss->self, 4.0+1.6s cast, single-target
    RuinfallTower = 36187, // Helper->self, 5.6s cast, range 6 circle
    RuinfallKB = 36189, // Helper->self, 8.0s cast, range 40 width 40 rect, knockback 21, forward
    RuinfallAOE = 39129, // Helper->location, 9.7s cast, range 6 circle

    NorthernCross1 = 36168, // Helper->self, 3.0s cast, range 60 width 25 rect
    NorthernCross2 = 36169, // Helper->self, 3.0s cast, range 60 width 25 rect

    FreezingDust = 36177, // Boss->self, 5.0+0.8s cast, range 80 circle, move or be frozen

    ChillingCataclysmVisual = 39264, // ArcaneSphere2->self, 1.0s cast, single-target
    ChillingCataclysm = 39265, // Helper->self, 1.5s cast, range 40 width 5 cross

    RuinForetold = 38545, // Boss->self, 5.0s cast, range 80 circle, raidwide, summons 3 adds that must be destroyed

    CalamitousCryVisual1 = 36192, // Boss->self, 5.1+0.9s cast, single-target
    CalamitousCryVisual2 = 36193, // Boss->self, no cast, single-target
    CalamitousCryMarker1 = 26708, // Helper->player, no cast, single-target
    CalamitousCryMarker2 = 34722, // Helper->player, no cast, single-target
    CalamitousCry = 36194, // Helper->self, no cast, range 80 width 6 rect

    CalamitousEcho = 36195, // Helper->self, 5.0s cast, range 40 20-degree cone

    LimitBreakVisual1 = 38245, // FlameKissedBeacon->Boss, no cast, single-target
    LimitBreakVisual2 = 38247, // GlacialBeacon->Boss, no cast, single-target
    LimitBreakVisual3 = 38246, // ThunderousBeacon->Boss, no cast, single-target
    LimitBreakVisual4 = 38323, // Boss->self, no cast, single-target, limit break phase ends

    TulidisasterVisual = 36197, // Boss->self, 7.0+3.0s cast, single-target, 3x raidwide, enrage if adds not killed
    Tulidisaster1 = 36199, // Helper->self, no cast, range 80 circle
    Tulidisaster2 = 36200, // Helper->self, no cast, range 80 circle
    Tulidisaster3 = 36198, // Helper->self, no cast, range 80 circle

    EruptionVisual = 36190, // Boss->self, 3.0s cast, single-target
    Eruption = 36191, // Helper->location, 3.0s cast, range 6 circle, baited AOEs

    IceTalonVisual = 36184, // Boss->self, 4.0+1.0s cast, single-target, AOE Tankbuster
    IceTalon = 36185 // Valigarmanda1/Valigarmanda2->player, no cast, range 6 circle
}

public enum SID : uint
{
    FreezingUp = 3523, // Boss->player, extra=0x0
    DeepFreeze = 4150, // Boss/Helper->player, extra=0x0
    Levitate = 3974 // none->player, extra=0xD7
}

public enum IconID : uint
{
    Tankbuster = 344, // player
    FreezingUp = 225, // player
}
