namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

public enum OID : uint
{
    Boss = 0x4781, // R7.0-19.712
    Wall = 0x4785, // R7.0
    BruteAbombinator = 0x481B, // R0.0
    BloomingAbomination = 0x4782, // R3.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 42263, // Boss->player, no cast, single-target
    AutoAttack2 = 43155, // BruteAbombinator->player, no cast, single-target
    AutoAttackAdd = 872, // BloomingAbomination->player, no cast, single-target
    Teleport = 42262, // Boss->location, no cast, single-target

    BrutalImpactVisual = 42264, // Boss->self, 5.0s cast, single-target
    BrutalImpact = 42265, // Boss->self, no cast, range 60 circle

    StoneringerTheBludgeoning1 = 42266, // Boss->self, 2.0+3,5s cast, single-target
    StoneringerTheBludgeoning2 = 42290, // Boss->self, 2.0+3,5s cast, single-target
    StoneringerTheStabbing1 = 42267, // Boss->self, 2.0+3,5s cast, single-target
    StoneringerTheStabbing2 = 42291, // Boss->self, 2.0+3,5s cast, single-target
    Stoneringer2Stoneringers1 = 42314, // Boss->self, 2.0+3,5s cast, single-target
    Stoneringer2Stoneringers2 = 42315, // Boss->self, 2.0+3,5s cast, single-target

    BrutishSwingVisual1 = 42268, // Boss->self, 4.0+1,0s cast, single-target, BrutishSwingCircle2
    BrutishSwingVisual2 = 42269, // Boss->self, 4.0+1,0s cast, single-target, BrutishSwingDonut
    BrutishSwingVisual3 = 42292, // Boss->self, 3.0+1,0s cast, single-target, BrutishSwingCone1
    BrutishSwingVisual4 = 42297, // Boss->location, 4.0+3,8s cast, single-target, BrutishSwingDonutSegment1
    BrutishSwingVisual5 = 42301, // Boss->location, 4.0+3,8s cast, single-target, BrutishSwingDonutSegment1
    BrutishSwingVisual6 = 42316, // Boss->location, 3.0+3,8s cast, single-target, BrutishSwingCone2
    BrutishSwingVisual7 = 42325, // Boss->location, 3.0+3,8s cast, single-target, BrutishSwingCone2
    BrutishSwingVisual8 = 42326, // Boss->location, 3.0+3,8s cast, single-target, BrutishSwingDonutSegment2
    BrutishSwingVisual9 = 42305, // Boss->self, no cast, single-target
    BrutishSwingVisual10 = 42318, // Boss->self, no cast, single-target
    BrutishSwingVisual11 = 42320, // Boss->self, no cast, single-target
    BrutishSwingVisual12 = 42554, // Boss->self, no cast, single-target
    BrutishSwingVisual13 = 42555, // Boss->self, no cast, single-target

    BrutishSwingCircle1 = 42274, // Boss->location, no cast, range 6 circle, no idea what this is, only saw it in one of 10+ logs after all players were already dead
    BrutishSwingCircle2 = 42270, // Helper->self, 5.0s cast, range 12 circle
    BrutishSwingDonut = 42271, // Helper->self, 5.0s cast, range 9-60 donut
    BrutishSwingCone1 = 42293, // Helper->self, 4.0s cast, range 25 180-degree cone
    BrutishSwingCone2 = 42317, // Helper->self, 6.7s cast, range 25 180-degree cone
    BrutishSwingDonutSegment1 = 42303, // Helper->self, 8.1s cast, range 22-88 180-degree donut segment
    BrutishSwingDonutSegment2 = 42319, // Helper->self, 6.7s cast, range 22-88 180-degree donut segment

    BrutalSmashTB1 = 42272, // Boss->players, no cast, range 6 circle, shared tankbuster, 5.9s after icon
    BrutalSmashTB2 = 42273, // Boss->players, no cast, range 6 circle

    SporeSacVisual = 42281, // Boss->self, 3.0s cast, single-target
    SporeSac = 42282, // Helper->location, 4.0s cast, range 8 circle
    Pollen = 42283, // Helper->location, 4.5s cast, range 8 circle

    PulpSmashVisual1 = 42276, // Boss->self, 3.0+2,0s cast, single-target, stack, 5.2s after icon
    PulpSmashVisual2 = 42277, // Boss->player, no cast, range 1 circle
    PulpSmash = 42278, // Helper->players, no cast, range 6 circle

    TheUnpotted = 42280, // Helper->self, 3.0s cast, range 60 30-degree cone
    ItCameFromTheDirt = 42279, // Helper->location, 3.0s cast, range 6 circle

    CrossingCrosswinds = 43276, // BloomingAbomination->self, 8.5s cast, range 50 width 10 cross
    QuarrySwamp = 42285, // Boss->self, 4.0s cast, range 60 circle, need to hide behind dead or alive add or get petrified
    Explosion = 42286, // Helper->location, 9.0s cast, range 60 circle, fall off AOE, optimal around 25

    NeoBombarianSpecial = 42287, // Boss->self, 8.0s cast, range 60 circle, raidwide, knockback 58, arena change
    GrapplingIvy = 42288, // Boss->location, no cast, single-target
    WindingWildwinds = 43275, // BloomingAbomination->self, 8.5s cast, range 5-60 donut

    GlowerPowerVisual = 42310, // Boss->self, 3.7+1,3s cast, single-target
    GlowerPower = 43339, // Helper->self, 5.0s cast, range 65 width 14 rect

    ElectrogeneticForce = 42311, // Helper->players, 5.0s cast, range 6 circle, spread

    AbominableBlinkVisual = 42306, // Boss->self, 5.3+1,0s cast, single-target
    AbominableBlink = 43154, // Helper->players, no cast, range 60 circle, flare

    RevengeOfTheVines1 = 42307, // Boss->self, 5.0s cast, range 60 circle
    RevengeOfTheVines2 = 42552, // Boss->self, no cast, range 60 circle
    SporesplosionVisual = 42308, // Boss->self, 4.0s cast, single-target
    Sporesplosion = 42309, // Helper->location, 5.0s cast, range 8 circle

    Powerslam = 42312, // Boss->location, 5.0s cast, range 60 circle, raidwide, arena change

    LashingLariatVisual1 = 42321, // Boss->location, 5.0+0,5s cast, single-target
    LashingLariatVisual2 = 42323, // Boss->location, 5.0+0,5s cast, single-target
    LashingLariat1 = 42322, // Helper->self, 5.5s cast, range 70 width 32 rect
    LashingLariat2 = 42324, // Helper->self, 5.5s cast, range 70 width 32 rect

    SlaminatorVisual = 42327, // Boss->location, 4.0+1,0s cast, single-target
    Slaminator = 42328 // Helper->self, 5.0s cast, range 8 circle
}

public enum IconID : uint
{
    BrutalSmashTB = 600, // player->self
    PulpSmash = 161, // player->self
    AbominableBlink = 327 // player->self
}
