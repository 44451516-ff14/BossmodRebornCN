namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V25Enenra;

public enum OID : uint
{
    Enenra = 0x3EAD,
    EnenraClone = 0x3EAE, // R2.8
    SmokeVisual1 = 0x1EB88F, // R0.5
    SmokeVisual2 = 0x1EB890, // R0.5
    SmokeVisual3 = 0x1EB891, // R0.5
    Smoldering = 0x1EB892, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Enenra/EnenraClone->player, no cast, single-target
    Teleport1 = 32846, // Enenra/EnenraClone->location, no cast, single-target
    Teleport2 = 33986, // Enenra/EnenraClone->location, no cast, single-target
    Teleport3 = 32837, // EnenraClone->location, no cast, single-target

    KiseruClamor = 32840, // Enenra/EnenraClone->location, 3.0s cast, range 6 circle, cascading earthquake AoEs
    BedrockUplift1 = 32841, // Helper->self, 5.0s cast, range 6-12 donut
    BedrockUplift2 = 32842, // Helper->self, 7.0s cast, range 12-18 donut
    BedrockUplift3 = 32843, // Helper->self, 9.0s cast, range 18-24 donut

    ClearingSmokeVisual = 32866, // EnenraClone/Enenra->self, 8.0s cast, single-target
    ClearingSmoke = 32850, // Helper->self, 11.5s cast, range 21 circle, knockback
    SmokeRingsVisual = 32867, // EnenraClone/Enenra->self, 8.0s cast, single-target
    SmokeRings = 32851, // Helper->self, 11.5s cast, range 16 circle

    FlagrantCombustion = 32834, // Enenra/EnenraClone->self, 5.0s cast, range 50 circle, raidwide

    OutOfTheSmoke = 32844, // Enenra/EnenraClone->self, 12.0s cast, single-target
    IntoTheFireVisual = 32845, // Enenra/EnenraClone->self, 1.0s cast, single-target
    IntoTheFire = 32856, // Helper->self, 1.5s cast, range 50 width 50 rect, frontal cleave

    PipeCleaner = 32852, // Enenra->self, 5.0s cast, single-target
    PipeCleanerAOE = 32853, // Enenra->self, no cast, range 50 width 6 rect, tethers one player and hits them with a line AoE.

    SmokeAndMirrors = 32835, // Enenra->self, 2.5s cast, single-target

    SmokeStackBoss = 32838, // Enenra->location, 2.0s cast, single-target, recombine
    SmokeStackClone = 32839, // EnenraClone->location, 2.0s cast, single-targe, recombine

    Smoldering = 32848, // Helper->self, 7.0s cast, range 8 circle
    SmolderingDamnation = 32847, // Enenra->self, 4.0s cast, single-target

    Snuff = 32854, // Enenra->player, 5.0s cast, range 6 circle, AoE tankbuster and baited AOE
    Uplift = 32855 // Helper->location, 3.5s cast, range 6 circle
}

public enum TetherID : uint
{
    PipeCleaner = 17 // player->Enenra
}
