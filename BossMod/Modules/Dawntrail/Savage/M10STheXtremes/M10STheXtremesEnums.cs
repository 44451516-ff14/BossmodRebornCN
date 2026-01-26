namespace BossMod.Dawntrail.Savage.M10STheXtremes;

public enum OID : uint
{
    RedHot = 0x4B57, // R4.000, x?
    DeepBlue = 0x4B58, // R4.000, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    XtremeAether = 0x4B59, // R1.500, x?
    SickSwell = 0x4B5A, // R1.000, x?
    TheXtremes = 0x4BDF, // R1.000, x?
    _Gen_WateryGrave = 0x4AC0, // R1.000, x?
    WateryGrave = 0x4B5C, // R4.000, x0 (spawn during fight)
    PuddleFlameFloater = 0x1EBF30, // R0.500, x0 (spawn during fight), EventObj type (FLameFloater puddles)
    PuddleAlleyOopInferno = 0x1EBF31, // R0.500, x0 (spawn during fight), EventObj type (AlleyOppInferno puddles)
    PuddlePyrorotation = 0x1EBF32, // R0.500, x0 (spawn during fight), EventObj type (Pyrorotation puddles)
    PuddleCutbackBlaze = 0x1EBF33, // R0.500, x0 (spawn during fight), EventObj type (Cutback Blaze puddle)
    PuddleInsaneAirCone = 0x1EBFCE, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_ = 0x1E932D, // R2.000, x?, EventObj type
    _Gen_1 = 0xF680F, // R1.100, x?, EventNpc type
    _Gen_2 = 0x4AE7, // R1.000, x?
}

public enum AID : uint
{
    // Red Hot
    _AutoAttack_ = 48639, // 4B57->player, no cast, single-target
    HotImpact = 46518, // 4B57->players, 5.0s cast, range 6 circle
    _Ability_ = 46456, // 4B57->location, no cast, single-target
    FlameFloaterCast = 46522, // 4B57->self, 7.0s cast, single-target
    FlameFloater1 = 46523, // 4B57->location, no cast, range 60 width 8 rect
    FlameFloater2 = 46524, // 4B57->location, no cast, range 60 width 8 rect
    FlameFloater3 = 46525, // 4B57->location, no cast, range 60 width 8 rect
    FlameFloater4 = 46526, // 4B57->location, no cast, range 60 width 8 rect
    _Ability_1 = 46460, // 4B57->self, no cast, single-target
    AlleyOopInfernoCast = 46528, // 4B57->self, 4.3+0.7s cast, single-target
    AlleyOopInferno = 46529, // 233C->players, 5.0s cast, range 5 circle
    CutbackBlazeCast = 46537, // 4B57->self, 4.3+0.7s cast, single-target
    CutbackBlaze = 46538, // 233C->self, no cast, range 60 ?-degree cone
    PyrotationCast = 46530, // 4B57->self, 4.3+0.7s cast, single-target
    Pyrotation = 46531, // 233C->players, no cast, range 6 circle
    DiversDareRed = 46520, // 4B57->self, 5.0s cast, range 60 circle

