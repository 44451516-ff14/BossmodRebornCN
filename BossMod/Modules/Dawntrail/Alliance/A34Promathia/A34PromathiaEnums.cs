namespace BossMod.Dawntrail.Alliance.A34Promathia;

public enum OID : uint
{
    Promathia = 0x4DEE, // R8.000, x1
    EmptyThinker = 0x233C, // R0.500, x30 (spawn during fight), Helper type
    LinkOfPromathia = 0x4DEF, // R2.240, x10

    Alxaal = 0x4D66, // R0.500, x1
    Actor1ec097 = 0x1EC097, // R0.500, x0 (spawn during fight), EventObj type
    EmptyWeeper = 0x4DF2, // R2.000, x0 (spawn during fight)
    EmptyWanderer = 0x4DF1, // R1.200, x0 (spawn during fight)
    EmptyThinker1 = 0x4DF3, // R2.300, x0 (spawn during fight)
    MemoryReceptacle = 0x4DF0, // R2.040, x0 (spawn during fight)
    Actor1ec098 = 0x1EC098, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    SavageBlade = 50389, // Alxaal->Promathia, no cast, single-target
    AutoAttack1 = 45308, // Promathia->player, no cast, single-target
    UnknownAbility1 = 50342, // Promathia->location, no cast, single-target
    VorpalBlade = 50390, // Alxaal->Promathia, no cast, single-target
    AutoAttack2 = 50721, // Alxaal->Promathia, no cast, single-target
    EmptySalvation = 50317, // Promathia->location, 5.0s cast, range 25 circle
    FleetingEternity = 50318, // Promathia->self, 3.5+1.0s cast, single-target
    Explosion = 50320, // EmptyThinker->self, 5.0s cast, range 16 circle
    WheelOfImpregnability = 50321, // Promathia->self, 2.0+1.0s cast, single-target
    WheelOfImpregnability1 = 50323, // EmptyThinker->self, no cast, range 13 circle
    BastionOfTwilight = 50322, // Promathia->self, 2.0+1.0s cast, single-target
    PestilentPenance = 50330, // Promathia->self, 6.4+0.6s cast, single-target
    BastionOfTwilight1 = 50324, // EmptyThinker->self, no cast, range ?-50 donut
    PestilentPenance1 = 50331, // EmptyThinker->self, 7.0s cast, range 50 width 50 rect
    FleetingEternity1 = 50319, // Promathia->self, 3.5+1.0s cast, single-target
    Comet = 50337, // Promathia->self, 4.5+0.5s cast, single-target
    Comet1 = 50338, // EmptyThinker->player, 0.5s cast, range 6 circle
    UnknownAbility2 = 50345, // EmptyThinker->Alxaal, 1.0s cast, single-target
    UnknownAbility3 = 50636, // Alxaal->self, no cast, single-target
    FalseGenesis = 50343, // Promathia->self, 9.5+0.5s cast, single-target
    FalseGenesis1 = 50344, // EmptyThinker->self, 0.5s cast, range 25 circle
    WindsOfPromyvion = 50352, // EmptyThinker1->self, 3.9+0.6s cast, single-target
    WindsOfPromyvion1 = 50353, // EmptyThinker->self, 4.5s cast, range 16 width 6 rect
    WindsOfPromyvion2 = 50460, // EmptyThinker1->self, no cast, single-target
    WindsOfPromyvion3 = 50354, // EmptyThinker->self, 0.6s cast, range 16 width 6 rect
    EmptyBeleaguer = 50351, // EmptyWanderer->self, 6.0s cast, range 6 circle
    AuroralDrape = 50355, // EmptyWeeper->self, 7.0s cast, range 7 width 7 rect
    EmptySeed = 50349, // MemoryReceptacle->self, 5.0s cast, range 10 circle
    UnknownAbility4 = 50455, // Alxaal->self, no cast, single-target
    DeadlyRebirth = 50694, // Promathia->self, 8.0+2.0s cast, single-target
    DeadlyRebirth1 = 50347, // EmptyThinker->self, 2.0s cast, range 50 circle
    EarthboundHeaven = 50333, // Promathia->self, 2.0+1.0s cast, single-target

    MalevolentBlessing1 = 50326, // Promathia->self, 5.7+0.8s cast, single-target
    MalevolentBlessing2 = 50327, // Promathia->self, 5.7+0.8s cast, single-target
    MalevolentBlessing3 = 50328, // EmptyThinker->self, 6.5s cast, range 40 23.000-degree cone
    MalevolentBlessing4 = 50329, // EmptyThinker->self, 6.5s cast, range 50 width 50 rect

    PestilentPenance2 = 50332, // LinkOfPromathia->self, 7.5s cast, range 50 width 5 rect
    InfernalDeliverance = 50334, // Promathia->self, 5.5+1.5s cast, single-target
    InfernalDeliverance1 = 50335, // EmptyThinker->self, 7.0s cast, range 4 circle
    InfernalDeliverance2 = 50565, // EmptyThinker->self, 5.0s cast, range 8 circle
    Meteor = 50339, // Promathia->self, 4.5+0.5s cast, single-target
    Meteor1 = 50340, // EmptyThinker->players, 0.5s cast, range 6 circle
    Meteor2 = 50341, // EmptyThinker->players, 0.5s cast, range 6 circle
    SpiritsWithin = 50391, // Alxaal->Promathia, no cast, single-target
}

public enum SID : uint
{
    Heavy = 1796, // none->player, extra=0x32
    UnknownStatus1 = 2552, // none->Promathia, extra=0x48D/0x458
    VulnerabilityUp = 1789, // EmptyThinker/EmptyWeeper/LinkOfPromathia->player, extra=0x1/0x2/0x3/0x4/0x5/0x7
    DirectionalDisregard = 3808, // none->Promathia, extra=0x0
    UnknownStatus2 = 2056, // none->EmptyWeeper, extra=0x498
    SystemLock = 2578, // none->player, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    UnknownStatus3 = 2160, // none->Alxaal, extra=0x3931
    UnknownStatus4 = 2273, // Promathia->Promathia, extra=0x226
    DownForTheCount = 3908, // EmptyThinker->player/Alxaal, extra=0xEC7
}

public enum IconID : uint
{
    Icon_m1001_lockon_c0w = 687, // Promathia->self
    Icon_m1001_lockon_c1w = 688, // Promathia->self
    Icon_tank_lockonae_6m_5s_01t = 344, // player->self
    Icon_m1001_turning_left01w = 690, // EmptyThinker1->self
    Icon_m1001_turning_right01w = 689, // EmptyThinker1->self
    Icon_loc06sp_05ak1 = 466, // player->self
}

public enum TetherID : uint
{
    Tether_chm_m1001_01w = 427, // Promathia->Alxaal
    Tether_chn_nomal01f = 12, // MemoryReceptacle->Alxaal
}
