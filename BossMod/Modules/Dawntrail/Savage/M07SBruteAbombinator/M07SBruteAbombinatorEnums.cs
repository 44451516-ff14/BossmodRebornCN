namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

public enum OID : uint
{
    Boss = 0x4783, // R7.0-19.712
    Wall = 0x4785, // R7.0
    BruteAbombinator = 0x481C, // R0.0
    BloomingAbomination = 0x4784, // R3.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 42330, // Boss->player, no cast, single-target
    AutoAttack2 = 43157, // BruteAbombinator->player, no cast, single-target
    AutoAttackAdd = 872, // BloomingAbomination->player, no cast, single-target
    Teleport = 42262, // Boss->location, no cast, single-target

    BrutalImpactVisual = 42331, // Boss->self, 5.0s cast, single-target
    BrutalImpact = 42332, // Boss->self, no cast, range 60 circle
    Stoneringer1 = 42334, // Boss->self, 2.0+3,5s cast, single-target, donut
    Stoneringer2 = 42368, // Boss->self, 2.0+3,5s cast, single-target
    Stoneringer3 = 42333, // Boss->self, 2.0+3,5s cast, single-target, circle
    Stoneringer4 = 42367, // Boss->self, 2.0+3,5s cast, single-target, circle
    Stoneringer2Stoneringers1 = 42401, // Boss->self, 2.0+3,5s cast, single-target
    Stoneringer2Stoneringers2 = 42400, // Boss->self, 2.0+3,5s cast, single-target

    BrutishSwingVisual1 = 42340, // Boss->self, no cast, single-target
    BrutishSwingVisual2 = 42380, // Boss->location, 4.0+3,8s cast, single-target
    BrutishSwingVisual3 = 42389, // Boss->self, no cast, single-target
    BrutishSwingVisual4 = 42383, // Boss->location, 4.0+3,8s cast, single-target
    BrutishSwingVisual5 = 42385, // Boss->location, 4.0+3,8s cast, single-target
    BrutishSwingVisual6 = 42402, // Boss->location, 3.0+3,8s cast, single-target
    BrutishSwingVisual7 = 42557, // Boss->self, no cast, single-target
    BrutishSwingVisual8 = 42411, // Boss->location, 3.0+3,8s cast, single-target
    BrutishSwingVisual9 = 42556, // Boss->self, no cast, single-target
    BrutishSwingVisual10 = 42339, // Boss->self, no cast, single-target
    BrutishSwingVisual11 = 42388, // Boss->self, no cast, single-target
    BrutishSwingVisual12 = 42384, // Boss->location, 4.0+3,8s cast, single-target
    BrutishSwingVisual13 = 42412, // Boss->location, 3.0+3,8s cast, single-target
    BrutishSwingVisual14 = 42381, // Boss->location, 4.0+3,8s cast, single-target
    BrutishSwingVisual15 = 42406, // Boss->self, no cast, single-target
    BrutishSwingVisual16 = 42404, // Boss->self, no cast, single-target
    BrutishSwingVisual17 = 42382, // Boss->location, 4.0+3,8s cast, single-target

    BrutishSwingCircle = 42337, // Helper->self, 4.0s cast, range 12 circle
    BrutishSwingCone1 = 42403, // Helper->self, 6.7s cast, range 25 180-degree cone
    BrutishSwingCone2 = 42386, // Helper->self, 8.1s cast, range 25 180-degree cone
    BrutishSwingDonut = 42338, // Helper->self, 4.0s cast, range 9-60 donut
    BrutishSwingDonutSegment1 = 42387, // Helper->self, 8.1s cast, range 22-88 180-degree donut segment
    BrutishSwingDonutSegment2 = 42405, // Helper->self, 6.7s cast, range 22-88 180-degree donut segment

    SmashHere = 42335, // Boss->self, 3.0+1,0s cast, single-target, shared proximity tankbusters, closest target
    SmashThere = 42336, // Boss->self, 3.0+1,0s cast, single-target, furthest target
    BrutalSmash1 = 42342, // Boss->players, no cast, range 6 circle
    BrutalSmash2 = 42341, // Boss->players, no cast, range 6 circle
    BrutalSmash3 = 42343, // Boss->location, no cast, range 6 circle, seems to be cast if all players are dead when tb is supposed to happen
    BrutalSmash4 = 42344, // Boss->location, no cast, range 6 circle, seems to be cast if all players are dead when tb is supposed to happen

    SporeSacVisual = 42345, // Boss->self, 3.0s cast, single-target
    SporeSac = 42346, // Helper->location, 4.0s cast, range 8 circle

    SinisterSeedsVisual = 42349, // Boss->self, 4.0+1,0s cast, single-
    SinisterSeeds = 42353, // Helper->location, 3.0s cast, range 7 circle
    SinisterSeedsSpread = 42350, // Helper->players, 7.0s cast, range 6 circle
    Pollen = 42347, // Helper->location, 4.0s cast, range 8 circle

    ImpactVisual = 42356, // Boss->self, no cast, single-target
    Impact = 42355, // Helper->players, no cast, range 6 circle

