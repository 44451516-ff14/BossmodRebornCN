namespace BossMod.Dawntrail.Trial.T04Zelenia;

public enum OID : uint
{
    Boss = 0x47C3, // R5.5
    BriarThorn1 = 0x47C4, // R1.0
    BriarThorn2 = 0x47C5, // R0.5
    RosebloodDrop = 0x485B, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 43055, // Boss->player, no cast, single-target
    Teleport = 43054, // Boss->location, no cast, single-target

    AlexandrianThunderIVVisual = 43480, // Boss->self, 6.0+0,7s cast, single-target
    AlexandrianThunderIVCircle1 = 43084, // Helper->self, 6.7s cast, range 8 circle
    AlexandrianThunderIVDonut1 = 43085, // Helper->self, 6.7s cast, range 8-24 donut
    AlexandrianThunderIVCircle2 = 43446, // Helper->self, 6.7s cast, range 8 circle
    AlexandrianThunderIVDonut2 = 43447, // Helper->self, 6.7s cast, range 8-24 donut

    ShockVisual1 = 43056, // Boss->self, 3.0s cast, single-target
    ShockLock = 43057, // Helper->self, no cast, single-target, locks position for voidzone
    Shock1 = 43058, // Helper->self, no cast, range 4 circle
    Shock2 = 43059, // Helper->self, no cast, range 4 circle
    Shock3 = 43060, // Helper->self, no cast, range 4 circle
    Shock4 = 43061, // Helper->self, no cast, range 4 circle
    Shock5 = 43062, // Helper->self, no cast, range 4 circle
    Shock6 = 43063, // Helper->self, no cast, range 4 circle

    PowerBreak1 = 43112, // Boss->self, 6.0s cast, range 24 width 64 rect
    PowerBreak2 = 43113, // Boss->self, 6.0s cast, range 24 width 64 rect

    HolyHazardVisual1 = 43124, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual2 = 43122, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual3 = 43120, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual4 = 43125, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual5 = 43121, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual6 = 43123, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual7 = 43065, // Boss->self, no cast, single-target
    HolyHazard = 43126, // Helper->self, 7.7s cast, range 24 120-degree cone

    SpecterOfTheLostVisual = 43128, // Boss->self, 5.0+0,7s cast, single-target
    SpecterOfTheLost = 43129, // Helper->player, 5.7s cast, range 50 45-degree cone

    ThunderSlashVisual1 = 43078, // Boss->self, 6.0+0,7s cast, single-target
    ThunderSlashVisual2 = 43079, // Boss->self, 6.0+0,7s cast, single-target
    ThunderSlash = 43083, // Helper->self, 6.7s cast, range 24 60-degree cone

    RosebloodBloomVisual1 = 43093, // Boss->self, 3.9s cast, range 50 circle
    RosebloodBloomVisual2 = 43091, // Boss->self, 2.6+0,4s cast, single-target
    RosebloodBloom = 43479, // Helper->self, 5.0s cast, range 50 circle, knockback 10, away from source, ignores immunity

    PerfumedQuietusVisual = 43094, // Boss->self, 4.0s cast, single-target
    PerfumedQuietus = 43095, // Helper->self, 25.1s cast, range 50 circle, phase transition raidwide

    QueensCrusade = 43092, // Helper->self, 3.7s cast, range 4 circle
    AlexandrianThunderIIIVisual = 43100, // Boss->self, 3.0s cast, single-target
    AlexandrianThunderIII1 = 43102, // Helper->location, 7.0s cast, custom AOE shape stuff, seems to activate a cone and/or circles at the same time, circle is radius 4
    AlexandrianThunderIII2 = 43439, // Helper->location, 6.0s cast, custom AOE shape stuff, seems to activate a cones and/or circles at the same time, circle is radius 4

    StockBreakVisual = 43086, // Boss->self, 7.0s cast, single-target, stack, 4 hits
    StockBreak1 = 43087, // Helper->location, no cast, range 6 circle
    StockBreak2 = 43088, // Helper->location, no cast, range 6 circle
    StockBreak3 = 43089, // Helper->location, no cast, range 6 circle
    StockBreak4 = 43090, // Helper->location, no cast, range 6 circle

    RosebloodDropSpawn = 43442, // Boss->self, 3.0s cast, ???
    RosebloodDropAOE = 43443, // RosebloodDrop->self, no cast, seems to activate a donut segment

    ValorousAscension = 43070, // Boss->self, 3.0s cast, single-target
    ValorousAscension1 = 43071, // Helper->self, 3.7s cast, range 50 circle
    ValorousAscension2 = 43072, // Helper->self, no cast, range 50 circle
    ValorousAscension3 = 43073, // Helper->self, no cast, range 50 circle
    ValorousAscensionRect = 43074, // BriarThorn1->self, 5.0s cast, range 40 width 8 rect
    ThornedCatharsis = 43127 // Boss->self, 5.0s cast, range 50 circle
}

public enum IconID : uint
{
    Shock = 581, // player->self
    StockBreak = 590 // player->self
}
