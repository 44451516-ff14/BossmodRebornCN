namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

public enum OID : uint
{
    Boss = 0x47B9, // R4.998
    Frogtourage = 0x47BA, // R3.142
    Spotlight = 0x47BB, // R1.0
    DancingGreen = 0x233C
}

public enum AID : uint
{
    AutoAttack = 41767, // Boss->player, no cast, single-target
    Teleport = 42693, // Boss->location, no cast, single-target

    DeepCutVisual = 42785, // Boss->self, 5.0s cast, single-target
    DeepCut = 42786, // DancingGreen->self, no cast, range 60 45-degree cone

    // left: -90.004°, right: 89.999°
    TwoSnapTwistDropTheNeedleFirst1 = 42792, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    TwoSnapTwistDropTheNeedleFirst2 = 42793, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    TwoSnapTwistDropTheNeedleFirst3 = 42794, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    TwoSnapTwistDropTheNeedleFirst4 = 42795, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    TwoSnapTwistDropTheNeedleFirst5 = 42796, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    TwoSnapTwistDropTheNeedleFirst6 = 42797, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    TwoSnapTwistDropTheNeedleFirst7 = 42203, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    TwoSnapTwistDropTheNeedleFirst8 = 42204, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    TwoSnapTwistDropTheNeedle2 = 42798, // DancingGreen->self, 1.5s cast, range 25 width 50 rect
    TwoSnapTwistDropTheNeedle3 = 42799, // DancingGreen->self, 3.5s cast, range 25 width 50 rect

    ThreeSnapTwistDropTheNeedleFirst1 = 42205, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    ThreeSnapTwistDropTheNeedleFirst2 = 42206, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    ThreeSnapTwistDropTheNeedleFirst3 = 42800, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    ThreeSnapTwistDropTheNeedleFirst4 = 42801, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    ThreeSnapTwistDropTheNeedleFirst5 = 42802, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    ThreeSnapTwistDropTheNeedleFirst6 = 42803, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    ThreeSnapTwistDropTheNeedleFirst7 = 42804, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    ThreeSnapTwistDropTheNeedleFirst8 = 42805, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    ThreeSnapTwistDropTheNeedle2 = 42806, // DancingGreen->self, 1.2s cast, range 25 width 50 rect
    ThreeSnapTwistDropTheNeedle3 = 42807, // DancingGreen->self, 1.9s cast, range 25 width 50 rect
    ThreeSnapTwistDropTheNeedle4 = 42808, // DancingGreen->self, 3.5s cast, range 25 width 50 rect

    FourSnapTwistDropTheNeedleFirst1 = 42207, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    FourSnapTwistDropTheNeedleFirst2 = 42208, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    FourSnapTwistDropTheNeedleFirst3 = 42809, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    FourSnapTwistDropTheNeedleFirst4 = 42810, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    FourSnapTwistDropTheNeedleFirst5 = 42811, // Boss->self, 5.0s cast, range 20 width 40 rect, right
    FourSnapTwistDropTheNeedleFirst6 = 42812, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    FourSnapTwistDropTheNeedleFirst7 = 42813, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    FourSnapTwistDropTheNeedleFirst8 = 42814, // Boss->self, 5.0s cast, range 20 width 40 rect, left
    FourSnapTwistDropTheNeedle2 = 42815, // DancingGreen->self, 1.0s cast, range 25 width 50 rect
    FourSnapTwistDropTheNeedle3 = 42816, // DancingGreen->self, 1.5s cast, range 25 width 50 rect
    FourSnapTwistDropTheNeedle4 = 42817, // DancingGreen->self, 2.0s cast, range 25 width 50 rect
    FourSnapTwistDropTheNeedle5 = 42818, // DancingGreen->self, 3.5s cast, range 25 width 50 rect

    FlipToBSide = 42881, // Boss->self, 4.0s cast, single-target
    PlayBSideVisual = 37833, // Boss->self, no cast, single-target
    PlayBSide = 42884, // DancingGreen->self, no cast, range 50 width 8 rect
    FlipToASide = 42880, // Boss->self, 4.0s cast, single-target
    PlayASideVisual = 37832, // Boss->self, no cast, single-target
    PlayASide = 42883, // DancingGreen->self, no cast, range 60 45-degree cone

