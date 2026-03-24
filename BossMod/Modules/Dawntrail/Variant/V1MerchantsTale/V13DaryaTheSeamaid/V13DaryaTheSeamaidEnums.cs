namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V13DaryaTheSeamaid;

public enum OID : uint
{
    DaryaTheSeaMaid = 0x4A8F,
    Helper = 0x233C,
    // Path 5
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    _Gen_Actor1ebf48 = 0x1EBF48, // R0.500, x1, EventObj type
    SeabornSteed = 0x4A90, // R2.200, x0 (spawn during fight)
    SeabornStalwart = 0x4A91, // R2.000, x0 (spawn during fight)
    HydrofallOrb = 0x1EBF1B, // R0.500, x0 (spawn during fight), EventObj type
    SunkenTreasureOrb = 0x1EBF1C, // R0.500, x0 (spawn during fight), EventObj type
    SeabornSongstress = 0x4A92, // R1.900, x0 (spawn during fight)
    SirenSphere = 0x4A93, // R1.000, x0 (spawn during fight)
    // Path 6
    AquaSpearVoidzone = 0x1EBF1E, // R0.500, x0 (spawn during fight), EventObj type
    // Path 7
    SeabornSpeaker = 0x4CB5, // R3.500, x0 (spawn during fight)
}

public enum AID : uint
{
    // Path 5
    _AutoAttack_ = 45769, // DaryaTheSeaMaid->player, no cast, single-target
    PiercingPlunge = 45803, // DaryaTheSeaMaid->self, 5.0s cast, range 70 circle
    _Ability_ = 45770, // DaryaTheSeaMaid->location, no cast, single-target
    _Ability_FamiliarCall = 45771, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    EchoedSerenade = 45772, // DaryaTheSeaMaid->self, 6.5+0.5s cast, range 60 circle
    EchoedSerenade1 = 45773, // DaryaTheSeaMaid->self, 8.5+0.5s cast, range 60 circle
    Watersong = 45774, // 4A90->self, 1.0s cast, range 40 width 8 rect
    Watersong1 = 45775, // 4A91->self, 1.0s cast, range 40 width 8 rect
    SurgingCurrentVisual = 45791, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    SurgingCurrent = 47052, // Helper->self, 5.0s cast, range 60 180-degree cone
    _Spell_ = 47051, // Helper->self, 5.0s cast, single-target
    _Ability_SwimmingInTheAir = 45776, // DaryaTheSeaMaid->self, 4.0s cast, single-target
    Hydrofall = 45777, // Helper->location, 2.0s cast, range 12 circle
    _Ability_SunkenTreasure = 45778, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    SphereShatter = 45779, // Helper->self, 1.5s cast, range 18 circle
    _Spell_AquaBall = 45799, // DaryaTheSeaMaid->self, 2.0+1.0s cast, single-target
    AquaBall = 45800, // Helper->location, 3.0s cast, range 5 circle
    RecedingTwinTides = 45793, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    NearTide = 45794, // Helper->location, 4.0s cast, range 10 circle
    NearTide1 = 45795, // Helper->location, no cast, range 10 circle
    EncroachingTwinTides = 45796, // DaryaTheSeaMaid->self, 3.0+1.0s cast, single-target
    FarTide = 45797, // Helper->location, 4.0s cast, range 10-40 donut
    FarTide1 = 45798, // Helper->location, no cast, range 10-40 donut
    _Ability_1 = 45780, // 4A92->self, no cast, single-target
    _Ability_2 = 45781, // 4A92->self, no cast, single-target
    _Spell_Hydrocannon = 45801, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    Hydrocannon = 45802, // Helper->self/player, 5.0s cast, range 70 width 6 rect
    _Ability_Explosion = 45782, // SirenSphere->player, no cast, single-target
    // Path 6
    _Ability_AquaSpear = 45783, // DaryaTheSeaMaid->self, 4.0s cast, single-target
    AquaSpear = 45784, // Helper->self, 3.0s cast, range 8 width 8 rect
    AlluringOrder = 45804, // DaryaTheSeaMaid->self, 4.0s cast, range 70 circle
    // Path 7
    _Spell_BigWave = 45785, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    BigWave = 45786, // Helper->self, 5.5s cast, range 60 width 60 rect
    AquaBall1 = 45787, // Helper->location, 7.5s cast, range 5 circle
    // Path 8
    _Spell_CeaselessCurrent = 45788, // DaryaTheSeaMaid->self, 4.0+1.0s cast, single-target
    CeaselessCurrentFirst = 45789, // Helper->self, 5.0s cast, range 8 width 40 rect
    CeaselessCurrentRest = 45790, // Helper->self, no cast, range 8 width 40 rect
}

public enum SID : uint
{
    // Path 5
    _Gen_ = 2193, // 4A92->4A92, extra=0x446
    // Path 6
    ForwardMarch = 2161, // DaryaTheSeaMaid->player, extra=0x0, other marches possible?
    ForcedMarch = 1257, // DaryaTheSeaMaid->player, extra=0x1
    // Path 7
    _Gen_Dropsy1 = 3075, // none->player, extra=0x0
    _Gen_Dropsy2 = 3076, // none->player, extra=0x0
}

public enum IconID : uint
{
    // Path 5
    TankLaserLockon = 471, // player->self
}
