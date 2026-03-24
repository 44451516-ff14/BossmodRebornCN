namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

public enum OID : uint {
    PariOfPlenty = 0x4A70, // R5.016, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    _Gen_FieryBauble = 0x4A72, // R6.000, x8
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    _Gen_FlyingCarpet = 0x4A74, // R4.356, x8
    _Gen_ = 0x4A75, // R1.000, x6
    _Gen_FalseFlame = 0x4A71, // R5.016, x4
    _Gen_Actor1ebf49 = 0x1EBF49, // R0.500, x1, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_IcyBauble = 0x4A73, // R5.000, x1
    _Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    _Gen_Actor1ebf20 = 0x1EBF20, // R0.500, x0 (spawn during fight), EventObj type
}


public enum AID : uint {
    AutoAttack = 45545, // PariOfPlenty->player, no cast, single-target
    HeatBurst = 45517, // PariOfPlenty->self, 5.0s cast, range 60 circle
    Ability = 45421, // PariOfPlenty->location, no cast, single-target
    
    FireflightByPyrelightLeft = 45436, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByPyrelightRight = 45437, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByEmberlightLeft = 45434, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByEmberlightRight = 45435, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByEmberlightSpread = 45444, // Helper->players, no cast, range 3 circle
    FireflightByPyrelightStack = 45445, // Helper->players, no cast, range 3 circle
    
    CarpetRide1 = 45442, // Helper->location, 2.0s cast, ???
    CarpetRide2 = 45443, // Helper->location, 2.0s cast, ???
    
    SunCirclet = 45448, // PariOfPlenty->self, 2.0s cast, range ?-60 donut
    

    _Ability_CarpetRide = 45441, // PariOfPlenty/4A71->location, no cast, single-target
    
    WheelOfFableflightRight = 45478, // _Gen_FalseFlame->self, 11.0s cast, range 40 width 4 rect
    WheelOfFableflightLeft = 45479, // 4A71->self, 11.0s cast, range 40 width 4 rect
    
    WheelOfFireflight = 45469, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    WheelOfFireflight1 = 45470, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    WheelOfFireflight2 = 45471, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    WheelOfFireflight3 = 45472, // PariOfPlenty/4A71->self, no cast, range 40 ?-degree cone
    
    KindledFlameStack = 45538, // Boss->self, 8.0s cast, single-target
    ScatteredKindlingSpread = 45536, // PariOfPlenty->self, 8.0s cast, single-target
    _Ability_KindledFlame1 = 45539, // Helper->players, no cast, range 6 circle
    _Ability_ScatteredKindling1 = 45537, // Helper->player, no cast, range 6 circle
    
    FireOfVictory = 45519, // PariOfPlenty->player, 5.0s cast, range 4 circle
    
    FireflightFourLongNightsRight = 45467, // Boss->self, 17.0s cast, range 40 width 4 rect
    FireflightFourLongNightsLeft = 45468, // Boss->self, 17.0s cast, range 40 width 4 rect
    FellFirework = 45476, // Helper->player, no cast, range 3 circle
    FireWell = 45477, // Helper->players, no cast, range 3 circle
    
