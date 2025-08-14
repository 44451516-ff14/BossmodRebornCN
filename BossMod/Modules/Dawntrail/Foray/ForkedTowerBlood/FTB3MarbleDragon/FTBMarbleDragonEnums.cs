namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

public enum OID : uint
{
    MarbleDragon = 0x3974, // R7.6
    DeathwallHelper = 0x46D5, // R0.5
    Deathwall = 0x1EBD51, // R0.5
    Icewind = 0x3976, // R1.0
    IceGolem = 0x398A, // R2.85
    IceSprite = 0x39C2, // R1.04
    GelidGaol = 0x39D9, // R1.0
    WaterPuddleCircle = 0x1EBD52, // R0.5
    WaterPuddleCross = 0x1EBD53, // R0.5
    WaterPuddleTower = 0x1EBD54, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 30059, // Boss->player, no cast, single-target
    AutoAttackAdd = 39462, // IceGolem->player, no cast, single-target
    Teleport = 30060, // Boss->location, no cast, single-target
    Deathwall = 30786, // DeathwallHelper->self, no cast, range 30-40 donut

    ImitationStarVisual = 30705, // Boss->self, 5.0+1,6s cast, single-target, raidwide
    ImitationStar = 40652, // Helper->self, no cast, ???

    DraconiformMotionVisual = 30657, // Boss->self, 4.0+0,8s cast, single-target
    DraconiformMotion1 = 30694, // Helper->self, 4.8s cast, range 60 90-degree cone
    DraconiformMotion2 = 30693, // Helper->self, 4.8s cast, range 60 90-degree cone
    ImitationRainVisual = 30343, // Helper->self, no cast, single-target
    ImitationRain = 30615, // Helper->self, no cast, ???

    ImitationIcicleVisual = 30063, // Boss->self, 3.0s cast, single-target
    ImitationIcicle = 30180, // Helper->self, 7.0s cast, range 8 circle
    BallOfIce = 42773, // Helper->self, 0.5s cast, range 8 circle
    BallOfIceTower = 42774, // Helper->self, 0.5s cast, range 4 circle

    ImitationBlizzardCross = 30228, // Helper->self, 1.0s cast, range 60 width 16 cross
    ImitationBlizzardCircle = 30210, // Helper->self, 1.0s cast, range 20 circle
    ImitationBlizzardTower = 30229, // Helper->self, 4.0s cast, range 4 circle
    ImitationBlizzardTowerFailVisual = 30230, // Helper->self, no cast, single-target
    ImitationBlizzardTowerFail = 30417, // Helper->self, no cast, ???

    DreadDelugeVisual = 30696, // Boss->self, 3.0+2,0s cast, single-target
    DreadDeluge = 30704, // Helper->player, 5.0s cast, single-target
    FrigidTwisterVisual = 30264, // Boss->self, 4.0s cast, single-target
    FrigidTwister = 30415, // Helper->location, no cast, range 5 circle
    WitheringEternity = 30419, // Boss->self, 5.0s cast, single-target

    FrigidDiveVisual1 = 30613, // Boss->self, no cast, single-target
    FrigidDiveVisual2 = 30614, // Boss->self, 7.2+0,8s cast, single-target
    FrigidDive = 37819, // Helper->self, 8.0s cast, range 60 width 20 rect

    FrozenHeart = 37823, // IceGolem->self, 3.0s cast, single-target
    ChangeModelState = 30416, // Boss->self, no cast, single-target
    LifelessLegacyVisual = 30616, // Boss->self, 35.0+1,6s cast, single-target, raidwide
    LifelessLegacy = 37818, // Helper->self, no cast, ???
    Recharge = 30617, // IceSprite->Boss, no cast, single-target, buffs boss for Lifeless Legacy
    WickedWater = 30695, // Boss->self, 4.0s cast, single-target

    LifelessLegacyEnrageVisual = 30061, // Boss->self, 20.0+1,6s cast, single-target, enrage
    LifelessLegacyEnrage1 = 40653, // Helper->self, no cast, ???
    LifelessLegacyEnrage2 = 30062 // Boss->self, no cast, range 60 circle
}

public enum SID : uint
{
    Invincibility = 4410, // none->Boss, extra=0x0
    VulnerabilityDown = 2198, // none->IceGolem, extra=0x0
    WickedWater = 4334, // none->player, extra=0x19
    GelidGaol = 4335, // Helper->player, extra=0x0
    DamageUp = 2550 // IceGolem->IceGolem, extra=0x1
}
