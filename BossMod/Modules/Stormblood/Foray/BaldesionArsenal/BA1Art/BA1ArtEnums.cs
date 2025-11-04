namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Art;

public enum OID : uint
{
    Boss = 0x265A, // R2.7
    Orlasrach = 0x265B, // R2.7
    Owain = 0x2662, // R2.7
    ShadowLinksHelper = 0x1EA1A1, // R2.0 (if pos -134.917, 750.44)
    Helper = 0x265C
}

public enum AID : uint
{
    AutoAttack = 14678, // Boss->player, no cast, single-target

    Thricecull = 14644, // Boss->player, 4.0s cast, single-target, tankbuster
    Legendspinner = 14633, // Boss->self, 4.5s cast, range 7-22 donut
    Legendcarver = 14632, // Boss->self, 4.5s cast, range 15 circle

    AcallamNaSenorach = 14645, // Boss->self, 4.0s cast, range 60 circle, raidwide
    AcallamNaSenorachArt = 14628, // Boss->self, 7.0s cast, range 80 circle, enrage if Owain side does not get pulled, Owain teleports to Art
    AcallamNaSenorachOwain = 14629, // Owain->self, 7.0s cast, range 80 circle

    Mythcall = 14631, // Boss->self, 2.0s cast, single-target
    Mythspinner = 14635, // Orlasrach->self, no cast, range 7-22 donut
    Mythcarver = 14634, // Orlasrach->self, no cast, range 15 circle
    LegendaryGeas = 14642, // Boss->location, 4.0s cast, range 8 circle
    DefilersDeserts = 14643, // Helper->self, 3.5s cast, range 35+R width 8 rect
    GloryUnearthedFirst = 14636, // Helper->location, 5.0s cast, range 10 circle
    GloryUnearthedRest = 14637, // Helper->location, no cast, range 10 circle
    Pitfall = 14639, // Boss->location, 5.0s cast, range 80 circle, damage fall off AOE
    PiercingDarkVisual = 14640, // Boss->self, 2.5s cast, single-target
    PiercingDark = 14641 // Helper->player, 5.0s cast, range 6 circle, spread
}

public enum IconID : uint
{
    GloryUnearthed = 92 // player->self
}
