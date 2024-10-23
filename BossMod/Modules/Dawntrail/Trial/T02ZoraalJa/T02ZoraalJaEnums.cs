namespace BossMod.Dawntrail.Trial.T02ZoraalJa;

public enum OID : uint
{
    Boss = 0x42A9, // R2.500
    Fang1 = 0x42AA, // R1.0
    Fang2 = 0x42B6, // R1.0
    ShadowOfTural1 = 0x43A8, // R0.5
    ShadowOfTural2 = 0x42AC, // R1.0
    ShadowOfTural3 = 0x42AD, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 39137, // Boss->location, no cast, single-target

    SoulOverflow1 = 37707, // Boss->self, 5.0s cast, range 100 circle, raidwide
    SoulOverflow2 = 37744, // Boss->self, 7.0s cast, range 100 circle, raidwide + phase change

    DoubleEdgedSwordsVisual = 37713, // Boss->self, 4.4+0.6s cast, single-target
    DoubleEdgedSwords = 37714, // Helper->self, 5.0s cast, range 30 180-degree cone

    PatricidalPique = 37715, // Boss->player, 5.0s cast, single-target
    CalamitysEdge = 37708, // Boss->self, 5.0s cast, range 100 circle
    Burst = 37709, // ShadowOfTural1->self, 8.0s cast, range 8 circle

    VorpalTrailVisual1 = 37710, // Boss->self, 3.7+0.3s cast, single-target
    VorpalTrailVisual2 = 38183, // Fang1->location, no cast, width 4 rect charge
    VorpalTrailVisual3 = 37711, // Fang1->location, 1.0s cast, width 4 rect charge
    VorpalTrail1 = 38184, // Helper->location, 4.3s cast, width 4 rect charge
    VorpalTrail2 = 37712 // Helper->location, 2.3s cast, width 4 rect charge
}