    _Ability_ParisCurse = 45551, // PariOfPlenty->self, 5.0s cast, range 60 circle
    _Ability_CharmingBaubles = 45503, // PariOfPlenty->self, 3.0s cast, single-target
    _Ability_ThievesWeave = 46754, // PariOfPlenty->self, 3.0s cast, single-target
    _Ability_1 = 45504, // 4A74->self, 1.0s cast, single-target
    _Ability_2 = 47153, // 4A74->location, no cast, single-target
    _Ability_CharmingBaubles1 = 45497, // PariOfPlenty->self, no cast, single-target
    _Ability_Unravel = 45506, // 4A74->self, 3.0s cast, single-target
    _Ability_Fableflight = 45439, // 4A71->self, 24.1s cast, range 40 width 4 rect
    _Ability_BurningGleam = 45548, // 4A72->self, 7.0s cast, range 40 width 10 cross
    _Ability_ChillingGleam = 45501, // 4A73->self, 2.0s cast, range 40 width 10 cross
    _Ability_HighFirePowder = 45524, // Helper->location, no cast, range 15 circle
    _Ability_FirePowder = 45523, // Helper->self, no cast, range 15 circle
    _Ability_SpurningFlames = 45482, // PariOfPlenty->self, 7.0s cast, range 40 circle
    _Ability_ImpassionedSparks = 45483, // PariOfPlenty->self, 5.0s cast, single-target
    _Ability_ImpassionedSparks1 = 45484, // PariOfPlenty->self, no cast, single-target
    _Ability_ImpassionedSparks2 = 45485, // Helper->self, 2.0s cast, single-target
    _Ability_ImpassionedSparks3 = 45488, // Helper->self, 6.0s cast, range 8 circle
    _Ability_BurningPillar = 45527, // Helper->self, 4.0s cast, range 10 circle
    _Ability_HotFoot = 45542, // Helper->player, no cast, range 10 circle
    _Ability_ScouringScorn = 45491, // PariOfPlenty->self, 6.0s cast, range 40 circle
    _Ability_Doubling = 47041, // PariOfPlenty->self, 3.0s cast, single-target
    _Ability_Fableflight1 = 45450, // 4A71->self, 8.0s cast, range 40 width 4 rect
    _Ability_Fableflight2 = 45449, // 4A71->self, 8.0s cast, range 40 width 4 rect
    _Ability_Explosion = 45532, // Helper->self, 8.0s cast, range 4 circle
    _Ability_CarpetRide2 = 45440, // 4A71->location, no cast, single-target
    _Ability_CarpetRide3 = 46574, // Helper->location, 2.1s cast, ???
    _Ability_CarpetRide4 = 46573, // Helper->location, 2.1s cast, ???
    _Ability_Fableflight3 = 45452, // 4A71->self, 15.0s cast, range 40 width 4 rect
    _Ability_Fableflight4 = 45451, // 4A71->self, 15.0s cast, range 40 width 4 rect
    _Ability_KindledFlame3 = 45540, // PariOfPlenty->self, 5.0s cast, single-target
    _Ability_KindledFlame2 = 45541, // Helper->players, no cast, range 6 circle
    _Ability_CharmdFlightFourNights = 47031, // PariOfPlenty->self, 17.5s cast, range 40 width 4 rect
}

public enum IconID : uint
{
    FalseFlameRight = 628, // _Gen_FalseFlame->self
    FalseFlameLeft = 629, // _Gen_FalseFlame->self
    FalseFlameRRight = 646, // _Gen_FalseFlame->self
    FalseFlameRLeft = 647, // _Gen_FalseFlame->self
    
    TurnRight = 624, // Boss->self
    TurnLeft = 625, // Boss->self
    TurnRLeft = 645, // Boss->self
    TurnRRight = 644, // Boss->self
    
    _Gen_Icon_tank_lockonae_4m_5s_01t = 342, // player->self
    _Gen_Icon_m0376trg_fire3_a0p = 97, // player->self
    _Gen_Icon_m0489trg_c0c = 138, // player->self
    _Gen_Icon_m6d1_rug01_8s_c0e1 = 631, // player->self
    _Gen_Icon_com_share3t = 161, // player->self
}

public enum SID : uint {
    WitchHunt = 2970, // none->Boss, extra=0x3F5/0x3F4
    DarkResistanceDown = 3619, // Helper->player, extra=0x0
}

public enum TetherID : uint
{
    CarpetRideTether = 355, // _Gen_->_Gen_
    _Gen_Tether_chn_hfchain1f = 9, // player->player
    _Gen_Tether_chn_tergetfix1f = 17, // _Gen_FalseFlame->player
}
