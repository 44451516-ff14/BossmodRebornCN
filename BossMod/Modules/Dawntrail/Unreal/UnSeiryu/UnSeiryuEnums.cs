namespace BossMod.Dawntrail.Unreal.UnSeiryu;

public enum OID : uint
{
    Seiryu = 0x48D3, // R4.4
    AoNoShiki = 0x48D6, // R3.0
    AkaNoShiki = 0x48D5, // R2.6
    IwaNoShiki = 0x48D7, // R4.0
    TenNoShiki = 0x48DD, // R2.7
    YamaNoShiki = 0x48DA, // R12.0
    DoroNoShiki = 0x48D9, // R1.44
    NumaNoShiki = 0x48D8, // R2.4
    BlueOrochi1 = 0x48DE, // R1.0
    BlueOrochi2 = 0x48D4, // R1.0
    BlueOrochi3 = 0x48DC, // R1.0
    BlueOrochi4 = 0x48DB, // R1.0
    Tower = 0x1EAA3C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Seiryu->player, no cast, single-target
    AutoAttackAdds = 872, // DoroNoShiki/NumaNoShiki->player, no cast, single-target
    Teleport1 = 43980, // Seiryu->location, no cast, ???
    Teleport2 = 43963, // Seiryu->location, no cast, ???
    TeleportAdd = 44003, // IwaNoShiki->location, no cast, ???

    FifthElement = 43962, // Seiryu->self, 4.0s cast, range 100 circle

    KujiKiri = 43992, // Seiryu->self, 4.0s cast, single-target

    SerpentDescendingSpread = 43988, // Helper->player, 6.0s cast, range 5 circle, spread
    SerpentDescendingAOE = 43989, // Helper->location, 3.0s cast, range 5 circle

    FortuneBladeSigil1 = 43993, // Helper->self, 6.5s cast, range 50+R width 4 rect
    FortuneBladeSigil2 = 43994, // Helper->self, 8.0s cast, range 50+R width 4 rect

    OnmyoSigilVisual = 43981, // Seiryu->self, no cast, single-target
    OnmyoSigil = 44024, // Helper->self, 3.0s cast, range 12 circle
    OnmyoSigilFirstVisual = 43983, // Seiryu->self, no cast, single-target
    OnmyoSigilFirst = 44026, // Helper->self, 3.0s cast, range 12 circle
    OnmyoSigilSecondVisual = 43986, // Seiryu->self, no cast, single-target
    OnmyoSigilSecond = 44029, // Helper->self, 0.5s cast, range 12 circle
    SerpentEyeSigilSecondVisual = 43984, // Seiryu->self, no cast, single-target
    SerpentEyeSigilSecond = 44027, // Helper->self, 0.5s cast, range 7-30 donut
    SerpentEyeSigilFirstVisual = 43985, // Seiryu->self, no cast, single-target
    SerpentEyeSigilFirst = 44028, // Helper->self, 3.0s cast, range 7-30 donut

    Cursekeeper = 43977, // Seiryu->player, 5.0s cast, single-target, tank swap
    InfirmSoul = 43979, // Seiryu->player, no cast, range 4 circle, tankbuster
    KarmicCurse = 43978, // Helper->self, no cast, range 90 circle

    SummonShiki1 = 43973, // Seiryu->self, 3.0s cast, single-target
    SummonShiki2 = 43975, // Seiryu->self, 3.0s cast, single-target
    SummonShiki3 = 43972, // Seiryu->self, 3.0s cast, single-target
    SummonShiki4 = 43974, // Seiryu->self, 5.0s cast, single-target

    BlueBoltMarker = 44006, // Helper->player, no cast, single-target
    BlueBolt = 44007, // AoNoShiki->self, no cast, range 80+R width 5 rect
    RedRush = 44008, // AkaNoShiki->self, no cast, range 80+R width 5 rect, knockback 18, away from source
    HundredTonzeSwing = 44004, // IwaNoShiki->self, 4.0s cast, range 16 circle
    YamaKagura = 44030, // TenNoShiki->self, 5.0s cast, range 60+R width 6 rect
    KanaboVisual = 44020, // IwaNoShiki->self, 6.0s cast, single-target
    Kanabo = 44005, // IwaNoShiki->self, no cast, range 40+R 60-degree cone

