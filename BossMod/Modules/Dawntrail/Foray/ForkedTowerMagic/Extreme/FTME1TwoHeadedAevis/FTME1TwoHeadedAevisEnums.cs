namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

public enum OID : uint
{
    TwoHeadedAevis = 0x4C18, // R18.000, x1
    GreenHead = 0x4C19,
    BlueHead = 0x4C1A,
    Helper = 0x233C,
    GreenHead1 = 0x4C1B, // R1.000, x1
    BlueHead1 = 0x4C1C, // R1.000, x1
    CrossBlazeTarget = 0x4C24, // R1.000, x2
    ArcaneFontGreen = 0x4C22, // R1.000, x0 (spawn during fight)
    ArcaneFontBlue = 0x4C23, // R1.000, x0 (spawn during fight)
    CharmedLightning = 0x4C1F, // R3.000, x0 (spawn during fight)
    CharmedIce = 0x4C20, // R3.000, x0 (spawn during fight)
    BreathyDuetTarget = 0x4A35, // R0.010, x2
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R0.500-2.000, x3, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    _AutoAttack_ = 47755, // 4C1B->player, no cast, single-target
    _AutoAttack_1 = 47756, // 4C1C->player, no cast, single-target
    _Ability_ = 50712, // Helper->player, no cast, single-target
    _Ability_1 = 50711, // Helper->player, no cast, single-target
    Buffet = 49726, // GreenHead/BlueHead->self, 5.0s cast, single-target
    _Ability_2 = 49727, // 4C18->self, 5.0s cast, single-target
    _Ability_3 = 47636, // 4C18->self, 8.2+0.8s cast, single-target

    StormsBreathCast = 47631, // GreenHead->self, 9.0s cast, single-target
    StormsBreath = 47638, // Helper->location, 9.0s cast, range 0 ???
    FreezingFugue1 = 47641, // Helper->self, 9.0s cast, range 20 circle
    FreezingFugue2 = 50728, // Helper->self, 11.0s cast, range 20 circle
    FreezingFugue3 = 47630, // Helper->self, 11.0s cast, range 20 circle

    PoisonBreath = 47639, // Helper->location, 9.0s cast, range 18 circle
    FulgurousFugue1 = 47640, // Helper->self, 9.0s cast, range 6-60 donut (says 6-60, at least 1st instance is hitbox radius)
    FulgurousFugue2 = 47629, // Helper->self, 11.0s cast, range 6-60 donut
    FulgurousFugue3 = 50727, // Helper->self, 11.0s cast, range 6-60 donut

    ThunderfrostTempest = 47739, // GreenHead/BlueHead->self, 5.0s cast, single-target
    _Ability_ThunderfrostTempest1 = 47740, // Helper->self, no cast, range 0 ???
    _Ability_ThunderfrostTempest2 = 47741, // Helper->self, no cast, range 0 ???

    Crossblaze = 47685, // Helper->self, 2.0s cast, range 35 width 10 cross
    Blazeloop = 47686, // Helper->self, 2.0s cast, range 5-60 donut
    BlazeFirst = 50706, // Helper->location, 11.0s cast, range 5 circle
    BlazeSecond = 50707, // Helper->location, 11.0s cast, range 5 circle
    BlazeFollowup = 50708, // Helper->location, 6.0s cast, range 5 circle
    CrossblazeAndRepeat1 = 47671, // GreenHead/BlueHead->self, 11.0s cast, single-target
    BlazeloopAndRepeat1 = 47672, // GreenHead->self, 11.0s cast, single-target
    CrossblazeBlazeloop1 = 47673, // BlueHead->self, 11.0s cast, single-target
    BlazeloopCrossblaze1 = 47674, // BlueHead->self, 11.0s cast, single-target
    CrossblazeAndRepeat2 = 47675, // BlueHead->self, 10.3+0.7s cast, single-target
    BlazeloopAndRepeat2 = 47676, // BlueHead/GreenHead->self, 10.3+0.7s cast, single-target
    CrossblazeBlazeloop2 = 47677, // GreenHead->self, 10.3+0.7s cast, single-target
    BlazeloopCrossblaze2 = 47678, // GreenHead->self, 10.3+0.7s cast, single-target
    BlazeFirstCast = 47683, // 4C1B/4C1C->location, 11.0s cast, single-target
    BlazeSecondCast = 47684, // 4C1C/4C1B->location, 11.0s cast, single-target
    CrossblazeCast = 47687, // GreenHead/BlueHead->self, 5.3+0.7s cast, single-target
    BlazeloopCast = 47688, // BlueHead/GreenHead->self, 5.3+0.7s cast, single-target
    BlazeFollowupCast = 47689, // 4C1B/4C1C->location, 6.0s cast, single-target

    ArchaeofuryCast = 47749, // GreenHead/BlueHead->self, 5.0s cast, single-target
    Archaeofury1 = 47751, // Helper->player, no cast, range 6 circle
    Archaeofury2 = 47752, // Helper->player, no cast, range 6 circle

