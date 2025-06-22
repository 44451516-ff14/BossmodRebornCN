namespace BossMod.Dawntrail.Unreal.UnSuzaku;

public enum OID : uint
{
    Boss = 0x47D1, // R2.8-6.93

    ScarletLady = 0x47D2, // R1.12
    ScarletPlume = 0x47D3, // R1.0
    ScarletTailFeather = 0x47D4, // R1.8

    RapturousEcho = 0x47D5, // R1.5
    RapturousEchoPlatform = 0x1EA1A1, // R2.0

    SongOfSorrow = 0x47CE, // R1.5
    SongOfOblivion = 0x47CF, // R1.5
    SongOfFire = 0x47CD, // R1.5
    SongOfDurance = 0x47D0, // R1.5

    NorthernPyre = 0x47D7, // R2.0
    EasternPyre = 0x47D8, // R2.0
    SouthernPyre = 0x47D9, // R2.0
    WesternPyre = 0x47DA, // R2.0

    Towers = 0x1EA9FF, // R0.5

    Helper3 = 0x47DB, // R1.0
    Helper2 = 0x47D6, // R2.1
    Helper1 = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 871, // Boss->player, no cast, single-target
    AutoAttack2 = 12863, // Boss->player, no cast, single-target
    AutoAttackScarletLady = 43030, // ScarletLady->player, no cast, single-target
    Teleport = 42994, // Boss->location, no cast, single-target

    Cremate = 43003, // Boss->player, 3s cast, single-target tankbuster
    AshesToAshes = 43002, // ScarletLady->self, 3s cast, range 40 circle raidwide

    ScreamsOfTheDamned = 43004, // Boss->self, 3s cast, range 40 circle raidwide
    ScarletFever = 43009, // Helper1->self, 7s cast, range 41 circle raidwide, arena change
    SouthronStar = 43015, // Boss->self, 4s cast, range 41 circle raidwide
    Rout = 43027, // Boss->self, 3s cast, range 55 width 6 rect

    Rekindle = 43016, // Helper1->players, no cast, range 6 circle, spread

    FleetingSummer = 43005, // Boss->self, 3s cast, range 40 90-degree cone

    PhoenixDown = 42992, // Boss->self, 3s cast, single-target
    WingAndAPrayerPlume = 43001, // ScarletPlume->self, 20s cast, range 9 circle
    WingAndAPrayerTailFeather = 43006, // ScarletTailFeather->self, 20s cast, range 9 circle
    EternalFlame = 42991, // Boss->self, 3s cast, range 80 circle
    LovesTrueForm = 42993, // Boss->self, no cast, single-target

    RapturousEcho1 = 43028, // Boss->self, no cast, single-target
    RapturousEcho2 = 43029, // Boss->self, no cast, single-target
    ScarletHymn = 42995, // Boss->self, no cast, single-target
    ScarletHymnPlayer = 43007, // RapturousEcho->player, no cast, single-target
    ScarletHymnBoss = 43008, // RapturousEcho->Suzaku3, no cast, single-target

    MesmerizingMelody = 43010, // Boss->self, 4s cast, range 41 circle, pull 11 between centers
    RuthlessRefrain = 43011, // Boss->self, 4s cast, range 41 circle, knockback 11, away from source

    WellOfFlame = 43017, // Boss->self, 4s cast, range 41 width 20 rect

    ScathingNet = 43000, // Helper1->player, no cast, range 6 circle, stack

    PhantomFlurryVisual = 43012, // Boss->self, 4s cast, single-target, tank swap tankbuster
    PhantomFlurryTB = 43013, // Helper->players, no cast, single-target
    PhantomFlurryAOE = 43014, // Helper->self, 6s cast, range 41 180-degree cone

    Hotspot = 43018, // Helper->self, 0.9s cast, range 21 90-degree cone
    HotspotsEnd = 42997, // Boss->self, no cast, single-target

    CloseQuarterCrescendo = 43019, // Boss->self, 4s cast, single-target
    PayThePiperNorth = 43022, // NorthernPyre->player, no cast, single-target
    PayThePiperEast = 43023, // EasternPyre->player, no cast, single-target
    PayThePiperSouth = 43025, // SouthernPyre->player, no cast, single-target
    PayThePiperWest = 43024, // WesternPyre->player, no cast, single-target

    IncandescentInterlude = 42998, // Boss->self, 4.0s cast, single-target
    Immolate = 43021, // Helper->self, no cast, range 41 circle, tower fail
    Burn = 43020, // Helper->self, no cast, range 4 circle, tower success

    EnrageSouthronStar = 43026, // Boss->self, 39.0s cast, range 41 circle
    HotspotEnrage = 43031 // Helper->self, 0.9s cast, range 21 90-degree cone
}

public enum SID : uint
{
    PrimaryTarget = 1689, // ScarletLady->player, extra=0x0
    PayingThePiper = 1681 // WesternPyre/EasternPyre/SouthernPyre->player, extra=0x8/0x4/0x2/0x1
}

public enum IconID : uint
{
    Spreadmarker = 139, // player
    Stackmarker = 161 // player
}

public enum TetherID : uint
{
    Birds = 14, // ScarletLady->ScarletLady
    PayThePiper = 79 // Helper2/WesternPyre/EasternPyre/SouthernPyre/NorthernPyre->Boss/player
}
