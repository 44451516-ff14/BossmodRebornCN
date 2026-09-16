namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

public enum OID : uint
{
    Index = 0x4B67,
    Helper = 0x233C,
    _Gen_HiddenTrap = 0x4D28, // R1.000, x4
    _Gen_AncientDoor = 0x1EBFE7, // R0.500, x1, EventObj type
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R0.500, x1, EventObj type
    _Gen_TeleportationSigil = 0x1EC0D3, // R0.500, x1, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    HolyLance = 0x4B6A, // R1.000, x3
    _Gen_TranscribedIndex = 0x4B6F, // R7.500, x3
    _Gen_TeleportationSigil1 = 0x1EBFF2, // R2.000, x1, EventObj type

    OmniElementFire = 0x1EC008, // R0.500, x0 (spawn during fight), EventObj type
    OmniElementIce = 0x1EC009, // R0.500, x0 (spawn during fight), EventObj type
    OmniElementThunder = 0x1EC00A, // R0.500, x0 (spawn during fight), EventObj type

    AllKnowingFlamesPlatforms = 0x1EC010, // R0.500, x0 (spawn during fight), EventObj type, NW NE S cones

    ExpansionFire = 0x1EC00B, // R0.500, x0 (spawn during fight), EventObj type
    ExpansionIce = 0x1EC00C, // R0.500, x0 (spawn during fight), EventObj type
    ExpansionThunder = 0x1EC00D, // R0.500, x0 (spawn during fight), EventObj type

    ForetoldPhenomenon = 0x4B6B, // R1.000, x0 (spawn during fight)
    ForetoldMarker = 0x1EC00F, // R0.500, x0 (spawn during fight), EventObj type

    _Gen_Actor1ec00e = 0x1EC00E, // R0.500, x0 (spawn during fight), EventObj type, 1 actor for all 6 soak spots
    ChemistrySwirlingOrb = 0x4E03, // R1.500, x0 (spawn during fight)
    ChemistryBallOfFire = 0x4E04, // R1.500, x0 (spawn during fight)
    ChemistryBallOfLevin = 0x4E05, // R1.500, x0 (spawn during fight)

    SummonedBomb = 0x4B68, // R2.100, x0 (spawn during fight)
    SummonedBird = 0x4B69, // R2.100, x0 (spawn during fight)
    _Gen_Actor1ec011 = 0x1EC011, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_Actor1ec012 = 0x1EC012, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_Actor1ec013 = 0x1EC013, // R0.500, x0 (spawn during fight), EventObj type
    SwirlingOrb = 0x4B6C, // R1.500, x0 (spawn during fight)
    BallOfFire = 0x4B6D, // R1.500, x0 (spawn during fight)
    BallOfLevin = 0x4B6E, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 48457, // Index->player, no cast, single-target
    Dualcast = 48407, // Index->self, 3.0s cast, single-target
    FlareCast = 48415, // Index->self, 5.0s cast, single-target
    Flare = 48456, // Helper->self, no cast, range 60 ???
    _Spell_Flare2 = 48416, // Index->self, no cast, single-target

    PropulsiveProphecy = 48403, // Index->self, 3.0s cast, single-target
    Jump = 48404, // 4B6F->self, no cast, single-target
    Shockwave = 48447, // Helper->self, 5.0s cast, range 15 ???
    ShockwaveCast = 48405, // 4B6A->self, 5.0s cast, single-target

    QuadrilogyOfImplements = 48907, // Index->self, 13.5+2.1s cast, single-target
    Aim = 48913, // Helper->self, no cast, range 11 circle
    _Weaponskill_SealedImplements = 50364, // Index->self, no cast, single-target
    WindSlash = 48915, // Helper->self, no cast, range 30 60-degree cone
    _Weaponskill_SealedImplements1 = 50363, // Index->self, 1.1s cast, single-target
    Iainuki = 48914, // Helper->self, no cast, range 30 60-degree cone
    _Weaponskill_SealedImplements2 = 48910, // Index->self, 0.5s cast, single-target
    RomeosBallad = 48912, // Helper->self, no cast, range 15 circle

    _Weaponskill_ = 50665, // Index->self, no cast, single-target
    AllKnowingFlames = 50472, // Index->self, 10.0s cast, single-target
    AllConsumingFlames = 48459, // Helper->player, no cast, range 6 circle
    AllMightyFlames = 48458, // Helper->players, no cast, range 6 circle, TB

    OmniElements = 48394, // Index->self, 4.0+1.0s cast, single-target
    OmniElements1 = 48427, // Helper->self, no cast, range 60 ???
    ElementaryExpansion = 48399, // Index->self, 3.0s cast, single-target
    BlizzardIV = 48432, // Helper->self, no cast, range 30 60-degree cone
    FireIV = 48431, // Helper->self, no cast, range 30 60-degree cone
    ThunderIV = 48433, // Helper->self, no cast, range 30 60-degree cone
    Predict = 48412, // Index->self, 3.0s cast, single-target
    Cleansing = 48452, // 4B6B->self, 0.5s cast, range 4-15 donut
    Starfall = 48451, // 4B6B->self, 0.5s cast, range 10 circle
    FireIII = 48428, // Helper->self, no cast, range 60 ???
    BlizzardIII = 48429, // Helper->self, no cast, range 60 ???
    ThunderIII = 48430, // Helper->self, no cast, range 60 ???

