namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

public enum OID : uint
{
    Boss = 0x47C6, // R5.5
    ZeleniasShade = 0x47CA, // R5.5
    RosebloodDrop1 = 0x485B, // R1.0
    RosebloodDrop2 = 0x47C2, // R1.0
    BriarThorn = 0x47C7, // R1.0
    Helper2 = 0x47C8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 43242, // Boss->player, no cast, single-target
    Teleport = 43054, // Boss->location, no cast, single-target

    ThornedCatharsis = 43166, // Boss->self, 5.0s cast, range 50 circle
    AlexandrianHoly = 43200, // Boss/ZeleniasShade->self, 3.0s cast, single-target

    Shock = 43169, // Boss/ZeleniasShade->self, 3.0s cast, single-target
    ShockCircleLock = 43170, // Helper->self, no cast, single-target
    Shock1 = 43171, // Helper->self, no cast, range 4 circle
    Shock2 = 43172, // Helper->self, no cast, range 4 circle
    Shock3 = 43173, // Helper->self, no cast, range 4 circle
    Shock4 = 43174, // Helper->self, no cast, range 4 circle
    Shock5 = 43175, // Helper->self, no cast, range 4 circle
    Shock6 = 43176, // Helper->self, no cast, range 4 circle
    ShockDonutLock = 43177, // Helper->self, no cast, single-target
    ShockDonut1 = 43178, // Helper->self, no cast, range 1-6 donut
    ShockDonut2 = 43179, // Helper->self, no cast, range 1-6 donut

    Explosion1 = 43226, // Helper->self, 7.0s cast, range 3 circle, tower
    Explosion2 = 43068, // Helper->self, 6.0s cast, range 3 circle, tower
    ExplosionDonutSectorTower = 43201, // Helper->self, 13.0s cast
    UnmitigatedExplosion1 = 43227, // Helper->self, no cast, range 50 circle, tower 1 fail
    UnmitigatedExplosion2 = 43069, // Helper->self, no cast, range 50 circle, tower 2 fail
    UnmitigatedExplosion3 = 43202, // Helper->self, no cast, range 50 circle, donut sector tower fail

    SpecterOfTheLostVisual = 43167, // Boss->self, 7.0s cast, single-target
    SpecterOfTheLost = 43168, // Helper->player, 0.7s cast, range 48 45-degree cone

    EscelonsFallVisual1 = 43181, // Boss->self, 13.0s cast, single-target
    EscelonsFallVisual2 = 43182, // Boss->self, no cast, single-target
    EscelonsFall = 43183, // Helper->players, no cast, range 24 45-degree cone

    StockBreakVisual = 43221, // Boss->self, 7.0s cast, single-target
    StockBreak1 = 43222, // Helper->location, no cast, range 6 circle
    StockBreak2 = 43223, // Helper->location, no cast, range 6 circle
    StockBreak3 = 43224, // Helper->location, no cast, range 6 circle
    StockBreak4 = 43225, // Helper->location, no cast, range 6 circle

    BlessedBarricade = 43189, // Boss->self, 3.0s cast, single-target
    SpearpointPush1 = 43187, // ZeleniasShade->location, 1.5+0,7s cast, range 33 width 74 rect
    SpearpointPush2 = 43188, // ZeleniasShade->location, 1.5+0,7s cast, range 33 width 74 rect

    PerfumedQuietusVisual = 43191, // Boss->self, 3.0+6,2s cast, range 50 circle
    PerfumedQuietus1 = 43213, // Helper->self, 9.2s cast, range 50 circle
    PerfumedQuietusEnrageVisual = 43192, // Boss->self, 3.0+6,2s cast, range 50 circle
    PerfumedQuietusEnrage = 43214, // Helper->self, 9.2s cast, range 50 circle

    RosebloodBloom = 43193, // Boss->self, 2.6+0,4s cast, single-target
    Roseblood2NdBloom = 43540, // Boss->self, 2.6+0,4s cast, single-target
    Roseblood3RdBloom = 43541, // Boss->self, 2.6+0,4s cast, single-target
    Roseblood4ThBloom = 43542, // Boss->self, 2.6+0,4s cast, single-target
    Roseblood5ThBloom = 43543, // Boss->self, 2.6+0,4s cast, single-target
    Roseblood6ThBloom = 43544, // Boss->self, 2.6+0,4s cast, single-target
    QueensCrusade = 43194, // Helper->self, 3.7s cast, range 2 circle

