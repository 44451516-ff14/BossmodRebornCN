namespace BossMod.Dawntrail.Trial.T06Arkveld;

public enum OID : uint
{
    GuardianArkveld = 0x48E2,
    Helper = 0x233C,
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    _Gen_CrackedCrystal = 0x48E3, // R0.700, x0 (spawn during fight)
    _Gen_CrackedCrystal1 = 0x48E4, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    Roar = 43886, // GuardianArkveld->self, 5.0s cast, range 60 circle
    ChainbladeBlow = 43828, // GuardianArkveld->self, 5.0s cast, single-target
    ChainbladeBlow1 = 43830, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlow2 = 43831, // Helper->self, 6.6s cast, range 40 width 4 rect
    WyvernsRadiance = 43832, // Helper->self, 7.2s cast, range 80 width 28 rect
    ChainbladeBlow3 = 43829, // GuardianArkveld->self, 5.0s cast, single-target
    ChainbladeBlow4 = 45050, // Helper->self, 6.2s cast, range 40 width 4 rect
    ChainbladeBlow5 = 45051, // Helper->self, 6.6s cast, range 40 width 4 rect
    WyvernsRadiance1 = 43833, // Helper->self, 7.2s cast, range 80 width 28 rect
    _AutoAttack_ = 43341, // GuardianArkveld->player, no cast, single-target
    _Weaponskill_ = 45175, // GuardianArkveld->location, no cast, single-target
    GuardianSiegeflight = 43834, // GuardianArkveld->location, 5.0s cast, range 40 width 4 rect
    GuardianSiegeflight1 = 43835, // Helper->self, 6.5s cast, range 40 width 8 rect
    _Weaponskill_1 = 45123, // Helper->self, 7.2s cast, range 40 width 8 rect
    GuardianResonance = 43836, // Helper->self, 10.0s cast, range 40 width 16 rect
    WyvernsRattle = 43876, // GuardianArkveld->self, no cast, single-target
    WyvernsRadiance2 = 43877, // Helper->self, 2.5s cast, range 8 width 40 rect
    WyvernsRadiance3 = 43878, // Helper->self, 1.0s cast, range 8 width 40 rect
    WyvernsSiegeflight = 43837, // GuardianArkveld->location, 5.0s cast, range 40 width 4 rect
    WyvernsSiegeflight1 = 43838, // Helper->self, 6.5s cast, range 40 width 8 rect
    _Weaponskill_2 = 45076, // Helper->self, 7.2s cast, range 40 width 8 rect
    WyvernsRadiance4 = 43839, // Helper->self, 10.0s cast, range 40 width 18 rect
    WyvernsRadiance5 = 43840, // Helper->self, 10.0s cast, range 40 width 18 rect
    _Weaponskill_3 = 43841, // Helper->location, 2.5s cast, width 12 rect charge
    _Weaponskill_4 = 45075, // Helper->self, 3.5s cast, range 8 circle
    Rush = 43842, // GuardianArkveld->location, 6.0s cast, width 12 rect charge
    WyvernsRadiance6 = 43844, // Helper->self, 7.5s cast, range 8 circle
    Rush1 = 43843, // GuardianArkveld->location, no cast, width 12 rect charge
    WyvernsRadiance7 = 43845, // Helper->self, 9.5s cast, range 20-14 donut
    WyvernsRadiance8 = 43846, // Helper->self, 11.5s cast, range 14-20 donut
    WyvernsRadiance9 = 43847, // Helper->self, 13.5s cast, range 20-26 donut
    ChainbladeCharge = 43883, // GuardianArkveld->self, 6.0s cast, single-target
    ChainbladeCharge1 = 43884, // GuardianArkveld->player, no cast, single-target
    ChainbladeCharge2 = 44811, // Helper->location, no cast, range 6 circle
    WyvernsOuroblade = 43848, // GuardianArkveld->self, 6.0+1.5s cast, single-target
    WyvernsOuroblade1 = 43849, // Helper->self, 7.0s cast, range 40 180.000-degree cone
    _Weaponskill_5 = 43827, // GuardianArkveld->location, no cast, single-target
    GuardianResonance1 = 43856, // Helper->location, 4.0s cast, range 6 circle
    AethericResonance = 43852, // GuardianArkveld->self, 11.7+1.3s cast, single-target
    GuardianResonance2 = 43853, // Helper->location, 13.0s cast, range 2 circle
    GuardianResonance3 = 43854, // Helper->location, 13.0s cast, range 4 circle
    _Weaponskill_6 = 43859, // GuardianArkveld->self, no cast, single-target
    WyvernsVengeance = 43860, // Helper->self, 5.0s cast, range 6 circle
    WyvernsVengeance1 = 43861, // Helper->location, no cast, range 6 circle
    WyvernsRadiance10 = 43857, // 48E3->self, 1.4s cast, range 6 circle
    WyvernsRadiance11 = 43858, // 48E4->self, 1.4s cast, range 12 circle
    WyvernsRadiance12 = 44807, // Helper->self, 2.0s cast, range 6 circle
    WyvernsRadiance13 = 44808, // Helper->self, 2.0s cast, range 12 circle
    WildEnergy = 43866, // Helper->players, 8.0s cast, range 6 circle
    ForgedFury = 43869, // GuardianArkveld->self, 5.0s cast, single-target
    ForgedFury1 = 43870, // Helper->self, 7.0s cast, range 60 circle
    ForgedFury2 = 44790, // Helper->self, 7.8s cast, range 60 circle
    ForgedFury3 = 44791, // Helper->self, 10.2s cast, range 60 circle
    Roar1 = 45201, // GuardianArkveld->self, 5.0s cast, range 60 circle
    WyvernsWeal = 45047, // GuardianArkveld->self, 6.5+1.5s cast, single-target
    WyvernsWeal1 = 45049, // Helper->self, 8.0s cast, range 60 width 6 rect
    WyvernsWeal2 = 43875, // Helper->self, no cast, range 60 width 6 rect
    _Weaponskill_7 = 43873, // GuardianArkveld->self, no cast, single-target
    WyvernsSiegeflight2 = 45068, // GuardianArkveld->location, 4.0s cast, range 40 width 4 rect
    WyvernsSiegeflight3 = 45071, // Helper->self, 5.5s cast, range 40 width 8 rect
    _Weaponskill_8 = 45074, // Helper->self, 6.2s cast, range 40 width 8 rect
    WyvernsRadiance14 = 45073, // Helper->self, 9.0s cast, range 40 width 18 rect
    WyvernsRadiance15 = 45072, // Helper->self, 9.0s cast, range 40 width 18 rect
    ChainbladeBlow6 = 45052, // GuardianArkveld->self, 4.0s cast, single-target
    ChainbladeBlow7 = 45054, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlow8 = 45055, // Helper->self, 5.6s cast, range 40 width 4 rect
    WyvernsRadiance16 = 45056, // Helper->self, 6.2s cast, range 80 width 28 rect
    GuardianSiegeflight2 = 45067, // GuardianArkveld->location, 4.0s cast, range 40 width 4 rect
    GuardianSiegeflight3 = 45069, // Helper->self, 5.5s cast, range 40 width 8 rect
    _Weaponskill_9 = 45124, // Helper->self, 6.2s cast, range 40 width 8 rect
    GuardianResonance4 = 45070, // Helper->self, 9.0s cast, range 40 width 16 rect
    ChainbladeBlow9 = 45053, // GuardianArkveld->self, 4.0s cast, single-target
    ChainbladeBlow10 = 45057, // Helper->self, 5.2s cast, range 40 width 4 rect
    ChainbladeBlow11 = 45058, // Helper->self, 5.6s cast, range 40 width 4 rect
    WyvernsRadiance17 = 45059, // Helper->self, 6.2s cast, range 80 width 28 rect
    WrathfulRattle = 43879, // GuardianArkveld->self, 1.0+2.5s cast, single-target
    WyvernsRadiance18 = 43880, // Helper->self, 3.5s cast, range 40 width 8 rect
    WyvernsRadiance19 = 43881, // Helper->self, 1.0s cast, range 40 width 4 rect
    WyvernsRadiance20 = 43882, // Helper->self, 2.0s cast, range 40 width 4 rect
    WyvernsOuroblade2 = 45060, // GuardianArkveld->self, 5.0+1.5s cast, single-target
    WyvernsOuroblade3 = 45061, // Helper->self, 6.0s cast, range 40 180.000-degree cone
    SteeltailThrust = 45064, // GuardianArkveld->self, 3.0s cast, range 60 width 6 rect
    SteeltailThrust1 = 44804, // Helper->self, 3.6s cast, range 60 width 6 rect
    GreaterResonance = 43855, // Helper->location, no cast, range 60 circle
    WyvernsWeal3 = 45046, // GuardianArkveld->self, 6.5+1.5s cast, single-target
    WyvernsWeal4 = 45048, // Helper->self, 8.0s cast, range 60 width 6 rect
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 1789, // Helper/GuardianArkveld->player, extra=0x1/0x2/0x3/0x4
    _Gen_ = 2193, // GuardianArkveld->GuardianArkveld, extra=0x3BC/0x3C8
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_GuardianWill = 4574, // none->player, extra=0x0
    _Gen_2 = 2056, // GuardianArkveld->GuardianArkveld, extra=0x3A2

}
public enum IconID : uint
{
    _Gen_Icon_com_share2i = 100, // player->self
    _Gen_Icon_m0074g01ai = 101, // player->self
    _Gen_Icon_m0851_turnleft_c0g = 502, // GuardianArkveld->self
    _Gen_Icon_m0851_turnright_c0g = 501, // GuardianArkveld->self
}

