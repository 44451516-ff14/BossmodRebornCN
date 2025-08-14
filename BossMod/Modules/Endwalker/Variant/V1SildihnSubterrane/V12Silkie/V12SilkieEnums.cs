namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

public enum OID : uint
{
    Silkie = 0x39EF, // R6.0
    EasternEwer = 0x39F1, // R2.4
    SilkenPuff = 0x39F0, // R1.0
    WaterVoidzone = 0x1E9230, // R0.5
    Cotton = 0x1EB76E, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Silkie->player, no cast, single-target

    CarpetBeater = 30507, // Silkie->player, 5.0s cast, single-target, tankbuster
    TotalWash = 30508, // Silkie->self, 5.0s cast, range 60 circle, raidwide
    DustBluster = 30532, // Silkie->location, 5.0s cast, range 60 circle, knockback 16, away from source

    BracingSuds = 30517, // Silkie->self, 5.0s cast, single-target, applies green status
    ChillingSuds = 30518, // Silkie->self, 5.0s cast, single-target, applies blue status
    SoapsUp = 30519, // Silkie->self, 4.0s cast, single-target, removes color statuses
    FreshPuff = 30525, // Silkie->self, 4.0s cast, single-target, visual (statuses on puffs)
    SoapingSpreeBoss = 30526, // Silkie->self, 6.0s cast, single-target, visual
    SoapingSpreePuff = 30529, // SilkenPuff->self, 6.0s cast, single-target, removes color statuses

    ChillingDuster1 = 30520, // Helper->self, 5.0s cast, range 60 width 10 cross
    ChillingDuster2 = 30523, // Helper->self, 8.5s cast, range 60 width 10 cross
    ChillingDuster3 = 30527, // Helper->self, 7.0s cast, range 60 width 10 cross

    BracingDuster1 = 30521, // Helper->self, 5.0s cast, range 5-60 donut
    BracingDuster2 = 30524, // Helper->self, 8.5s cast, range 5-60 donut
    BracingDuster3 = 30528, // Helper->self, 8.5s cast, range 5-60 donut

    SlipperySoap = 30522, // Silkie->location, 5.0s cast, width 10 rect charge

    SpotRemoverVisual = 30530, // Silkie->self, 3.5s cast, single-target
    SpotRemover = 30531, // Helper->location, 3.5s cast, range 5 circle, spawns voidzone

    SqueakyCleanVisualE = 30509, // Silkie->self, 4.5s cast, single-target
    SqueakyClean1E = 30511, // Helper->self, 6.0s cast, range 60 90-degree cone
    SqueakyClean2E = 30512, // Helper->self, 7.7s cast, range 60 90-degree cone
    SqueakyClean3E = 30513, // Helper->self, 9.2s cast, range 60 225-degree cone

    SqueakyCleanVisualW = 30510, // Silkie->self, 4.5s cast, single-target
    SqueakyClean1W = 30514, // Helper->self, 6.0s cast, range 60 90-degree cone
    SqueakyClean2W = 30515, // Helper->self, 7.7s cast, range 60 90-degree cone
    SqueakyClean3W = 30516, // Helper->self, 9.2s cast, range 60 225-degree cone

    // route 1
    EasternEwers = 30535, // Silkie->self, 4.0s cast, single-target
    BrimOver = 30536, // EasternEwer->self, 3.0s cast, range 4 circle
    Rinse = 30537, // Helper->self, no cast, range 4 circle

    // route 2
    WashOutVisual = 30533, // Silkie->self, 8.0s cast, single-target
    WashOut = 30534, // Helper->self, 8.0s cast, range 60 width 60 rect, knockback 35, dir forward

    // route 3
    Sweep = 30541, // Helper->player, no cast, single-target, hit by broom, knockback 5 away from source

    // route 4
    PuffAndTumbleVisualFirst = 30538, // SilkenPuff->location, 3.0s cast, single-target
    PuffAndTumbleVisualRest = 30539, // SilkenPuff->location, no cast, single-target
    PuffAndTumbleFirst = 30540, // Helper->location, 4.6s cast, range 4 circle
    PuffAndTumbleRest = 30656 // Helper->location, 1.6s cast, range 4 circle
}
