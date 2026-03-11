namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V12PariOfPlenty;

public enum OID : uint
{
    // Common
    // Path 1
    PariOfPlenty = 0x4A68,
    Helper = 0x233C,
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type\
    _Gen_FieryBauble = 0x4A69, // R6.000, x8
    _Gen_ = 0x4A75, // R1.000, x6
    _Gen_FlyingCarpet = 0x4A74, // R4.356, x4
    _Gen_Actor1ebf49 = 0x1EBF49, // R0.500, x1, EventObj type
    CapriciousChambermaid = 0x4A6A, // R0.600, x5
    LegendaryBird1 = 0x4A88, // R4.320, x1

    // Path 2
    LegendaryBird2 = 0x4A6B, // R4.320, x1

    // Path 3
    _Gen_SummonedSpirit = 0x4A6C, // R2.400, x1
    _Gen_Whirlwind = 0x4A76, // R4.450, x0 (spawn during fight)
}

public enum AID : uint
{
    // Common
    _AutoAttack_ = 45543, // PariOfPlenty->player, no cast, single-target
    HeatBurst = 45515, // PariOfPlenty->self, 5.0s cast, range 60 circle
    _Ability_ = 45421, // PariOfPlenty->location, no cast, single-target
    _Ability_1 = 44505, // 4A74->self, no cast, single-target
    _Ability_Fireflight = 45422, // PariOfPlenty->self, 10.0s cast, single-target
    _Ability_CarpetRide = 45423, // PariOfPlenty->location, no cast, single-target
    CarpetRide = 45424, // Helper->location, 2.0s cast, width 10 rect charge
    SunCirclet = 45446, // PariOfPlenty->self, 1.0s cast, range ?-60 donut, bit smaller than max melee?
    RightFireflightTwoNights = 45918, // PariOfPlenty->self, 9.0s cast, range 40 width 4 rect
    LeftFireflightTwoNights = 45919, // PariOfPlenty->self, 9.0s cast, range 40 width 4 rect
    RightFireflightThreeNights = 45459, // PariOfPlenty->self, 13.0s cast, range 40 width 4 rect
    LeftFireflightThreeNights = 45460, // PariOfPlenty->self, 13.0s cast, range 40 width 4 rect
    WheelOfFireflight1 = 45455, // PariOfPlenty->self, no cast, range 40 ?-degree cone
    WheelOfFireflight2 = 45456, // PariOfPlenty->self, no cast, range 40 180-degree cone
    WheelOfFireflight3 = 45457, // PariOfPlenty->self, no cast, range 40 ?-degree cone
    WheelOfFireflight4 = 45458, // PariOfPlenty->self, no cast, range 40 ?-degree cone
    SpurningFlames = 45480, // PariOfPlenty->self, 7.0s cast, range 40 circle
    _Ability_ImpassionedSparks = 45483, // PariOfPlenty->self, 5.0s cast, single-target
    _Ability_ImpassionedSparks1 = 45484, // PariOfPlenty->self, no cast, single-target
    _Ability_ImpassionedSparks2 = 45485, // Helper->self, 2.0s cast, single-target
    ImpassionedSparks = 45486, // Helper->self, 6.0s cast, range 8 circle
    ScouringScorn = 45489, // PariOfPlenty->self, 6.0s cast, range 40 circle

    // Path 1
    _Ability_CharmingBaubles = 45495, // PariOfPlenty->self, 3.0s cast, single-target
    BurningGleam1 = 45498, // 4A69->self, 5.0s cast, range 40 width 10 cross
    _Ability_2 = 45511, // 4A6A->location, no cast, single-target
    GaleCannon = 45514, // 4A88->self, 4.0s cast, range 35 width 10 rect, bird cast to kill sprites
    GaleForce = 45513, // 4A6A->self, 11.0s cast, range 15 circle, bird kills some sprites before this finishes

    // Path 2
    PredatorySwoop = 45510, // 4A6B->self, 5.0s cast, range 12 circle
    TranscendentFlight = 45512, // 4A6B->location, 3.0s cast, range 12 circle

    // Path 3
    StrongWindDirection = 46755, // Helper->self, 8.0s cast, range 40 width 20 rect
    StrongWindCast = 45507, // 4A6C->self, 8.0s cast, single-target, rotation for direction
    StrongWindFirst = 45508, // Helper->self, 8.0s cast, range 22 circle
    StrongWindRest = 45509, // Helper->self, no cast, range 22 circle, roughly per sec
    BurningGleam2 = 46809, // _Gen_FieryBauble->self, 6.0s cast, range 40 width 10 cross

    // Path 4
    CharmingBaublesThievesWeave = 45502, // PariOfPlenty->self, 3.0s cast, single-target
    ThievesWeave = 46753, // PariOfPlenty->self, 4.0s cast, single-target
    CarpetCover = 45504, // _Gen_FlyingCarpet->self, 1.0s cast, single-target
    CarpetJump = 45505, // _Gen_FlyingCarpet->location, no cast, single-target
    Unravel = 45506, // _Gen_FlyingCarpet->self, 3.0s cast, single-target
    BurningGleamShort = 45547, // _Gen_FieryBauble->self, 1.0s cast, range 40 width 10 cross
}
public enum SID : uint
{
    // Path 1
    _Gen_ = 2056, // PariOfPlenty->PariOfPlenty/4A69, extra=0x3EA/0x3D9/0x448/0x3DA (0x3DA = wheeloffirelight right x2?)
    _Gen_Fury = 4627, // PariOfPlenty->PariOfPlenty, extra=0x16B

}

public enum IconID : uint
{
    // Path 1
    TurningRight = 624, // PariOfPlenty->self
    TurningLeft = 625, // PariOfPlenty->self
    TurningRRight = 644, // PariOfPlenty->self
    TurningRLeft = 645, // PariOfPlenty->self

    // Path 2
    _Gen_Icon_tar_ring0af = 1, // _Gen_CapriciousChambermaid->self
}

public enum TetherID : uint
{
    // Path 1
    Fireflight = 355, // 4A75->4A75

    // Path 2
    _Gen_Tether_chn_tergetfix1f = 17, // 4A6B->_Gen_CapriciousChambermaid
}
