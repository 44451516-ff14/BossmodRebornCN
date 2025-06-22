namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

public enum OID : uint
{
    Boss = 0x4722, // R5.005
    MoonlitShadow = 0x4724, // R5.005
    GleamingFang = 0x4726, // R1.4
    HowlingBladeShadow = 0x4723, // R4.235
    WolfOfStone1 = 0x47B1, // R1.5
    WolfOfStone2 = 0x4732, // R1.5
    WolfOfStone3 = 0x484A, // R1.5
    WolfOfWind1 = 0x47B0, // R1.5
    WolfOfWind2 = 0x4725, // R1.5
    WolfOfWind3 = 0x4849, // R1.5
    Helper = 0x233C // R0.5
}

public enum AID : uint
{
    AutoAttack = 42221, // Boss->player, no cast, single-target
    AutoAttackAdd1 = 42224, // WolfOfStone2->player, no cast, single-target
    AutoAttackAdd2 = 42223, // WolfOfWind2->player, no cast, single-target
    Teleport = 41835, // Boss->location, no cast, single-target

    ExtraplanarPursuitVisual = 41870, // Boss->self, 1.6+2,4s cast, single-target
    ExtraplanarPursuit = 42830, // Helper->self, 4.0s cast, range 40 circle

    GreatDivide = 41869, // Boss->self/players, 5.0s cast, range 60 width 6 rect, shared tankbuster

    WolvesReignVisual1 = 43373, // Boss->self, 5.1s cast, single-target
    WolvesReignVisual2 = 43385, // Boss->self, 2.5+0,5s cast, single-target
    WolvesReignVisual3 = 43374, // Boss->self, 5.1s cast, single-target
    WolvesReignVisual4 = 43279, // Boss->self, 4.6s cast, single-target
    WolvesReignVisual5 = 41842, // Boss->self, 1.5+0,5s cast, single-target
    WolvesReignVisual6 = 43280, // Boss->self, 4.6s cast, single-target
    WolvesReignTeleport1 = 43375, // Boss->location, no cast, single-target
    WolvesReignTeleport2 = 43383, // Boss->location, 2.0+0,5s cast, single-target
    WolvesReignTeleport3 = 43296, // Boss->location, no cast, single-target
    WolvesReignTeleport4 = 41841, // Boss->location, 1.0+0,5s cast, single-target
    WolvesReignCloneVisual1 = 43376, // HowlingBladeShadow->self, 6.3s cast, single-target
    WolvesReignCloneVisual2 = 43377, // HowlingBladeShadow->self, 6.4s cast, single-target
    WolvesReignCloneVisual3 = 43378, // HowlingBladeShadow->self, 6.5s cast, single-target
    WolvesReignCloneVisual4 = 43299, // HowlingBladeShadow->self, 5.8s cast, single-target
    WolvesReignCloneVisual5 = 43300, // HowlingBladeShadow->self, 5.9s cast, single-target
    WolvesReignCloneVisual6 = 43301, // HowlingBladeShadow->self, 6.0s cast, single-target
    WolvesReignCircle1 = 43380, // Helper->self, 6.7s cast, range 6 circle
    WolvesReignCircle2 = 43381, // Helper->self, 6.8s cast, range 6 circle
    WolvesReignCircle3 = 43382, // Helper->self, 6.9s cast, range 6 circle
    WolvesReignCircle4 = 43379, // Helper->self, 7.0s cast, range 6 circle
    WolvesReignCircle5 = 43302, // Helper->self, 6.2s cast, range 6 circle
    WolvesReignCircle6 = 43303, // Helper->self, 6.3s cast, range 6 circle
    WolvesReignCircle7 = 43304, // Helper->self, 6.4s cast, range 6 circle
    WolvesReignCircle8 = 43311, // Helper->self, 6.5s cast, range 6 circle
    WolvesReignRect1 = 43384, // Helper->self, 2.5s cast, range 36 width 10 rect
    WolvesReignRect2 = 43368, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignCone1 = 43386, // Helper->self, 3.0s cast, range 40 120-degree cone
    WolvesReignCone2 = 42928, // Helper->self, 2.0s cast, range 40 120-degree cone

    HeavensearthVisual = 41865, // Boss->self, 5.0s cast, single-target, stack
    Heavensearth1 = 41866, // Helper->players, 5.3s cast, range 6 circle
    Heavensearth2 = 41859, // Helper->players, 5.0s cast, range 6 circle