    AlexandrianThunderIIVisual = 43198, // Boss->self, 5.0s cast, single-target
    AlexandrianThunderIIFirst = 43199, // Helper->self, 5.7s cast, seems to activate a cone and/or circles at the same time, cone is range 24 45-degree
    AlexandrianThunderIIRepeat = 43064, // Helper->self, no cast, seems to activate a cone and/or circles at the same time, cone is range 24 45-degree
    AlexandrianThunderIIIVisual = 43235, // Boss->self, 4.3s cast, single-target
    AlexandrianThunderIIISpread = 43236, // Helper->player, 5.0s cast, seems to activate tiles and circles at the same time, circle is radius 4
    AlexandrianThunderIIIAOE = 43238, // Helper->location, 7.5s cast, seems to activate tiles and circles at the same time, circle is radius 4

    ThunderSlashVisual1 = 43448, // Boss->self, 6.0+0,7s cast, single-target
    ThunderSlashVisual2 = 43449, // Boss->self, 6.0+0,7s cast, single-target
    ThunderSlash = 43216, // Helper->self, 6.7s cast, range 24 60-degree cone

    AlexandrianThunderIVCircle = 43450, // Helper->self, 6.7s cast, range 8 circle
    AlexandrianThunderIVDonut = 43451, // Helper->self, 6.7s cast, range 8-24 donut

    BudOfValor = 43186, // Boss->self, 3.0s cast, single-target
    EmblazonVisual = 43195, // Boss->self, 3.0s cast, single-target
    Emblazon = 43040, // Helper->player, no cast, single-target

    AlexandrianBanishIIVisual = 43217, // ZeleniasShade->self, 5.0s cast, single-target, light party stcks
    AlexandrianBanishII = 43218, // Helper->players, no cast, range 4 circle
    AlexandrianBanishIIIVisual = 43240, // Boss->self, 4.0s cast, single-target, stack
    AlexandrianBanishIII = 43241, // Helper->players, no cast, range 4 circle

    EncirclingThorns = 43203, // Boss->self, 4.0s cast, single-target, chains
    EncirclingThornsFail = 43204, // Helper->self, no cast, ???

    PowerBreak1 = 43184, // ZeleniasShade->self, 6.0s cast, range 24 width 64 rect
    PowerBreak2 = 43185, // ZeleniasShade->self, 6.0s cast, range 24 width 64 rect

    ValorousAscensionVisual = 43206, // Boss->self, 3.0s cast, single-target
    ValorousAscension1 = 43207, // Helper->self, 3.7s cast, range 50 circle
    ValorousAscension2 = 43208, // Helper->self, no cast, range 50 circle
    ValorousAscension3 = 43209, // Helper->self, no cast, range 50 circle
    ValorousAscensionRect = 43210, // BriarThorn->self, 5.0s cast, range 40 width 8 rect

    HolyHazardVisual1 = 43231, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual2 = 43233, // Boss->self, 7.0+0,7s cast, single-target
    HolyHazardVisual3 = 43065, // Boss->self, no cast, single-target
    HolyHazard = 43234, // Helper->self, 7.7s cast, range 24 120-degree cone

    MagicBreak1 = 43196, // Helper->self, no cast, range ?-8 donut
    MagicBreak2 = 43197, // Helper->self, no cast, range ?-16 donut

    RosebloodWithering = 43545, // Boss->self, 2.6+0,4s cast, single-target
    RoseRed = 43205 // Boss->self, 10.0s cast, range 50 circle
}

public enum SID : uint
{
    ThornyVine = 445, // none->player, extra=0x0
    EscelonsFall = 2970 // none->Boss, extra=0x2F7/0x2F6 -> 0x2F7 far, 0x2F6 close
}

public enum IconID : uint
{
    ShockDonut = 580, // player->self
    ShockCircle = 581, // player->self
    StockBreak = 590, // player->self
    SpearpointPush = 23, // player->self
    RotateCW = 167, // Boss
    RotateCCW = 168, // Boss
    Emblazon = 592, // player->self
    AlexandrianBanishII = 93, // player->self
    AlexandrianThunderIII = 596, // player->self
    AlexandrianBanishIII = 597 // player->self
}

public enum TetherID : uint
{
    SpecterOfTheLost = 89, // player->Boss
    SpearpointPush = 17, // ZeleniasShade->player
    ThornyVine = 18 // player->player
}
