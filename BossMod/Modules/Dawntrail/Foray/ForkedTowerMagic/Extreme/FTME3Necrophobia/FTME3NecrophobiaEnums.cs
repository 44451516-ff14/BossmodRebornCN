namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME3Necrophobia;

public enum OID : uint
{
    Necrophobia = 0x4BE7,
    Helper = 0x233C,
    _Gen_HiddenTrap = 0x4D28, // R1.000, x1
    SeveringHead = 0x4BE8, // R1.410, x8
    _Gen_Necrophobia = 0x4BE9, // R1.000, x1
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_Actor1ebfaa = 0x1EBFAA, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    _AutoAttack_ = 47481, // Necrophobia->player, no cast, single-target
    HailOfHellflares = 47482, // Necrophobia->self, 5.0s cast, single-target
    HailOfHellflaresDamage1 = 47483, // Helper->self, no cast, range 60 ???
    _Spell_HailOfHellflares2 = 48958, // Helper->self, no cast, single-target
    HailOfHellflaresDamage2 = 48959, // Helper->self, no cast, range 60 ???
    Deathwall = 47484, // 4BE9->self, no cast, range ?-30 donut

    _Ability_1 = 47480, // Necrophobia->location, no cast, single-target
    _Ability_Capitation = 47485, // Necrophobia->self, no cast, single-target
    _Ability_2 = 47487, // 4BE8->location, no cast, single-target
    DeathShroud = 47486, // Necrophobia->self, 7.0s cast, single-target
    HeadsRoll = 47488, // Necrophobia->self, 3.0s cast, single-target
    HeadsRollMovement = 47489, // 4BE8->location, no cast, single-target

    SeveredFire = 47490, // Necrophobia->self, 5.0s cast, range 18 circle
    AncientFire1 = 47494, // 4BE8->self, 5.0s cast, range 18 circle
    AncientFire2 = 47521, // Necrophobia->self, 5.0s cast, range 18 circle

    SeveredBlizzard = 47491, // Necrophobia->self, 5.0s cast, range 45 width 15 cross
    AncientBlizzard1 = 47495, // 4BE8->self, 5.0s cast, range 45 width 15 cross
    AncientBlizzard2 = 47522, // Necrophobia->self, 5.0s cast, range 45 width 15 cross

    AncientThunder1 = 47493, // Helper->self, 5.0s cast, range 60 45.000-degree cone
    AncientThunder2 = 47497, // Helper->self, 5.0s cast, range 60 45.000-degree cone
    SeveredThunder = 50358, // Helper->self, 5.0s cast, range 60 45.000-degree cone
    _Spell_SeveredThunderIII = 47492, // Necrophobia->self, 4.2+0.8s cast, single-target
    _Spell_AncientThunderIII = 47496, // 4BE8->self, 4.2+0.8s cast, single-target
    _Spell_AncientThunderIII2 = 47512, // 4BE8->self, 0.7+0.8s cast, single-target
    _Spell_AncientThunderIII4 = 47523, // Necrophobia->self, 4.2+0.8s cast, single-target

    _Ability_4 = 47498, // 4BE8->location, no cast, single-target
    _Ability_HeadsRoll1 = 47503, // Necrophobia->self, no cast, single-target
    DarkCurrentCast = 47499, // Necrophobia->self, 2.7+1.3s cast, single-target
    DarkCurrent1 = 47500, // Helper->self, 4.0s cast, range 60 width 10 rect
    DarkCurrent2 = 47501, // Helper->self, 1.0s cast, range 10 width 60 rect
    VacuumWave = 47502, // Necrophobia->self, 4.0s cast, range 30 180.000-degree cone
    DeathlyRay = 47504, // 4BE8->self, 4.0s cast, range 30 width 6 rect
    CorpseMangler = 47505, // Necrophobia->player, 5.0s cast, single-target

    DigThreeGraves = 47506, // Necrophobia->self, 3.0s cast, single-target
    SeveredDarkCurrent = 47507, // Necrophobia->self, 8.7+1.3s cast, single-target
    _Weaponskill_DarkCurrent3 = 47508, // Necrophobia->self, no cast, single-target
    DarkCurrent3 = 47509, // Helper->self, 1.5s cast, range 60 width 10 rect
    AncientFireShort = 47510, // 4BE8->self, 1.5s cast, range 18 circle
    AncientBlizzardShort = 47511, // 4BE8->self, 1.5s cast, range 45 width 15 cross
    AncientThunderShort = 47513, // Helper->self, 1.5s cast, range 60 45.000-degree cone

    FertileGroundCast = 47514, // Necrophobia->self, 5.0s cast, single-target
    FertileGround = 48960, // Helper->self, no cast, range 60 ???
    SpellProcession = 47515, // Necrophobia->self, 5.0s cast, single-target
    _Spell_SowingFear1 = 47516, // 4BE8->self, no cast, single-target
    SowingDread1 = 47517, // Helper->self, no cast, range 80 width 30 rect
    SowingPanic1 = 47518, // Helper->self, no cast, range 80 width 30 rect
    SowingDread2 = 47519, // Helper->self, no cast, range 80 width 30 rect, by head with AOE or last?
    SowingPanic2 = 47520, // Helper->self, no cast, range 80 width 30 rect, by head with AOE or last?
    _Spell_SowingFear2 = 47574, // 4BE8->self, no cast, single-target, by head with AOE or last?

    NihilityCast = 47524, // Necrophobia->self, 10.0s cast, single-target, enrage
    _Spell_Nihility1 = 48961, // Helper->self, no cast, range 60 ???
    Nihility = 47525, // Necrophobia->self, 2.0s cast, range 60 circle
}

public enum SID : uint
{
    _Gen_Unk = 4956, // none->4BE8, extra=0x2C4
    Element = 2552, // none->Necrophobia/4BE8, extra=0x45A/0x45B/0x45C/0x45E/0x45D, 0x45A = fire, 0x45B = ice, 0x45C = lightning, 0x45D/0x45E = dread/panic
    DigThreeGraves = 5135, // none->Necrophobia, extra=0x0
    GrowingDread = 5136, // Helper->player, extra=0x0
    GrowingPanic = 5137, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
}

public enum TetherID : uint
{
    Fire = 400, // 4BE8->Necrophobia
    Ice = 401, // 4BE8->Necrophobia
    Lightning = 402, // 4BE8->Necrophobia
    FertileGround = 403, // 4BE8->Necrophobia
}
