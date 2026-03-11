namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V15DeadlyDandan;

public enum OID : uint
{
    DeadlyDandan = 0x4ACF, // R24.000, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    DeadlyDandan_Tentacle = 0x4B96, // R1.000, x2, actual helper rotating for Stinging Tentacle
    _Gen_DeadlyDandan1 = 0x4AD2, // R0.000, x1, Part type
    _Gen_Tentacle = 0x4AD0, // R1.200, x0 (spawn during fight)
    _Gen_Actor1ebf0e = 0x1EBF0E, // R0.500, x0 (spawn during fight), EventObj type
    AiryBubble = 0x4AD1, // R1.300, x0 (spawn during fight)
    _Gen_AqueousOrb = 0x4C44, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 45617, // 4AD2->player, no cast, single-target
    MurkyWaters = 45615, // DeadlyDandan->location, 5.0s cast, range 50 circle
    _Ability_ = 47546, // DeadlyDandan->location, no cast, single-target
    DevourCast = 45589, // DeadlyDandan->self, 5.0s cast, single-target
    DevourMove1 = 45592, // DeadlyDandan->location, no cast, single-target, move 1, status extra 0x3F9
    DevourMove2 = 45593, // DeadlyDandan->location, no cast, single-target. move 2
    DevourMove3 = 45596, // DeadlyDandan->location, no cast, single-target, move 1, status extra 0x3F8
    DevourMove4 = 45597, // DeadlyDandan->location, no cast, single-target, move 2
    _Ability_Devour3 = 45598, // Helper->self, 0.5s cast, range 15 width 20 rect, casts 3-6 in same spot, roughly the same time?
    _Ability_Devour4 = 45599, // Helper->self, 0.5s cast, range 15 width 20 rect
    _Ability_Devour5 = 45600, // Helper->self, 0.5s cast, range 15 width 20 rect, used for player eaten?
    _Ability_Devour6 = 45601, // Helper->self, 0.5s cast, range 15 width 20 rect
    Spit = 45602, // DeadlyDandan->self, 5.0s cast, range 50 120.000-degree cone
    Dropsea = 47398, // Helper->player, 5.0s cast, range 5 circle
    StingingTentacleCast = 45607, // DeadlyDandan->self, 7.5s cast, single-target
    StingingTentacle = 45608, // Helper->self, 0.5s cast, range 50 width 14 rect
    _Ability_StrewnBubbles = 45611, // DeadlyDandan->self, 3.0s cast, single-target
    _Ability_MawOfTheDeep = 45609, // DeadlyDandan->self, 3.0s cast, single-target
    MawOfTheDeep = 45610, // Helper->self, 5.0s cast, range 8 circle
    _Ability_UnfathomableHorror = 48048, // DeadlyDandan->self, 3.0s cast, single-target
    _Spell_UnfathomableHorror1 = 48043, // 4C44->self, 5.0s cast, single-target
    UnfathomableHorror1 = 48044, // Helper->self, 5.0s cast, range 8 circle
    UnfathomableHorror2 = 48045, // Helper->self, 7.5s cast, range 10-16 donut
    UnfathomableHorror3 = 48046, // Helper->self, 10.0s cast, range 20-24 donut
    UnfathomableHorror4 = 48047, // Helper->self, 12.5s cast, range 24-36 donut
    _Ability_TidalGuillotine = 45604, // DeadlyDandan->location, 8.0s cast, single-target
    TidalGuillotine = 45605, // Helper->self, 8.3s cast, range 20 circle
    SwallowedSea = 45616, // DeadlyDandan->self, 5.0s cast, range 50 120.000-degree cone
    _Ability_Spit1 = 45603, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    DevourDirection = 2195, // none->DeadlyDandan, extra=0x3F9/0x3F8, 0x3F8 = jump "back", 0x3F9 = jump "forward"
    TentacleRotate = 2056, // none->4B96, extra=0x3FE, tentacle helper rotate start, loses right as attack starts
    Devoured = 4619, // DeadlyDandan->player, extra=0x12, player got devoured

}
public enum IconID : uint
{
    Dropsea = 558, // player->self
    Countdown = 599, // 4AD0->self
}
