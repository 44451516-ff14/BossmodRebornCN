namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

public enum OID : uint
{
    Boss = 0x2EC4, // R5.29
    ThePuppets1 = 0x2EC5, // R5.29
    ThePuppets2 = 0x2FA5, // R0.5
    ThePuppets3 = 0x2FA4, // R1.0
    Compound2P = 0x2EC6, // R6.0

    CompoundPod = 0x2EC8, // R1.36
    Puppet2P = 0x2EC7, // R6.0
    Towers1 = 0x1EB06D, // R0.5
    Towers2 = 0x1EB06E, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    // P1
    AutoAttackP1 = 21450, // Boss->player, no cast, single-target
    MechanicalLaceration = 20920, // Boss->self, 5.0s cast, range 100 circle
    MechanicalDissection = 20915, // Boss->self, 6.0s cast, range 85 width 11 rect
    MechanicalDecapitation = 20916, // Boss->self, 6.0s cast, range 8-43 donut
    MechanicalContusionVisual = 20917, // Boss->self, 3.0s cast, single-target
    MechanicalContusionAOE = 20919, // Helper->location, 4.0s cast, range 6 circle
    MechanicalContusionSpread = 20918, // Helper->player, 5.0s cast, range 6 circle
    IncongruousSpinVisual = 20913, // ThePuppets1->self, 8.0s cast, single-target
    IncongruousSpin = 20914, // Helper->self, 8.5s cast, range 80 width 150 rect
    MechanicalLacerationPhaseChange = 21461, // Boss->self, no cast, range 100 circle, knockback 10 away from source

    // P2
    AutoAttackP2 = 21490, // Boss->player, no cast, single-target
    Teleport1 = 20899, // Boss->location, no cast, single-target
    Teleport2 = 21347, // Helper->location, no cast, single-target
    Visual1 = 21457, // Helper->self, no cast, single-target, ususally used to spawn P2 puppets
    Visual2 = 21456, // Helper->self, no cast, single-target, used when moving or destroying actors?
    Visual3 = 21459, // Helper->self, no cast, single-target

    CentrifugalSlice = 20912, // Boss->self, 5.0s cast, range 100 circle

    PrimeBladeCircle1 = 21535, // Boss->self, 7.0s cast, range 20 circle
    PrimeBladeCircle2 = 20888, // Boss->self, 10.0s cast, range 20 circle
    PrimeBladeRect1 = 21536, // Boss->self, 7.0s cast, range 85 width 20 rect
    PrimeBladeRect2 = 20889, // Boss->self, 11.0s cast, range 85 width 20 rect
    PrimeBladeDonut1 = 21537, // Boss/Puppet2P->self, 7.0s cast, range 7-43 donut
    PrimeBladeDonut2 = 20890, // Boss->self, 10.0s cast, range 7-43 donut

    RelentlessSpiralVisual = 20905, // Boss->self, 3.5s cast, single-target
    RelentlessSpiral1 = 20906, // Helper->location, 3.5s cast, range 8 circle
    RelentlessSpiral2 = 20939, // Helper->self, 1.0s cast, range 8 circle

    ThreePartsDisdain1 = 20891, // Boss->players, 6.0s cast, range 6 circle, knockback 8, dir forward
    ThreePartsDisdain2 = 20892, // Boss->players, no cast, range 6 circle, knockback 8, dir forward
    ThreePartsDisdain3 = 20893, // Boss->players, no cast, range 6 circle, knockback 12, dir forward

    FourPartsResolveVisual = 20894, // Boss->self, 8.0s cast, single-target
    FourPartsResolveCircle = 20895, // Boss->players, no cast, range 6 circle
    FourPartsResolveRect = 20896, // Boss->self, no cast, range 85 width 12 rect

    CompoundPodR011 = 20900, // Boss->self, 4.0s cast, single-target
    CompoundPodR012 = 20907, // Boss->self, 3.0s cast, single-target

    R012LaserVisual = 20908, // CompoundPod->self, no cast, single-target
    R012LaserTB = 20909, // Helper->player, 5.0s cast, range 6 circle
    R012LaserSpread = 20910, // Helper->players, 5.0s cast, range 6 circle
    R012LaserAOE = 20911, // Helper->location, 4.0s cast, range 6 circle
    R011LaserVisual = 20901, // CompoundPod->self, 11.5s cast, single-target
    R011Laser = 21531, // Helper->self, 1.5s cast, range 70 width 15 rect

    Reproduce = 20897, // Boss->self, 4.0s cast, single-target

    EnergyCompression = 20902, // Boss->self, 4.0s cast, single-target
    Explosion = 20903, // Helper->self, no cast, range 5 circle
    BigExplosion = 20904, // Helper->self, no cast, range 100 circle, tower fail

    ForcedTransfer1 = 20898, // Boss->self, 6.5s cast, single-target
    ForcedTransfer2 = 21562 // Boss->self, 8.5s cast, single-target
}

public enum IconID : uint
{
    Icon1 = 79, // player
    Icon2 = 80, // player
    Icon3 = 81, // player
    Icon4 = 82 // player
}

public enum TetherID : uint
{
    Transfer1 = 116, // Helper->Helper
    Transfer2 = 117 // Helper->Helper
}
