namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

public enum OID : uint
{
    TwoHeadedAevis = 0x4C11, // R18.000, x1
    GreenHead = 0x4C12, // R15.000, x1
    BlueHead = 0x4C13, // R15.000, x1
    GreenHead1 = 0x4C14, // R1.000, x1
    BlueHead1 = 0x4C15, // R1.000, x1
    SwirlingOrb = 0x4C17, // R2.800, x0 (spawn during fight)
    BallLightning = 0x4C16, // R2.400, x0 (spawn during fight)
    ArcaneFont = 0x4B73, // R1.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackGreen = 47753, // GreenHead1->player, no cast, single-target
    AutoAttackBlue = 47754, // BlueHead1->player, no cast, single-target

    Buffet = 49726, // BlueHead/GreenHead->self, 5.0s cast, single-target
    Summon = 47704, // BlueHead/GreenHead->self, 3.0s cast, single-target

    PoisonBreathVisual = 50715, // BlueHead->self, 8.0s cast, single-target
    PoisonBreath = 47617, // Helper->location, 8.0s cast, range 18 circle
    StormsBreathCast = 47613, // GreenHead->self, 8.0s cast, single-target
    StormsBreathVisual = 48243, // Helper->location, 8.0s cast, range 30 circle
    StormsBreath = 47616, // Helper->location, 8.0s cast, ???
    TwoTerrorsVisual = 50655, // BlueHead/GreenHead->self, 6.0s cast, single-target
    TwoTerrors = 50658, // Helper->self, 6.0s cast, range 40 width 10 rect

    Aethersplit = 48642, // GreenHead1->BlueHead1, no cast, single-target

    _Ability_ = 50709, // Helper->player, no cast, single-target
    _Ability_1 = 50710, // Helper->player, no cast, single-target
    _Ability_2 = 49727, // TwoHeadedAevis->self, 5.0s cast, single-target
    _Ability_3 = 47615, // TwoHeadedAevis->self, 7.2+0,8s cast, single-target
    _Ability_4 = 47614, // TwoHeadedAevis->self, 7.2+0,8s cast, single-target
    _Ability_6 = 47736, // TwoHeadedAevis->self, 5.0s cast, single-target
    _Ability_7 = 50657, // TwoHeadedAevis->self, 5.0s cast, single-target
    _Ability_8 = 50656, // TwoHeadedAevis->self, 5.0s cast, single-target
    _Ability_9 = 49723, // TwoHeadedAevis->self, 3.0s cast, single-target
    _Ability_10 = 47705, // TwoHeadedAevis->self, 3.0s cast, single-target
    _Ability_11 = 47643, // TwoHeadedAevis->self, 7.4s cast, single-target
    _Ability_12 = 47656, // TwoHeadedAevis->self, 5.3s cast, single-target
    _Ability_13 = 47655, // TwoHeadedAevis->self, 5.3s cast, single-target
    _Ability_14 = 47658, // TwoHeadedAevis->self, no cast, single-target
    _Ability_15 = 49717, // TwoHeadedAevis->self, 3.0s cast, single-target
    _Ability_16 = 47657, // TwoHeadedAevis->self, no cast, single-target

    HissingReprise = 49722, // GreenHead/BlueHead->self, 3.0s cast, single-target
    BuffetEastern = 49724, // Helper->self, no cast, ???
    BuffetWestern = 49725, // Helper->self, no cast, ???

    IceClusterVisual1 = 48220, // BlueHead->self, 8.0s cast, single-target
    IceClusterVisual2 = 47645, // BlueHead1->location, 8.0s cast, single-target
    LightningClusterVisual1 = 47642, // GreenHead->self, 8.0s cast, single-target
    LightningClusterVisual2 = 47644, // GreenHead1->location, 8.0s cast, single-target
    LightningCluster = 50697, // Helper->location, 8.0s cast, range 15 circle
    IceCluster = 50698, // Helper->location, 8.0s cast, range 15 circle
    HypothermalCombustion = 47707, // SwirlingOrb->self, 2.0s cast, range 15 circle
    ThunderfrostTempestVisual = 47735, // GreenHead/BlueHead->self, 5.0s cast, single-target
    ThunderfrostTempest1 = 47738, // Helper->self, no cast, range 0 ???
    ThunderfrostTempest2 = 47737, // Helper->self, no cast, range 0 ???
    Shock = 47706, // BallLightning->self, 2.0s cast, range 15 circle

    BlazeVisual1 = 47663, // GreenHead1/BlueHead1->location, 6.0s cast, single-target
    BlazeVisual2 = 47664, // BlueHead1/GreenHead1->location, 6.0s cast, single-target
    BlazeVisual3 = 47659, // BlueHead1->location, 6.0s cast, single-target
    Blaze1 = 50703, // Helper->location, 6.0s cast, range 5 circle
    Blaze2 = 50704, // Helper->location, 6.0s cast, range 5 circle
    Blaze3 = 50705, // Helper->location, 6.0s cast, range 5 circle

    BlazeloopVisual1 = 47661, // GreenHead/BlueHead->self, 6.0s cast, single-target
    BlazeloopVisual2 = 47662, // BlueHead/GreenHead->self, 5.3+0,7s cast, single-target
    BlazeloopVisual3 = 47654, // BlueHead->self, 6.0s cast, single-target
    Blazeloop = 47660, // Helper->self, 2.5s cast, range 5-60 donut

    ArcaneRevelation = 49716, // BlueHead/GreenHead->self, 3.0s cast, single-target
    ArcaneBeacon = 49720, // ArcaneFont->self, 4.0s cast, range 60 width 5 rect

    Archaeofury1 = 47747, // Helper->player, 5.0s cast, range 6 circle
    Archaeofury2 = 47748, // Helper->player, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    EpicHero = 4192, // none->player, extra=0x0
    EpicVillain = 5400, // none->GreenHead, extra=0x0
    FatedHero = 4194, // none->player, extra=0x0
    FatedVillain = 5401, // none->BlueHead, extra=0x0
    EasterlyReprise = 5403, // none->player, extra=0x0
    WesterlyReprise = 5404, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 344, // player->self
    KnockbackTimer = 585, // player->self
}

public enum TetherID : uint
{
    Tether_chn_m0560_0t2 = 411, // GreenHead1/BlueHead1->UnknownActor
    Tether_chn_m0560_elc_0t2 = 412, // GreenHead1->UnknownActor
    Tether_chn_m0560_ice_0t2 = 413, // BlueHead1->UnknownActor
    Buffet = 429, // player->BlueHead1/GreenHead1
}
