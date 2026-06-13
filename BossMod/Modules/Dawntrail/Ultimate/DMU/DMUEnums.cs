namespace BossMod.Dawntrail.Ultimate.DMU;

public enum OID : uint {
    Kefka = 0x4C30, // R6.000, x1
    Helper = 0x233C, // R0.500, x37, Helper type
    GravenImage = 0x4C31, // R0.500, x9
    StatueBodyOrb = 0x1EBFBB, // R0.500, x2, EventObj type
    PurplePuddles = 0x1EC022, // R0.500, x0 (spawn during fight), EventObj type
    StatuePurpleEye = 0x1EBFBE, // R0.500, x2, EventObj type // Purple eye for sleep
    StatueYellowEye = 0x1EBFBF, // R0.500, x2, EventObj type // Yellow eye for confusion
    Actor1e8536 = 0x1E8536, // R0.500-2.000, x1, EventObj type
    Actor1ebfb4 = 0x1EBFB4, // R0.500, x1, EventObj type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    YellowOrb = 0x1EBFBD, // R0.500, x2, EventObj type // Yellow orb
    PurpleOrb = 0x1EBFBC, // R0.500, x2, EventObj type // Purple orb

    BossP2 = 0x4C32,
    P2KefkaHelpers = 0x4C39, // R3.500, x0 (spawn during fight)
    YellowTriangle = 0x1EBFB2, // R0.500, x0 (spawn during fight), EventObj type
    YellowTriangle1 = 0x1EBFB3, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttack = 49746, // Kefka->player, no cast, single-target
    Ability = 50173, // Kefka->location, no cast, single-target

    RevoltingRuinIII = 50179, // Kefka->self/players, 5.0s cast, range 100 ?-degree cone
    RevoltingRuinIII1 = 50401, // Kefka->self, no cast, range 100 ?-degree cone

    GravenImage = 48370, // Kefka->self, 3.0s cast, single-target
    PulseWave = 47785, // 4C31->player, no cast, single-target
    BlizzardIIIBlowout = 47774, // Helper->self, 5.0s cast, range 40 ?-degree cone - Empty zone
    BlizzardIIIBlowout1 = 47771, // Helper->self, 5.0s cast, range 40 ?-degree cone - Questionmark zone
    BlizzardIIIBlowout2 = 47768, // Helper->self, 5.0s cast, range 40 ?-degree cone - Questionmark zone
    BlizzardIIIBlowout3 = 47765, // Kefka->self, 5.0s cast, single-target - Not needed it seems
    MysteryMagic = 47764, // Kefka->self, 5.0s cast, single-target
    FlagrantFireIIIStack = 47779, // Helper->players, no cast, range 6 circle - Stack
    FlagrantFireIIISpread = 47778, // Helper->players, no cast, range 5 circle

    WaveCannon = 47784, // 4C31->self, no cast, range 100 width 6 rect
    TowerExplosion = 47786, // Helper->self, 3.0s cast, range 4 circle
    UnmitigatedExplosion = 47787, // Helper->self, no cast, range 100 circle - AOE if tower is missed, we don't care about this

    DoubleTroubleTrap = 47782, // Kefka->self, 3.0s cast, single-target
    DoubleTroubleTrap1 = 47783, // Helper->player, no cast, range 6 circle

    ThrummingThunderIII = 47775, // Helper->self, 5.0s cast, range 40 width 10 rect
    ThrummingThunderIII1 = 47776, // Helper->self, 5.0s cast, range 40 width 10 rect
    ThrummingThunderIII2 = 47777, // Helper->self, 5.0s cast, range 40 width 10 rect

    LightOfJudgment = 50722, // Kefka->self, 5.0s cast, range 100 circle
    Hyperdrive = 49739, // Kefka->player, no cast, range 5 circle

    Gravitas = 47788, // 4C31->player, no cast, range 5 circle - Purple puddle - Stack
    Vitrophyre = 47792, // _Gen_GravenImage->players, no cast, range 5 circle - Rock - Spread
    GravityIII = 47791, // Helper->self, no cast, range 5 circle
    GravitationalExplosion = 47789, // Helper->self, no cast, range 100 circle
    GravitationalWave = 47793, // 4C31->self, no cast, range 100 ?-degree cone - Purple left side
    IntemperateWill = 47794, // _Gen_GravenImage->self, no cast, range 100 ?-degree cone - Yellow right side

    TeleTrouncing = 47801, // Kefka->self, 5.0s cast, single-target
    TeleTrouncing1 = 47802, // Helper->players, no cast, range 2 circle
    IdyllicWill = 47798, // _Gen_GravenImage->players, no cast, range 5 circle
    IndulgentWill = 47797, // _Gen_GravenImage->player, no cast, single-target
    Weaponskill = 50516, // Kefka->self, 3.0s cast, single-target

