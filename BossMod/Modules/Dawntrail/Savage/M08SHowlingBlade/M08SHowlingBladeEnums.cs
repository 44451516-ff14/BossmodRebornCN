namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

public enum OID : uint
{
    Boss = 0x4727, // R5.005
    BossP2 = 0x472E, // R19.0
    HowlingBladePart = 0x4733, // R0.0

    GleamingFang1 = 0x472D, // R1.4
    GleamingFang2 = 0x472C, // R1.4
    GleamingFangP21 = 0x472F, // R3.0
    GleamingFangP22 = 0x4730, // R3.0
    MoonlitShadow = 0x4729, // R5.005
    HowlingBladeShadow = 0x4728, // R4.235

    FontOfWindAether = 0x4755, // R1.0-1.5
    FontOfEarthAether = 0x4756, // R1.0-1.5
    WolfOfWind1 = 0x472A, // R1.5
    WolfOfWind2 = 0x472B, // R1.5
    WolfOfWind3 = 0x47B2, // R1.5
    WolfOfWind4 = 0x485F, // R1.5
    WolfOfWindP2 = 0x486E, // R1.5
    WolfOfStone1 = 0x47B3, // R1.5
    WolfOfStone2 = 0x4731, // R1.5
    WolfOfStone3 = 0x484B, // R1.5
    WolfOfStoneP2 = 0x486F, // R1.5
    WindVoidzone = 0x1EBD8F, // R0.5, growing, final size 9
    Helper = 0x233C, // R0.5
}

public enum AID : uint
{
    AutoAttack = 42222, // Boss->player, no cast, single-target
    AutoAttackAdd1 = 42225, // WolfOfWind1->player, no cast, single-target
    AutoAttackAdd2 = 42226, // WolfOfStone2->player, no cast, single-target
    Teleport = 41871, // Boss->location, no cast, single-target

    ExtraplanarPursuitVisual = 41946, // Boss->self, 1.6+2,4s cast, single-target
    ExtraplanarPursuit = 42831, // Helper->self, 4.0s cast, range 40 circle

    StonefangCircle = 41904, // Helper->self, 6.0s cast, range 9 circle
    StonefangCross1 = 41890, // Boss->self, 6.0s cast, range 15 width 6 cross
    StonefangCross2 = 41889, // Boss->self, 6.0s cast, range 15 width 6 cross
    StonefangBait = 41905, // Helper->self, no cast, range 40 30-degree cone
    WindfangDonut = 41887, // Helper->self, 6.0s cast, range 8-20 donut
    WindfangCross1 = 41885, // Boss->self, 6.0s cast, range 15 width 6 cross
    WindfangCross2 = 41886, // Boss->self, 6.0s cast, range 15 width 6 cross
    WindfangBait = 41888, // Helper->self, no cast, range 40 30-degree cone

    EminentReignVisual1 = 43282, // Boss->self, 5.1s cast, single-target
    EminentReignVisual2 = 43281, // Boss->self, 5.1s cast, single-target
    EminentReignTeleport = 43297, // Boss->location, no cast, single-target
    WolvesReignVisual1 = 43305, // HowlingBladeShadow->self, 6.4s cast, single-target
    WolvesReignVisual2 = 43306, // HowlingBladeShadow->self, 6.5s cast, single-target
    WolvesReignVisual3 = 43307, // HowlingBladeShadow->self, 6.6s cast, single-target
    WolvesReignVisual4 = 41881, // Boss->self, 1.0+0,5s cast, single-target
    WolvesReignVisual5 = 41882, // Boss->self, 1.0+0,5s cast, single-target
    WolvesReignTeleport1 = 41880, // Boss->location, 1.0+0,5s cast, single-target, cone after
    WolvesReignTeleport2 = 42927, // Boss->location, 1.0+0,5s cast, single-target, circle after

    WolvesReignCircle1 = 43308, // Helper->self, 6.7s cast, range 6 circle
    WolvesReignCircle2 = 43309, // Helper->self, 6.8s cast, range 6 circle
    WolvesReignCircle3 = 43310, // Helper->self, 6.9s cast, range 6 circle
    WolvesReignCircleBig = 42930, // Helper->self, 1.5s cast, range 14 circle
    EminentReign = 43312, // Helper->self, 7.0s cast, range 6 circle

