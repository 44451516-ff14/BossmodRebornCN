namespace BossMod.Shadowbringers.Alliance.A22SuperiorFlightUnits;

public enum OID : uint
{
    FlightUnitALpha = 0x2E10, // R6.0
    FlightUnitBEta = 0x2E11, // R6.0
    FlightUnitCHi = 0x2E12, // R6.0
    FireVoidzoneSmall = 0x1EAEC8, // R0.5
    FireVoidzoneBig = 0x1EB07B, // R0.5
    ALphaHelper = 0x2EF9,
    BEtaHelper = 0x2EFA,
    CHiHelper = 0x2EFB,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 21423, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->player, no cast, single-target
    Teleport = 26807, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->location, no cast, single-target
    ApplyShieldProtocol1 = 20390, // FlightUnitALpha->self, 5.0s cast, single-target
    ApplyShieldProtocol2 = 20392, // FlightUnitCHi->self, 5.0s cast, single-target
    ApplyShieldProtocol3 = 20391, // FlightUnitBEta->self, 5.0s cast, single-target

    FormationSharpTurn = 20395, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 3.0s cast, single-target
    SharpTurnAlpha1 = 20393, // FlightUnitALpha->self, 9.0s cast, single-target
    SharpTurnAlpha2 = 20394, // FlightUnitALpha->self, 9.0s cast, single-target
    SharpTurnBeta1 = 21777, // FlightUnitBEta->self, 9.0s cast, single-target
    SharpTurnBeta2 = 21778, // FlightUnitBEta->self, 9.0s cast, single-target
    SharpTurnChi1 = 21779, // FlightUnitCHi->self, 9.0s cast, single-target
    SharpTurnChi2 = 21780, // FlightUnitCHi->self, 9.0s cast, single-target
    SharpTurn1 = 20589, // Helper->self, no cast, range 110 width 30 rect
    SharpTurn2 = 20590, // Helper->self, no cast, range 110 width 30 rect

    FormationSlidingSwipe = 20398, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target
    SlidingSwipeAlphaR = 20396, // FlightUnitALpha->self, 6.0s cast, single-target
    SlidingSwipeAlphaL = 20397, // FlightUnitALpha->self, 6.0s cast, single-target
    SlidingSwipeBetaL = 21774, // FlightUnitBEta->self, 6.0s cast, single-target
    SlidingSwipeChiR = 21775, // FlightUnitCHi->self, 6.0s cast, single-target
    SlidingSwipe1 = 20591, // Helper->self, no cast, range 130 width 30 rect
    SlidingSwipe2 = 20592, // Helper->self, no cast, range 130 width 30 rect

    IncendiaryBarrage = 20399, // Helper->location, 9.0s cast, range 27 circle
    FormationAirRaid = 20400, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target

    StandardSurfaceMissile1 = 20401, // Helper->location, 5.0s cast, range 10 circle
    StandardSurfaceMissile2 = 20402, // Helper->location, 5.0s cast, range 10 circle

    LethalRevolution = 20403, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, range 15 circle

    ManeuverHighPoweredLaserMarker = 20406, // Helper->player, no cast, single-target
    ManeuverHighPoweredLaserVisual = 20404, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target
    ManeuverHighPoweredLaser = 20405, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->players, no cast, range 80 width 14 rect

    ManeuverAreaBombardment = 20407, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target

    GuidedMissile = 20408, // Helper->location, 3.0s cast, range 4 circle
    IncendiaryBombing = 20409, // Helper->location, 5.0s cast, range 8 circle
    SurfaceMissile = 20410, // Helper->location, 3.0s cast, range 6 circle
    AntiPersonnelMissile = 20411, // Helper->player, 5.0s cast, range 6 circle

    SuperiorMobility = 20412, // FlightUnitBEta/FlightUnitCHi/FlightUnitALpha->location, no cast, single-target
    ManeuverMissileCommand = 20413, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 4.0s cast, single-target
    BarrageImpact = 20414, // Helper->self, no cast, range 50 circle

    ManeuverIncendiaryBombing = 20419, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 5.0s cast, single-target
    ManeuverPrecisionGuidedMissile = 20420, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 4.0s cast, single-target
    PrecisionGuidedMissile = 20421, // Helper->players, 4.0s cast, range 6 circle, tankbuster

    ManeuverHighOrderExplosiveBlast = 20415, // FlightUnitALpha/FlightUnitCHi/FlightUnitBEta->self, 4.0s cast, single-target
    HighOrderExplosiveBlastCircle = 20416, // Helper->location, 4.0s cast, range 6 circle
    HighOrderExplosiveBlastCross = 20417, // Helper->self, 1.5s cast, range 20 width 5 cross
}

public enum SID : uint
{
    ShieldProtocolA = 2288, // none->player, extra=0x0
    ShieldProtocolC = 2290, // none->player, extra=0x0
    ShieldProtocolB = 2289 // none->player, extra=0x0
}

public enum IconID : uint
{
    IncendiaryBombing = 23 // player
}

public enum TetherID : uint
{
    ShieldProtocol = 7 // player->FlightUnitCHi/FlightUnitALpha/FlightUnitBEta
}