    AveMaria = 47795, // _Gen_GravenImage->self, no cast, range 100 circle
    IndolentWill = 47796, // _Gen_GravenImage->self, no cast, range 100 circle

    Weaponskill2 = 50517, // Kefka->self, no cast, single-target
    LightOfJudgment2 = 47803, // Kefka->self, 5.0s cast, range 100 circle

    // Phase 2
    UltimateEmbrace = 49740, // BossP2->players, 5.0s cast, range 5 circle
    Forsaken = 47804, // BossP2->self, 7.0s cast, range 100 circle
    ThePathOfLight = 47806, // Helper->self, no cast, range 4 circle
    TheRiverOfLight = 47807, // Helper->self, no cast, range 100 circle

    Spellscatter = 47809, // Helper->player, no cast, range 5 circle
    Spelldriver = 47808, // Helper->player, no cast, range 5 circle
    Spellwave = 47810, // Helper->self, no cast, range 40 ?-degree cone

    PastsEnd = 47827, // BossP2->self, 6.4s cast, single-target
    FuturesEnd = 47826, // BossP2->self, 6.4s cast, single-target
    FuturesEndSpread = 47830, // BossP2->players, no cast, range 5 circle
    FuturesEndSpread1 = 47832, // 4C39->players, no cast, range 5 circle
    PastsEndSpread = 47831, // BossP2->player, no cast, range 5 circle
    PastsEndSpread1 = 47833, // 4C39->players, no cast, range 5 circle

    AllThingsEnding = 47837, // BossP2->self, 5.0s cast, range 100 ?-degree cone
    AllThingsEnding1 = 47836, // 4C39/BossP2->self, 5.0s cast, range 100 ?-degree cone

    LightOfJudgmentP2 = 47805, // BossP2->self, 5.0s cast, range 100 circle

    Trine = 47839, // BossP2->self, 3.0s cast, single-target
    Trine1 = 47840, // Helper->self, no cast, range 6 circle
    WingsOfDestructionLeft = 47821, // BossP2->self, 4.0s cast, range 80 width 40 rect
    WingsOfDestructionRight = 47822, // BossP2->self, 4.0s cast, range 80 width 40 rect
    WingsOfDestructionTB = 50311, // BossP2->self, 4.0s cast, single-target
    WingsOfDestructionTB1 = 47823, // Helper->player, no cast, range 7 circle
}

public enum SID : uint {
    DoubleTroubleTrap = 5078, // none->player, extra=0x0
    TelePortentRIGHT = 4878, // Helper->player, extra=0x0
    TelePortentUP = 4876, // Helper->player, extra=0x0
    TelePortentDOWN = 4877, // Helper->player, extra=0x0
    TelePortentLEFT = 4879, // Helper->player, extra=0x0
    TelePortentUP2 = 5079, // Helper->player, extra=0x0
    TelePortentDOWN2 = 5080, // Helper->player, extra=0x0
    TelePortentLEFT2 = 5082, // Helper->player, extra=0x0
    TelePortentRIGHT2 = 5081, // Helper->player, extra=0x0

    MagicVulnerabilityUp = 2941, // Helper/4C31->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    Confused = 1283, // _Gen_GravenImage->player, extra=0x0
    Sleep = 4894, // _Gen_GravenImage->player, extra=0x0
    DamageDown = 2911, // Helper->player, extra=0x0

    _Gen_SpellsTrouble = 5083, // none->player, extra=0x4/0x3
    _Gen_1 = 5085, // none->player, extra=0x0
    _Gen_2 = 5086, // none->player, extra=0x0
    _Gen_3 = 5084, // none->player, extra=0x0
}

public enum IconID : uint {
    TankIcon = 218, // player->self
    spreadIcon = 127, // player->self // Spread
    stackIcon = 128, // player->self // Stack
    FireRingQuestionMark = 673, // Kefka->self // Questionmark - 2A1
    FireRingBlueOrb = 674, // Kefka->self // Blue orb - 2A2
    BlueRingQuestionMark = 675, // Kefka->self // Questionmark - 2A3
    BlueRingBlueOrb = 676, // Kefka->self // Blue orb - 2A4
    PurpleRingQuestionMark = 677, // Kefka->self // Questionmark
    PurpleRingBlueOrb = 678, // Kefka->self // Blue orb

    TowerStackIcon = 715, // player->self
    TowerSpreadIcon = 716, // player->self
    TowerConeIcon = 717, // player->self

    _Gen_Icon_m0676trg_tw_d0t1p = 259, // player->self
}

public enum Animations : uint {
    PulseOrbStart = 4194432,
    PulseOrbEnd = 16777728,
    PuddleSoakReady = 1048608,
    PuddleExplosion = 262152,
    EyeStart = 4194432,
    EyeEnd = 16777728,
    TriangleFlyingDown = 1048608,
    TriangleLanded = 4194432,
    TriangleExplosion = 262152,
}

public enum TetherID : uint {
    GravenImageTether = 45, // 4C31->player
}