    // Deep Blue
    _AutoAttack_1 = 48640, // 4B58->player, no cast, single-target
    _Weaponskill_SickSwell = 46539, // 4B58->self, 3.0s cast, single-target
    _Weaponskill_SickestTakeOff = 46541, // 4B58->self, 4.0s cast, single-target
    SickestTakeOff = 46542, // 233C->self, 7.0s cast, range 50 width 15 rect (unsafe lane)
    SickSwell1 = 46540, // 233C->self, 7.0s cast, range 50 width 50 rect (raidwide + knockback)
    AwesomeSplash = 46543, // 233C->players, no cast, range 5 circle
    AwesomeSlab = 46544, // 233C->players, no cast, range 6 circle
    AlleyOopDoubleDipCast = 46557, // 4B58->self, 5.0s cast, single-target
    AlleyOopDoubleDip1 = 46558, // 233C->self, no cast, range 60 ?-degree cone
    AlleyOopDoubleDip2 = 46559, // 233C->self, no cast, range 60 ?-degree cone
    ReverseAlleyOopCast = 46560, // 4B58->self, 5.0s cast, single-target
    ReverseAlleyOop1 = 46561, // 233C->self, no cast, range 60 ?-degree cone
    ReverseAlleyOop2 = 46562, // 233C->self, no cast, range 60 ?-degree cone
    DeepImpactCast = 46519, // 4B58->self, 4.9s cast, single-target
    DeepImpact = 44486, // 4B58->players, no cast, range 6 circle
    DiversDareBlue = 46521, // 4B58->self, 5.0s cast, range 60 circle
    _Ability_2 = 46457, // 4B58->location, no cast, single-target
    XtremeSpectacularCastRed = 46553, // 4B57->self, 4.0s cast, single-target
    XtremeSpectacularCastBlue = 46554, // 4B58->self, 4.0s cast, single-target
    XtremeSpectacular = 46500, // 4BDF->self, 7.4s cast, range 50 width 40 rect
    XtremeSpectacularRest = 46556, // 4BDF->self, no cast, range 60 circle
    XtremeSpectacularLast = 47050, // 4BDF->self, no cast, range 60 circle
    _Ability_EpicBrotherhood = 46459, // 4B58->4B57, no cast, single-target
    _Ability_EpicBrotherhood1 = 46458, // 4B57->4B58, no cast, single-target

    // Together
    InsaneAirRed = 47255, // 4B57->self, 5.9+1.5s cast, single-target
    InsaneAirBlue = 47256, // 4B58->self, 5.9+1.5s cast, single-target
    InsaneAirRedRest = 47257, // 4B57->self, 3.9+1.5s cast, single-target
    InsaneAirBlueRest = 47258, // 4B58->self, 3.9+1.5s cast, single-target
    _Weaponskill_ReEntryPlunge = 46580, // 4B58->self, no cast, single-target
    BlastingSnapVisual = 46575, // 4B57->self, no cast, single-target
    PlungingSnapVisual = 46576, // 4B58->self, no cast, single-target
    BlastingSnap = 46577, // 233C->self, no cast, range 60 ?-degree cone (spread)
    PlungingSnap = 46578, // 233C->self, no cast, range 60 ?-degree cone
    ReEntryBlastVisual = 46579, // 4B57->self, no cast, single-target
    ReEntryPlungeVisual = 46580, // 4B57->self, no cast, single-target
    ReEntryBlast = 46581, // 233C->self, no cast, range 60 ?-degree cone (stack)
    ReEntryPlunge = 46582, // 233C->self, no cast, range 60 ?-degree cone
    VerticalBlastVisual = 46583, // 4B57->self, no cast, single-target
    VerticalPlungeVisual = 46584, // 4B58->self, no cast, single-target
    VerticalBlast = 46585, // 233C->player, no cast, range 6 circle (tb)
    VerticalPlunge = 46586, // 233C->player, no cast, range 6 circle
    Unknown6Circle = 46587, // 233C->player, no cast, range 6 circle
    _Ability_3 = 46461, // 4B58->self, no cast, single-target
    Firesnaking = 45953, // 4B57->self, 5.0s cast, range 60 circle
    Watersnaking = 45954, // 4B58->self, 5.0s cast, range 60 circle
    SteamBurst = 46587, // 4B59->self, 3.0s cast, range 9 circle
    HotImpact2 = 46464, // 4B57->players, 5.0s cast, range 6 circle
    DeepVarialCast = 47249, // 4B58->location, 5.3+1.0s cast, ???
    DeepVarial = 46547, // 233C->self, 6.8s cast, range 60 120.000-degree cone
    AwesomeSplashAerial = 46551, // 233C->players, no cast, range 5 circle
    AwesomeSlabAerial = 46552, // 233C->players, no cast, range 6 circle
    HotAeriaCast = 46532, // 4B57->self, 5.0s cast, single-target
    HotAerialJump = 47389, // 4B57->player, no cast, single-target
    HotAerial1 = 47390, // 233C->players, no cast, range 6 circle
    HotAerial2 = 47391, // 233C->players, no cast, range 6 circle
    HotAerial3 = 47392, // 233C->player, no cast, range 6 circle
    HotAerial4 = 47393, // 233C->players, no cast, range 6 circle
    DeepAerialCast = 46563, // 4B58->location, 5.0s cast, single-target
    DeepAerial = 46564, // 233C->self, 6.0s cast, range 6 circle
    _Weaponskill_ = 46570, // 4AC0->player, no cast, single-target
    XtremeWaveRedFirstCast = 46533, // 4B57->self, 4.9s cast, single-target
    XtremeWaveBlueFirstCast = 46534, // 4B58->self, 4.9s cast, single-target
    XtremeWaveRedRestCast = 46535, // 4B57->self, 4.9s cast, single-target
    XtremeWaveBlueRestCast = 46536, // 4B58->self, 4.9s cast, single-target
    XtremeWaveRed = 46545, // 4B57->location, no cast, range 60 width 8 rect
    XtremeWaveBlue = 46546, // 4B58->location, no cast, range 60 width 8 rect
    ScathingSteam = 44487, // 4B5C->self, 1.0s cast, range 60 circle (XtremeWaveRed going through orb)
    _Weaponskill_ImpactZone = 46572, // 4B5C->self, no cast, range 60 circle
    _Weaponskill_ImpactZone1 = 46571, // 4AC0->self, no cast, range 60 circle
    FlameFloaterSplitCast = 46548, // 4B57->self, 5.0s cast, single-target
    FlameFloaterSplit = 46527, // 4B57->location, no cast, range 60 width 8 rect
    FreakyPyrotationCast = 46486, // 4B57->self, 4.3+0.7s cast, single-target
    FreakyPyrotation = 46487, // 233C->player, no cast, range 6 circle
    XtremeWatersnaking = 46511, // DeepBlue->self, 5.0s cast, range 60 circle
    XtremeFiresnaking = 46510, // RedHot->self, 5.0s cast, range 60 circle
    XtremeInsaneAirRed = 46566, // RedHot->self, 6.9+1.5s cast, single-target
    XtremeInsaneAirBlue = 46567, // DeepBlue->self, 6.9+1.5s cast, single-target
    Bailout1 = 46512, // Helper->players, 1.0s cast, range 15 circle
    Bailout2 = 46513, // Helper->players, 1.0s cast, range 15 circle
    OverTheFallsRed = 46588, // RedHot->self, 9.0s cast, range 60 circle
    OverTheFallsBlue = 46589, // DeepBlue->self, 9.0s cast, range 60 circle
}

