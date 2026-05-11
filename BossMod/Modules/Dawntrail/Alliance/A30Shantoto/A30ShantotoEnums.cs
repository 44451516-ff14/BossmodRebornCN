namespace BossMod.Dawntrail.Alliance.A30Shantoto;

public enum OID : uint
{
    ShantottoTheDemon = 0x4D9F, // R4.900, x1
    Helper = 0x233C, // R0.500, x16, Helper type

    Alxaal = 0x4D66, // R0.500, x1
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Unknown1 = 0x4DDD, // R1.000, x21
    Unknown2 = 0x4DDC, // R2.400, x2, Helper type
    SpatialRift = 0x1EBFFB, // R0.500, x1, EventObj type
    Actor1ec087 = 0x1EC087, // R0.500, x0 (spawn during fight), EventObj type
    Teleporter = 0x1EBFF6, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ec086 = 0x1EC086, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ec084 = 0x1EC084, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ec083 = 0x1EC083, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ec085 = 0x1EC085, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ec015 = 0x1EC015, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    // Helpful NPC action set - keeping so that we don't keep rediscovering them
    SavageBlade = 50389, // Alxaal->ShantottoTheDemon, no cast, single-target
    Attack1 = 50721, // Alxaal->ShantottoTheDemon, no cast, single-target
    VorpalBlade = 50390, // Alxaal->ShantottoTheDemon, no cast, single-target
    SpiritsWithin = 50391, // Alxaal->ShantottoTheDemon, no cast, single-target
    UrielBlade = 50392, // Alxaal->ShantottoTheDemon, no cast, single-target
    Holy = 50393, // Alxaal->self, 3.0s cast, range 6 circle

    Attack2 = 872, // ShantottoTheDemon->player, no cast, single-target
    FlarePlay = 50215, // ShantottoTheDemon->self, 5.0s cast, range 78 circle
    Vidohunir = 50213, // ShantottoTheDemon->player, 5.0+1.0s cast, single-target
    Vidohunir1 = 50214, // Helper->players, 6.0s cast, range 5 circle
    EmpiricalResearch = 50206, // ShantottoTheDemon->self, 3.0s cast, single-target
    EmpiricalResearch1 = 50208, // Helper->self, 3.8s cast, range 80 width 12 rect
    SuperiorStoneII = 50193, // ShantottoTheDemon->self, 4.0s cast, range 60 circle
    SuperiorStoneII1 = 50194, // Helper->self, 4.8s cast, range 21 width 13 rect
    SuperiorStoneII2 = 51025, // Helper->self, no cast, range 21 width 13 rect
    GroundbreakingQuake = 50195, // ShantottoTheDemon->self, 8.0s cast, single-target
    GroundbreakingQuake1 = 50196, // Helper->self, 9.0s cast, range 30 width 12 rect
    PainfulPressure = 50197, // Helper->self, no cast, ???
    UnknownAbility1 = 50192, // ShantottoTheDemon->location, no cast, single-target
    DiagrammaticDoorway = 50200, // ShantottoTheDemon->self, 3.0s cast, single-target
    DaintyStep = 50205, // ShantottoTheDemon->location, no cast, single-target
    CircumscribedFire = 50201, // ShantottoTheDemon->self, 7.0s cast, range ?-70 donut
    CircumscribedFire1 = 50202, // ShantottoTheDemon->self, 1.0s cast, range 6-70 donut
    LocalizedBlizzard = 50203, // ShantottoTheDemon->self, 2.2s cast, range 10 circle
    ThunderAndError = 50204, // Helper->players, 5.0s cast, range 5 circle
    MeteoricRhyme = 50182, // ShantottoTheDemon->self, 3.0s cast, single-target
    SmallSpecimen = 50184, // Helper->location, 4.0s cast, range 6 circle
    LargeSpecimen = 50186, // Unknown2->self, 6.0s cast, range 50 circle
    StardustSpecimen = 50185, // Helper->players, 6.0s cast, range 6 circle
    UnknownSpell1 = 50183, // ShantottoTheDemon->location, no cast, range 30 circle
    UnknownAbility2 = 50648, // ShantottoTheDemon->location, no cast, single-target
    Shockwave = 50187, // Helper->self, 10.5s cast, range 48 width 60 rect
    FallingRubble = 50191, // Helper->self, 4.5s cast, range 35 width 10 rect
    FallingRubble1 = 50189, // Helper->self, 4.5s cast, range 12 circle
    FallingRubble2 = 50188, // Helper->self, 4.5s cast, range 8 circle
    FallingRubble3 = 50190, // Helper->self, 4.5s cast, range 25 width 6 rect
    AeroDynamics = 50198, // ShantottoTheDemon->self, 3.0s cast, single-target
    AeroDynamics1 = 50199, // Helper->self, no cast, range 48 width 60 rect
    UnknownSpell2 = 50382, // Helper->self, no cast, range 48 width 60 rect
    FinalExam = 50210, // ShantottoTheDemon->player, 4.2+0.8s cast, single-target
    FinalExam1 = 50211, // Helper->player, 5.0s cast, range 6 circle
}

public enum IconID : uint
{
    Icon_m0926trg_t0a1 = 570, // player->self
    Icon_target_ae_5m_s5_0k2 = 558, // player->self
    Icon_com_share3_6s0p = 318, // player->self
    Icon_com_s8count05x = 713, // player/Alxaal->self
    Icon_com_share4a1 = 305, // player->self
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/ShantottoTheDemon->player, extra=0x1/0x2/0x3
    UnknownStatus1 = 2552, // ShantottoTheDemon->ShantottoTheDemon/Unknown2, extra=0x475
    WesterlyWinds = 5399, // none->player, extra=0x0
    EasterlyWinds = 5398, // none->player/Alxaal, extra=0x0
    UnknownStatus2 = 2160, // none->Alxaal, extra=0x3931
}

public enum TetherID : uint
{
    Tether_chn_d1088_c1v = 384, // Unknown1->Unknown1
    Tether_chn_d1088_c0v = 383, // Unknown1->Unknown1
    Tether_chn_mto_tykdn_1v = 382, // Unknown1->Unknown1
}

