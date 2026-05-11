namespace BossMod.Dawntrail.Alliance.A32Alexander;

public enum OID : uint
{
    AlexanderResurrected = 0x4D5C,
    Helper = 0x233C, // R0.500, x44, Helper type

    Alxaal = 0x4D66, // R0.500, x1
    Teleporter = 0x1EBFF8, // R0.500, x1, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    LeatherBoundMemo = 0x1EC002, // R0.500, x1, EventObj type
    SpatialRift = 0x1EBFFC, // R0.500, x1, EventObj type
    GordiusSystem = 0x4D5D, // R4.005, x0 (spawn during fight)
}

public enum AID : uint
{
    SavageBlade = 50389, // Alxaal->AlexanderResurrected/GordiusSystem, no cast, single-target
    AutoAttack = 50180, // AlexanderResurrected->player, no cast, single-target
    VorpalBlade = 50390, // Alxaal->AlexanderResurrected/GordiusSystem, no cast, single-target
    BanishgaIV = 50161, // AlexanderResurrected->self, 5.0s cast, range 80 circle
    SpiritsWithin = 50391, // Alxaal->AlexanderResurrected/GordiusSystem, no cast, single-target
    UnknownAbility = 50123, // AlexanderResurrected->location, no cast, single-target
    UrielBlade = 50392, // Alxaal->AlexanderResurrected/GordiusSystem, no cast, single-target

    DivineArrow1 = 50124, // AlexanderResurrected->self, 10.0s cast, single-target
    DivineArrow2 = 50125, // AlexanderResurrected->self, 10.0s cast, single-target
    DivineArrow3 = 50126, // AlexanderResurrected->self, no cast, single-target
    DivineArrow4 = 50128, // AlexanderResurrected->self, no cast, single-target
    DivineArrow5 = 50129, // AlexanderResurrected->self, no cast, single-target
    DivineArrow6 = 50130, // Helper->self, 1.0s cast, range 45 90.000-degree cone
    DivineArrow7 = 50131, // Helper->self, no cast, range 10 circle
    DivineArrow8 = 50132, // Helper->self, no cast, range ?-23 donut
    DivineArrow9 = 50133, // Helper->self, no cast, range ?-36 donut
    DivineArrow10 = 50134, // Helper->self, 13.5s cast, range 10 circle
    DivineArrow11 = 50135, // Helper->self, 11.5s cast, range ?-23 donut
    DivineArrow12 = 50136, // Helper->self, 9.5s cast, range ?-36 donut
    DivineArrow13 = 50137, // Helper->self, 3.5s cast, range 60 width 10 rect
    DivineArrow14 = 50138, // Helper->self, 5.5s cast, range 60 width 10 rect
    DivineArrow15 = 50478, // Helper->self, no cast, range 45 ?-degree cone

    HolyII = 50165, // Helper->location, 5.0s cast, range 6 circle
    BanishgaIV1 = 50163, // Helper->players, 5.0s cast, range 6 circle

    ImpartialRuling1 = 50144, // AlexanderResurrected->self, 6.3+0.7s cast, single-target
    ImpartialRuling2 = 50145, // AlexanderResurrected->self, 6.3+0.7s cast, single-target
    ImpartialRuling3 = 50146, // Helper->self, 7.0s cast, range 50 ?-degree cone
    ImpartialRuling4 = 50147, // Helper->self, 10.0s cast, range 50 ?-degree cone

    CanonizeCoordinates = 50139, // AlexanderResurrected->self, no cast, single-target
    RadiantSacrament = 50140, // AlexanderResurrected->self, 4.0s cast, single-target
    RadiantSacrament1 = 50141, // Helper->self, no cast, range 10 width 10 rect
    DivineSpear = 50142, // AlexanderResurrected->self, 4.0+1.0s cast, single-target
    DivineSpear1 = 50143, // Helper->self, 8.0s cast, ???
    MegaHoly = 50157, // AlexanderResurrected->self, 6.5+0.5s cast, single-target
    MegaHoly1 = 50158, // Helper->players, 7.0s cast, range 6 circle
    MegaHoly2 = 50159, // Helper->players, no cast, range 6 circle
    SacredAssembly = 50148, // AlexanderResurrected->self, 3.0+1.0s cast, single-target
    Activate = 50219, // Helper->self, 5.0s cast, range 3 circle
    PerfectDefense = 50149, // AlexanderResurrected->self, 6.0s cast, single-target
    Holy = 50393, // Alxaal->self, 3.0s cast, range 6 circle
    KarmicShielding = 50686, // GordiusSystem->self, 4.0s cast, single-target
    Repay = 50166, // Helper->player, no cast, single-target
    HolyFlame = 50150, // Helper->location, 3.0s cast, range 5 circle
    Shock = 50152, // GordiusSystem->self, 4.0s cast, range 7 circle
    CircuitShock = 50169, // GordiusSystem->self, 4.0s cast, range ?-18 donut
    DivineJudgment = 50153, // AlexanderResurrected->self, 10.0s cast, range 50 circle
    Reinforcements = 50154, // AlexanderResurrected->self, 2.0+1.0s cast, single-target
    Electrify = 50156, // GordiusSystem->self, 1.0s cast, range 18 circle
    DivineBolt = 50160, // AlexanderResurrected->self/player, 5.0s cast, range 60 width 6 rect
    DivineBolt1 = 50849, // Helper->self, no cast, range 60 width 6 rect
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    VulnerabilityUp = 1789, // Helper/GordiusSystem->player, extra=0x1/0x2/0x3/0x4
    PerfectDefense1 = 5376, // AlexanderResurrected->AlexanderResurrected, extra=0x0
    PerfectDefense2 = 5377, // none->GordiusSystem, extra=0x0
    UnknownStatus = 2766, // none->GordiusSystem, extra=0xF
}

public enum IconID : uint
{
    Icon_m1002_east01_c0g = 693, // AlexanderResurrected->self
    Icon_target_ae_shine_s5x = 215, // player->self
    Icon_m1002_ls_rf_c0g = 700, // AlexanderResurrected->self
    Icon_com_share6m7s_1v = 590, // player->self
    Icon_m1002_north01_c0g = 691, // AlexanderResurrected->self
    Icon_tank_laser_5sec_lockon_c0a1 = 471, // player->self
    Icon_m1002_lf_rs_c0g = 699, // AlexanderResurrected->self
    Icon_m1002_south02_c0g = 696, // AlexanderResurrected->self

}

public enum TetherID : uint
{
    Tether_chn_m1002_c0g = 414, // GordiusSystem->AlexanderResurrected
    Tether_chn_m1002_inst_c0g = 428, // GordiusSystem->AlexanderResurrected
}