public enum SID : uint
{
    DirectionalDisregard = 3808, // none->RedHot/DeepBlue, extra=0x0
    FirstInLine = 3004, // none->player, extra=0x0
    SecondInLine = 3005, // none->player, extra=0x0
    ThirdInLine = 3006, // none->player, extra=0x0
    FourthInLine = 3451, // none->player, extra=0x0
    Burns1 = 3065, // none->player, extra=0x0
    Burns2 = 3066, // none->player, extra=0x0
    _Gen_ = 2056, // none->DeepBlue, extra=0x3ED/0x3EE/0x435/0x3EF/0x3F0 (3ED = LP stack, 3EE = spread, 3EF = snek stack, 3F0 = snek spread)
    BrotherlyLove = 4972, // none->DeepBlue/RedHot, extra=0x0
    Watersnaking = 4975, // none->player, extra=0x0
    Firesnaking = 4974, // none->player, extra=0x0
    WateryGrave = 4829, // none->player, extra=0x12C
    XtremeFiresnaking = 4827, // none->player, extra=0x0
    XtremeWatersnaking = 4828, // none->player, extra=0x0
}

public enum IconID : uint
{
    SharedTankbuster = 259, // player->self
    _Gen_Icon_target_ae_5m_s5_fire0c = 660, // player->self
    PyrorotationStack = 666, // player->self
    TetherBlue = 635, // player->self
    TetherRed = 636, // player->self
    FreakyPyrotation = 659, // player->self
}

public enum TetherID : uint
{
    FlameTetherShort = 378, // _Gen_2/player->player
    FlameTetherLong = 379, // _Gen_2/player->player/_Gen_2
    _Gen_Tether_chn_m0982_2c = 372, // DeepBlue->_Gen_2
    TetherShort = 57, // _Gen_2->player
    TetherLong = 17, // _Gen_2->player
}
