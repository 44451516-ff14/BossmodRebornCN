namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V13Gladiator;

public enum OID : uint
{
    Gladiator = 0x399F, // R6.5
    Regret = 0x39A1, // R1.0
    WhirlwindUpdraft = 0x39A2, // R2.0
    WhirlwindSmall = 0x3AEC, // R1.33
    AntiqueBoulder = 0x39A3, // R1.8
    HatefulVisage = 0x39A0, // R2.25
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Gladiator->player, no cast, single-target
    Teleport = 30265, // Gladiator->location, no cast, single-target

    FlashOfSteel1 = 30284, // Gladiator->self, 5.0s cast, range 60 circle raidwide
    FlashOfSteel2 = 30294, // Gladiator->self, 5.0s cast, range 60 circle raidwide
    MightySmite = 30295, // Gladiator->player, 5.0s cast, single-target tankbuster

    WrathOfRuin = 30277, // Gladiator->self, 3.0s cast, single-target, visual
    RackAndRuin = 30278, // Regret->location, 4.0s cast, range 40 width 5 rect

    RingOfMightVisual = 30655, // Helper->self, 10.0s cast, range 18 circle
    RingOfMight1Out = 30271, // Gladiator->self, 10.0s cast, range 8 circle
    RingOfMight2Out = 30272, // Gladiator->self, 10.0s cast, range 13 circle
    RingOfMight3Out = 30273, // Gladiator->self, 10.0s cast, range 18 circle
    RingOfMight1In = 30274, // Helper->self, 12.0s cast, range 8-30 donut
    RingOfMight2In = 30275, // Helper->self, 12.0s cast, range 13-30 donut
    RingOfMight3In = 30276, // Helper->self, 12.0s cast, range 18-30 donut

    RushOfMight1 = 30266, // Gladiator->location, 10.0s cast, range 25 width 3 rect
    RushOfMight2 = 30267, // Gladiator->location, 10.0s cast, range 25 width 3 rect
    RushOfMight3 = 30268, // Gladiator->location, 10.0s cast, range 25 width 3 rect
    RushOfMightFront = 30269, // Helper->self, 10.5s cast, range 60 180-degree cone
    RushOfMightBack = 30270, // Helper->self, 12.5s cast, range 60 180-degree cone

    SculptorsPassion = 30282, // Gladiator->self, 5.0s cast, range 60 width 8 rect
    ShatteringSteel = 30283, // Gladiator->self, 12.0s cast, range 60 circle raidwide
    SunderedRemainsVisual = 30280, // Gladiator->self, 3.0s cast, single-target
    SunderedRemains = 30281, // Helper->self, 9.0s cast, range 10 circle aoe

    // route 8
    BitingWindUpdraft = 30285, // Helper->self, 4.0s cast, range 6 circle
    BitingWindKnockup = 30286, // Helper->player, no cast, single-target
    BitingWindAOE = 31222, // Helper->self, 4.0s cast, range 4 circle
    BitingWind = 31223, // Helper->player, no cast, single-target, knockback 5, if walked into non updraft

    // route 9 
    FlashOfSteelMeteor = 30287, // Gladiator->self, 5.0s cast, range 60 circle
    Landing = 30288, // AntiqueBoulder->self, 7.0s cast, range 50 circle

    // route 10
    HatefulVisage = 30289, // Gladiator->self, 3.0s cast, single-target
    GoldenFlame = 30290, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect

    // route 11
    SilverFlameFirstCW = 30291, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect
    SilverFlameFirstCCW = 30292, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect
    SilverFlameRest = 30293 // HatefulVisage->self, no cast, range 60 width 10 rect
}
