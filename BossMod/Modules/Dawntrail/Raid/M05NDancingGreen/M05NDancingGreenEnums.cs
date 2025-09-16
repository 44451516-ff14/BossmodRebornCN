namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

public enum OID : uint
{
    Boss = 0x47B6, // R4.998
    Frogtourage = 0x47B7, // R3.142
    Spotlight = 0x47B8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44484, // Boss->player, no cast, single-target
    Teleport = 42693, // Boss->location, no cast, single-target

    DoTheHustle1 = 42697, // Boss->self, 5.0s cast, range 50 180-degree cone
    DoTheHustle2 = 42698, // Boss->self, 5.0s cast, range 50 180-degree cone

    TwoSnapTwistFirst1 = 42704, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst2 = 42701, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst3 = 42699, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst4 = 42702, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst5 = 42197, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst6 = 42700, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst7 = 42703, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst8 = 42198, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist2 = 42705, // Helper->self, 1.5s cast, range 25 width 50 rect
    TwoSnapTwist3 = 42706, // Helper->self, 3.5s cast, range 25 width 50 rect
    FourSnapTwistFirst1 = 42719, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst2 = 42720, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst3 = 42716, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst4 = 42718, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst5 = 42201, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst6 = 42717, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst7 = 42721, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst8 = 42202, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist2 = 42722, // Helper->self, 1.0s cast, range 25 width 50 rect
    FourSnapTwist3 = 42723, // Helper->self, 1.5s cast, range 25 width 50 rect
    FourSnapTwist4 = 42724, // Helper->self, 2.0s cast, range 25 width 50 rect
    FourSnapTwist5 = 42725, // Helper->self, 3.5s cast, range 25 width 50 rect

    DeepCutVisual = 42694, // Boss->self, 5.0s cast, single-target, tankbuster
    DeepCut = 42695, // Helper->self, no cast, range 60 45-degree cone

    FunkyFloorVisual = 42741, // Boss->self, 2.5+0,5s cast, single-target, checkerboard
    FunkyFloor = 42742, // Helper->self, no cast, ???

    FullBeatVisual = 42750, // Boss->self, 7.0s cast, single-target, stack
    FullBeat = 42751, // Helper->players, 7.0s cast, range 6 circle

    DiscoInfernal = 42745, // Boss->self, 4.0s cast, range 60 circle
    Shame = 42747, // Helper->player, 1.0s cast, single-target, failed spotlight

    CelebrateGoodTimes = 42696, // Boss->self, 5.0s cast, range 60 circle

    RideTheWavesVisual = 42743, // Boss->self, 3.5+0,5s cast, single-target, rectangle exaflares
    RideTheWaves = 42744, // Helper->self, no cast, ???

    EnsembleAssemble = 39472, // Boss->self, 3.0s cast, single-target
    ArcadyNightFever = 42757, // Boss->self, 4.8s cast, single-target
    FrogtourageDanceVisual1 = 42758, // Boss->self, no cast, single-target
    FrogtourageDanceVisual2 = 37825, // Helper->Frogtourage, 1.2s cast, single-target
    FrogtourageDanceVisual3 = 37824, // Boss->self, no cast, single-target
    FrogtourageDanceVisual4 = 37897, // Boss->self, no cast, single-target
    FrogtourageDanceVisual5 = 37909, // Boss->self, no cast, single-target
    FrogtourageDanceVisual6 = 37930, // Boss->self, no cast, single-target
    FrogtourageDanceVisual7 = 37836, // Frogtourage->self, no cast, single-target
    FrogtourageDanceVisualRight = 42764, // Frogtourage->self, 1.7s cast, single-target, frog changes to modelstate 5
    FrogtourageDanceVisualLeft = 42765, // Frogtourage->self, 1.7s cast, single-target, frog changes to modelstate 7
    Stone = 39092, // Boss->self, no cast, single-target

    LetsDanceVisual1 = 42772, // Boss->self, 5.8s cast, single-target
    LetsDanceVisual2 = 42775, // Boss->self, no cast, single-target
    LetsDanceVisual3 = 42776, // Boss->self, no cast, single-target
    LetsDanceVisual4 = 37835, // Frogtourage->self, no cast, single-target, frog changes to modelstate 6
    LetsDance = 39900, // Helper->self, no cast, range 25 width 90 rect

    LetsPoseVisual1 = 37844, // Frogtourage->self, 5.0s cast, single-target
    LetsPoseVisual2 = 37843, // Frogtourage->self, 5.0s cast, single-target
    LetsPose1 = 42777, // Boss->self, 5.0s cast, range 60 circle
    LetsPose2 = 42778, // Boss->self, 5.0s cast, range 60 circle

    Frogtourage = 42756, // Boss->self, 3.0s cast, single-target
    MoonburnVisualStart = 42781, // Frogtourage->self, no cast, single-target
    MoonburnVisualEnd = 42782, // Frogtourage->self, 1.0s cast, single-target
    Moonburn1 = 42784, // Helper->self, 10.5s cast, range 40 width 15 rect
    Moonburn2 = 42783, // Helper->self, 10.5s cast, range 40 width 15 rect

    EighthBeatsVisual = 42754, // Boss->self, 5.0s cast, single-target
    EighthBeats = 42755 // Helper->player, 5.0s cast, range 5 circle, spread
}

public enum IconID : uint
{
    DeepCut = 471, // player->self
    BurnBabyBurn = 601 // player->self
}

public enum SID : uint
{
    BurnBabyBurn = 4460 // Helper->player, extra=0x0
}
