namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

public enum OID : uint
{
    Boss = 0x2E6E, // R0.5-3.5
    Huma = 0x2E70, // R1.12
    Peri = 0x2E6F, // R4.0
    Deathwall = 0x1EB031, // R0.5
    DeathwallHelper = 0x2EE8, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 21402, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    Teleport1 = 21330, // Boss->location, no cast, single-target
    Teleport2 = 21400, // Boss->location, no cast, single-target
    Deathwall = 21320, // DeathwallHelper->player, no cast, single-target

    MeltdownVisual = 20653, // Boss->self, 3.0s cast, single-target
    Meltdown = 20654, // Helper->self, no cast, range 5 circle

    TimeEruptionVisual = 20641, // Boss->self, 2.0s cast, single-target
    TimeEruption1 = 20642, // Helper->self, 5.0s cast, range 20 width 20 rect
    TimeEruption2 = 20636, // Helper->self, 8.0s cast, range 20 width 20 rect

    ThermalGustVisual = 20655, // Boss->self, 4.0s cast, single-target
    ThermalGust = 20656, // Helper->self, 4.0s cast, range 44 width 10 rect
    Phenex = 20637, // Boss->self, 2.0s cast, single-target
    Flamedive = 20638, // Huma->self, 5.0s cast, range 55 width 6 rect
    SearingWind = 20640, // Helper->self, no cast, range 3 circle
    PyrolatryVisual = 20639, // Boss->self, 6.0s cast, single-target
    Pyrolatry = 21401, // Helper->self, no cast, range 100 circle

    FlashoverVisual = 20643, // Boss->self, 2.0s cast, single-target
    Flashover = 20644, // Peri->self, 10.0s cast, range 19 circle
    FlamingRainVisual = 20647, // Boss->self, 2.0s cast, single-target
    FlamingRain = 20648, // Helper->self, 3.0s cast, range 6 circle
    BackdraftVisual = 20634, // Boss->self, 2.0s cast, single-target
    Backdraft = 20635, // Peri->self, 10.0s cast, range 100 circle, knockback 16, away from source

    ThermalWaveVisual1 = 20649, // Boss->self, 3.0s cast, single-target
    ThermalWaveVisual2 = 20650, // Boss->self, no cast, single-target
    ThermalWaveVisual3 = 20652, // Helper->self, 4.0s cast, range 60 90-degree cone
    ThermalWave = 20651, // Helper->self, no cast, range 60 90-degree cone

    PillarOfFlameVisual = 20645, // Boss->self, 2.0s cast, single-target
    PillarOfFlame = 20646, // Helper->location, 8.0s cast, range 8 circle
    BioIV = 21326, // Boss->player, 3.0s cast, single-target
}

public enum SID : uint
{
    Poison = 18 // Boss->player, extra=0x0
}
