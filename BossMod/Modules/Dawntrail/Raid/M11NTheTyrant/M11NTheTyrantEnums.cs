namespace BossMod.Modules.Dawntrail.Raid.M11NTheTyrant;

/* Boss Actors */
public enum OID : uint
{
    TheTyrant = 0x4AEE, // not targetable r3
    Boss = 0x4AE8,   // targetable r5
    Helper = 0x233C,
    Comet = 0x4AE9,
    Axe = 0x4AF0,   // aoe
    Scythe = 0x4AF1,  // inverted aoe
    Sword = 0x4AF2,  // Cross centered on the tyrant
    Maelstrom = 0x4AEB,
}

/* Abilities */
public enum AID : uint
{
    _AutoAttack_ = 46005, // Boss->player, no cast, single-target
    _Weaponskill_CrownOfArcadia = 46006, // Boss->self, 5.0s cast, range 60 circle

    _Weaponskill_DrawSteel = 46007, // Boss->self, 2.0+4.0s cast, single-target
    _Weaponskill_DrawSteel1 = 46008, // Boss->self, 2.0+4.0s cast, single-target
    _Weaponskill_DrawSteel2 = 46009, // Boss->self, 2.0+4.0s cast, single-target   -

    _Weaponskill_SmashdownAxeVisual = 46010, // Boss->self, 2.0+1.0s cast, single-target - Smashdown Axe Visual
    _Weaponskill_SmashdownAxeAOE = 46011, // Helper->self, 3.0s cast, range 8 circle - Smashdown Axe aoe
    _Weaponskill_SmashdownScytheVisual = 46012, // Boss->self, 2.0+1.0s cast, single-target - Smashdown Scythe Visual
    _Weaponskill_SmashdownScytheAOE = 46013, // Helper->self, 3.0s cast, range 5-60 donut - Smashdown Scythe aoe
    _Weaponskill_SmashdownSwordVisual = 46014, // Boss->self, 2.0+1.0s cast, single-target   - Sword Visual
    _Weaponskill_SmashdownSwordAOE = 46015, // Helper->self, 3.0s cast, range 40 width 10 cross - Sword aoe

    _Spell_VoidStardust = 46024, // Boss->self, 5.0s cast, single-target
    _Spell_Comet = 46025, // Helper->player, 5.0s cast, range 4 circle
    _Spell_Comet1 = 46027, // Helper->players, no cast, range 4 circle
    _Spell_Cometite = 46026, // Helper->location, 2.5s cast, range 4 circle
    _Ability_ = 46004, // Boss->location, no cast, single-target
    _Weaponskill_TrophyWeapons = 46028, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AssaultEvolvedCast = 46029, // Boss->self, 5.0s cast, single-target
    _Weaponskill_AssaultEvolvedScytheVisual = 46401, // Boss->location, no cast, single-target
    _Weaponskill_AssaultEvolvedScythe = 46031, // Helper->self, 2.0s cast, range 5-60 donut
    _Weaponskill_AssaultEvolvedSwordVisual = 46402, // Boss->location, no cast, single-target
    _Weaponskill_AssaultEvolvedSword = 46032, // Helper->self, 2.0s cast, range 40 width 10 cross
    _Weaponskill_AssaultEvolvedAxeVisual = 46400, // Boss->location, no cast, single-target
    _Weaponskill_AssaultEvolvedAxe = 46030, // Helper->self, 2.0s cast, range 8 circle

    _Weaponskill_DanceOfDominationTrophy = 47034, // Boss->self, 2.0+4.0s cast, single-target
    _Weaponskill_DanceOfDomination = 46033, // Boss->self, 4.5+0.7s cast, single-target
    _Weaponskill_DanceOfDomination1 = 46034, // Helper->self, 5.0s cast, range 60 circle
    _Weaponskill_DanceOfDomination2 = 46036, // Helper->self, no cast, range 60 circle
    _Weaponskill_DanceOfDomination3 = 47081, // Helper->self, no cast, range 60 circle
    _Weaponskill_Explosion = 46035, // Helper->self, 6.0s cast, range 60 width 10 rect
    _Weaponskill_Explosion1 = 47033, // Helper->self, 6.5s cast, range 60 width 10 rect

    _Weaponskill_RawSteelTrophy = 46037, // Boss->self, 2.0+4.0s cast, single-target
    _Weaponskill_RawSteel = 46016, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_RawSteel1 = 46017, // Boss->players, no cast, range 6 circle
    _Weaponskill_Impact = 46018, // Helper->player, 6.5s cast, range 6 circle

    _Spell_Charybdistopia = 46039, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_UltimateTrophyWeapons = 47083, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AssaultApex = 47084, // Boss->self, 5.0s cast, single-target

