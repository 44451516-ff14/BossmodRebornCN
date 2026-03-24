namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad012DaryaTheSeamaid;

public enum OID : uint
{
    DaryaTheSeaMaid = 0x4A94,
    Helper = 0x233C,
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_TheMerchantsTaleAbridged = 0x1EBFAB, // R0.500, x1, EventObj type
    _Gen_AetherialFlow = 0x1EBFA3, // R2.000, x1, EventObj type
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    _Gen_Actor1ebf48 = 0x1EBF48, // R0.500, x1, EventObj type
    SeabornSteed = 0x4A95, // R2.200, x0 (spawn during fight)
    SeabornStalwart = 0x4A96, // R2.000, x0 (spawn during fight)
    SeabornSteward = 0x4A97, // R2.200, x0 (spawn during fight)
    SeabornSoldier = 0x4A98, // R2.200, x0 (spawn during fight)
    SunkenTreasureOrb = 0x1EBF1C, // R0.500, x0 (spawn during fight), EventObj type
    SunkenTreasureDonut = 0x1EBF1D, // R0.500, x0 (spawn during fight), EventObj type
    AquaSpearVoidzone = 0x1EBF1E, // R0.500, x0 (spawn during fight), EventObj type
    HydrofallOrb = 0x1EBF1B, // R0.500, x0 (spawn during fight), EventObj type

}
public enum AID : uint
{
    _AutoAttack_ = 45769, // DaryaTheSeaMaid->player, no cast, single-target
    PiercingPlunge = 45837, // DaryaTheSeaMaid->self, 5.0s cast, range 70 circle
    _Ability_ = 45770, // DaryaTheSeaMaid->location, no cast, single-target
    _Ability_FamiliarCall = 45771, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    EchoedSerenade = 45773, // DaryaTheSeaMaid->self, 8.5+0.5s cast, range 60 circle
    Watersong1 = 45805, // 4A95->self, 1.0s cast, range 40 width 8 rect
    Watersong2 = 45806, // 4A96->self, 1.0s cast, range 40 width 8 rect
    Watersong3 = 45807, // 4A97->self, 1.0s cast, range 40 width 8 rect
    Watersong4 = 45808, // 4A98->self, 1.0s cast, range 40 width 8 rect
    _Ability_SunkenTreasure = 45812, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    _Spell_Hydrobullet = 45815, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    Hydrobullet = 45816, // Helper->players, 5.0s cast, range 15 circle
    SphereShatter = 45813, // Helper->self, no cast, range 18 circle
    SphereShatterDonut = 45814, // Helper->self, no cast, range ?-20 donut
    _Spell_Hydrocannon = 45801, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    Hydrocannon = 45836, // Helper->self/player, 5.0s cast, range 70 width 6 rect
    _Ability_AquaSpear = 45817, // DaryaTheSeaMaid->self, 4.0s cast, single-target
    AquaSpear = 45818, // Helper->self, 3.0s cast, range 8 width 8 rect
    _Ability_SeaShackles = 45821, // DaryaTheSeaMaid->self, 4.0+1.0s cast, range 70 circle
    _Spell_TidalWave = 45819, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    TidalWave = 45820, // Helper->self, 6.0s cast, range 60 width 60 rect
    _Ability_SwimmingInTheAir = 45809, // DaryaTheSeaMaid->self, 4.0s cast, single-target
    Tidalspout = 47089, // Helper->players, 6.0s cast, range 6 circle, stack
    HydrobulletSpread = 45811, // Helper->players, 6.0s cast, range 15 circle, spread
    Hydrofall = 45810, // Helper->location, 1.0s cast, range 12 circle
    _Spell_CeaselessCurrent = 45823, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    CeaselessCurrentFirst = 45824, // Helper->self, 5.0s cast, range 8 width 40 rect
    CeaselessCurrentRest = 45825, // Helper->self, no cast, range 8 width 40 rect
    _Spell_SurgingCurrent = 45826, // DaryaTheSeaMaid->self, 7.0+1.0s cast, single-target
    SurgingCurrent = 45827, // Helper->self, 8.0s cast, range 60 ?-degree cone
    _Ability_AlluringOrder = 47090, // DaryaTheSeaMaid->self, 4.0s cast, range 70 circle
    _Spell_AquaBall = 45834, // DaryaTheSeaMaid->self, 2.0+1.0s cast, single-target
    AquaBall = 45835, // Helper->location, 3.0s cast, range 5 circle
    _Spell_RecedingTwinTides = 45828, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    _Spell_EncroachingTwinTides = 45831, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    NearTide = 45829, // Helper->location, 4.0s cast, range 10 circle
    NearTide1 = 45830, // Helper->location, no cast, range 10 circle
    FarTide = 45832, // Helper->location, 4.0s cast, range 10-40 donut
    FarTide1 = 45833, // Helper->location, no cast, range 10-40 donut
}
public enum SID : uint
{
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    NearShoreShackles = 4724, // none->player, extra=0x0
    _Gen_Dropsy1 = 3075, // none->player, extra=0x0
    _Gen_Dropsy2 = 3076, // none->player, extra=0x0
    _Gen_Dropsy3 = 3797, // none->player, extra=0x0
    _Gen_Dropsy4 = 3798, // none->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // Helper/4A98->player, extra=0x1
    ForwardMarch = 2161, // DaryaTheSeaMaid->player, extra=0x0
    AboutFace = 2162, // DaryaTheSeaMaid->player, extra=0x0
    LeftFace = 2163, // DaryaTheSeaMaid->player, extra=0x0
    RightFace = 2164, // DaryaTheSeaMaid->player, extra=0x0
    ForcedMarch = 1257, // DaryaTheSeaMaid->player, extra=0x4/0x1

}
public enum IconID : uint
{
    IconLockon = 23, // player->self
    TankLaserLockon = 471, // player->self
    SharedTankbuster = 318, // player->self
    Hydrobullet = 658, // player->self
}
public enum TetherID : uint
{
    TetherBad = 376, // player->player
    TetherGood = 377, // player->player
}