    BeckonMoonlight1 = 43387, // Boss->self, 4.0s cast, single-target
    BeckonMoonlight2 = 41845, // Boss->self, 3.0s cast, single-target
    MoonlitShadowTeleport1 = 43390, // MoonlitShadow->location, no cast, single-target
    MoonlitShadowTeleport2 = 43391, // MoonlitShadow->location, no cast, single-target
    MoonlitShadowTeleport3 = 41950, // MoonlitShadow->location, no cast, single-target
    MoonlitShadowTeleport4 = 41951, // MoonlitShadow->location, no cast, single-target
    MoonbeamsBite1 = 43388, // MoonlitShadow->self, 6.0s cast, range 40 width 20 rect
    MoonbeamsBite2 = 43389, // MoonlitShadow->self, 6.0s cast, range 40 width 20 rect
    MoonbeamsBite3 = 41847, // MoonlitShadow->self, 5.0s cast, range 40 width 20 rect
    MoonbeamsBite4 = 41846, // MoonlitShadow->self, 5.0s cast, range 40 width 20 rect

    ShadowchaseVisual1 = 43392, // Boss->self, 4.0s cast, single-target
    ShadowchaseVisual2 = 41843, // Boss->self, 4.0s cast, single-target
    Shadowchase1 = 43393, // HowlingBladeShadow->self, 4.0s cast, range 40 width 8 rect
    Shadowchase2 = 41844, // HowlingBladeShadow->self, 3.0s cast, range 40 width 8 rect

    BareFangs = 41860, // Boss->self, 3.0s cast, single-target
    TerrestrialRage = 41862, // Boss->self, 3.0s cast, single-target
    FangedCharge = 41861, // GleamingFang->self, 4.0s cast, range 46 width 6 rect
    TargetedQuakeVisual = 42885, // Boss->self, no cast, single-target
    TargetedQuake = 41864, // Helper->location, 4.0s cast, range 4 circle
    TacticalPack = 41852, // Boss->self, 3.0s cast, single-target
    LimitBreakPhaseStart = 41853, // Boss->self, no cast, single-target

    Gust = 41858, // Helper->player, 5.0s cast, range 5 circle, spread

    WealOfStoneVisual1 = 41894, // WolfOfStone2->self, 5.0s cast, single-target
    WealOfStoneVisual2 = 43400, // WolfOfStone3->self, 2.5s cast, single-target
    WealOfStoneVisual3 = 43404, // WolfOfStone3->self, 2.5s cast, single-target
    WealOfStone1 = 41895, // Helper->self, 5.0s cast, range 40 width 6 rect
    WealOfStone2 = 43401, // Helper->self, 2.5s cast, range 40 width 6 rect
    WealOfStone3 = 43405, // Helper->self, 2.5s cast, range 40 width 6 rect
    GrowlingWindVisual = 41892, // WolfOfWind2->self, 5.0s cast, single-target
    GrowlingWind = 41893, // Helper->self, 5.0s cast, range 40 width 6 rect

    RavenousSaber1 = 42819, // Helper->self, 3.4s cast, range 40 circle
    RavenousSaber2 = 42820, // Helper->self, 3.6s cast, range 40 circle
    RavenousSaber3 = 42821, // Helper->self, 3.9s cast, range 40 circle
    RavenousSaber4 = 43516, // Helper->self, 6.0s cast, range 40 circle
    RavenousSaber5 = 41855, // Helper->self, 7.3s cast, range 40 circle
    RavenousSaberEnrage1 = 42822, // Helper->self, 3.4s cast, range 40 circle
    RavenousSaberEnrage2 = 42823, // Helper->self, 3.6s cast, range 40 circle
    RavenousSaberEnrage3 = 42824, // Helper->self, 3.9s cast, range 40 circle
    RavenousSaberEnrage4 = 43517, // Helper->self, 6.0s cast, range 40 circle
    RavenousSaberEnrage5 = 41856, // Helper->self, 7.3s cast, range 40 circle

    TerrestrialTitansVisual = 43316, // Boss->self, 3.0s cast, single-target
    TerrestrialTitans = 43317, // Helper->self, 3.0s cast, range 3 circle
    TitanicPursuitVisual = 41851, // Boss->self, no cast, range 40 circle
    TitanicPursuit = 42832, // Helper->self, 2.4s cast, range 40 circle
    Towerfall = 43315, // Helper->self, 4.4s cast, range 30 width 6 rect

    RoaringWindVisual = 43396, // WolfOfWind3->self, 2.5s cast, single-target
    RoaringWind = 43397, // Helper->self, 2.5s cast, range 40 width 8 rect

    TrackingTremorsVisual = 42210, // Boss->self, 5.0s cast, single-target
    TrackingTremors = 42211 // Helper->players, no cast, range 6 circle, stack x5, 5s after icon
}

public enum IconID : uint
{
    TrackingTremors = 316 // player->self
}