    WolvesReignRect1 = 43369, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignRect2 = 43370, // Helper->self, 1.5s cast, range 28 width 10 rect
    WolvesReignCone = 42929, // Helper->self, 1.5s cast, range 40 120-degree cone
    ReignsEnd = 41883, // Helper->self, no cast, range 40 60-degree cone
    SovereignScar = 41884, // Helper->self, no cast, range 40 30-degree cone

    RevolutionaryReignVisual1 = 43284, // Boss->self, 5.1s cast, single-target
    RevolutionaryReignVisual2 = 43283, // Boss->self, 5.1s cast, single-target
    RevolutionaryReignTeleport = 43298, // Boss->location, no cast, single-target
    RevolutionaryReign = 43313, // Helper->self, 7.0s cast, range 6 circle

    MillennialDecay = 41906, // Boss->self, 5.0s cast, range 40 circle
    AeroIIIVisual = 41911, // Boss->self, 5.0s cast, single-target
    AeroIII = 41912, // Helper->self, 5.0s cast, range 40 circle, knockback 8, away from source
    BreathOfDecay = 41908, // WolfOfWind->self, 8.0s cast, range 40 width 8 rect
    Gust = 41907, // Helper->players, no cast, range 5 circle, spread
    ProwlingGale = 41910, // Helper->location, 7.0s cast, range 2 circle
    GreatWhirlwind = 41957, // Helper->self, no cast, range 40 circle, tower fail
    WindsOfDecay = 41909, // WolfOfWind->self, no cast, range 40 45-degree cone
    TrackingTremorsVisual = 41913, // Boss->self, 5.0s cast, single-target
    TrackingTremors = 41915, // Helper->players, no cast, range 6 circle

    GreatDivide = 41944, // Boss->self/players, 5.0s cast, range 60 width 6 rect, shared tankbuster

    TerrestrialTitansVisual = 41924, // Boss->self, 4.0s cast, single-target
    TerrestrialTitans = 41925, // Helper->self, 4.0s cast, range 5 circle
    Towerfall = 41926, // Helper->self, 8.0s cast, range 30 width 10 rect

    TitanicPursuitVisual = 41927, // Boss->self, 1.6+2,4s cast, range 40 circle
    TitanicPursuit = 42833, // Helper->self, 4.0s cast, range 40 circle
    BareFangs1 = 42188, // Boss->self, no cast, single-target
    BareFangs2 = 42187, // Boss->self, no cast, single-target
    FangedCrossing = 41943, // GleamingFang1->self, 4.0s cast, range 21 width 7 cross

    TacticalPackStart = 42886, // Boss->self, no cast, single-target
    TacticalPack = 41928, // Boss->self, 3.0s cast, single-target
    TacticalPackVisual = 41929, // Boss->self, no cast, single-target, model state changes to 31
    HowlingHavocVisual1 = 41947, // WolfOfWind1->self, 5.0s cast, single-target
    HowlingHavocVisual2 = 41948, // WolfOfStone2->self, 5.0s cast, single-target
    HowlingHavoc = 41949, // Helper->self, 5.0s cast, range 40 circle
    PackPredation = 41932, // WolfOfWind1/WolfOfStone2->self, 5.0s cast, single-target
    AlphaWind = 41933, // Helper->self, no cast, range 40 90-degree cone
    StalkingWind = 41935, // Helper->self, no cast, range 40 width 6 rect
    AlphaStone = 41954, // Helper->self, no cast, range 40 90-degree cone
    StalkingStone = 41956, // Helper->self, no cast, range 40 width 6 rect

