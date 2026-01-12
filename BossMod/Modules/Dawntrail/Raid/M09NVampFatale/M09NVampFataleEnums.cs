namespace BossMod.Modules.Dawntrail.Raid.M09NVampFatale;

public enum OID : uint
{
    VampFatale = 0x4ADC,
    Helper = 0x233C,
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    VampetteFatale = 0x4C2E, // R1.200, x0 (spawn during fight)
    Coffinmaker = 0x4ADD, // R10.000, x0 (spawn during fight)
    FatalFlail = 0x4ADE, // R3.000, x0 (spawn during fight)
    Neckbiter = 0x4AE5, // R3.000, x0 (spawn during fight)
    Coffinmaker1 = 0x4AE6, // R1.000, x0 (spawn during fight)
}
public enum AID : uint
{
    AutoAttack1 = 48038, // VampFatale->player, no cast, single-target
    AutoAttack2 = 47047, // VampFatale->player, no cast, single-target

    KillerVoice = 45921, // VampFatale->self, 5.0s cast, range 60 circle
    HalfMoonVisual1 = 48825, // VampFatale->self, 4.3+0.7s cast, single-target
    HalfMoonVisual2 = 48823, // VampFatale->self, 4.3+0,7s cast, single-target
    HalfMoonVisual3 = 48826, // VampFatale->self, 4.3+0.7s cast, single-target
    HalfMoonVisual4 = 48824, // VampFatale->self, 4.3+0.7s cast, single-target
    HalfMoon1 = 45910, // Helper->self, 5.0s cast, range 60 180-degree cone
    HalfMoon2 = 45911, // Helper->self, 8.5s cast, range 60 180-degree cone
    HalfMoon3 = 45906, // Helper->self, 5.0s cast, range 60 180.000-degree cone
    HalfMoon4 = 45907, // Helper->self, 8.5s cast, range 60 180.000-degree cone
    HalfMoon5 = 45912, // Helper->self, 5.0s cast, range 64 180-degree cone
    HalfMoon6 = 45913, // Helper->self, 8.5s cast, range 64 180-degree cone
    HalfMoon7 = 45908, // Helper->self, 5.0s cast, range 64 180-degree cone
    HalfMoon8 = 45909, // Helper->self, 8.5s cast, range 64 180-degree cone

    VampStomp = 45898, // VampFatale->location, 4.1+0.9s cast, single-target
    VampStomp1 = 45899, // Helper->self, 5.0s cast, range 10 circle
    Ability = 45900, // Helper->self, no cast, single-target
    BlastBeat = 45901, // 4C2E->self, 1.5s cast, range 8 circle
    Hardcore = 45914, // VampFatale->self, 3.0+2.0s cast, single-target
    Hardcore1 = 45915, // Helper->player, 5.0s cast, range 6 circle
    Ability1 = 45874, // VampFatale->location, no cast, single-target
    SadisticScreech = 45875, // VampFatale->self, 5.0s cast, single-target
    SadisticScreech1 = 45876, // Helper->self, no cast, range 60 circle
    DeadWake = 46853, // 4ADD->self, 4.5+0.5s cast, single-target
    DeadWake1 = 45877, // Helper->self, 5.0s cast, range 10 width 20 rect
    Coffinfiller = 45878, // Helper->self, 5.0s cast, range 32 width 5 rect
    Coffinfiller1 = 46854, // 4ADD->self, 5.0s cast, single-target
    FlayingFry = 45922, // VampFatale->self, 4.3+0.7s cast, single-target
    FlayingFry1 = 45923, // Helper->players, 5.0s cast, range 5 circle
    Coffinfiller2 = 45879, // Helper->self, 5.0s cast, range 22 width 5 rect
    PenetratingPitch = 45924, // VampFatale->self, 6.3+0.7s cast, single-target
    PenetratingPitch1 = 45925, // Helper->players, 7.0s cast, range 5 circle
    Coffinfiller3 = 45880, // Helper->self, 5.0s cast, range 12 width 5 rect

    MassiveImpact = 45884, // Helper->self, no cast, range 60 circle, tower fail
    CrowdKill = 45886, // VampFatale->self, 0.5+4.9s cast, single-target
    CrowdKill1 = 45887, // Helper->self, no cast, range 60 circle
    FinaleFatale = 45888, // VampFatale->self, 5.0s cast, single-target
    FinaleFatale1 = 45890, // Helper->self, no cast, range 60 circle
    PulpingPulse = 45894, // Helper->location, 4.0s cast, range 5 circle
    Aetherletting = 45895, // VampFatale->self, 5.8+1.2s cast, single-target
    Aetherletting1 = 45896, // Helper->players, 7.0s cast, range 6 circle
    Ability2 = 47570, // Helper->self, 7.0s cast, range 40 width 6 cross
    Aetherletting2 = 45897, // Helper->self, 7.0s cast, range 40 width 6 cross
    BrutalRain = 45917, // VampFatale->self, 3.8+1.2s cast, single-target
    BrutalRain1 = 45920, // Helper->players, no cast, range 6 circle
    InsatiableThirst = 45892, // VampFatale->self, 2.8+2.2s cast, single-target
    InsatiableThirst1 = 45893, // Helper->self, no cast, range 60 circle
    Gravegrazer = 45882, // Helper->self, no cast, range 5 width 5 rect
    Gravegrazer1 = 45881, // Helper->self, no cast, range 10 width 5 rect
    Plummet = 45883, // Helper->self, 7.0s cast, range 3 circle

    Hardcore2 = 45916, // Helper->player, 5.0s cast, range 15 circle
    FinaleFatale2 = 45889, // VampFatale->self, 5.0s cast, single-target
    FinaleFatale3 = 45891, // Helper->self, no cast, range 60 circle

    BarbedBurst = 45885, // 4ADE->self, 16.0s cast, range 60 circle
}

public enum IconID : uint
{
    BrutalRain = 305, // player->self
}
