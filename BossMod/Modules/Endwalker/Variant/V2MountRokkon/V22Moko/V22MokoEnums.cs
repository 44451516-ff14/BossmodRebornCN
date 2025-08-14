namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V22Moko;

public enum OID : uint
{
    Moko = 0x3F81, // R6.0, path 1, 3, and 4
    MokoP2 = 0x3FB3, // R3.45
    AshigaruKyuhei1 = 0x3F84, // R1.0
    AshigaruKyuhei2 = 0x1EB8C9, // R0.5
    AncientKatana = 0x3F88, // R1.5
    AshigaruSoheiFast = 0x3F82, // R1.0
    AshigaruSoheiSlow = 0x3F83, // R1.0
    OnisClaw = 0x3F85, // R13.2
    IllComeTengu = 0x3F86, // R2.0
    Spiritflame = 0x3F87, // R2.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 34698, // Moko->player, no cast, single-target
    Teleport = 34223, // Moko->location, no cast, single-target

    KenkiRelease = 34221, // Moko->self, 5.0s cast, range 60 circle raidwide
    AzureAuspice = 34204, // Moko->self, 5.0s cast, range 6-40 donut

    BoundlessAzureVisual = 34205, // Moko->self, 2.4s cast, single-target
    BoundlessAzure = 34206, // Helper->self, 3.0s cast, range 60 width 10 rect
    UpwellFirst = 34207, // Helper->self, 8.0s cast, range 60 width 10 rect
    UpwellRest = 34208, // Helper->self, 1.0s cast, range 60 width 5 rect

    IaiKasumiGiri1 = 34183, // Moko->self, 5.0s cast, range 60 270-degree cone, back safe
    IaiKasumiGiri2 = 34184, // Moko->self, 5.0s cast, range 60 270-degree cone, left safe
    IaiKasumiGiri3 = 34185, // Moko->self, 5.0s cast, range 60 270-degree cone, front safe
    IaiKasumiGiri4 = 34186, // Moko->self, 5.0s cast, range 60 270-degree cone, right safe

    DoubleKasumiGiriFirst1 = 34187, // Moko->self, 11.0s cast, range 60 270-degree cone, back safe
    DoubleKasumiGiriFirst2 = 34188, // Moko->self, 11.0s cast, range 60 270-degree cone, left safe
    DoubleKasumiGiriFirst3 = 34189, // Moko->self, 11.0s cast, range 60 270-degree cone, front safe
    DoubleKasumiGiriFirst4 = 34190, // Moko->self, 11.0s cast, range 60 270-degree cone, right safe

    DoubleKasumiGiriRest1 = 34191, // Moko->self, 1.5s cast, range 60 270-degree cone, back safe
    DoubleKasumiGiriRest2 = 34192, // Moko->self, 1.5s cast, range 60 270-degree cone, left safe
    DoubleKasumiGiriRest3 = 34193, // Moko->self, 1.5s cast, range 60 270-degree cone, front safe
    DoubleKasumiGiriRest4 = 34194, // Moko->self, 1.5s cast, range 60 270-degree cone, right safe

    SoldiersOfDeath = 34195, // Moko->self, 3.0s cast, single-target
    IronRain = 34196, // AshigaruKyuhei->location, 8.0s cast, range 10 circle

    // Route 1
    UntemperedSword = 34216, // Moko->self, 3.0s cast, single-target
    Unsheathing = 34217, // AncientKatana->location, 2.0s cast, range 3 circle
    VeilSever = 34218, // AncientKatana->self, 5.0s cast, range 40 width 5 rect

    // Route 2
    ScarletAuspice = 34200, // Moko->self, 5.0s cast, range 6 circle
    MoonlessNight = 34219, // Moko->self, 3.0s cast, range 60 circle
    Clearout = 34220, // OnisClaw->self, 5.0s cast, range 22 180-degree cone
    BoundlessScarletVisual = 34201, // Moko->self, 2.4s cast, single-target
    BoundlessScarlet = 34202, // Helper->self, 3.0s cast, range 60 width 10 rect
    Explosion = 34203, // Helper->self, 10.0s cast, range 60 width 30 rect

    // Route 3
    TenguYobi = 34209, // Moko->self, 3.0s cast, single-target
    YamaKagura = 34210, // IllComeTengu->self, 9.0s cast, range 40 width 5 rect, knockback 33 dir forward
    GhastlyGraspVisual = 34211, // Moko->self, 11.0s cast, single-target
    GhastlyGrasp = 34212, // Helper->location, 11.0s cast, range 5 circle

    // Route 4
    Spiritspark = 34213, // Moko->self, 3.0s cast, single-target
    Spiritflame = 34214, // Helper->location, 4.0s cast, range 6 circle
    ArmOfPurgatory = 34215, // Helper->player, no cast, single-target, touch spiritflame

    SpearmansOrders = 34197, // Moko->self, 5.5s cast, single-target
    SpearmansOrdersVisual = 34198, // Helper->self, 5.5s cast, range 40 width 40 rect
    SpearpointPushVisual = 34588, // Helper->self, no cast, single-target
    SpearpointPushFast = 34199, // Helper->self, no cast, range 3 width 4 rect
    SpearpointPushSlow = 34514 // Helper->self, no cast, range 2 width 4 rect
}

public enum SID : uint
{
    GiriDirection = 2970 // none->Boss, extra=0x248/0x24A/0x249/0x24B
}
