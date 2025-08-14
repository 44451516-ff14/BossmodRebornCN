namespace BossMod.Shadowbringers.Alliance.A23HeavyArtilleryUnit;

public enum OID : uint
{
    Boss = 0x2E6A, // R4.0
    Energy = 0x2E6C, // R1.0
    ArenaFeatures = 0x1EA1A1, // R2.0
    Pod = 0x2E6D, // R0.8
    Tower = 0x1EB055, // R0.5
    Helper2 = 0x18D6,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 21561, // Boss->player, no cast, single-target

    ManeuverVoltArray = 20486, // Boss->self, 4.0s cast, range 60 circle

    LowerLaserFirstCone = 20614, // Helper->self, 1.8s cast, range 30 60-degree cone
    UpperLaserFirstSector1 = 20615, // Helper->self, 1.8s cast, range 6-16 60-degree donut sector
    UpperLaserFirstSector2 = 20616, // Helper->self, 4.8s cast, range 14-23 60-degree donut sector
    UpperLaserFirstSector3 = 20617, // Helper->self, 7.8s cast, range 21-30 60-degree donut sector
    LowerLaserRepeatCone = 20470, // Helper->self, no cast, range 60 60-degree cone
    UpperLaserRepeatSector1 = 20471, // Helper->self, no cast, range 6-16 60-degree donut sector
    UpperLaserRepeatSector2 = 20472, // Helper->self, no cast, range 14-23 60-degree donut sector
    UpperLaserRepeatSector3 = 20473, // Helper->self, no cast, range 21-30 60-degree donut sector

    EnergyBomb = 20474, // Energy->player, no cast, single-target
    EnergyBombardmentVisual = 20475, // Boss->self, 2.0s cast, single-target
    EnergyBombardment = 20476, // Helper->location, 3.0s cast, range 4 circle
    ManeuverImpactCrusherVisual1 = 20477, // Boss->self, 4.0s cast, single-target
    ManeuverImpactCrusherVisual2 = 20479, // Helper->location, 4.0s cast, range 8 circle
    ManeuverImpactCrusher = 20478, // Boss->location, no cast, range 8 circle
    ManeuverRevolvingLaser = 20480, // Boss->self, 3.0s cast, range 12-60 donut

    ManeuverHighPoweredLaserVisual = 20481, // Boss->self, 5.0s cast, single-target
    ManeuverHighPoweredLaser = 20482, // Boss->self, no cast, range 60 width 8 rect

    ManeuverUnconventionalVoltage = 20483, // Boss->self, 6.0s cast, single-target
    UnconventionalVoltageMarker = 20485, // Helper->player, no cast, single-target
    UnconventionalVoltage = 20484, // Helper->self, no cast, range 60 30-degree cone

    OperationSynthesizeCompound = 20460, // Boss->self, 3.0s cast, single-target
    OperationActivateLaserTurret = 20461, // Boss->self, 6.0s cast, single-target
    OperationActivateSuppressiveUnit = 20462, // Boss->self, 6.0s cast, single-target
    OperationAccessSelfConsciousnessData = 20463, // Boss->self, 8.0s cast, single-target
    R010Laser = 20464, // Pod->self, 10.0s cast, range 60 width 12 rect
    R030Hammer = 20465, // Pod->self, 10.0s cast, range 18 circle

    ChemicalBurn = 20468, // Helper->self, no cast, range 3 circle
    ChemicalConflagration = 20469, // Helper->self, no cast, range 60 circle, tower fail

    SupportPod = 20457, // Boss->self, 2.0s cast, single-target
    OperationPodProgram = 20458 // Boss->self, 6.0s cast, single-target
}

public enum IconID : uint
{
    ManeuverHighPoweredLaser = 230, // player
    UnconventionalVoltage = 172 // player
}
