namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

public enum OID : uint
{
    Boss = 0x30AE, // R2.4
    BloodyWraith = 0x30B0, // R2.0
    MistyWraith = 0x30B1, // R2.0
    MiasmaLowRect = 0x1EB0DD, // R0.5
    MiasmaLowCircle = 0x1EB0DE, // R0.5
    MiasmaLowDonut = 0x1EB0DF, // R0.5
    MiasmaHighRect = 0x1EB0E0, // R0.5
    MiasmaHighCircle = 0x1EB0E1, // R0.5
    MiasmaHighDonut = 0x1EB0E2, // R0.5
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    MaledictionOfAgony = 22461, // Boss->self, 4.0s cast, single-target, visual (raidwide)
    MaledictionOfAgonyAOE = 23350, // Helper->self, no cast, range 40 circle raidwide
    WeaveMiasma = 22450, // Boss->self, 3.0s cast, single-target, visual (create miasma markers)
    ManipulateMiasma = 22451, // Boss->self, 9.0s cast, single-target, visual (low -> high)
    InvertMiasma = 23022, // Boss->self, 9.0s cast, single-target, visual (high -> low)
    CreepingMiasmaFirst = 22452, // Helper->self, 10.0s cast, range 50 width 12 rect
    CreepingMiasmaRest = 22453, // Helper->self, 1.0s cast, range 50 width 12 rect
    LingeringMiasmaFirst = 22454, // Helper->location, 10.0s cast, range 8 circle
    LingeringMiasmaRest = 22455, // Helper->location, 1.0s cast, range 8 circle
    SwirlingMiasmaFirst = 22456, // Helper->location, 10.0s cast, range 5-19 donut
    SwirlingMiasmaRest = 22457, // Helper->location, 1.0s cast, range 5-19 donut
    Transference = 22445, // Boss->location, no cast, single-target, teleport
    Summon = 22464, // Boss->self, 3.0s cast, single-target, visual (go untargetable and spawn adds)
    MaledictionOfRuin = 22465 // Boss->self, 43.0s cast, single-target
}