    _Spell_PowerfulGust = 46041, // Helper->self, 6.0s cast, range 60 45.000-degree cone
    _Ability_ImmortalReign = 46042, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_OneAndOnly = 46043, // Boss->self, 6.0+2.0s cast, single-target
    _Weaponskill_OneAndOnly1 = 46044, // Helper->self, 9.0s cast, range 60 circle
    _Spell_Meteorain = 46045, // Boss->self, 5.0s cast, single-target
    _Ability_CosmicKiss = 46046, // 4AE9->self, 8.0s cast, range 4 circle
    _Spell_ForegoneFatality = 46048, // 4AEE->player, no cast, single-target
    _Ability_MassiveMeteor = 46051, // Helper->players, 6.0s cast, range 6 circle
    _Weaponskill_DoubleTyrannhilation = 46052, // Boss->self, no cast, single-target
    _Weaponskill_DoubleTyrannhilation1 = 46053, // Boss->self, 8.0+1.0s cast, range 30 circle
    _Weaponskill_Shockwave = 46054, // Helper->self, no cast, range 60 circle
    _Ability_1 = 46055, // 4AE9->self, no cast, single-target
    _Weaponskill_HiddenTyrannhilation = 46215, // Helper->self, 4.5s cast, range 30 circle
    _Weaponskill_1 = 46082, // Boss->self, no cast, single-target
    _Weaponskill_Flatliner = 46056, // Boss->self, 4.0+2.0s cast, single-target
    _Weaponskill_FlatlinerKnockup = 47759, // Helper->self, 6.0s cast, range 60 circle
    _Spell_MajesticMeteor = 46057, // Boss->self, 4.0s cast, single-target
    _Spell_MajesticMeteor1 = 46058, // Helper->location, 4.0s cast, range 6 circle
    _Spell_MajesticMeteorain = 46059, // Helper->self, 6.0s cast, range 60 width 10 rect
    _Weaponskill_FireAndFury = 46071, // Boss->self, 5.0+1.0s cast, single-target
    _Spell_MammothMeteor = 46060, // Helper->location, 6.5s cast, range 60 circle
    _Weaponskill_FireAndFuryCone2 = 46072, // Helper->self, 6.0s cast, range 60 90.000-degree cone
    _Weaponskill_FireAndFuryCone1 = 46073, // Helper->self, 6.0s cast, range 60 90.000-degree cone
    ExplosionKnockUp = 46061, // Helper->self, 10.0s cast, range 4 circle
    _Spell_UnmitigatedExplosion = 46062, // Helper->self, no cast, range 60 circle
    _Weaponskill_ArcadionAvalanche = 46065, // Boss->self, 8.0+9.5s cast, single-target
    _Weaponskill_ArcadionAvalanche1 = 46066, // Helper->self, 17.5s cast, range 40 width 40 rect
    ArcadionAvalanche = 46069, // 4AE8->self, 8.0+9.5s cast, single-target
    ArcadionAvalancheToss = 46070, // 233C->self, 17.5s cast, range 40 width 40 rect
    _Weaponskill_2 = 46078, // Boss->self, no cast, single-target
    _Weaponskill_HeartbreakKick = 46079, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_HeartbreakKick1 = 46080, // Helper->self, no cast, range 4 circle
    _Weaponskill_3 = 46081, // Boss->self, no cast, single-target
    _Weaponskill_GreatWallOfFire = 46074, // Boss->self, 5.0s cast, single-target
    _Weaponskill_GreatWallOfFire1 = 46075, // Boss->self, no cast, range 60 width 6 rect
    _Weaponskill_GreatWallOfFire2 = 46076, // Helper->self, no cast, single-target
    _Weaponskill_Explosion2 = 46077, // Helper->self, 3.0s cast, range 60 width 6 rect

}

/* Status */
public enum SID : uint
{
    _Gen_PhysicalVulnerabilityUp = 2940, // Boss/Helper/_Gen_Comet->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2941, // Helper/Boss/_Gen_TheTyrant->player/_Gen_Comet, extra=0x0
    _Gen_DamageDown = 2911, // Helper/_Gen_Comet->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_ = 3913, // none->_Gen_1/_Gen_/Boss/_Gen_2, extra=0x3EB/0x3EC/0x0/0x432
    _Gen_FireResistanceDownII = 2937, // none->player, extra=0x0
    _Gen_DirectionalDisregard = 3808, // none->_Gen_TheTyrant/Boss, extra=0x0
    _Gen_1 = 4435, // none->_Gen_TheTyrant/Boss, extra=0xA
    _Gen_SustainedDamage = 4149, // none->player, extra=0x2/0x4/0x3/0x5/0x1
}

/* Tethers */
public enum TetherID : uint
{
    _Gen_TankInterceptTether = 356, // _Gen_TheTyrant->_Gen_Comet/player
    _Gen_Tether_chn_arrow01f = 57, // _Gen_TheTyrant->player
    _Gen_Tether_chn_tergetfix2k1 = 249, // _Gen_TheTyrant->player
}

/* Icons */
public enum IconID : uint
{
    RawSteelSpread = 311, // player->self
    MassiveMeteorStack = 318, // player->self
    WallOfFireTankbuster = 598, // player->self
    RawSteelSharedTankbuster = 600, // player->self
    VoidStardustSpread = 630, // player->self
}