    Explosion1 = 44010, // DoroNoShiki->self, no cast, range 80 circle, on death
    Explosion2 = 44009, // NumaNoShiki->self, no cast, range 80 circle, on death
    Stoneskin = 44011, // NumaNoShiki->self, 5.0s cast, range 10 circle, interrupt

    StrengthOfSpirit = 43968, // Seiryu->self, 5.0s cast, range 80 circle, phase change, knockback 24, away from source
    DragonsWakeVisual = 43969, // Seiryu->self, no cast, single-target
    DragonsWake = 43970, // Helper->self, 24.0s cast, range 80 circle

    GreatTyphoonCone = 44015, // Helper->self, 4.0s cast, range 20-45 30-degree donut segment
    GreatTyphoon1 = 44016, // Helper->self, 3.0s cast, range 20-28 donut
    GreatTyphoon2 = 44017, // Helper->self, 3.0s cast, range 26-34 donut
    GreatTyphoon3 = 44018, // Helper->self, 3.0s cast, range 32-40 donut

    HandprintVisual1 = 43996, // YamaNoShiki->self, no cast, single-target
    HandprintVisual2 = 43997, // YamaNoShiki->self, no cast, single-target
    Handprint1 = 43999, // Helper->self, 4.5s cast, range 40 180-degree cone, inside water
    Handprint2 = 43998, // Helper->self, 4.5s cast, range 20 180-degree cone, on land

    CoursingRiver1 = 44012, // BlueOrochi2->self, 5.0s cast, single-target
    CoursingRiver2 = 44022, // BlueOrochi3->self, 5.0s cast, single-target
    CoursingRiver3 = 44023, // BlueOrochi4->self, 5.0s cast, single-target

    CoursingRiver = 44014, // Helper->self, 7.5s cast, range 90+R width 90 rect, damage if inside water
    CoursingRiverKB = 44013, // Helper->self, 7.5s cast, range 21 circle, knockback 25, dir forward

    ForceOfNatureVisual = 44000, // YamaNoShiki->self, no cast, single-target
    ForceOfNature = 44001, // Helper->self, 5.0s cast, range 5 circle
    ForceOfNatureKB = 44002, // Helper->self, 5.0s cast, range 21 circle, knockback 10, away from source

    ForbiddenArtsMarker = 43964, // Helper->player, no cast, single-target
    ForbiddenArtsFirst1 = 43966, // Seiryu->self, no cast, range 80+R width 8 rect
    ForbiddenArtsSecond1 = 43967, // Seiryu->self, no cast, range 80+R width 8 rect
    ForbiddenArtsFirst2 = 44031, // Seiryu->self, no cast, range 80+R width 8 rect
    ForbiddenArtsSecond2 = 44032, // Seiryu->self, no cast, range 80+R width 8 rect

    BlazingAramitama = 43995, // Seiryu->self, 5.0s cast, single-target

    SerpentAscending1 = 43987, // Seiryu->self, no cast, single-target
    SerpentAscending2 = 44034, // Seiryu->self, 4.0s cast, single-target
    SerpentsJaws = 43991, // Helper->self, no cast, range 80 circle, tower fail
    SerpentsFang = 43990, // Helper->self, no cast, range 3 circle, tower

    FifthElementEnrage = 44035 // Seiryu->self, 18.0s cast, range 100 circle
}

public enum IconID : uint
{
    SerpentDescending = 169 // player->self
}

public enum TetherID : uint
{
    BlueBolt = 57, // AoNoShiki->player
    RedRush = 17, // AkaNoShiki->player
    Kanabo = 84 // IwaNoShiki->player
}