    CelebrateGoodTimes = 42787, // Boss->self, 5.0s cast, range 60 circle
    DiscoInfernal = 42838, // Boss->self, 4.0s cast, range 60 circle

    FunkyFloorVisual = 42834, // Boss->self, 2.5+0,5s cast, single-target, checkerboard
    FunkyFloor = 42835, // DancingGreen->self, no cast, ???

    InsideOutVisual1 = 42876, // Boss->self, 5.0s cast, single-target
    InsideOutVisual2 = 42877, // Boss->self, no cast, single-target
    OutsideInVisual1 = 42878, // Boss->self, 5.0s cast, single-target
    OutsideInVisual2 = 42879, // Boss->self, no cast, single-target
    InsideOutCircle = 37826, // DancingGreen->self, no cast, range 7 circle
    InsideOuDonut = 37827, // DancingGreen->self, no cast, range 5-40 donut
    OutsideInDonut = 37828, // DancingGreen->self, no cast, range 5-40 donut
    OutsideInCircle = 37829, // DancingGreen->self, no cast, range 7 circle

    EnsembleAssemble = 39474, // Boss->self, 3.0s cast, single-target
    ArcadyNightFever = 42848, // Boss->self, 4.8s cast, single-target
    GetDownBait = 42852, // DancingGreen->self, no cast, range 40 45-degree cone
    GetDownCone = 42853, // DancingGreen->self, 2.5s cast, range 40 45-degree cone
    GetDownDonut = 42851, // DancingGreen->self, no cast, range 5-40 donut
    GetDownCircle1 = 39908, // DancingGreen->self, 5.2s cast, range 7 circle
    GetDownCircle2 = 42850, // DancingGreen->self, no cast, range 7 circle

    FrogtourageDanceVisual1 = 42849, // Boss->self, no cast, single-target
    FrogtourageDanceVisual2 = 37825, // DancingGreen->Frogtourage, 1.2s cast, single-target
    FrogtourageDanceVisual3 = 37830, // Boss->self, no cast, single-target
    FrogtourageDanceVisual4 = 38464, // Boss->self, no cast, single-target
    FrogtourageDanceVisual5 = 38465, // Boss->self, no cast, single-target
    FrogtourageDanceVisual6 = 39091, // Boss->self, no cast, single-target
    FrogtourageDanceVisual7 = 39907, // Frogtourage->self, no cast, single-target
    FrogtourageDanceVisual8 = 39906, // Frogtourage->self, no cast, single-target
    FrogtourageDanceVisualRight = 42764, // Frogtourage->self, 1.7s cast, single-target, frog changes to modelstate 5
    FrogtourageDanceVisualLeft = 42765, // Frogtourage->self, 1.7s cast, single-target, frog changes to modelstate 7

    Fire = 39093, // Boss->self, no cast, single-target
    LetsDanceVisual1 = 42858, // Boss->self, 5.8s cast, single-target
    LetsDanceVisual2 = 42862, // Boss->self, no cast, single-target
    LetsDanceVisual3 = 42861, // Boss->self, no cast, single-target
    LetsDance = 39901, // DancingGreen->self, no cast, range 25 width 50 rect

    FrogtourageLetsPoseVisual1 = 37844, // Frogtourage->self, 5.0s cast, single-target
    FrogtourageLetsPoseVisual2 = 37843, // Frogtourage->self, 5.0s cast, single-target
    LetsPose = 42863, // Boss->self, 5.0s cast, range 60 circle

    RideTheWavesVisual = 42836, // Boss->self, 3.5+0,5s cast, single-target
    RideTheWaves = 42837, // DancingGreen->self, no cast, ???

    EighthBeatsVisual = 42845, // Boss->self, 5.0s cast, single-target, spread
    EighthBeats = 42846, // DancingGreen->player, 5.0s cast, range 5 circle

    QuarterBeatsVisual = 42843, // Boss->self, 5.0s cast, single-target, light party stack
    QuarterBeats = 42844, // DancingGreen->players, 5.0s cast, range 4 circle

