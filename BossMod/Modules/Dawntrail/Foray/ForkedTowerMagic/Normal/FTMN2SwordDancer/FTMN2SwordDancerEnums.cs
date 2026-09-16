namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

public enum OID : uint
{
    SwordDancer = 0x4D76, // R6.000, x1

    DancingSwordCyclosword = 0x4D79, // R2.000, x3
    DancingSwordSteelsbreath = 0x4D7A, // R1.000, x5
    DancingSwordSurgesword = 0x4D7C, // R2.000, x16
    SwordDanceMarker = 0x1EC033, // R0.500, x0 (spawn during fight), EventObj type
    DancingSword1 = 0x4D7B, // R2.000, x2
    DancingSword2 = 0x4D77, // R2.000, x4
    Deathwall = 0x4D7D, // R1.000, x1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50925, // SwordDancer->player, no cast, single-target
    Teleport = 49558, // SwordDancer->location, no cast, single-target

    Deathwall = 49557, // 4D7D->self, no cast, range 24-30 donut

    SwordStormCast = 49617, // SwordDancer->self, 5.0s cast, ???
    SwordStorm = 49684, // DancingSword5->self, no cast, ???
    ThrowingSwords1 = 49559, // SwordDancer->self, 2.0+1.0s cast, single-target
    ThrowingSwords2 = 49560, // SwordDancer->self, no cast, single-target

    Rush1 = 50525, // DancingSword4->location, 3.0s cast, width 7 rect charge
    Rush2 = 50526, // DancingSword4->location, 3.0s cast, width 7 rect charge
    TurnInner1 = 49575, // DancingSword5->self, 3.5s cast, range 9-14 donut
    TurnOuter1 = 49577, // DancingSword5->self, 3.5s cast, range 19-24 donut
    TurnInner2 = 49578, // DancingSword5->self, 3.5s cast, range 9-14 donut
    TurnOuter2 = 49580, // DancingSword5->self, 3.5s cast, range 19-24 donut
    TurnaboutInner = 49883, // DancingSword5->self, 3.5s cast, range 9-14 donut
    TurnaboutOuter = 49889, // DancingSword5->self, 3.5s cast, range 19-24 donut
    Turn1 = 49565, // DancingSword2->location, 3.5s cast, ???
    Turn2 = 49566, // DancingSword2->location, 3.5s cast, ???
    Turn3 = 49572, // DancingSword2->location, 3.5s cast, ???
    Turn4 = 49571, // DancingSword2->location, 3.5s cast, ???
    Turn5 = 49568, // 4D77->location, 3.5s cast, ???
    Turn6 = 49563, // 4D77->location, 3.5s cast, ???
    Turn7 = 49574, // 4D77->location, 3.5s cast, ???
    Turn8 = 49569, // 4D77->location, 3.5s cast, ???

    MartialMystiqueVisual1 = 49584, // SwordDancer->self, 4.0+1,5s cast, single-target
    MartialMystiqueVisual2 = 49583, // SwordDancer->self, 4.0+1,5s cast, single-target
    MartialMystique = 49585, // DancingSword5->self, 5.5s cast, range 48 width 96 rect

    CycloswordsUnsheathed = 49586, // SwordDancer->self, 3.0s cast, single-target
    Cycloswords = 49587, // SwordDancer->self, 3.0s cast, single-target
    Spin = 49589, // DancingSword3->self, 1.0s cast, range 15-60 donut
    Spin1 = 49590, // DancingSwordCyclosword->self, 1.0s cast, range 20-60 donut
    Spin2 = 49592, // DancingSword3->self, 1.0s cast, range 15 circle
    Spin3 = 49593, // DancingSword3->self, 1.0s cast, range 20 circle

    SwordDanceCast = 49609, // SwordDancer->self, 4.4+0.6s cast, single-target
    SwordDance = 49614, // DancingSword5->self, 1.5s cast, range 60 width 20 rect
    SwordDance1 = 49610, // Helper->self, 5.0s cast, range 0 ???
    SwordDance2 = 49611, // Helper->self, no cast, range 0 ???
    SwordDance3 = 49612, // Helper->self, no cast, range 0 ???
    SwordDance4 = 49613, // Helper->self, no cast, range 0 ???

    LeapingLiftVisual = 49594, // SwordDancer->self, 3.0s cast, single-target
    LeapingLiftTeleport = 49597, // SwordDancer->location, no cast, single-target
    LeapingLift1 = 49596, // SwordDancer->location, no cast, ???
    LeapingLift2 = 49598, // SwordDancer->location, no cast, ???

    Pierce = 49595, // DancingSword2->self, 3.6s cast, range 5 circle

    Swordpointe = 49685, // SwordDancer->self, 2.0+1.0s cast, single-target
    Steelsbreath = 50359, // DancingSword5->self, 2.0s cast, ???
    Steelsbreath1 = 49599, // DancingSword2->self, 2.0s cast, ???

    SurgeswordsUnsheathed = 49615, // SwordDancer->self, 3.0s cast, single-target
    RushSurgesword = 49616, // DancingSword->self, 4.0s cast, range 30 width 6 rect
}
public enum SID : uint
{
    Cyclosword = 3558, // none->DancingSword3, extra=0x46E/0x46F, cyclosword spin
    LeapingLift = 2056, // none->SwordDancer/DancingSword2, extra=0x47A/0x47B
}
public enum TetherID : uint
{
    Tether_chn_sworddancer_r01t1 = 423, // DancingSword4->SwordDancer
    Tether_chn_sworddancer_l01t1 = 424, // DancingSword4->SwordDancer
}
