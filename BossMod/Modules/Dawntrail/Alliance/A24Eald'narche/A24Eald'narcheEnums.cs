namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

public enum OID : uint
{
    Ealdnarche = 0x460F, // R7.0
    OrbitalFlame = 0x4939, // R1.3
    OrbitalWind = 0x493B, // R1.0
    OrbitalLevin = 0x493A, // R1.5
    Helper3 = 0x4824, // R2.0
    Helper2 = 0x49E2, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44359, // Ealdnarche->player, no cast, single-target
    Teleport = 44360, // Ealdnarche->location, no cast, single-target

    UranosCascadeVisual = 44369, // Ealdnarche->self, 4.0+1,0s cast, single-target
    UranosCascade = 44370, // Helper->players, 5.0s cast, range 6 circle, tankbuster

    CronosSlingVisual1 = 44361, // Ealdnarche->self, 7.0s cast, single-target
    CronosSlingVisual2 = 44362, // Ealdnarche->self, 7.0s cast, single-target
    CronosSlingVisual3 = 44363, // Ealdnarche->self, 7.0s cast, single-target
    CronosSlingVisual4 = 44364, // Ealdnarche->self, 7.0s cast, single-target

    CronosSlingCircle = 44365, // Helper->self, 7.5s cast, range 9 circle
    CronosSlingDonut = 44366, // Helper->self, 7.5s cast, range 6-70 donut
    CronosSlingRect1 = 44367, // Helper->self, 13.3s cast, range 70 width 136 rect
    CronosSlingRect2 = 44368, // Helper->self, 13.3s cast, range 70 width 136 rect

    EmpyrealVortexVisual1 = 44397, // Ealdnarche->self, 4.0+1,0s cast, single-target
    EmpyrealVortexVisual2 = 44398, // Helper->self, 4.3s cast, single-target
    EmpyrealVortexRW = 44399, // Helper->self, no cast, range 75 circle
    EmpyrealVortexAOE = 44401, // Helper->location, 5.0s cast, range 6 circle
    EmpyrealVortexSpread = 44400, // Helper->players, 5.0s cast, range 5 circle, spread

    WarpVisual1 = 44375, // Helper->self, 4.0s cast, range 6 circle
    WarpVisual2 = 44374, // Ealdnarche->self, 4.0s cast, single-target
    Sleepga = 44376, // Ealdnarche->self, 3.0s cast, range 70 width 70 rect

    GaeaStreamVisual = 44371, // Ealdnarche->self, 3.0+1,0s cast, single-target
    GaeaStreamFirst = 44372, // Helper->self, 4.0s cast, range 4 width 24 rect
    GaeaStreamRest = 44373, // Helper->self, 2.0s cast, range 4 width 24 rect

    OmegaJavelinVisual = 44380, // Ealdnarche->self, 4.0+1,0s cast, single-target
    OmegaJavelinSpread = 44381, // Helper->location, no cast, range 6 circle
    OmegaJavelinAOE = 44382, // Helper->location, 4.5s cast, range 6 circle

    DuplicateVisual = 44407, // Ealdnarche->self, 2.0+1,0s cast, single-target
    Duplicate1 = 44408, // Helper->self, 0.7s cast, range 16 width 16 rect
    Duplicate2 = 44406, // Helper->self, 1.0s cast, range 16 width 16 rect

    Excelsior = 45120, // Ealdnarche->self, 7.0s cast, range 35 circle, visual related to phase change
    PhaseChange = 44823, // Helper->self, 7.5s cast, range 100 circle, pull 45 between hitboxes
    PhaseShift1 = 44377, // Ealdnarche->self, 3.0s cast, single-target
    PhaseShift2 = 44334, // Ealdnarche->self, no cast, single-target
    PhaseShift3 = 44378, // Helper->self, 22.0s cast, range 100 circle
    DestroyTile = 44379, // Helper->self, 13.0s cast, range 16 width 16 rect

    VisionsOfParadise = 44405, // Ealdnarche->self, 7.0+1,0s cast, single-target

    StellarBurstMarker = 44413, // Helper->player, 4.0s cast, single-target
    StellarBurstVisual1 = 44402, // Ealdnarche->self, 4.0+1,0s cast, single-target
    StellarBurstVisual2 = 44403, // Helper->player, 5.0s cast, single-target
    StellarBurst = 44404, // Helper->location, 5.0s cast, range 24 circle

    Freeze = 44389, // Helper->self, 6.0s cast, range 16 width 48 rect, freezes tiles
    Quake = 44388, // Helper->self, 6.0s cast, range 16 width 48 rect, turns tiles into quicksand
    Flood = 44390, // Helper->self, 6.0s cast, range 35 circle, proximity AOE

    AncientTriad = 44383, // Ealdnarche->self, 6.0s cast, single-target
    FlareCircle = 44384, // Helper->location, 7.0s cast, range 5 circle
    FlareRect = 44385, // OrbitalFlame->location, 4.0s cast, range 70 width 6 rect

    Flood1 = 44391, // Helper->self, 10.0s cast, range 8 circle
    Flood2 = 44392, // Helper->self, 12.0s cast, range 8-16 donut
    Flood3 = 44393, // Helper->self, 14.0s cast, range 16-24 donut
    Flood4 = 44394, // Helper->self, 16.0s cast, range 24-36 donut

    TornadoPull = 44395, // Helper->self, 6.0s cast, range 35 circle, pull 16 between centers
    TornadoAOE = 44341, // Helper->self, 7.0s cast, range 5 circle
    TornadoVoidzone = 44396, // Helper->self, no cast, range 3 circle

    Burst = 44386, // Helper->location, 7.0s cast, range 5 circle
    Shock = 44387, // OrbitalLevin->player, no cast, single-target
}

public enum IconID : uint
{
    OmegaJavelin = 466, // player->self
    StellarBurst = 608 // Helper2->self
}

public enum SID : uint
{
    Sleep = 3466, // Ealdnarche->player, extra=0x0
    Paralysis = 3463 // OrbitalLevin->player, extra=0x0
}

public enum TetherID : uint
{
    OrbitalLevin = 6 // OrbitalLevin->player
}