    SealedImplementsHarp = 48384, // Index->self, 5.0+2.0s cast, single-target
    RomeosBallad1 = 48422, // Helper->self, 7.0s cast, range 15 circle
    SealedImplementsBow = 48386, // Index->self, 5.0+2.1s cast, single-target
    Aim1 = 48423, // Helper->self, 7.1s cast, range 11 circle
    _Spell_ElementaryAbsorption = 48435, // Helper->self, no cast, range 60 circle
    _Spell_HypothermalCombustion = 48438, // 4E03->self, no cast, single-target
    _Spell_ArmOfPurgatory = 48437, // 4E04->self, no cast, single-target
    _Spell_Shock = 48439, // 4E05->self, no cast, single-target
    _Spell_ArmOfPurgatory1 = 48441, // Helper->self, no cast, range 2 ???
    _Spell_Shock1 = 48443, // Helper->self, no cast, range 2 ???
    _Spell_HypothermalCombustion1 = 48442, // Helper->self, no cast, range 2 ???

    ElementaryChemistry = 48434, // Index->self, 20.0s cast, single-target
    ElementaryChemistryPlatform = 48916, // Helper->self, 20.0s cast, range 15 width 15 rect
    _Spell_ElementaryChemistry1 = 50934, // Helper->self, no cast, range 60 ???
    _Spell_ElementaryChemistry2 = 48436, // Helper->self, no cast, range 60 ???

    Summon = 48408, // Index->self, 3.0s cast, single-target
    SunderingSpellbladeCast = 48444, // Index->self, 5.0s cast, single-target
    SunderingSpellblade = 48445, // Helper->self, 5.0s cast, range 6 circle
    SunderingSpellblade1 = 48446, // Helper->self, no cast, range 6 circle
    _AutoAttack_Attack = 6498, // 4B69->player, no cast, single-target
    BladeblitzCast = 48453, // Index->self, 5.0s cast, single-target
    _Weaponskill_Bladeblitz1 = 48454, // Index->self, no cast, single-target
    FeatherBreeze = 48450, // 4B69->self, 3.0s cast, range 40 90.000-degree cone
    Bladeblitz = 48455, // Helper->self, 9.0s cast, range 15 width 8 rect

    _Weaponskill_ElementaryEvocation = 48400, // Index->self, 3.0s cast, single-target
    _Weaponskill_QuadrilogyOfImplements1 = 48909, // Index->self, 13.5+2.1s cast, single-target
    _Weaponskill_SealedImplements5 = 48911, // Index->self, no cast, single-target
    ElementaryChemistryEnrage = 48917, // Index->self, 10.0s cast, single-target
    _Spell_ElementaryChemistry4 = 48918, // Helper->self, no cast, range 60 ???
    _Spell_ElementaryChemistry5 = 48919, // Index->self, no cast, range 60 circle
    _Weaponskill_QuadrilogyOfImplements2 = 48906, // Index->self, 13.6+2.0s cast, single-target
    _Weaponskill_SelfDestruct = 48449, // Helper->self, no cast, range 60 ???
    _Weaponskill_SelfDestruct1 = 48448, // _Gen_SummonedBomb->self, no cast, single-target
    _Weaponskill_QuadrilogyOfImplements3 = 48908, // Index->self, 14.6+1.0s cast, single-target
    _Weaponskill_WindUnbound = 49895, // _Gen_SummonedBird->self, 5.0s cast, single-target
}

public enum SID : uint
{
    Dualcast = 5438, // Index->Index, extra=0x0

    SealOfTheBell = 5532, // none->Index, extra=0x403
    SealOfTheBlade = 5533, // none->Index, extra=0x402
    SealOfTheBow = 5534, // none->Index, extra=0x401
    SealOfTheHarp = 5535, // none->Index, extra=0x404

    Predict = 2552, // none->ForetoldPhenomenon, extra=0x44C/0x44D, donut/circle

    ElementaryDeficiency = 5153, // none->player, extra=0x3/0x2/0x1
    FireResistanceDownII = 2902, // Helper->player, extra=0x0
    IceResistanceDownII = 2903, // Helper->player, extra=0x0
    LightningResistanceDownII = 2998, // Helper->player, extra=0x0

    Petrification = 3007, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    SpreadTankbuster = 344, // player->self
    Spread = 466, // player->self
    FireIce = 670, // player->self, _Gen_Icon_m0947_ring_fi_c0p, fire ice?
    IceThunder = 671, // player->self, _Gen_Icon_m0947_ring_it_c0p, ice thunder?
    ThunderFire = 672, // player->self, _Gen_Icon_m0947_ring_tf_c0p, thunder fire?
}

public enum TetherID : uint
{
    Thunder = 363, // 4B6E->4B6E
    Ice = 364, // 4B6C->4B6C
    Fire = 365, // 4B6D->4B6D
}