    _Ability_FulgurousFugue = 47632, // GreenHead->self, 9.0s cast, single-target
    _Ability_PoisonBreath1 = 50717, // BlueHead->self, 9.0s cast, single-target
    _Ability_4 = 47758, // 4C18->self, 5.0s cast, single-target
    _Ability_5 = 47679, // 4C18->self, 10.3s cast, single-target
    _Ability_6 = 47682, // 4C18->self, no cast, single-target
    _Ability_7 = 47681, // 4C18->self, no cast, single-target
    _Ability_8 = 47750, // 4C18->self, 4.3s cast, single-target
    _Ability_9 = 47635, // 4C18->self, 8.2+0.8s cast, single-target
    _Ability_10 = 48245, // Helper->location, 9.0s cast, range 30 circle
    _Ability_FreezingFugue1 = 47633, // BlueHead->self, 9.0s cast, single-target
    ArcaneRevelation = 47719, // GreenHead/BlueHead->self, 3.0s cast, single-target
    _Ability_11 = 47720, // 4C18->self, 3.0s cast, single-target
    _Ability_12 = 47700, // 4C18->self, 6.0s cast, single-target
    ArcaneBeacon1 = 47721, // 4C22->self, 0.7s cast, range 60 width 5 rect
    ArcaneBeacon2 = 47722, // 4C23->self, 0.7s cast, range 60 width 5 rect
    TwoTerrors1 = 47702, // Helper->self, 7.0s cast, range 40 width 20 rect
    TwoTerrors2 = 47703, // Helper->self, 7.0s cast, range 40 width 10 rect
    _Ability_TwoTerrors1 = 47697, // GreenHead/BlueHead->self, 7.0s cast, single-

    Summon = 47710, // GreenHead/BlueHead->self, 3.0s cast, single-target
    _Ability_13 = 47711, // 4C18->self, 3.0s cast, single-target
    _Ability_14 = 47653, // 4A35->location, 3.0s cast, single-target
    _Ability_15 = 47647, // 4C18->self, 16.4s cast, single-target
    BreathyDuet = 47646, // GreenHead/BlueHead->self, 17.0s cast, single-target
    _Ability_LightningCluster = 47649, // 4C1B->location, 17.0s cast, single-target
    _Ability_IceCluster = 47650, // 4C1C->location, 17.0s cast, single-target
    LightningCluster1 = 50699, // Helper->location, 17.3s cast, range 15 circle
    IceCluster1 = 50700, // Helper->location, 17.3s cast, range 15 circle
    LevinWave = 47714, // 4C1F->self, no cast, range 45 ?-degree cone
    IceWave = 47715, // 4C20->self, no cast, range 45 ?-degree cone
    _Ability_16 = 47648, // 4C18->self, 1.0s cast, single-target
    _Ability_LightningCluster2 = 47651, // 4C1B->location, 1.0s cast, single-target
    _Ability_IceCluster2 = 47652, // 4C1C->location, 1.0s cast, single-target
    LightningCluster2 = 50701, // Helper->location, 1.3s cast, range 15 circle
    IceCluster2 = 50702, // Helper->location, 1.3s cast, range 15 circle

    _Ability_17 = 50726, // 4C18->self, 10.2+0.8s cast, single-target
    _Ability_FreezingFugue3 = 50724, // BlueHead->self, 11.0s cast, single-target
    _Ability_FulgurousFugue2 = 47619, // GreenHead->self, 10.2+0.8s cast, single-target
    _Ability_18 = 47623, // 4C18->self, no cast, single-target
    HissingResonance = 47723, // GreenHead/BlueHead->self, 3.0s cast, single-target
    _Ability_19 = 47724, // 4C18->self, 3.0s cast, single-target
    _Ability_20 = 47680, // 4C18->self, 10.3s cast, single-target
    Buffet1 = 47725, // Helper->self, 1.0s cast, range 0 ???
    Buffet2 = 47726, // Helper->self, 1.0s cast, range 0 ???
    Buffet3 = 47727, // Helper->self, 1.0s cast, range 0 ???
    Buffet4 = 47728, // Helper->self, 1.0s cast, range 0 ???
    _Ability_21 = 47701, // _Gen_TwoHeadedAevis->self, 6.0s cast, single-target
    _Weaponskill_Aethersplit = 48642, // _Gen_GreenHead->_Gen_BlueHead, no cast, single-target
    _Ability_22 = 50725, // _Gen_TwoHeadedAevis->self, 10.2+0.8s cast, single-target
    _Ability_FulgurousFugue5 = 50723, // GreenHead->self, 11.0s cast, single-target
    _Ability_FreezingFugue4 = 47620, // BlueHead->self, 10.2+0.8s cast, single-target
    _Ability_23 = 47626, // _Gen_TwoHeadedAevis->self, no cast, single-target
    _Ability_24 = 47699, // _Gen_TwoHeadedAevis->self, 6.0s cast, single-target
}

public enum SID : uint
{
    EpicHero = 4192, // none->player, extra=0x0
    FatedHero = 4194, // none->player, extra=0x0
    EpicVillain = 5400, // none->GreenHead, extra=0x0
    FatedVillain = 5401, // none->BlueHead, extra=0x0
    _Gen_ThriceComeRuin = 3478, // Helper/4C22/4C1F/4C23->player, extra=0x1/0x2
    _Gen_ = 2552, // none->4C18, extra=0x44B
    GreenNoiseEasterly = 5052, // none->player, extra=0x0
    GreenNoiseWesterly = 5053, // none->player, extra=0x0
    BlueNoiseEasterly = 5054, // none->player, extra=0x0
    BlueNoiseWesterly = 5055, // none->player, extra=
    _Gen_Doom = 2519, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 344, // player->self
    Lockon1 = 722, // 4A35->self
    Lockon2 = 723, // 4A35->self
    Lockon3 = 724, // 4A35->self
    Lockon4 = 725, // 4A35->self
    GreenEast = 708, // player->self
    GreenWest = 709, // player->self
    BlueEast = 710, // player->self
    BlueWest = 711, // player->self
}

public enum TetherID : uint
{
    Buffet = 429, // player->4C1C/4C1B
    Tether = 411, // 4C1C/4C1B->4C24/4A35
}
