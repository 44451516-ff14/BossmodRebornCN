namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

public enum OID : uint
{
    UltimaTheFeared = 0x4919, // R16.0
    OmegaTheOne = 0x4918, // R9.0
    ManaScreen1 = 0x4947, // R1.0
    ManaScreen2 = 0x1EBE8B, // R0.5
    ManaScreen3 = 0x1EBE8C, // R0.5
    EnergyOrb = 0x491A, // R1.0
    UltimaHelper = 0x49AF, // R0.0-0.5
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackUltima = 44317, // UltimaHelper->player, no cast, single-target
    AutoAttackOmega = 45308, // OmegaTheOne->player, no cast, single-target
    Teleport = 44332, // OmegaTheOne->location, no cast, single-target

    EnergizingEquilibrium1 = 44316, // UltimaTheFeared->OmegaTheOne, no cast, single-target
    EnergizingEquilibrium2 = 44336, // OmegaTheOne->UltimaTheFeared, no cast, single-target

    IonEfflux = 44331, // OmegaTheOne->self, 6.5s cast, range 65 circle, raidwide
    AntimatterVisual = 44304, // UltimaTheFeared->self, 5.0s cast, single-target
    Antimatter = 44305, // Helper->player, 6.0s cast, single-target

    EnergyOrb = 44296, // UltimaTheFeared->self, 3.0s cast, single-target
    EnergyRay = 44338, // EnergyOrb->self, 5.0s cast, range 40 width 16 rect
    ManaScreen = 44297, // UltimaTheFeared->self, 3.0s cast, single-target
    EnergyRayReflect1 = 44300, // Helper->self, no cast, range 48 width 20 rect
    EnergyRayReflect2 = 44301, // Helper->self, no cast, range 48 width 20 rect
    EnergyRayReflect3 = 44298, // Helper->self, no cast, range 40 width 16 rect
    EnergyRayReflect4 = 44299, // Helper->self, no cast, range 40 width 16 rect

    AftToForeFire = 44327, // OmegaTheOne->self, 6.0s cast, single-target
    ForeToAftFire = 44325, // OmegaTheOne->self, 6.0s cast, single-target
    ForewardBlaster = 44326, // OmegaTheOne->self, no cast, single-target
    AftwardBlaster = 44328, // OmegaTheOne->self, no cast, single-target
    OmegaBlasterFirst = 44329, // Helper->self, 6.5s cast, range 50 180-degree cone
    OmegaBlasterSecond = 44330, // Helper->self, 8.8s cast, range 50 180-degree cone

    TractorBeamVisual = 44294, // UltimaTheFeared->self, 10.0s cast, range 40 width 48 rect
    TractorBeam = 45190, // Helper->self, 10.5s cast, range 40 width 24 rect, pull 25 to -5 between centers
    Crash = 44295, // Helper->self, 10.5s cast, range 40 width 24 rect

    AntiPersonnelMissileVisual = 45191, // OmegaTheOne->self, no cast, single-target
    AntiPersonnelMissile = 45192, // Helper->players, 5.0s cast, range 6 circle, spread
    SurfaceMissileVisual = 45173, // OmegaTheOne->self, 9.0s cast, single-target
    SurfaceMissile = 45174, // Helper->self, 1.0s cast, range 12 width 20 rect

    TrajectoryProjection = 44323, // OmegaTheOne->self, 3.5s cast, single-target
    GuidedMissile = 44324, // Helper->self, 1.0s cast, range 6 circle

    TractorFieldVisual = 44306, // UltimaTheFeared->self, 5.0s cast, single-target
    TractorField = 44307, // Helper->self, 5.5s cast, range 50 circle, pull 50, between centers

    MultiMissileVisual = 45035, // OmegaTheOne->self, 2.1s cast, single-target
    MultiMissileSmall = 45037, // Helper->self, 4.0s cast, range 6 circle
    MultiMissileBig = 45036, // Helper->self, 4.1s cast, range 10 circle

    CitadelSiegeVisual1 = 44308, // UltimaTheFeared->self, no cast, single-target
    CitadelSiegeVisual2 = 44309, // UltimaTheFeared->self, no cast, single-target
    CitadelSiegeVisual3 = 44310, // UltimaTheFeared->self, no cast, single-target
    CitadelSiegeVisual4 = 44311, // UltimaTheFeared->self, no cast, single-target
    CitadelSiege = 44312, // Helper->self, 5.0s cast, range 48 width 10 rect

    CitadelBuster = 44315, // UltimaTheFeared->location, 6.0s cast, range 50 circle
    HyperPulse = 44335, // OmegaTheOne->self, 5.0s cast, range 50 circle, proximity AOE

    ChemicalBombVisual = 44302, // UltimaTheFeared->self, 6.5s cast, single-target
    ChemicalBomb = 44303 // Helper->self, 7.0s cast, range 50 circle, proximity AOE
}

public enum IconID : uint
{
    SurfaceMissile = 616, // Helper->self
    GuidedMissileEast = 617, // player->self
    GuidedMissileWest = 618, // player->self
    GuidedMissileSouth = 619, // player->self
    GuidedMissileNorth = 620 // player->self
}
