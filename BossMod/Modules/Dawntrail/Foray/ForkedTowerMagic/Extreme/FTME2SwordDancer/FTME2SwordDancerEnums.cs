namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME2SwordDancer;

public enum OID : uint
{
    SwordDancer = 0x4D7E,
    Helper = 0x233C,
    _Gen_ControlSigil = 0x1EBFDA, // R0.500, x1, EventObj type
    _Gen_ControlSigil1 = 0x1EBFD8, // R0.500, x1, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_DancingSword = 0x4D84, // R2.000, x16
    DancingSwordLeapingLift = 0x4D82, // R1.000, x5
    _Gen_DancingSword2 = 0x4D83, // R2.000, x2
    DancingSwordCyclosword = 0x4D81, // R2.000, x3
    DancingSwordRush = 0x4D7F, // R2.000, x4
    _Gen_SwordDancer = 0x4D85, // R1.000, x1
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    _Gen_TeleportationSigil = 0x1EBFF0, // R0.500, x1, EventObj type
    _Gen_Actor1ec032 = 0x1EC032, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_ = 0x4D86, // R1.000, x0 (spawn during fight)
    SwordDanceMarker = 0x1EC033, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    _AutoAttack_Attack = 50783, // SwordDancer->player, no cast, single-target
    SwordStorm = 49675, // SwordDancer->self, 5.0s cast, range 0 ???
    _Ability_SwordStorm = 49686, // Helper->self, no cast, range 0 ???
    _Ability_ = 49558, // SwordDancer->location, no cast, single-target
    Deathwall = 49618, // 4D85->self, no cast, range ?-30 donut

    ThrowingSwordsCast = 49619, // SwordDancer->self, 2.0+1.0s cast, single-target
    ThrowingSwords = 49620, // SwordDancer->self, no cast, single-target
    Rush1 = 49621, // DancingSwordRush->location, 3.0s cast, width 7 rect charge (far?) 24f
    Rush2 = 49622, // DancingSwordRush->location, 3.6s cast, width 7 rect charge (far?) 48f (start from end point of 1st Rush1)
    Rush3 = 50527, // DancingSwordRush->location, 3.0s cast, width 7 rect charge (near?) 11.5f
    Rush4 = 50528, // DancingSwordRush->location, 3.0s cast, width 7 rect charge (near?) 21.5f
    _Ability_Turn1 = 49623, // DancingSwordRush->location, 3.5s cast, ???
    _Ability_Turn2 = 49628, // DancingSwordRush->location, 3.5s cast, ???
    _Ability_Turn3 = 49634, // DancingSwordRush->location, 3.5s cast, ???
    Turn1 = 49635, // Helper->self, 3.5s cast, range 9-14 donut
    Turn2 = 49637, // Helper->self, 3.5s cast, range 19-24 donut
    Turnabout2 = 50064, // Helper->self, 3.5s cast, range 19-24 donut
    RushLong = 49674, // 4D84->self, 6.0s cast, range 30 width 6 rect
    _Weaponskill_ = 49676, // SwordDancer->location, no cast, single-target
    Turn3 = 49636, // Helper->self, 3.5s cast, range 14-19 donut
    Turn4 = 49639, // Helper->self, 3.5s cast, range 14-19 donut
    Turnabout1 = 50063, // Helper->self, 3.5s cast, range 14-19 donut

    MartialMystiqueCast1 = 49641, // SwordDancer->self, 4.0+1.5s cast, single-target
    MartialMystiqueCast2 = 49642, // SwordDancer->self, 4.0+1.5s cast, single-target
    MartialMystiqueCast3 = 49643, // SwordDancer->self, 4.0+1.5s cast, single-target
    MartialMystiqueCast4 = 49644, // SwordDancer->self, 4.0+1.5s cast, single-target
    MartialMystique = 49645, // Helper->self, 5.5s cast, range 48 width 96 

