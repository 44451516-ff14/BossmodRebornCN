namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL3Adrammelech;

public enum OID : uint
{
    Boss = 0x2E01, // R5.005, x1
    Deathwall = 0x1EB037, // R0.5
    Orbs = 0x1EB038, // R0.5
    AqueousOrb = 0x2E04, // R1.3
    FrozenOrb = 0x2E03, // R1.3
    SabulousOrb = 0x2E07, // R1.3
    TorridOrb = 0x2E02, // R1.3
    VorticalOrb = 0x2E06, // R0.7
    ChargedOrb = 0x2E05, // R1.3
    ElectricCharge = 0x2E08, // R1.3
    Twister = 0x2E09, // R1.95
    ArcaneSphere = 0x2E0E, // R1.0
    Meteor = 0x1EB039, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 20372, // Boss->player, no cast, single-target

    HolyIV = 20374, // Boss->self, 4.0s cast, range 60 circle
    CurseOfTheFiend = 20346, // Boss->self, 3.0s cast, single-target
    AccursedBecoming = 20347, // Boss->self, 4.0s cast, single-target

    WaterIVVisual = 21464, // Helper->self, 6.5s cast, range 60 circle
    WaterIV1 = 20556, // Helper->self, 6.5s cast, range 60 circle, knockback 12, away from source, 1+2 happen at the same time, probably to get around max target limit
    WaterIV2 = 20350, // Helper->self, 6.5s cast, range 60 circle, knockback 12, away from source
    WaterIV3 = 20356, // Helper->self, 6.5s cast, range 60 circle, knockback 12, away from source, 3+4 happen at the same time, probably to get around max target limit
    WaterIV4 = 20557, // Helper->self, 6.5s cast, range 60 circle, knockback 12, away from source

    BlizzardIV1 = 21595, // Helper->self, 6.5s cast, range 60 circle, deep freeze if not moving at the end, 1+2 happen at the same time, probably to get around max target limit
    BlizzardIV2 = 21597, // Helper->self, 6.5s cast, range 60 circle
    BlizzardIV3 = 20555, // Helper->self, 6.5s cast, range 60 circle, deep freeze if not moving at the end, 3+4 happen at the same time, probably to get around max target limit
    BlizzardIV4 = 20349, // Helper->self, 6.5s cast, range 60 circle

    StoneIV1 = 20353, // Helper->self, 6.5s cast, range 10 circle
    StoneIV2 = 20354, // Helper->self, 8.5s cast, range 10-20 donut
    StoneIV3 = 20355, // Helper->self, 10.5s cast, range 20-30 donut
    StoneIV4 = 20359, // Helper->self, 6.5s cast, range 10 circle
    StoneIV5 = 20360, // Helper->self, 8.5s cast, range 10-20 donut
    StoneIV6 = 20361, // Helper->self, 10.5s cast, range 20-30 donut

    AeroIV1 = 20352, // Helper->self, 6.5s cast, range 15-30 donut
    AeroIV2 = 20358, // Helper->self, 6.5s cast, range 15-30 donut

    ThunderIV1 = 20351, // Helper->self, 6.5s cast, range 18 circle
    ThunderIV2 = 20357, // Helper->self, 6.5s cast, range 18 circle

    FireIV1 = 21596, // Helper->self, 6.5s cast, range 60 circle, apply pyretic, 1+2 happen at the same time, probably to get around max target limit
    FireIV2 = 21594, // Helper->self, 6.5s cast, range 60 circle, apply pyretic
    FireIV3 = 20554, // Helper->self, 6.5s cast, range 60 circle, apply pyretic, 3+4 happen at the same time, probably to get around max target limit
    FireIV4 = 20348, // Helper->self, 6.5s cast, range 60 circle, apply pyretic

    BurstIIVisual = 20362, // Boss->self, 4.0s cast, single-target
    BurstII = 20363, // Helper->location, 4.0s cast, range 6 circle
    WarpedLightVisual = 20364, // Boss->self, 8.0s cast, single-target
    WarpedLight1 = 20365, // Helper->location, 8.0s cast, width 3 rect charge
    WarpedLight2 = 20558, // Helper->location, 8.5s cast, width 3 rect charge
    WarpedLight3 = 20559, // Helper->location, 9.0s cast, width 3 rect charge
    WarpedLight4 = 20560, // Helper->location, 9.5s cast, width 3 rect charge
    WarpedLight5 = 20561, // Helper->location, 10.0s cast, width 3 rect charge
    WarpedLight6 = 20562, // Helper->location, 10.5s cast, width 3 rect charge
    Shock = 20366, // Helper->self, 11.0s cast, range 35 circle

    TornadoVisual = 20367, // Boss->self, 4.0s cast, single-target
    Tornado = 20368, // Helper->location, 4.0s cast, range 6 circle
    Updraft = 20369, // Helper->self, no cast, range 6 circle

    Flare = 20373, // Boss->player, 4.0s cast, single-target, tankbuster
    MeteorVisual = 20370, // Boss->self, 4.0s cast, single-target
    Meteor = 20371 // ArcaneSphere->self, no cast, range 60 circle
}

public enum SID : uint
{
    Pyretic = 960 // Helper->player, extra=0x0
}