    GaleSurge = 41967, // FontOfWindAether->self, no cast, range 40 circle, enrage
    StoneSurge = 41968, // FontOfEarthAether->self, no cast, range 40 circle, enrage
    SandSurge1 = 41966, // FontOfEarthAether->self, no cast, ???
    SandSurge2 = 43138, // FontOfEarthAether->self, no cast, ???
    SandSurge3 = 43520, // FontOfEarthAether->self, no cast, ???
    WindSurge1 = 43137, // FontOfWindAether->self, no cast, ???
    WindSurge2 = 43519, // FontOfWindAether->self, no cast, ???
    WindSurge3 = 41965, // FontOfWindAether->self, no cast, ???
    RavenousSaber1 = 42825, // Helper->self, 3.4s cast, range 40 circle
    RavenousSaber2 = 42826, // Helper->self, 3.6s cast, range 40 circle
    RavenousSaber3 = 42827, // Helper->self, 3.9s cast, range 40 circle
    RavenousSaber4 = 43518, // Helper->self, 6.0s cast, range 40 circle
    RavenousSaber5 = 41931, // Helper->self, 7.3s cast, range 40 circle

    TerrestrialRage = 41918, // Boss->self, 3.0s cast, single-target
    FangedCharge = 41942, // GleamingFang2->self, 4.0s cast, range 30 width 6 rect
    SuspendedStone = 41919, // Helper->players, no cast, range 6 circle
    Heavensearth = 41920, // Helper->players, no cast, range 6 circle
    ShadowchaseVisual = 41916, // Boss->self, 3.0s cast, single-target
    Shadowchase = 41917, // HowlingBladeShadow->self, 2.0s cast, range 40 width 8 rect

    RoaringWindVisual = 42889, // WolfOfWind3->self, 2.5s cast, single-target
    RoaringWind = 42890, // Helper->self, 2.5s cast, range 40 width 8 rect
    WealOfStoneVisual1 = 42893, // WolfOfStone3->self, 2.5s cast, single-target
    WealOfStoneVisual2 = 42897, // WolfOfStone3->self, 2.5s cast, single-target
    WealOfStone1 = 42894, // Helper->self, 2.5s cast, range 40 width 6 rect
    WealOfStone2 = 42898, // Helper->self, 2.5s cast, range 40 width 6 rect

    BeckonMoonlight = 41921, // Boss->self, 3.0s cast, single-target
    BeckonMoonlightTeleport1 = 41952, // MoonlitShadow->location, no cast, single-target
    BeckonMoonlightTeleport2 = 41953, // MoonlitShadow->location, no cast, single-target
    MoonbeamsBite1 = 41922, // MoonlitShadow->self, 9.0s cast, range 40 width 20 rect
    MoonbeamsBite2 = 41923, // MoonlitShadow->self, 9.0s cast, range 40 width 20 rect

    ForlornStoneVisual1 = 41939, // WolfOfStone2->self, 8.0s cast, single-target
    ForlornStoneVisual2 = 41940, // WolfOfStone2->self, no cast, single-target
    ForlornStone = 41941, // Helper->self, no cast, range 40 circle, enrage
    ForlornWindVisual1 = 41936, // WolfOfWind1->self, 8.0s cast, single-target
    ForlornWindVisual2 = 41937, // WolfOfWind1->self, no cast, single-target
    ForlornWind = 41938, // Helper->self, no cast, range 40 circle, enrage

    ExtraplanarFeast = 41969, // Boss->self, 4.0s cast, range 40 circle, enrage phase 1

    PhaseChangePull = 43053, // Helper->self, no cast, range 60 circle, pull 50 between hitboxes

    AutoAttackP2Visual = 42227, // BossP2->player, no cast, single-target
    AutoAttackP21 = 42228, // HowlingBladePart->player, no cast, single-target

    QuakeIIIVisual = 42074, // BossP2->self, 5.0s cast, single-target, stack marker
    QuakeIII = 42075, // Helper->self, no cast, ???, hits the whole platform, range 8 circle
    UltraviolentRayVisual = 42076, // BossP2->self, 6.0s cast, single-target
    UltraviolentRay = 42077, // Helper->self, no cast, ???, hits the whole platform, range 40 width 17 rect
    GleamingBeam = 42078, // GleamingFangP21->self, 4.0s cast, range 31 width 8 rect

    TwinbiteVisual = 42189, // BossP2->self, 7.0s cast, single-target, tankbuster
    Twinbite = 42190, // Helper->self, no cast, ???, range 8 circle, hits the whole platform