    Frogtourage = 42847, // Boss->self, 3.0s cast, single-target
    MoonburnVisualStart = 42781, // Frogtourage->self, no cast, single-target
    MoonburnVisualEnd = 42782, // Frogtourage->self, 1.0s cast, single-target
    Moonburn1 = 42867, // DancingGreen->self, 10.5s cast, range 40 width 15 rect
    Moonburn2 = 42868, // DancingGreen->self, 10.5s cast, range 40 width 15 rect
    BackUpDanceVisual = 42871, // Frogtourage->self, 8.9s cast, single-target
    BackUpDance = 42872, // DancingGreen->self, no cast, range 60 45-degree cone
    Shame = 42840, // DancingGreen->player, 1.0s cast, single-target

    ArcadyNightEncore = 41840, // Boss->self, 4.8s cast, single-target
    FrogtourageDanceRemixVisualBack = 42763, // Frogtourage->self, 1.7s cast, single-target, single-target, frog changes to modelstate 32, 180°
    FrogtourageDanceVisualRemixFront = 42762, // Frogtourage->self, 1.7s cast, single-target, single-target, frog changes to modelstate 31, -0.003°
    LetsDanceRemixVisual1 = 41872, // Boss->self, 5.8s cast, single-target
    LetsDanceRemixVisual2 = 41876, // Boss->self, no cast, single-target
    LetsDanceRemixVisual3 = 41875, // Boss->self, no cast, single-target
    LetsDanceRemixVisual4 = 41874, // Boss->self, no cast, single-target
    LetsDanceRemixVisual5 = 41873, // Boss->self, no cast, single-target
    FrogtourageDanceRemixVisual1 = 41837, // Frogtourage->self, no cast, single-target
    FrogtourageDanceRemixVisual2 = 41836, // Frogtourage->self, no cast, single-target
    FrogtourageDanceRemixVisual3 = 41839, // Frogtourage->self, no cast, single-target
    FrogtourageDanceRemixVisual4 = 41838, // Frogtourage->self, no cast, single-target
    FrogtourageDanceRemixVisual5 = 39904, // Frogtourage->self, 5.0s cast, single-target
    FrogtourageDanceRemixVisual6 = 39905, // Frogtourage->self, 5.0s cast, single-target
    LetsPoseRemix = 42864, // Boss->self, 5.0s cast, range 60 circle
    LetsDanceRemix = 41877, // DancingGreen->self, no cast, range 25 width 50 rect

    DoTheHustle1 = 42869, // Frogtourage->self, 5.0s cast, range 50 180-degree cone, right (caster rot +90°)
    DoTheHustle2 = 42870, // Frogtourage->self, 5.0s cast, range 50 180-degree cone, left (caster rot -90°)
    DoTheHustle3 = 42789, // Boss->self, 5.0s cast, range 50 180-degree cone, left
    DoTheHustle4 = 42788, // Boss->self, 5.0s cast, range 50 180-degree cone, right

    FreakOut1 = 42854, // DancingGreen->player, no cast, single-target
    FreakOut2 = 42855, // DancingGreen->player, no cast, single-target
    MinorFreakOut1 = 39478, // DancingGreen->location, no cast, range 2 circle
    MinorFreakOut2 = 39475, // DancingGreen->location, no cast, range 2 circle
    MinorFreakOut3 = 42856, // DancingGreen->location, no cast, range 2 circle
    MinorFreakOut4 = 39476, // DancingGreen->location, no cast, range 2 circle
    MajorFreakOut = 42857, // DancingGreen->players, no cast, range 60 circle

    FrogtourageFinale = 42209, // Boss->self, 3.0s cast, single-target
    FrogtourageFinaleSpawn = 42874, // Frogtourage->self, no cast, single-target
    FrogtourageFinaleFinish = 42875, // Frogtourage->self, no cast, single-target
    HiNRGFever = 42873 // Boss->self, 12.0s cast, range 60 circle, enrage
}

public enum IconID : uint
{
    DeepCut = 471 // player->self
}

public enum SID : uint
{
    BurnBabyBurn = 4461, // Helper->player, extra=0x0
    WavelengthAlpha = 4462, // none->player, extra=0x0
    WavelengthBeta = 4463 // none->player, extra=0x0
}
