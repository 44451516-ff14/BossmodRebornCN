namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

public enum OID : uint
{
    Boss = 0x3250, // R0.5-5.1
    IgnisEst = 0x3253, // R1.0
    Huma = 0x3251, // R1.12
    TowerVisual1 = 0x1EB214, // R0.5
    TowerVisual2 = 0x1EB213, // R0.5
    Helper2 = 0x3252, // R4.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    AutoAttackP2 = 25022, // Boss->player, no cast, single-target

    Burn = 24191, // Helper2->self, no cast, range 60 circle
    Immolate = 24192, // Helper2->self, no cast, range 60 circle
    BurningBlade = 24208, // Boss->player, 5.0s cast, single-target
    DoubleCast = 24205, // Boss->self, 4.0s cast, single-target
    Flamedive = 24179, // Huma->self, 4.0s cast, range 55 width 5 rect

    LeftBrand = 24204, // Boss->self, 5.0s cast, range 40 180-degree cone
    RightBrand = 24203, // Boss->self, 5.0s cast, range 40 180-degree cone

    MannatheihwonFlameVisual = 24199, // Boss->self, 4.0s cast, single-target
    MannatheihwonFlameRaidwide = 24200, // Helper->self, 5.0s cast, range 60 circle
    MannatheihwonFlameRect = 24201, // IgnisEst->self, 5.0s cast, range 50 width 8 rect
    MannatheihwonFlameCircle = 24202, // Helper->self, 5.0s cast, range 10 circle

    Phenex1 = 24197, // Boss->self, 4.0s cast, single-target
    Phenex2 = 24178, // Boss->self, 4.0s cast, single-target

    Pyroplexy = 24183, // Helper->location, no cast, range 4 circle, towers
    Pyroclysm = 24184, // Helper->location, no cast, range 40 circle, tower fail

    Pyrocrisis = 24207, // Helper->player, 8.0s cast, range 6 circle, spread
    Pyrodoxy = 24206, // Helper->player, 8.0s cast, range 6 circle, stack

    Pyrokinesis = 24188, // Boss->self, 5.0s cast, single-target
    PyrokinesisAOE = 24189, // Helper->self, 5.0s cast, range 60 circle
    PyrokinesisVisual = 24194, // Boss->self, 4.0s cast, single-target

    Hyperpyroplexy1 = 24198, // Boss->self, 4.0s cast, single-target
    Hyperpyroplexy2 = 24182, // Boss->self, 4.0s cast, single-target

    ReverseTimeEruptionVisual1 = 24173, // Boss->self, 7.0s cast, single-target
    ReverseTimeEruptionVisual2 = 24196, // Boss->self, 4.0s cast, single-target
    ReverseTimeEruption1 = 24176, // Helper->self, 7.0s cast, range 20 width 20 rect
    ReverseTimeEruption2 = 24177, // Helper->self, 5.0s cast, range 20 width 20 rect
    TimeEruptionVisual1 = 24172, // Boss->self, 7.0s cast, single-target
    TimeEruptionVisual2 = 24195, // Boss->self, 4.0s cast, single-target
    TimeEruption1 = 24174, // Helper->self, 5.0s cast, range 20 width 20 rect
    TimeEruption2 = 24175, // Helper->self, 7.0s cast, range 20 width 20 rect

    GrandCrossflame = 24186, // Boss->self, 5.0s cast, single-target
    GrandCrossflameAOE = 24187, // Helper->self, 5.0s cast, range 40 width 18 cross
    GrandSword = 24579, // 32E2->self, no cast, range 27 ?-degree cone

    ThermalGust = 24180, // Boss->self, 4.0s cast, single-target
    ThermalGustAOE = 24181, // Helper->self, 4.0s cast, range 44 width 10 rect

    Visual1 = 24185, // Boss->location, no cast, single-target
    Visual2 = 24190, // Boss->self, no cast, single-target
    Visual3 = 24193 // Boss->self, no cast, single-target
}
