namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

public enum OID : uint
{
    Boss = 0x2464, // R2.8-6.93

    ScarletLady = 0x2465, // R1.12
    ScarletPlume = 0x2466, // R1.0
    ScarletTailFeather = 0x2467, // R1.8

    RapturousEcho = 0x2468, // R1.5
    RapturousEchoPlatform = 0x1EA1A1, // R2.0

    SongOfSorrow = 0x2461, // R1.5
    SongOfOblivion = 0x2462, // R1.5
    SongOfFire = 0x2460, // R1.5
    SongOfDurance = 0x2463, // R1.5

    NorthernPyre = 0x246C, // R2.0
    EasternPyre = 0x246D, // R2.0
    SouthernPyre = 0x246E, // R2.0
    WesternPyre = 0x246F, // R2.0

    Towers = 0x1EA9FF, // R0.5

    Helper3 = 0x2570, // R1.0
    Helper2 = 0x246B, // R2.1
    Helper1 = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 871, // Boss->player, no cast, single-target
    AutoAttack2 = 12863, // Boss->player, no cast, single-target
    AutoAttackScarletLady = 14065, // ScarletLady->player, no cast, single-target
    Teleport = 12846, // Boss->location, no cast, single-target

    Cremate = 13009, // Boss->player, 3s cast, single-target tankbuster
    AshesToAshes = 13008, // ScarletLady->self, 3s cast, range 40 circle raidwide

    ScreamsOfTheDamned = 13010, // Boss->self, 3s cast, range 40 circle raidwide
    ScarletFever = 13017, // Helper1->self, 7s cast, range 41 circle raidwide, arena change
    SouthronStar = 13023, // Boss->self, 4s cast, range 41 circle raidwide
    HotspotsEnd = 12858, // Boss->self, no cast, single-target

    Rout = 13040, // Boss->self, 3s cast, range 55 width 6 rect

    Rekindle = 13024, // Helper1->players, no cast, range 6 circle, spread

    FleetingSummer = 13011, // Boss->self, 3s cast, range 40 90-degree cone

    PhoenixDown = 12836, // Boss->self, 3s cast, single-target
    WingAndAPrayerPlume = 12868, // ScarletPlume->self, 20s cast, range 9 circle
    WingAndAPrayerTailFeather = 13012, // ScarletTailFeather->self, 20s cast, range 9 circle
    EternalFlame = 12834, // Boss->self, 3s cast, range 80 circle
    LovesTrueForm = 12838, // Boss->self, no cast, single-target

    RapturousEcho1 = 13445, // Boss->self, no cast, single-target
    RapturousEcho2 = 13446, // Boss->self, no cast, single-target
    ScarletHymn = 12855, // Boss->self, no cast, single-target
    ScarletHymnPlayer = 13014, // RapturousEcho->player, no cast, single-target
    ScarletHymnBoss = 13015, // RapturousEcho->Suzaku3, no cast, single-target

    MesmerizingMelody = 13018, // Boss->self, 4s cast, range 41 circle, pull 11 between centers
    RuthlessRefrain = 13019, // Boss->self, 4s cast, range 41 circle, knockback 11, away from source

    WellOfFlame = 13025, // Boss->self, 4s cast, range 41 width 20 rect

    ScathingNet = 12867, // Helper1->player, no cast, range 6 circle, stack

    PhantomFlurryVisual = 13020, // Boss->self, 4s cast, single-target, tank swap tankbuster
    PhantomFlurryTB = 13021, // Helper->players, no cast, single-target
    PhantomFlurryAOE = 13022, // Helper->self, 6s cast, range 41 180-degree cone

    Hotspot = 13026, // Helper->self, 0.9s cast, range 21 90-degree cone

    CloseQuarterCrescendo = 13028, // Boss->self, 4s cast, single-target
    PayThePiperNorth = 13031, // NorthernPyre->player, no cast, single-target
    PayThePiperEast = 13032, // EasternPyre->player, no cast, single-target
    PayThePiperSouth = 13034, // SouthernPyre->player, no cast, single-target
    PayThePiperWest = 13033, // WesternPyre->player, no cast, single-target

    IncandescentInterlude = 12860, // Boss->self, 4.0s cast, single-target
    Immolate = 13030, // Helper->self, no cast, range 41 circle
    Burn = 13029, // Helper->self, no cast, range 4 circle

    EnrageSouthronStar = 13036, // Boss->self, 39.0s cast, range 41 circle
    HotspotEnrage = 14067 // Helper->self, 0.9s cast, range 21 90-degree cone
}

public enum SID : uint
{
    PrimaryTarget = 1689, // ScarletLady->player, extra=0x0
    Burns = 530, // ScarletLady->player, extra=0x1/0x3/0x4/0x2
    HPBoost = 586, // none->ScarletLady, extra=0x1/0x2/0xE/0x10/0xF
    Suppuration = 375, // ScarletLady->player, extra=0x1/0x2/0x4/0x5
    VulnerabilityDown = 350, // none->ScarletLady, extra=0x0
    LovesTrueForm = 1630, // Boss->Boss, extra=0xC6
    DamageUp = 505, // RapturousEcho->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA
    PhysicalVulnerabilityUp = 695, // Helper->player, extra=0x0
    VulnerabilityUp = 202, // Helper/Boss->player, extra=0x1/0x2/0x3/0x4
    Stun = 149, // Helper->player, extra=0x0
    LoomingCrescendo = 1699, // none->player, extra=0x0
    PayingThePiper = 1681, // WesternPyre/EasternPyre/SouthernPyre->player, extra=0x8/0x4/0x2/0x1
    DamageDown = 1016, // Helper->player, extra=0x3
    FireResistanceDownII = 1255, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Spreadmarker = 139, // player
    Stackmarker = 161, // player
}

public enum TetherID : uint
{
    Birds = 14, // ScarletLady->ScarletLady
    PayThePiper = 79, // Helper2/WesternPyre/EasternPyre/SouthernPyre/NorthernPyre->Boss/player
}