    CycloswordsUnsheathed = 49646, // SwordDancer->self, 3.0s cast, single-target
    _Weaponskill_Cycloswords = 49647, // SwordDancer->self, 3.0+1.0s cast, single-target
    SpinDonut10 = 49648, // DancingSwordCyclosword->self, 1.0s cast, range ?-60 donut, ModelState 0
    SpinDonut15 = 49649, // DancingSwordCyclosword->self, 1.0s cast, range 15-60 donut, ModelState 4
    SpinDonut20 = 49650, // DancingSwordCyclosword->self, 1.0s cast, range ?-60 donut, ModelState 5
    _Ability_2 = 50435, // DancingSwordCyclosword->self, no cast, single-target
    SpinCircle10 = 49651, // DancingSwordCyclosword->self, 1.0s cast, range 10 circle, ModelState 6
    SpinCircle15 = 49652, // DancingSwordCyclosword->self, 1.0s cast, range 15 circle, ModelState 7
    SpinCircle20 = 49653, // DancingSwordCyclosword->self, 1.0s cast, range 20 circle, ModelState 31

    SwordDanceCast = 49667, // SwordDancer->self, 4.4+0.6s cast, single-target
    _Ability_SwordDance = 49668, // Helper->self, 5.0s cast, range 0 ???
    _Ability_SwordDance1 = 49669, // Helper->self, no cast, range 0 ???
    _Ability_SwordDance2 = 49670, // Helper->self, no cast, range 0 ???
    _Ability_SwordDance3 = 49671, // Helper->self, no cast, range 0 ???
    SwordDance = 49672, // Helper->self, 1.5s cast, range 60 width 20 rect

    LeapingLiftCast = 49654, // SwordDancer->self, 3.0s cast, single-target
    Pierce = 49655, // 4D82->self, 3.6s cast, range 5 circle
    _Ability_LeapingLift = 49656, // SwordDancer->location, no cast, ???, 1st jump?
    _Ability_LeapingLift1 = 49657, // SwordDancer->location, no cast, single-target
    _Ability_LeapingLift2 = 49659, // SwordDancer->location, no cast, ???, last jump?
    _Weaponskill_Swordpointe = 49687, // SwordDancer->self, 2.0+1.0s cast, single-target
    _Ability_Steelsbreath = 50360, // Helper->self, 1.5s cast, range 0 ???
    Steelsbreath = 49660, // 4D82->self, 1.5s cast, range 60 ???, 26f KB
    Steelsforge = 49661, // Helper->self, 0.5s cast, range 13 circle

    _Ability_3 = 50431, // DancingSwordCyclosword->self, no cast, single-target
    _Ability_4 = 50433, // DancingSwordCyclosword->self, no cast, single-target
    _Ability_Turn6 = 49627, // DancingSwordRush->location, 3.5s cast, ???
    _Ability_Turn7 = 49633, // DancingSwordRush->location, 3.5s cast, ???
    _Ability_5 = 50432, // DancingSwordCyclosword->self, no cast, single-target
    _Ability_6 = 50436, // DancingSwordCyclosword->self, no cast, single-target
    _Ability_Turn8 = 49624, // DancingSwordRush->location, 3.5s cast, ???
    _Ability_Turn10 = 49630, // DancingSwordRush->location, 3.5s cast, ???
    SwordDanseMacabre = 49677, // SwordDancer->self, 12.0s cast, range 0 ??? enrage
    _Ability_SwordDanseMacabre1 = 49679, // Helper->self, no cast, range 0 ???
    _Ability_7 = 50434, // DancingSwordCyclosword->self, no cast, single-target
}

public enum SID : uint
{
    Cyclosword = 3558, // none->4D81, extra=0x46E/0x46D/0x46F
    LeapingLift = 2056, // none->SwordDancer/4D82, extra=0x47A/0x47B/0x495
}

public enum TetherID : uint
{
    _Gen_Tether_chn_sworddancer_l01t1 = 424, // 4D7F->SwordDancer
    _Gen_Tether_chn_sworddancer_r01t1 = 423, // 4D7F->SwordDancer
}
