namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V11Genie;

public enum OID : uint
{
    GenieOfTheLamp = 0x4867, // R3.750, x1
    Helper = 0x233C, // R0.500, x37, Helper type
    Airship = 0x4869, // R3.750, x3
    Lever = 0x486A, // R1.100, x0 (spawn during fight)
    SpectacularSpark1 = 0x1EBF68, // R0.500, x1, EventObj type
    SpectacularSpark2 = 0x1EBF69, // R0.500, x1, EventObj type
    // related to boats tracks?
    _Gen_Actor1ebf6a = 0x1EBF6A, // R0.500, x1, EventObj type
    _Gen_Actor1ebf6c_Tracks = 0x1EBF6C, // R0.500, x1, EventObj type
    _Gen_Actor1ebf6d = 0x1EBF6D, // R0.500, x1, EventObj type
    VoyageTracks = 0x1EBF6E, // R0.500, x1, EventObj type
    _Gen_Actor1ebf6f_Lever = 0x1EBF6F, // R0.500, x4, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x2 (spawn during fight), EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1 (spawn during fight), EventObj type
}

public enum AID : uint
{
    _AutoAttack_ = 46236, // GenieOfTheLamp->player, no cast, single-target
    _Ability_ = 43337, // GenieOfTheLamp->location, no cast, single-target
    FabulousFirecrackersVisual1 = 43351, // GenieOfTheLamp->self, 6.0+1.0s cast, single-target
    FabulousFirecrackersVisual2 = 43352, // GenieOfTheLamp->self, 6.0+1.0s cast, single-target
    FabulousFirecrackersBig = 43353, // Helper->self, 7.0s cast, range 30 180.000-degree cone
    FabulousFirecrackersSmall = 43354, // Helper->self, 7.0s cast, range 30 90.000-degree cone
    ParadeOfWonders = 43345, // GenieOfTheLamp->self, 5.0s cast, range 60 circle
    SpectacularSparksVisual = 43347, // GenieOfTheLamp->self, 4.0s cast, single-target
    SpectacularSparks = 43349, // Helper->self, 6.5s cast, range 36 width 6 rect
    _Spell_ASailorsTale = 43356, // GenieOfTheLamp->self, 4.5s cast, single-target
    _Spell_ChartCourse = 43359, // GenieOfTheLamp->self, 7.0s cast, single-target
    Voyage = 47042, // 4869->self, 8.3s cast, range 8 width 12 rect
    _Weaponskill_Voyage1 = 43360, // Helper->self, no cast, range 12 width 12 rect, initial ship (no crossover) 
    _Weaponskill_Voyage2 = 43363, // Helper->self, no cast, range 8 width 12 rect, initial south ship (crossover)
    _Weaponskill_Voyage3 = 43361, // Helper->self, 0.5s cast, range 12 width 12 rect, north/middle ship middle lane (south crossover)
    _Weaponskill_Voyage4 = 43362, // Helper->self, 0.5s cast, range 12 width 12 rect, north/middle ship middle lane (south crossover)
    _Weaponskill_Voyage5 = 43364, // Helper->self, 0.5s cast, range 22 width 12 rect, south cross track
    _Weaponskill_Voyage6 = 43646, // Helper->self, 0.5s cast, range 21 width 12 rect, south ship middle lane (south crossover)
    _Weaponskill_Voyage7 = 43674, // Helper->self, 0.5s cast, range 20 width 12 rect, north ship north lane
    _Weaponskill_Voyage8 = 43679, // Helper->self, 0.5s cast, range 22 width 12 rect, north cross track
    _Weaponskill_Voyage9 = 43729, // Helper->self, 0.6s cast, range 20 width 12 rect, north ship middle lane
    // north ship -> 1,4,3 || 7,8,9
    // middle ship -> 1,3,4
    // south ship -> 3,1,4 || 2,5,3
    ExplosiveEnding = 43346, // GenieOfTheLamp->self, 5.0s cast, range 60 circle
    PyromagicksVisual = 44260, // GenieOfTheLamp->self, 4.0+1.0s cast, single-target
    PyromagicksFirst = 44261, // Helper->self, 5.0s cast, range 6 circle
    PyromagicksRest = 44265, // Helper->self, no cast, range 6 circle
    LampLighting = 45586, // GenieOfTheLamp->self, 3.5s cast, range 60 width 8 rect
    FanningFlameVisual = 44253, // GenieOfTheLamp->self, 3.5+1.0s cast, single-target
    FanningFlame1 = 44254, // Helper->self, 4.5s cast, range 30 45.000-degree cone
    FanningFlame2 = 44255, // Helper->self, 6.5s cast, range 30 45.000-degree cone
    SupernaturalSurprise = 43343, // GenieOfTheLamp->self, 5.0s cast, range 60 circle
    RubBurn = 43344, // GenieOfTheLamp->player, 5.0s cast, single-target
    _Spell_SpectacularSparks2 = 43348, // GenieOfTheLamp->self, 4.0s cast, single-target

    // right path
    _Spell_RainbowRoad = 44663, // GenieOfTheLamp->self, 4.0+1.0s cast, single-target
    RainbowRoad1 = 44771, // Helper->self, 5.0s cast, range 15 circle
    RainbowRoad2 = 44821, // Helper->self, 12.0s cast, range 15 circle
    AetherialBlizzard = 44344, // 4868->self, 8.0s cast, range 36 width 4 rect
    LampOil = 44252, // Helper->self, 8.5s cast, range 8 circle
}

public enum SID : uint
{
    _Gen_ = 2056, // none->4869, extra=0x440
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_sinentai01p = 102, // 4869->GenieOfTheLamp
    ChartCourse = 86, // 486A->GenieOfTheLamp
}
