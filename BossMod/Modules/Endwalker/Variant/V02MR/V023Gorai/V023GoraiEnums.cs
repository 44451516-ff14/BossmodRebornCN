namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

public enum OID : uint
{
    Boss = 0x3F5C, // R3.45
    FlameAndSulphurFlame = 0x1EB893, // R0.5
    FlameAndSulphurRock = 0x1EB894, // R0.5
    ShishuWhiteBaboon = 0x3F5D, // R1.02
    BallOfLevin = 0x3F60, // R0.69-2.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 34004, // Boss->player, no cast, single-target
    Teleport = 34003, // Boss->location, no cast, single-target

    FlickeringFlame = 34005, // Boss->self, 3.0s cast, single-target, summons flames
    SulphuricStone = 34006, // Boss->self, 3.0s cast, single-target, summons stones
    FlameAndSulphur = 34007, // Boss->self, 3.0s cast, single-target, summons both

    BrazenBalladSplitting = 34009, // Boss->self, 5.0s cast, single-target, modifies stones and flames, extends flame rect width, turns stones into large aoe
    FallingRockSplit = 34014, // Helper->self, no cast, range 5-16 donut
    FireSpreadSplit = 34011, // Helper->self, no cast, range 46 width 5 rect

    BrazenBalladExpanding = 34008, // Boss->self, 5.0s cast, single-target, modifies stones and flames, moves flame rect out, turns stones to donut aoe
    FireSpreadExpand = 34010, // Helper->self, no cast, range 46 width 10 rect
    FallingRockExpand = 34013, // Helper->self, no cast, range 11 circle

    Unenlightenment = 34048, // Boss->self, 5.0s cast, single-target
    UnenlightenmentAOE = 34049, // Helper->self, no cast, range 60 circle

    ImpurePurgation = 34022, // Boss->self, 3.6s cast, single-target
    ImpurePurgationFirst = 34023, // Helper->self, 4.0s cast, range 60 45-degree cone
    ImpurePurgationSecond = 34024, // Helper->self, 6.0s cast, range 60 45-degree cone

    MalformedPrayer = 34017, // Boss->self, 4.0s cast, single-target, sequentially summons three orange towers
    Burst = 34018, // Helper->self, no cast, range 4 circle
    DramaticBurst = 34019, // Helper->self, no cast, range 60 circle, tower fail

    RousingReincarnation1 = 34015, // Boss->self, 5.0s cast, single-target
    RousingReincarnation2 = 34016, // Helper->player, no cast, single-target

    SpikeOfFlameVisual = 34020, // Boss->self, 2.6s cast, single-target //Baited circle AoEs on each player
    SpikeOfFlameAOE = 34021, // Helper->location, 3.0s cast, range 5 circle

    StringSnapVisual = 34025, // Boss->self, 2.6s cast, single-target
    StringSnap1 = 34026, // Helper->self, 3.0s cast, range 10 circle
    StringSnap2 = 34027, // Helper->self, 5.0s cast, range ?-20 donut
    StringSnap3 = 34028, // Helper->self, 7.0s cast, range ?-30 donut

    TorchingTormentVisual = 34046, // Boss->player, 5.0s cast, single-target, AoE tankbuster
    TorchingTorment = 34047, // Helper->player, no cast, range 6 circle

    //Route 5
    PureShockVisual = 34029, // Boss->self, 2.5s cast, single-target
    PureShock = 34030, // Helper->self, 3.0s cast, range 40 circle
    Pull = 34031, // Helper->player, no cast, single-target, pulls players into octagon
    SelfDestruct = 34033, // ShishuWhiteBaboon->self, 30.0s cast, range 10 circle
    WilyWall = 34032, // ShishuWhiteBaboon->self, 7.0s cast, single-target

    //Route 6
    HumbleHammerVisual = 34038, // Boss->self, 5.0s cast, single-target
    HumbleHammer = 34039, // Helper->location, 3.0s cast, range 3 circle
    ShockVisual1 = 34012, // BallOfLevin->self, 6.0s cast, range 18 circle
    ShockVisual2 = 34035, // BallOfLevin->self, 17.0s cast, range 8 circle
    ShockSmall = 34036, // BallOfLevin->self, no cast, range 8 circle
    ShockLarge = 34037, // BallOfLevin->self, no cast, range 8 circle
    Thundercall = 34034, // Boss->self, 3.0s cast, single-target

    //Route 7
    WorldlyPursuitFirstCW = 34043, // Boss->self, 5.0s cast, range 60 width 20 cross
    WorldlyPursuitFirstCCW = 34042, // Boss->self, 5.0s cast, range 60 width 20 cross
    WorldlyPursuitRest = 34044, // Boss->self, 1.5s cast, range 60 width 20 cross
    FightingSpiritsVisual = 34040, // Boss->self, 5.0s cast, single-target
    FightingSpirits = 34041, // Helper->self, 6.2s cast, range 30 circle
    BiwaBreakerFirst = 34045, // Boss->self, 4.0s cast, range 30 circle
    BiwaBreakerRest = 34513 // Boss->self, no cast, range 30 circle
}

public enum SID : uint
{
    SmallOrb = 2970 // Helper->BallOfLevin, extra=0x261
}

public enum IconID : uint
{
    Tankbuster = 344 // player
}
