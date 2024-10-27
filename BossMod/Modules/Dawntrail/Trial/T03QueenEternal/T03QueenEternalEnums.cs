namespace BossMod.Dawntrail.Trial.T03QueenEternal;

public enum OID : uint
{
    Boss = 0x41C0, // R22.0
    Unknown = 0x41C6, // R1.500, x0 (spawn during fight)
    QueenEternal1 = 0x41C1, // R0.5
    QueenEternal2 = 0x41C2, // R0.5
    QueenEternal3 = 0x41C4, // R1.0
    QueenEternal4 = 0x4477, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36656, // QueenEternal4->player, no cast, single-target

    LegitimateForceLL = 36638, // Boss->self, 8.0s cast, range 60 width 30 rect, L -> L
    LegitimateForceLR = 36639, // Boss->self, 8.0s cast, range 60 width 30 rect, L -> R
    LegitimateForceRR = 36640, // Boss->self, 8.0s cast, range 60 width 30 rect, R -> R
    LegitimateForceRL = 36641, // Boss->self, 8.0s cast, range 60 width 30 rect, R -> L
    LegitimateForceL = 36642, // Boss->self, no cast, range 60 width 30 rect, left second
    LegitimateForceR = 36643, // Boss->self, no cast, range 60 width 30 rect, right second

    AethertitheVisual = 36604, // Boss->self, 3.0s cast, single-target
    AethertitheRaidwide = 36605, // Helper->self, no cast, range 100 circle, many repeated raidwides during aethertithe mechanic
    Aethertithe1 = 36657, // Boss->self, no cast, range 100 70-degree cone, west
    Aethertithe2 = 36658, // Boss->self, no cast, range 100 70-degree cone, center
    Aethertithe3 = 36659, // Boss->self, no cast, range 100 70-degree cone, east

    Coronation = 36629, // Boss->self, 3.0s cast, single-target
    WaltzOfTheRegaliaVisual = 36631, // QueenEternal1->self, no cast, single-target, 
    WaltzOfTheRegalia = 36632, // Helper->self, 1.0s cast, range 14 width 4 rect

    ProsecutionOfWar = 36602, // Boss->player, 5.0s cast, single-target, tankbuster

    LockAndKey = 36630, // Boss->self, no cast, single-target

    VirtualShift1 = 36606, // Boss->self, 5.0s cast, range 100 circle, arena changes to X
    VirtualShift2 = 36607, // Boss->self, 5.0s cast, range 100 circle
    VirtualShift3 = 36608, // Boss->self, 5.0s cast, range 100 circle, arena changes to default

    RuthlessRegalia = 36634, // QueenEternal2->self, no cast, range 100 width 12 rect

    DownburstVisual = 36609, // Boss->self, 6.0s cast, single-target
    Downburst = 36610, // Helper->self, 6.0s cast, range 100 circle, knockback 10, away from source

    BrutalCrown = 36633, // QueenEternal1->self, 8.0s cast, range 5-60 donut

    PowerfulGustVisual = 36611, // Boss->self, 6.0s cast, single-target
    PowerfulGust = 36612, // Helper->self, 6.0s cast, range 60 width 60 rect, knockback 20, dir forward

    RoyalDomain = 36603, // Boss->location, 5.0s cast, range 100 circle, raidwide

    Castellation = 36613, // Boss->self, 3.0s cast, single-target
    Besiegement1 = 36614, // Helper->self, no cast, range 60 width 4 rect
    Besiegement2 = 36615, // Helper->self, no cast, range 60 width 8 rect
    Besiegement3 = 36616, // Helper->self, no cast, range 60 width 10 rect
    Besiegement4 = 36617, // Helper->self, no cast, range 60 width 12 rect
    Besiegement5 = 36618, // Helper->self, no cast, range 60 width 18 rect

    AbsoluteAuthorityVisual = 36619, // Boss->self, no cast, single-target
    AbsoluteAuthorityRaidwide1 = 39531, // Boss->self, 19.0s cast, range 100 circle
    AbsoluteAuthorityRaidwide2 = 36620, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityRaidwide3 = 36621, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityRaidwide4 = 36627, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityRaidwide5 = 36628, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityCircle = 36622, // Helper->self, 3.8s cast, range 8 circle
    AbsoluteAuthorityGaze = 36624, // Helper->players, no cast, range 100 circle
    AbsoluteAuthorityFlare = 39518, // Helper->players, no cast, range 100 circle
    AbsoluteAuthorityDoritoStack1 = 39519, // Helper->player, no cast, single-target, dorito stack success
    AbsoluteAuthorityDoritoStack2 = 39520, // Helper->player, no cast, single-target, dorito stack fail

    DivideAndConquerVisual = 36636, // Boss->self, 5.0s cast, single-target, baited line AOEs from the boss
    DivideAndConquer = 36637, // Helper->self, no cast, range 100 width 5 rect

    MorningStars = 39134, // Helper->player, no cast, single-target

    DynasticDiademVisual = 40058, // Boss->self, 5.0s cast, single-target
    DynasticDiadem = 40059, // Helper->self, 5.0s cast, range 6-70 donut

    RoyalBanishmentVisual = 36644, // Boss->self, 3.0s cast, single-target
    RoyalBanishmentRaidwide = 36645, // Helper->self, no cast, range 100 circle
    RoyalBanishment = 36647 // Helper->self, 3.5s cast, range 100 30-degree cone
}

public enum SID : uint
{
    AuthoritysGaze = 3815, // none->player, extra=0x0
    GravitationalAnomaly = 3814, // none->player, extra=0x15E
    AuthoritysHold = 4130 // none->player, extra=0x0
}

public enum IconID : uint
{
    DoritoStack = 55, // player
    AccelerationBomb = 75, // player
    Flare = 327, // player
    LineBaits = 521 // Boss
}