    HerosBlowVisual1 = 42081, // BossP2->self, 6.0+1,0s cast, single-target
    HerosBlowVisual2 = 42079, // BossP2->self, 6.0+1,0s cast, single-target
    HerosBlow1 = 42082, // Helper->self, 7.0s cast, range 40 180-degree cone
    HerosBlow2 = 42080, // Helper->self, 7.0s cast, range 40 180-degree cone
    FangedMaw = 42083, // GleamingFangP22->self, 7.0s cast, range 22 circle
    FangedPerimeter = 42084, // GleamingFangP22->self, 7.0s cast, range 15-30 donut

    MooncleaverVisual1 = 42085, // BossP2->self, 4.0+1,0s cast, single-target
    MooncleaverVisual2 = 42828, // BossP2->self, 3.0+1,0s cast, single-target
    Mooncleaver1 = 42086, // Helper->self, 5.0s cast, range 8 circle
    Mooncleaver2 = 42829, // Helper->self, 4.0s cast, range 8 circle

    ElementalPurge = 42087, // BossP2->self, 5.0s cast, single-target
    HuntersHarvestVisual = 42092, // BossP2->self, no cast, single-target
    HuntersHarvest = 42093, // Helper->self, 1.0s cast, range 40 210-degree cone, targets center of platform of maintank
    AerotemporalBlast = 42088, // Helper->self, no cast, range 16 circle
    GeotemporalBlast = 42089, // Helper->self, no cast, range 6 circle

    ProwlingGaleP2Visual = 42094, // BossP2->self, 3.0s cast, single-target
    ProwlingGaleP2 = 42095, // Helper->self, 8.0s cast, range 2 circle
    GreatWhirlwindP2 = 42096, // Helper->self, no cast, range 60 circle, tower fail
    RiseOfTheHowlingWind = 43050, // BossP2->self, 7.0s cast, single-target
    TwofoldTempestVisual1 = 42097, // BossP2->self, 7.0s cast, single-target
    TwofoldTempestVisual2 = 42100, // BossP2->self, no cast, single-target
    TwofoldTempestRect = 42099, // Helper->self, no cast, ???, hits the whole platform, , range 40 width 16 rect
    TwofoldTempestCircle = 42098, // Helper->player, no cast, range 6 circle
    VacuumWave = 42191, // Helper->self, no cast, range 60 circle, two voidzones intersecting
    WindSurge = 43153, // Helper->self, no cast, ???, wind voidzone pulsing damage

    BareFangs = 42101, // BossP2->self, 4.0s cast, single-target
    ChampionsCircuitCW = 42103, // BossP2->self, 7.3+0,7s cast, single-target
    ChampionsCircuitCCW = 42104, // BossP2->self, 7.3+0,7s cast, single-target
    GleamingBarrage = 42102, // GleamingFangP21->self, 2.8s cast, range 31 width 8 rect
    ChampionsCircuitDonutFirst = 42106, // Helper->self, 8.0s cast, range 4-13 donut
    ChampionsCircuitConeFirst = 42108, // Helper->self, 8.0s cast, range 22 60-degree cone
    ChampionsCircuitDonutSectorFirst1 = 42109, // Helper->self, 8.0s cast, range 16-28 60-degree donut sector
    ChampionsCircuitDonutSectorFirst2 = 42107, // Helper->self, 8.0s cast, range 16-28 60-degree donut sector
    ChampionsCircuitRectFirst = 42105, // Helper->self, 8.0s cast, range 30 width 12 rect
    ChampionsCircuitRestCWVisual = 42145, // BossP2->self, no cast, single-target
    ChampionsCircuitRestCCWVisual = 42146, // BossP2->self, no cast, single-target
    ChampionsCircuitDonutRest = 42111, // Helper->self, 0.7s cast, range 4-13 donut
    ChampionsCircuitDonutSectorRest1 = 42112, // Helper->self, 0.7s cast, range ?-28 donut
    ChampionsCircuitDonutSectorRest2 = 42114, // Helper->self, 0.7s cast, range ?-28 donut
    ChampionsCircuitConeRest = 42113, // Helper->self, 0.7s cast, range 22 60-degree cone
    ChampionsCircuitRectRest = 42110, // Helper->self, 0.7s cast, range 30 width 12 rect

