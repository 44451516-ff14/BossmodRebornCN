namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

public enum OID : uint
{
    FaithboundKirin = 0x493C, // R7.99
    DawnboundSeiryu = 0x493D, // R3.2
    DuskboundByakko = 0x4940, // R6.0
    SunboundSuzaku = 0x493F, // R6.0
    MoonboundGenbu = 0x493E, // R8.75
    SculptedArm1 = 0x4941, // R8.4
    SculptedArm2 = 0x4942, // R8.4
    ChiseledArm1 = 0x4944, // R24.0
    ChiseledArm2 = 0x4A21, // R15.0
    ChiseledArm3 = 0x4A20, // R15.0
    ChiseledArm4 = 0x4943, // R24.0
    BallOfFire = 0x4946, // R1.0
    Helper = 0x233C, // R0.5
}

public enum AID : uint
{
    AutoAttack = 45306, // FaithboundKirin->player, no cast, single-target
    Teleport = 44493, // FaithboundKirin->location, no cast, single-target
    TeleportSeiryu1 = 44415, // DawnboundSeiryu->location, no cast, single-target
    TeleportSeiryu2 = 45112, // DawnboundSeiryu->location, no cast, single-target
    TeleportByakko1 = 44430, // DuskboundByakko->location, no cast, single-target
    TeleportByakko2 = 44429, // DuskboundByakko->location, no cast, single-target

    StonegaIV = 44490, // FaithboundKirin->self, 5.0s cast, range 60 circle, raidwide
    Punishment = 44491, // FaithboundKirin->player, 5.0s cast, single-target

    WroughtArms = 44433, // FaithboundKirin->self, 3.5s cast, single-target

    SynchronizedStrikeVisual1 = 44437, // FaithboundKirin->self, 4.4+0,6s cast, single-target
    SynchronizedStrikeVisual2 = 44453, // FaithboundKirin->self, 1.4+0,6s cast, single-target
    SynchronizedStrikeTelegraph = 44464, // Helper->self, 3.0s cast, range 60 width 10 rect
    SynchronizedStrike1 = 44438, // Helper->self, 5.0s cast, range 60 width 10 rect
    SynchronizedStrike2 = 44454, // Helper->self, 2.0s cast, range 60 width 10 rect
    SynchronizedSmite1 = 44443, // SculptedArm1->self, 5.0s cast, range 60 width 32 rect
    SynchronizedSmite2 = 44444, // SculptedArm2->self, 5.0s cast, range 60 width 32 rect
    SynchronizedSmite3 = 44459, // SculptedArm1->self, 2.0s cast, range 60 width 32 rect
    SynchronizedSmite4 = 44460, // SculptedArm2->self, 2.0s cast, range 60 width 32 rect
    SynchronizedSequenceVisual = 44448, // FaithboundKirin->self, 9.4+0,6s cast, single-target
    SynchronizedSequence = 44449, // Helper->self, 10.0s cast, range 60 width 10 rect

    DeadWringerVisual1 = 44440, // SculptedArm2->self, 5.0s cast, single-target
    DeadWringerVisual2 = 44456, // SculptedArm2->self, 2.0s cast, single-target
    DoubleWringer = 44445, // FaithboundKirin->self, 10.0s cast, range 14 circle
    DeadWringer1 = 44439, // SculptedArm1->self, 5.0s cast, range 14-30 donut
    DeadWringer2 = 44455, // SculptedArm1->self, 2.0s cast, range 14-30 donut
    WringerTelegraph = 44461, // Helper->self, 3.0s cast, range 14 circle
    Wringer1 = 44434, // FaithboundKirin->self, 5.0s cast, range 14 circle
    Wringer2 = 44450, // FaithboundKirin->self, 2.0s cast, range 14 circle

    StrikingRightTelegraph = 44462, // Helper->self, 3.0s cast, range 10 circle
    StrikingLeftTelegraph = 44463, // Helper->self, 3.0s cast, range 10 circle
    StrikingRight1 = 44435, // FaithboundKirin->self, 5.0s cast, range 10 circle
    StrikingRight2 = 44451, // FaithboundKirin->self, 2.0s cast, range 10 circle
    StrikingLeft1 = 44436, // FaithboundKirin->self, 5.0s cast, range 10 circle
    StrikingLeft2 = 44452, // FaithboundKirin->self, 2.0s cast, range 10 circle
    SmitingLeft1 = 44442, // SculptedArm2->self, 5.0s cast, range 30 circle
    SmitingLeft2 = 44458, // SculptedArm2->self, 2.0s cast, range 30 circle
    SmitingRight1 = 44441, // SculptedArm1->self, 5.0s cast, range 30 circle
    SmitingRight2 = 44457, // SculptedArm1->self, 2.0s cast, range 30 circle
    SmitingLeftSequence = 44447, // FaithboundKirin->self, 10.0s cast, range 10 circle
    SmitingRightSequence = 44446, // FaithboundKirin->self, 10.0s cast, range 10 circle

