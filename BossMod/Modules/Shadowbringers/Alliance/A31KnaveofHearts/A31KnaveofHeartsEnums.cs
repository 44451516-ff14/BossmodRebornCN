namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

public enum OID : uint
{
    Boss = 0x31B8, // R30.0
    Spheroid = 0x322D, // R1.0
    CopiedKnave = 0x322C, // R30.0
    Energy = 0x322E, // R1.0
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    BladeFlurry1 = 23788, // Ally2B->Boss, no cast, single-target
    BladeFlurry2 = 23789, // Ally2B->Boss, no cast, single-target
    BossAutoAttack = 24668, // Boss->player, no cast, single-target
    DancingBlade = 23790, // Ally2B->Boss, no cast, width 2 rect charge
    Roar = 24245, // Boss->self, 5.0s cast, range 80 circle
    BalancedEdge = 23791, // Ally2B->self, 2.0s cast, range 5 circle
    WhirlingAssault = 23792, // Ally2B->self, 2.0s cast, range 40 width 4 rect

    ColossalImpactVisual1 = 24228, // Boss/CopiedKnave->self, 7.0s cast, single-target
    ColossalImpactVisual2 = 23805, // Boss->self, 8.5s cast, single-target
    ColossalImpact1 = 24774, // Helper->self, 9.5s cast, range 61 width 20 rect
    ColossalImpact2 = 24775, // Helper->self, 9.5s cast, range 61 width 20 rect
    ColossalImpact3 = 24776, // Boss->self, 9.5s cast, range 61 width 20 rect
    ColossalImpact4 = 24229, // Helper->self, 8.0s cast, range 61 width 20 rect, left
    ColossalImpact5 = 24230, // Helper->self, 8.0s cast, range 61 width 20 rect, right
    ColossalImpact6 = 24231, // Boss/CopiedKnave->self, 8.0s cast, range 61 width 20 rect, middle

    MagicArtilleryBetaVisual = 24242, // Boss->self, 3.0s cast, single-target
    MagicArtilleryBeta = 24243, // Helper->player, 5.0s cast, range 3 circle, tankbuster

    MagicArtilleryAlphaVisual = 24234, // Boss->self, 3.0s cast, single-target
    MagicArtilleryAlpha = 24235, // Helper->players, 6.0s cast, range 5 circle, spread

    StackingTheDeckVisual = 24816, // CopiedKnave->self, 6.0s cast, single-target
    StackingTheDeck = 23801, // Boss->self, 6.0s cast, single-target

    LightLeapVisual = 24238, // Boss->self, 7.0s cast, single-target
    LightLeap = 24239, // Helper->location, 8.5s cast, range 40 circle

    Replicate = 24233, // Boss->self, 3.0s cast, single-target
    Spheroids = 24232, // Boss->self, 4.0s cast, single-target
    KnavishBullets = 24237, // Spheroid->self, no cast, single-target
    Burst = 24244, // Energy->player, no cast, single-target
    BoxSpawn = 24240, // Helper->self, 4.0s cast, range 8 width 8 rect

    Lunge = 24241, // Boss/CopiedKnave->self, 8.0s cast, range 61 width 60 rect, knockback 60 dirforward
    MagicBarrage = 24236 // Spheroid->self, 6.0s cast, range 61 width 5 rect
}

public enum SID : uint
{
    TheHeatOfBattle = 365, // none->player, extra=0xA
    MeatAndMead = 360, // none->player, extra=0xA
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Boss/Helper/CopiedKnave/Energy/Spheroid->player, extra=0x1/0x2/0x3
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Icon279 = 279, // CopiedKnave
    Icon278 = 278, // Boss
    Icon169 = 169, // player
    Icon280 = 280, // CopiedKnave
}

public enum TetherID : uint
{
    Tether152 = 152, // CopiedKnave->Boss
}
