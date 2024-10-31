namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

public enum OID : uint
{
    Boss = 0x42B4, // R10.05
    Fang1 = 0x42AA, // R1.0
    Fang2 = 0x42B6, // R1.0
    HalfCircuitHelper = 0x42B9, // R10.05
    ForgedTrackHelper = 0x19A, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackP2 = 37798, // Boss->player, no cast, single-target
    TeleportP2 = 37717, // Boss->location, no cast, single-target

    SmitingCircuitVisual1 = 37731, // Boss->self, 7.0s cast, single-target
    SmitingCircuitVisual2 = 37732, // HalfCircuitHelper->self, no cast, single-target
    SmitingCircuitVisual3 = 37733, // HalfCircuitHelper->self, no cast, single-target
    SmitingCircuitDonut = 37734, // Helper->self, 7.0s cast, range 10-30 donut
    SmitingCircuitCircle = 37735, // Helper->self, 7.0s cast, range 10 circle

    DawnOfAnAge = 37716, // Boss->self, 7.0s cast, range 100 circle, raidwide and arena change
    Vollok = 37719, // Boss->self, 4.0s cast, single-target, summons swords

    BitterReapingVisual = 37753, // Boss->self, 4.4+0.6s cast, single-target
    BitterReaping = 37754, // Helper->player, 5.0s cast, single-target, double tankbusters

    Sync = 37721, // Boss->self, 5.0s cast, single-target, square AoE telegraphs on one outside platform
    Gateway = 37723, // Boss->self, 4.0s cast, single-target, spawns multiple blue tethers linking rows/columns of two outside platforms with different rows/columns of the central platform
    BladeWarp = 37726, // Boss->self, 4.0s cast, single-target, summons two blades on an outside platform that will face the middle arena.

    ChasmOfVollok1 = 37720, // Fang2->self, 7.0s cast, range 5 width 5 rect
    ChasmOfVollok2 = 37722, // Helper->self, 1.0s cast, range 5 width 5 rect

    ForgedTrackVisual1 = 37728, // Helper->ForgedTrackHelper, no cast, single-target
    ForgedTrackVisual2 = 37727, // Boss->self, 4.0s cast, single-target
    ForgedTrackTelegraph = 37729, // Fang1->self, 11.9s cast, range 20 width 5 rect
    ForgedTrack = 37730, // Fang1->self, no cast, range 20 width 5 rect

    Actualize = 37718, // Boss->self, 5.0s cast, range 100 circle, raidwide and arena change

    HalfFul1Visual1 = 37736, // Boss->self, 6.0s cast, single-target
    HalfFullVisual2 = 37737, // Boss->self, 6.0s cast, single-target
    HalfFull = 37738, // Helper->self, 6.3s cast, range 60 width 60 rect

    HalfCircuitVisual1 = 37739, // Boss->self, 7.0s cast, single-target
    HalfCircuitVisual2 = 37740, // Boss->self, 7.0s cast, single-target
    HalfCircuitRect = 37741, // Helper->self, 7.3s cast, range 60 width 60 rect
    HalfCircuitDonut = 37742, // Helper->self, 7.0s cast, range 10-30 donut
    HalfCircuitCircle = 37743, // Helper->self, 7.0s cast, range 10 circle

    FireIII = 37752, // Helper->player, no cast, range 5 circle, spread, 5.1s delay

    DutysEdgeMarker = 35567, // Helper->player, no cast, single-target, line stack, 4 hits, 5.4s delay
    DutysEdgeVisual1 = 37748, // Boss->self, 4.9s cast, single-target
    DutysEdgeVisual2 = 37749, // Boss->self, no cast, single-target
    DutysEdge = 37750 // Helper->self, no cast, range 100 width 8 rect
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Spreadmarker = 376 // player
}
