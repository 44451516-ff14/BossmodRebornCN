namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

public enum OID : uint
{
    Boss = 0x2EA2, // R17.1
    FlightUnit = 0x2ECB, // R2.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 20924, // Helper->player, no cast, single-target

    ManeuverBeamCannons = 20595, // Boss->self, 12.0s cast, single-target
    BeamCannons1 = 20596, // Helper->self, no cast, range 40 30-degree cone
    BeamCannons2 = 20597, // Helper->self, no cast, range 40 60-degree cone
    BeamCannons3 = 20598, // Helper->self, no cast, range 40 90-degree cone

    ChangeRotation1 = 20599, // Boss->self, no cast, single-target
    ChangeRotation2 = 20600, // Boss->self, no cast, single-target
    ChangeRotation3 = 20601, // Boss->self, no cast, single-target
    ChangeRotation4 = 20602, // Boss->self, no cast, single-target

    ManeuverColliderCannons1 = 20603, // Boss->self, 7.0s cast, single-target
    ManeuverColliderCannons2 = 20604, // Boss->self, 7.5s cast, single-target
    ManeuverColliderCannons3 = 20605, // Boss->self, 8.0s cast, single-target
    ColliderCannons = 20606, // Helper->self, no cast, range 40 30-degree cone

    ManeuverRefractionCannons1 = 20607, // Boss->self, 6.0s cast, single-target
    ManeuverRefractionCannons2 = 20608, // Boss->self, 6.0s cast, single-target
    RefractionCannons = 20609, // Helper->self, no cast, range 40 36-degree cone

    FlightPath = 20620, // FlightUnit->self, 3.0s cast, range 60 width 10 rect

    FiringOrderAntiPersonnelLaser = 20621, // Boss->self, 3.0s cast, single-target, tankbuster
    AntiPersonnelLaser = 20624, // Helper->players, no cast, range 3 circle

    FiringOrderSurfaceLaser = 20622, // Boss->self, 3.0s cast, single-target
    FiringOrderHighPoweredLaser = 20623, // Boss->self, 3.0s cast, single-target

    SurfaceLaserLock = 20625, // Helper->location, no cast, single-target
    SurfaceLaser = 20626, // Helper->location, no cast, range 4 circle
    HighPoweredLaser = 20627, // Helper->players, no cast, range 6 circle, stack

    ManeuverSaturationBombing = 20631, // FlightUnit->self, 25.0s cast, range 60 circle
    ManeuverDiffusionCannon = 20633, // Boss->self, 6.0s cast, range 60 circle

    AerialSupportSwoop = 20690, // Boss->self, 3.0s cast, single-target
    AerialSupportBombardment = 20691, // Boss->self, 3.0s cast, single-target

    LifesLastSongVisual = 21426, // Helper->self, no cast, single-target
    LifesLastSong = 21427 // Helper->self, 7.5s cast, range 30 100-degree cone
}

public enum IconID : uint
{
    SurfaceLaser = 23, // player
    HighPoweredLaser = 62, // player
    AntiPersonnelLaser = 198 // player
}
