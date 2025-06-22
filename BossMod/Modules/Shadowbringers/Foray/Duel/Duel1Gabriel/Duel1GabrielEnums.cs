namespace BossMod.Shadowbringers.Foray.Duel.Duel1Gabriel;

public enum OID : uint
{
    Boss = 0x2DB7, // R4.0
    Deathwall = 0x1EB02D, // R0.5
    DeathwallHelper = 0x2EE8, // R0.5
    MagitekCannonVoidzone = 0x1EB054, // R0.5
    MagitekMissile = 0x2DB8, // R1.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 21329, // Boss->player, no cast, single-target
    Teleport = 20112, // Boss->location, no cast, single-target
    ElectricEntrapment = 20868, // DeathwallHelper->self, no cast, range 15-20 donut

    MagitekCannon = 20108, // Boss->self, 2.0s cast, single-target
    MagitekCannonFirst = 20109, // Boss->location, 2.0s cast, range 3 circle
    MagitekCannonRest = 20110, // Boss->location, 2.0s cast, range 3 circle
    DynamicSensoryJammer = 21396, // Boss->self, 3.0s cast, range 40 circle, apply extreme caution
    MissileLauncherVisual1 = 20114, // Boss->self, no cast, single-target
    MissileLauncherVisual2 = 21479, // Boss->self, no cast, single-target
    MissileLauncher = 20115, // Helper->location, 3.0s cast, range 4 circle

    IntegratedScanner = 20113, // Boss->self, 4.0s cast, single-target

    EnhancedMobility = 20111, // Boss->player, 4.0s cast, single-target, knockback 12, away from source
    Burst = 21480, // Helper->location, 5.5s cast, range 4 circle
    BigBurst = 21481, // Helper->location, no cast, range 60 circle, tower fail

    CruiseMissile = 20116, // Boss->self, 2.0s cast, single-target
    InfraredHomingMissilePrey = 21397, // Boss->self, no cast, range 40 circle, apply prey, damage fall of aoe, optimal around 15
    InfraredHomingMissile = 21398, // Helper->location, 6.0s cast, range 60 circle
    MissileActivation = 10758, // MagitekMissile->self, no cast, single-target

    AreaBombardment = 21482 // Boss->self, 5.0s cast, single-target, enrage sequence start
}

public enum SID : uint
{
    ExtremeCaution = 1269, // Boss->player, extra=0x0
    Prey = 1253, // Boss->player, extra=0x0
    FullyAnalyzed = 2357, // none->player, extra=0x0
    LeftUnseen = 1708, // none->player, extra=0x58
    BackUnseen = 1709, // none->player, extra=0x56
    RightUnseen = 1707 // none->player, extra=0x57
}

public enum IconID : uint
{
    MagitekCannon = 197 // player->self
}
