namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

public enum OID : uint
{
    Boss = 0x30E6, // R3.4
    SparkArrow = 0x30E7, // R2.0
    FlameArrow = 0x30E8, // R2.0 (gives +2 temperature)
    FrostArrow = 0x30E9, // R2.0
    GlacierArrow = 0x30EA, // R2.0 (gives -2 temperature)
    SwirlingOrb = 0x30EB, // R1.0
    TempestuousOrb = 0x30EC, // R1.0
    BlazingOrb = 0x30ED, // R2.25
    RoaringOrb = 0x30EE, // R1.5
    AvowedAvatar = 0x30EF, // R3.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackSword = 22904, // Boss->player, no cast, single-target
    AutoAttackBow = 22905, // Boss->player, no cast, single-target
    AutoAttackStaff = 22906, // Boss->player, no cast, single-target

    WrathOfBozja = 22901, // Boss->self/player, 5.0s cast, range 60 90-degree cone
    GloryOfBozja = 22902, // Boss->self, 5.0s cast, range 85 circle

    RemoveSword = 22914, // Boss->self, no cast, single-target, visual (change model to default)
    RemoveBow = 22915, // Boss->self, no cast, single-target, visual (change model to default)
    RemoveStaff = 22916, // Boss->self, no cast, single-target, visual (change model to default)
    AllegiantArsenalSword = 22917, // Boss->self, 3.0s cast, single-target, visual (change model)
    AllegiantArsenalBow = 22918, // Boss->self, 3.0s cast, single-target, visual (change model)
    AllegiantArsenalStaff = 22919, // Boss->self, 3.0s cast, single-target, visual (change model)
    InfernalSlash = 22897, // Boss->self, 3.0s cast, range 70 270-degree cone aoe (when changing to sword)
    Flashvane = 22898, // Boss->self, 3.0s cast, range 70 270-degree cone aoe (when changing to bow)
    FuryOfBozja = 22899, // Boss->self, 3.0s cast, range 10 circle aoe (when changing to staff)

    FlamesOfBozja = 22910, // Boss->self, 3.0s cast, single-target, visual (prepare for big flame)
    FlamesOfBozjaExtra = 23353, // Helper->self, 7.0s cast, range 45 width 50 rect, visual?
    FlamesOfBozjaAOE = 22888, // Helper->self, 7.0s cast, range 45 width 50 rect aoe (oneshot)
    HotAndColdBow = 23472, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
    ShimmeringShot = 22911, // Boss->self, 3.0s cast, single-target, visual (creates arrows)
    ChillArrow = 22889, // SparkArrow->self, no cast, range 10 width 10 rect, gives +1 temperature (despite name)
    FreezingArrow = 22890, // FlameArrow->self, no cast, range 10 width 10 rect, gives +2 temperature (despite name)
    HeatedArrow = 22891, // FrostArrow->self, no cast, range 10 width 10 rect, gives -1 temperature (despite name)
    SearingArrow = 22892, // GlacierArrow->self, no cast, range 10 width 10 rect, gives -2 temperature (despite name)

    HotAndColdStaff = 22907, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
    FreedomOfBozja = 22908, // Boss->self, 3.0s cast, single-target, visual (spawn orbs)
    ElementalImpactVisual1 = 22880, // TempestuousOrb/SwirlingOrb->self, 5.0s cast, range 60 circle, visual (proximity)
    ElementalImpactVisual2 = 22882, // BlazingOrb/RoaringOrb->self, 5.0s cast, range 60 circle, visual (proximity)
    ElementalImpact1 = 20377, // Helper->self, no cast, range 60 circle with 20 falloff
    ElementalImpact2 = 20378, // Helper->self, no cast, range 60 circle with 20 falloff
    ElementalImpact3 = 20309, // Helper->self, no cast, range 60 circle with 20 falloff
    ElementalImpact4 = 20310, // Helper->self, no cast, range 60 circle with 20 falloff
    ChillBlast = 22884, // SwirlingOrb->self, 8.0s cast, range 22 circle, gives -1 temperature
    FreezingBlast = 22885, // TempestuousOrb->self, 8.0s cast, range 22 circle, gives -2 temperature
    HeatedBlast = 22886, // BlazingOrb->self, 8.0s cast, range 22 circle, gives +1 temperature
    SearingBlast = 22887, // RoaringOrb->self, 8.0s cast, range 22 circle, gives +2 temperature

    HotAndColdSword = 23471, // Boss->self, 3.0s cast, single-target, visual (applies temperature debuffs)
    BladeOfEntropyHot12 = 22893, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyCold11 = 22894, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyHot11 = 22895, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyCold12 = 22896, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyHot21 = 23397, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyCold22 = 23398, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyHot22 = 23399, // Boss->self, 5.0s cast, range 40 180-degree cone
    BladeOfEntropyCold21 = 23400, // Boss->self, 5.0s cast, range 40 180-degree cone

    UnseenEyeBow = 23476, // Boss->self, 3.0s cast, single-target, visual (show clones for crisscross aoe)
    UnseenEyeStaff = 22912, // Boss->self, 3.0s cast, single-target, visual (show clones for crisscross aoe)
    GleamingArrow = 22900, // AvowedAvatar->self, 6.0s cast, range 60 width 10 rect aoe
    ClearTemperatures = 23332 // Helper->self, no cast, range 60 circle, visual (clear temperatures?)
}

public enum SID : uint
{
    RunningCold1 = 2268, // Boss->player, extra=0x0
    RunningCold2 = 2274, // Boss->player, extra=0x0
    RunningHot1 = 2205, // Boss->player, extra=0x0
    RunningHot2 = 2212, // BlazingOrb/SparkArrow/Boss->player, extra=0x0
}
