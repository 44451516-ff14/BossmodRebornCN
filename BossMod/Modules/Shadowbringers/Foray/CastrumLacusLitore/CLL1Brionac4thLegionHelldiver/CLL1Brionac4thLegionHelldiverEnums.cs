namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

public enum OID : uint
{
    Boss = 0x2ECC, // R20.0
    FourthLegionHelldiver1 = 0x2ED3, // R3.64
    FourthLegionHelldiver2 = 0x2ED4, // R2.8
    FourthLegionHelldiver3 = 0x2ED5, // R2.8
    LacusLitoreDuplicarius = 0x300A, // R0.5
    TunnelArmor = 0x2ED7, // R7.8
    MagitekCore = 0x2F9A, // R10.0
    Lightsphere = 0x2ECD, // R1.0
    Shadowsphere = 0x2ECE, // R1.5
    FourthLegionSkyArmor = 0x2F4E, // R2.0
    CastrumGate = 0x2ED9, // R2.0
    Helper2 = 0x2ED6,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 21436, // FourthLegionHelldiver1/FourthLegionHelldiver3->player/TunnelArmor, no cast, single-target
    AutoAttack2 = 21446, // Boss->player, no cast, single-target
    AutoAttack3 = 21263, // FourthLegionSkyArmor->player, no cast, single-target
    InfallibleMissile = 21435, // LacusLitoreDuplicarius->player, no cast, single-target, enrage if not both bosses get pulled before arena closes

    // top
    ElectricAnvilVisual = 20956, // Boss->self, 4.0s cast, single-target, tankbuster
    ElectricAnvil = 20957, // Helper->player, 5.0s cast, single-target

    FalseThunder1 = 20943, // Boss->self, 8.0s cast, range 47 130-degree cone
    FalseThunder2 = 20942, // Boss->self, 8.0s cast, range 47 130-degree cone

    AntiWarmachinaWeaponry = 20941, // Boss->self, 5.0s cast, single-target

    LightningShowerVisual = 21444, // Boss->self, 4.0s cast, single-target
    LightningShower = 21445, // Helper->self, 5.0s cast, range 60 circle
    VoltstreamVisual = 20954, // Boss->self, 3.0s cast, single-target
    Voltstream = 20955, // Helper->self, 6.0s cast, range 40 width 10 rect

    EnergyGeneration = 20944, // Boss->self, 3.0s cast, single-target
    Lightburst = 20945, // Lightsphere->self, 2.0s cast, range 5-20 donut
    ShadowBurst = 20946, // Shadowsphere->self, 2.0s cast, range 12 circle
    PoleShiftVisual = 20947, // Boss->self, 8.0s cast, single-target
    PoleShift = 20948, // Helper->Lightsphere/Shadowsphere, no cast, single-target, pull 90 between centers
    MagitekMagnetism = 20949, // Boss->self, 6.0s cast, single-target
    PolarMagnetism = 20953, // Boss->self, 6.0s cast, single-target

    MagnetismKnockback = 20952, // Helper->self, no cast, knockback 30 away from source
    MagnetismPull = 20950, // Helper->self, no cast, pull 30 between centers
    MagneticJolt = 20951, // Helper->self, no cast, ???

    // bottom
    CommandDiveFormation = 20975, // FourthLegionHelldiver1->self, 5.0s cast, single-target
    CommandChainCannon = 20987, // FourthLegionHelldiver1->self, 4.0s cast, single-target
    DiveFormationVisual = 20976, // FourthLegionHelldiver2->TunnelArmor, 11.0s cast, width 6 rect charge
    DiveFormation = 20977, // Helper->TunnelArmor, 11.5s cast, width 10 rect charge
    CommandLinearDive = 20970, // FourthLegionHelldiver1->self, 5.0s cast, single-target
    LinearDive = 20971, // FourthLegionHelldiver2->TunnelArmor, 7.0s cast, width 6 rect charge

    CommandJointAttack = 20978, // FourthLegionHelldiver1->self, 5.0s cast, single-target
    ChainCannonFirst = 20985, // FourthLegionHelldiver1/FourthLegionHelldiver2->self, 2.5s cast, range 60 width 5 rect
    ChainCannonRepeat = 20986, // Helper->self, no cast, range 60 width 5 rect
    SurfaceMissileVisual = 20983, // FourthLegionHelldiver1->self, 3.0s cast, single-target
    SurfaceMissile = 20984, // Helper->location, 3.0s cast, range 6 circle
    CommandInfraredBlast = 20972, // FourthLegionHelldiver1->self, 5.0s cast, single-target
    InfraredBlastVisual = 20973, // FourthLegionHelldiver2->self, 1.0s cast, single-target
    InfraredBlast = 20974, // Helper->player/TunnelArmor, no cast, single-target

    MagitekMissilesVisual = 20990, // FourthLegionHelldiver1->self, 3.0s cast, single-target
    MagitekMissiles = 20991, // Helper->player, 5.0s cast, single-target, tankbuster
    MagitekThunder = 20993, // Helper2->TunnelArmor, no cast, single-target

    CommandSuppressiveFormationVisual = 20981, // FourthLegionHelldiver1->self, 3.0s cast, single-target
    CommandSuppressiveFormation = 20982, // FourthLegionHelldiver2->location, 5.0s cast, width 6 rect charge
    MRVMissileVisual = 20988, // FourthLegionHelldiver1->self, 3.0s cast, single-target
    MRVMissile = 20989, // Helper->self, 5.0s cast, range 60 circle

    AntiMaterielMissileVisual = 20979, // FourthLegionHelldiver3->self, 5.0s cast, single-target
    AntiMaterielMissile = 20980 // Helper->TunnelArmor, 2.0s cast, single-target
}

public enum IconID : uint
{
    PlayerPlus = 231, // player->self
    PlayerMinus = 232, // player->self
    OrbPlus = 162, // Lightsphere->self
    OrbMinus = 163 // Lightsphere->self
}

public enum TetherID : uint
{
    InfraredBlast = 121, // FourthLegionHelldiver2->TunnelArmor/player
    PoleShift = 21, // Shadowsphere->Lightsphere
    Magnetism = 124 // player->Lightsphere
}

public enum SID : uint
{
    FireResistanceDownII = 1255 // Helper->TunnelArmor/player, extra=0x0
}