    RiseOfTheHuntersBlade = 43052, // BossP2->self, 7.0s cast, single-target
    LoneWolfsLament = 42115, // BossP2->self, 3.0s cast, single-target
    ProwlingGaleLastVisual = 42117, // BossP2->self, 3.0s cast, single-target
    ProwlingGaleLast1 = 42118, // Helper->self, 8.0s cast, range 2 circle, 1p tower
    ProwlingGaleLast2 = 42119, // Helper->self, 8.0s cast, range 2 circle, 2p tower
    ProwlingGaleLast3 = 42120, // Helper->self, 8.0s cast, range 2 circle, 3p tower
    GreatWhirlwindLast = 42121, // Helper->self, no cast, range 60 circle, tower fail
    UnmitigatedExplosion = 42116, // Helper->player, no cast, single-target, chains fail

    HowlingEightVisualFirst = 43522, // BossP2->location, 8.0+1,0s cast, single-target
    HowlingEightFirst1 = 43523, // Helper->self, 9.1s cast, range 8 circle
    HowlingEightFirst2 = 43524, // Helper->self, 10.1s cast, range 8 circle
    HowlingEightFirst3 = 43525, // Helper->self, 11.0s cast, range 8 circle
    HowlingEightFirst4 = 43526, // Helper->self, 11.8s cast, range 8 circle
    HowlingEightFirst5 = 43527, // Helper->self, 12.4s cast, range 8 circle
    HowlingEightFirst6 = 43528, // Helper->self, 12.9s cast, range 8 circle
    HowlingEightFirst7 = 43529, // Helper->self, 13.3s cast, range 8 circle
    HowlingEightFirst8 = 43530, // Helper->self, 15.1s cast, range 8 circle
    HowlingEightVisualRest = 42132, // BossP2->location, 5.0+1,0s cast, single-target
    HowlingEightRest1 = 42133, // Helper->self, 6.1s cast, range 8 circle
    HowlingEightRest2 = 42134, // Helper->self, 7.1s cast, range 8 circle
    HowlingEightRest3 = 42135, // Helper->self, 8.0s cast, range 8 circle
    HowlingEightRest4 = 42136, // Helper->self, 8.8s cast, range 8 circle
    HowlingEightRest5 = 42137, // Helper->self, 9.4s cast, range 8 circle
    HowlingEightRest6 = 42138, // Helper->self, 9.9s cast, range 8 circle
    HowlingEightRest7 = 42139, // Helper->self, 10.3s cast, range 8 circle
    HowlingEightRest8 = 42140, // Helper->self, 12.1s cast, range 8 circle
    HowlingAgony = 43140, // Helper->self, no cast, range 100 circle, tower fail

    StarcleaverVisual = 42141, // BossP2->self, 10.0s cast, single-target
    Starcleaver = 42142 // Helper->self, 11.0s cast, range 8 circle
}

public enum SID : uint
{
    Windpack = 4389, // none->FontOfWindAether/player, extra=0x0
    Stonepack = 4390, // none->FontOfEarthAether/player, extra=0x0
    WindborneEnd = 4392, // none->player, extra=0x0
    EarthborneEnd = 4391, // none->player, extra=0x0
    ImmobileSuit = 2578, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0
    MagicVulnerabilityUp = 2941 // Helper/WolfOfWind1/FontOfEarthAether/FontOfWindAether->player, extra=0x0
}

public enum IconID : uint
{
    Gust = 376, // player->self
    TrackingTremors = 316, // player->self
    StalkingStoneWind = 23, // player->self, also used for Aerotemporal and Geotemporal Blast in P2
    SuspendedStone = 139, // player->self
    Heavensearth = 93, // player->self, also QuakeIII in P2
    UltraviolentRay = 14 // player->self
}

public enum TetherID : uint
{
    WindsOfDecayBad = 57, // WolfOfWind->player
    WindsOfDecayGood = 1, // WolfOfWind->player
    Stonetether = 335, // player->WolfOfStone2
    Windtether = 336, // player->WolfOfWind1
    TwofoldTempest = 84, // BossP2->player
    GreenChains = 317, // player->player
    BlueChains = 318 // player->player
}