    TendrilsOfTerrorCross1 = 42352, // Helper->self, 3.0s cast, range 60 width 4 cross
    TendrilsOfTerrorCircle1 = 42351, // Helper->self, 3.0s cast, range 10 circle, some sort of visual
    TendrilsOfTerrorCross2 = 42394, // Helper->self, 3.0s cast, range 60 width 4 cross
    TendrilsOfTerrorCircle2 = 42393, // Helper->self, 3.0s cast, range 10 circle, some sort of visual
    TendrilsOfTerrorCross3 = 42397, // Helper->self, 3.0s cast, range 60 width 4 cross
    TendrilsOfTerrorCircle3 = 42396, // Helper->self, 3.0s cast, range 10 circle, some sort of visual

    RootsOfEvil = 42354, // Helper->location, 3.0s cast, range 12 circle

    CrossingCrosswinds = 43278, // BloomingAbomination->self, 7.0s cast, range 50 width 10 cross
    WindingWildwinds = 43277, // BloomingAbomination->self, 7.0s cast, range 5-60 donut
    HurricaneForce = 42348, // BloomingAbomination->self, 6.0s cast, range 60 circle, enrage
    QuarrySwamp = 42357, // Boss->self, 4.0s cast, range 60 circle, line of sight AOE, take cover behind adds (dead or alive)
    Explosion = 42358, // Helper->location, 9.0s cast, range 60 circle, proximity AOE, about 25 optimal range

    PulpSmashVisual1 = 42359, // Boss->self, 3.0+2,0s cast, single-target, stack
    PulpSmashVisual2 = 42360, // Boss->player, no cast, single-target
    PulpSmash = 42361, // Helper->players, no cast, range 6 circle
    ItCameFromTheDirt = 42362, // Helper->location, 2.0s cast, range 6 circle
    TheUnpotted = 42363, // Helper->self, no cast, range 60 30-degree cone
    NeoBombarianSpecial = 42364, // Boss->self, 8.0s cast, range 60 circle
    GrapplingIvy = 42365, // Boss->location, no cast, single-target

    GlowerPowerVisual1 = 42373, // Boss->self, 2.7+1,3s cast, single-target
    GlowerPowerVisual2 = 43338, // Boss->self, 0.7+1,3s cast, single-target
    GlowerPower1 = 43340, // Helper->self, 4.0s cast, range 65 width 14 rect
    GlowerPower2 = 43358, // Helper->self, 2.0s cast, range 65 width 14 rect
    ElectrogeneticForce = 42374, // Helper->player, no cast, range 6 circle

    RevengeOfTheVines1 = 42375, // Boss->self, 5.0s cast, range 60 circle
    RevengeOfTheVines2 = 42553, // Boss->self, no cast, range 60 circle

    ThornyDeathmatch = 42376, // Boss->self, 3.0s cast, single-target
    AbominableBlinkVisual = 42377, // Boss->self, 5.3+1,3s cast, single-target
    AbominableBlink = 43156, // Helper->players, no cast, range 60 circle
    SporesplosionVisual = 42378, // Boss->self, 4.0s cast, single-target
    Sporesplosion = 42379, // Helper->location, 5.0s cast, range 8 circle

    DemolitionDeathmatch = 42390, // Boss->self, 3.0s cast, single-target
    StrangeSeedsVisual1 = 42391, // Boss->self, 4.0s cast, single-target
    StrangeSeedsVisual2 = 43274, // Boss->self, 4.0s cast, single-target
    StrangeSeeds = 42392, // Helper->player, 5.0s cast, range 6 circle
    KillerSeeds = 42395, // Helper->players, 5.0s cast, range 6 circle
    Powerslam = 42398, // Boss->location, 6.0s cast, range 60 circle

    LashingLariatVisual1 = 42407, // Boss->location, 3.5+0,5s cast, single-target
    LashingLariatVisual2 = 42409, // Boss->location, 3.5+0,5s cast, single-target
    LashingLariat1 = 42408, // Helper->self, 4.0s cast, range 70 width 32 rect
    LashingLariat2 = 42410, // Helper->self, 4.0s cast, range 70 width 32 rect

    SlaminatorVisual = 42413, // Boss->location, 4.0+1,0s cast, single-target
    Slaminator = 42414, // Helper->self, 5.0s cast, range 8 circle
    HeavyImpact = 42415, // Helper->self, no cast, range 60 circle, tower fail

    DebrisDeathmatch = 42416, // Boss->self, 3.0s cast, single-target

    SpecialBombarianSpecialVisual1 = 42417, // Boss->location, 10.0s cast, single-target, enrage
    SpecialBombarianSpecialVisual2 = 42418, // Boss->location, no cast, single-target
    SpecialBombarianSpecial = 42419 // Helper->self, no cast, range 60 circle
}

public enum IconID : uint
{
    PulpSmash = 161, // player->self
    AbominableBlink = 327 // player->self
}

public enum TetherID : uint
{
    ThornsOfDeathTank = 338, // Boss->player
    ThornsOfDeathNonTank = 325, // Boss/Wall->player
    ThornsOfDeathTakeable = 84, // Wall->player
    Wall = 339 // Wall->Boss
}

public enum SID : uint
{
    Stoneringer = 2056 // none->Boss, extra=0x388/0x38A/0x38B/0x377/0x389/0x378
}
