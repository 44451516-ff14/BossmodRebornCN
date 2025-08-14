namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

public enum OID : uint
{
    ZelessGah = 0x39A9, // R9.0
    InfernBrand1 = 0x39AD, // R2.0
    InfernBrand2 = 0x39AB, // R2.0
    MyrrhIncenseBurner = 0x1EB7CF, // R2.0
    BallOfFire = 0x39AF, // R1.0
    ArcaneFont = 0x39B1, // R0.5-1.0
    Portal = 0x1EB761, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // ZelessGah->player, no cast, single-target
    Teleport = 29886, // ZelessGah->location, no cast, single-target

    Burn = 29839, // BallOfFire->self, 1.5s cast, range 12 circle

    CastShadowVisual = 29850, // ZelessGah->self, 4.8s cast, single-target
    CastShadow1 = 29851, // Helper->self, 5.5s cast, range 65 30-degree cone
    CastShadow2 = 29853, // Helper->self, 7.5s cast, range 65 30-degree cone

    CrypticPortal1 = 29842, // ZelessGah->self, 5.0s cast, single-target
    CrypticPortal2 = 29843, // ZelessGah->self, 8.0s cast, single-target

    FiresteelFracture = 29868, // ZelessGah->self/player, 5.0s cast, range 40 90-degree cone, tankbuster
    InfernBrand = 29841, // ZelessGah->self, 4.0s cast, single-target

    InfernWard = 29846, // ZelessGah->self, 4.0s cast, single-target
    PureFireVisual = 29855, // ZelessGah->self, 3.0s cast, single-target
    PureFire = 29856, // Helper->location, 3.0s cast, range 6 circle
    ShowOfStrength = 29870, // ZelessGah->self, 5.0s cast, range 65 circle

    ActivateIncense = 29845, // Helper->InfernBrand1/InfernBrand2, no cast, single-target
    ActivateLaser = 29847, // Helper->self, no cast, single-target

    TrespassersPyre = 29848, // Helper->player, 1.0s cast, single-target
    TrespassersPyreKB = 29890, // Helper->self, no cast, range 60 width 100 rect, pull 50 between centers

    // route 5
    InfernGaleVisual = 29858, // ZelessGah->self, 4.0s cast, single-target
    InfernGale = 29859, // Helper->player, no cast, single-target, knockback 20, away from source   

    // route 6
    BlazingBenifice = 29861, // ArcaneFont->self, 1.5s cast, range 100 width 10 rect

    // route 7
    InfernWellVisual = 29863, // ZelessGah->self, 4.0s cast, single-target
    InfernWell = 29864, // Helper->player, no cast, single-target, pull 15 between hitboxes
    InfernWellAOE = 29866 // Helper->self, 1.5s cast, range 8 circle
}

public enum SID : uint
{
    MechanicStatus = 2397 // none->InfernBrand1/InfernBrand2, extra=0x1CA/0x1CB/0x1C1/0x1F3 - 0x1CA = knockback, 0x1CB = pull, 0x1CE/0x1F3 = laser wall
}
