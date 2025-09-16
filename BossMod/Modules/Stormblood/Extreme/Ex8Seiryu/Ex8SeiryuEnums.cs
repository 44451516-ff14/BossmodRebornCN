namespace BossMod.Stormblood.Extreme.Ex8Seiryu;

public enum OID : uint
{
    Seiryu = 0x25EC, // R4.4
    AoNoShiki = 0x25EF, // R3.0
    AkaNoShiki = 0x25EE, // R2.6
    IwaNoShiki = 0x25F0, // R4.0
    TenNoShiki = 0x2777, // R2.7
    YamaNoShiki = 0x25F3, // R12.0
    DoroNoShiki = 0x25F2, // R1.44
    NumaNoShiki = 0x25F1, // R2.4
    BlueOrochi1 = 0x2672, // R1.0
    BlueOrochi2 = 0x2657, // R1.0
    BlueOrochi3 = 0x25ED, // R1.0
    BlueOrochi4 = 0x2656, // R1.0
    Tower = 0x1EAA3C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Seiryu->player, no cast, single-target
    AutoAttackAdds = 872, // DoroNoShiki/NumaNoShiki->player, no cast, single-target
    Teleport1 = 14276, // Seiryu->location, no cast, ???
    Teleport2 = 14293, // Seiryu->location, no cast, ???
    TeleportAdd = 14316, // IwaNoShiki->location, no cast, ???

    FifthElement = 14275, // Seiryu->self, 4.0s cast, range 100 circle

    KujiKiri = 14305, // Seiryu->self, 4.0s cast, single-target

    SerpentDescendingSpread = 14301, // Helper->player, 6.0s cast, range 5 circle, spread
    SerpentDescendingAOE = 14302, // Helper->location, 3.0s cast, range 5 circle

    FortuneBladeSigil = 14306, // Helper->self, 6.5s cast, range 50+R width 4 rect
    CalamityBladeSigil = 14307, // Helper->self, 8.0s cast, range 50+R width 4 rect

    OnmyoSigilVisual = 14294, // Seiryu->self, no cast, single-target
    OnmyoSigil = 14849, // Helper->self, 3.0s cast, range 12 circle
    OnmyoSigilFirstVisual = 14296, // Seiryu->self, no cast, single-target
    OnmyoSigilFirst = 14851, // Helper->self, 3.0s cast, range 12 circle
    OnmyoSigilSecondVisual = 14299, // Seiryu->self, no cast, single-target
    OnmyoSigilSecond = 14854, // Helper->self, 0.5s cast, range 12 circle
    SerpentEyeSigilSecondVisual = 14297, // Seiryu->self, no cast, single-target
    SerpentEyeSigilSecond = 14852, // Helper->self, 0.5s cast, range 7-30 donut
    SerpentEyeSigilFirstVisual = 14298, // Seiryu->self, no cast, single-target
    SerpentEyeSigilFirst = 14853, // Helper->self, 3.0s cast, range 7-30 donut

    Cursekeeper = 14290, // Seiryu->player, 5.0s cast, single-target, tank swap
    InfirmSoul = 14292, // Seiryu->player, no cast, range 4 circle, tankbuster
    KarmicCurse = 14291, // Helper->self, no cast, range 90 circle

    SummonShiki1 = 14285, // Seiryu->self, 3.0s cast, single-target
    SummonShiki2 = 14286, // Seiryu->self, 3.0s cast, single-target
    SummonShiki3 = 14287, // Seiryu->self, 3.0s cast, single-target
    SummonShiki4 = 14288, // Seiryu->self, 5.0s cast, single-target

    BlueBoltMarker = 14319, // Helper->player, no cast, single-target
    BlueBolt = 14320, // AoNoShiki->self, no cast, range 80+R width 5 rect
    RedRush = 14321, // AkaNoShiki->self, no cast, range 80+R width 5 rect, knockback 18, away from source
    HundredTonzeSwing = 14317, // IwaNoShiki->self, 4.0s cast, range 16 circle
    YamaKagura = 15393, // TenNoShiki->self, 5.0s cast, range 60+R width 6 rect
    KanaboVisual = 14356, // IwaNoShiki->self, 6.0s cast, single-target
    Kanabo = 14318, // IwaNoShiki->self, no cast, range 40+R 60-degree cone

    Explosion1 = 14323, // DoroNoShiki->self, no cast, range 80 circle, on death
    Explosion2 = 14322, // NumaNoShiki->self, no cast, range 80 circle, on death
    Stoneskin = 14324, // NumaNoShiki->self, 5.0s cast, range 10 circle, interrupt

    StrengthOfSpirit = 14281, // Seiryu->self, 5.0s cast, range 80 circle, phase change, knockback 24, away from source
    DragonsWakeVisual = 14282, // Seiryu->self, no cast, single-target
    DragonsWake = 14283, // Helper->self, 24.0s cast, range 80 circle

    GreatTyphoonCone = 14328, // Helper->self, 4.0s cast, range 20-45 30-degree donut segment
    GreatTyphoon1 = 14329, // Helper->self, 3.0s cast, range 20-28 donut
    GreatTyphoon2 = 14330, // Helper->self, 3.0s cast, range 26-34 donut
    GreatTyphoon3 = 14331, // Helper->self, 3.0s cast, range 32-40 donut

    HandprintVisual1 = 14309, // YamaNoShiki->self, no cast, single-target
    HandprintVisual2 = 14310, // YamaNoShiki->self, no cast, single-target
    Handprint1 = 14312, // Helper->self, 4.5s cast, range 40 180-degree cone, inside water
    Handprint2 = 14311, // Helper->self, 4.5s cast, range 20 180-degree cone, on land

    CoursingRiver1 = 14627, // BlueOrochi2->self, 5.0s cast, single-target
    CoursingRiver2 = 14325, // BlueOrochi3->self, 5.0s cast, single-target
    CoursingRiver3 = 14626, // BlueOrochi4->self, 5.0s cast, single-target

    CoursingRiver = 14327, // Helper->self, 7.5s cast, range 90+R width 90 rect, damage if inside water
    CoursingRiverKB = 14326, // Helper->self, 7.5s cast, range 21 circle, knockback 25, dir forward

    ForceOfNatureVisual = 14313, // YamaNoShiki->self, no cast, single-target
    ForceOfNature = 14314, // Helper->self, 5.0s cast, range 5 circle
    ForceOfNatureKB = 14315, // Helper->self, 5.0s cast, range 21 circle, knockback 10, away from source

    ForbiddenArtsMarker = 14277, // Helper->player, no cast, single-target
    ForbiddenArtsFirst1 = 14279, // Seiryu->self, no cast, range 80+R width 8 rect
    ForbiddenArtsSecond1 = 14280, // Seiryu->self, no cast, range 80+R width 8 rect
    ForbiddenArtsFirst2 = 15394, // Seiryu->self, no cast, range 80+R width 8 rect
    ForbiddenArtsSecond2 = 15395, // Seiryu->self, no cast, range 80+R width 8 rect

    BlazingAramitama = 14308, // Seiryu->self, 5.0s cast, single-target

    SerpentAscending1 = 14300, // Seiryu->self, no cast, single-target
    SerpentAscending2 = 15397, // Seiryu->self, 4.0s cast, single-target
    SerpentsJaws = 14304, // Helper->self, no cast, range 80 circle, tower fail
    SerpentsFang = 14303, // Helper->self, no cast, range 3 circle, tower

    FifthElementEnrage = 15529 // Seiryu->self, 18.0s cast, range 100 circle
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