    CrimsonRiddle1 = 45044, // FaithboundKirin->self, 5.0s cast, range 30 180-degree cone
    CrimsonRiddle2 = 45045, // FaithboundKirin->self, 5.0s cast, range 30 180-degree cone

    SummonShijin = 44414, // FaithboundKirin->self, 7.0s cast, single-target

    EastwindWheelCWVisual = 44416, // DawnboundSeiryu->self, 8.0s cast, range 60 width 18 rect
    EastwindWheelCCWVisual = 44417, // DawnboundSeiryu->self, 8.0s cast, range 60 width 18 rect
    EastwindWheelRect = 44418, // Helper->self, no cast, range 60 width 18 rect
    EastwindWheelCone = 44419, // Helper->self, no cast, range 60 90-degree cone

    WallArenaChange = 44495, // Helper->self, 2.0s cast, range 5 width 16 rect
    GloamingGleam = 44431, // DuskboundByakko->location, 3.0s cast, range 50 width 12 rect
    RazorFang = 44432, // Helper->self, 4.5s cast, range 20 circle

    SunPowder = 44425, // SunboundSuzaku->location, no cast, single-target
    VermilionFlightVisual = 44426, // SunboundSuzaku->self, 4.5+3,5s cast, single-target
    VermilionFlight = 44795, // Helper->self, 8.0s cast, range 60 width 20 rect
    ArmOfPurgatoryVisual = 44796, // BallOfFire->self, 5.5s cast, single-target
    ArmOfPurgatory = 44794, // Helper->self, 11.0s cast, range 3 circle

    ShatteringStomp = 44420, // MoonboundGenbu->self, 3.0s cast, range 35 circle, raidwide
    MoontideFont1 = 44421, // Helper->self, 8.0s cast, range 9 circle
    MoontideFont2 = 44422, // Helper->self, 8.2s cast, range 9 circle
    MidwinterMarchVisual = 44423, // MoonboundGenbu->location, 6.0+1,0s cast, single-target
    MidwinterMarch = 44337, // Helper->location, 7.0s cast, range 12 circle
    NorthernCurrent = 44424, // MoonboundGenbu->self, 3.0s cast, range 12-60 donut

    DoubleCast = 43286, // FaithboundKirin->self, 3.0s cast, single-target
    QuakeSmall = 43288, // Helper->self, 8.0s cast, range 6 circle
    QuakeVisual = 45179, // FaithboundKirin->self, 3.0s cast, single-target
    QuakeBig = 45180, // Helper->self, 5.0s cast, range 10 circle

    StonegaIIIVisual = 45177, // FaithboundKirin->self, 3.0s cast, single-target
    StonegaIII1 = 43287, // Helper->players, 8.0s cast, range 6 circle, spread
    StonegaIII2 = 45178, // Helper->players, 8.0s cast, range 6 circle, spread

    MightyGrip = 44465, // FaithboundKirin->self, 7.0s cast, single-target
    Shockwave = 44480, // Helper->self, no cast, range 60 circle
    DeadlyHold1 = 44466, // FaithboundKirin->self, 12.5s cast, single-target, towers
    DeadlyHold2 = 44928, // ChiseledArm2/ChiseledArm3->self, 12.5s cast, single-target
    DeadlyHold3 = 44470, // ChiseledArm1->self, 14.0s cast, single-target
    DeadlyHold4 = 44469, // ChiseledArm4->self, 14.0s cast, single-target
    DeadlyHoldVisual1 = 44472, // ChiseledArm1->self, no cast, single-target
    DeadlyHoldVisual2 = 44471, // ChiseledArm4->self, no cast, single-target
    DeadlyHoldVisual3 = 44474, // ChiseledArm1->self, no cast, single-target
    DeadlyHoldVisual4 = 44473, // ChiseledArm4->self, no cast, single-target
    Bury = 44479, // Helper->player, no cast, single-target, tankbuster, 6.6s after towers end
    DeadlyHoldVisual5 = 44468, // FaithboundKirin->self, no cast, single-target
    DeadlyHoldEnrage1 = 44475, // ChiseledArm4->self, no cast, range 60 circle
    DeadlyHoldEnrage2 = 44476, // ChiseledArm1->self, no cast, range 60 circle

    KirinCaptivatorVisual1 = 44467, // FaithboundKirin->self, 30.0s cast, single-target
    KirinCaptivatorVisual2 = 44929, // ChiseledArm2/ChiseledArm3->self, 30.0s cast, single-target
    KirinCaptivatorEnrage1 = 44478, // ChiseledArm1->self, 31.5s cast, range 60 circle
    KirinCaptivatorEnrage2 = 44477, // ChiseledArm4->self, 31.5s cast, range 60 circle
    KirinCaptivatorEnrageRepeat1 = 44496, // ChiseledArm4->self, no cast, range 60 circle
    KirinCaptivatorEnrageRepeat2 = 44497 // ChiseledArm1->self, no cast, range 60 circle
}

public enum SID : uint
{
    StandingFirm = 4618 // none->player, extra=0x0
}
