namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V15ThorneKnight;

public enum OID : uint
{
    ThorneKnight = 0x3917, // R7.2
    AmaljaaArtilleryCarriage = 0x394A, // R0.5
    BallOfFire = 0x3989, // R1.5
    MagickedPuppet = 0x3949, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // ThorneKnight->player, no cast, single-target

    Slashburn1 = 28911, // ThorneKnight->self, 8.5s cast, single-target
    Slashburn2 = 28912, // ThorneKnight->self, 8.5s cast, single-target
    Slashburn3 = 28913, // ThorneKnight->self, 8.5s cast, single-target
    Slashburn4 = 28930, // ThorneKnight->self, 8.5s cast, single-target
    BlazingBeacon1 = 28921, // Helper->self, 8.5s cast, range 50 width 16 rect
    BlazingBeacon2 = 28928, // Helper->self, 8.5s cast, range 50 width 16 rect
    BlazingBeaconFake = 28926, // Helper->self, 6.5s cast, range 50 width 16 rect
    SacredFlay1 = 28922, // Helper->self, 8.5s cast, range 50 45-degree cone
    SacredFlay2 = 28929, // Helper->self, 8.5s cast, range 50 45-degree cone
    SacredFlayFake = 28927, // Helper->self, 6.5s cast, range 50 45-degree cone

    BlisteringBlow = 28906, // ThorneKnight->player, 5.0s cast, single-target, tankbuster
    Cogwheel = 28907, // ThorneKnight->self, 5.0s cast, range 50 circle
    Explosion = 28925, // BallOfFire->self, 7.0s cast, range 50 width 6 cross
    ForeHonor = 28908, // ThorneKnight->self, 6.0s cast, range 50 180-degree cone

    SpringToLife1 = 28909, // ThorneKnight->self, 7.5s cast, single-target
    SpringToLife2 = 28910, // ThorneKnight->self, 7.5s cast, single-target
    MagicCannon = 28919, // MagickedPuppet->self, no cast, range 45 width 6 rect
    AmaljaaArtillery = 28920, // AmaljaaArtilleryCarriage->self, no cast, range 45 width 6 rect
    ActivatePuppetVisual = 28966, // MagickedPuppet->self, no cast, single-target
    DeactivatePuppetVisual = 28967, // MagickedPuppet->self, no cast, single-target

    BlazeOfGlory = 28916, // ThorneKnight->self, 3.0s cast, single-target
    SignalFlareVisual = 28917, // ThorneKnight->self, 5.5s cast, single-target
    SignalFlare = 28923 // Helper->self, 7.0s cast, range 10 circle
}
