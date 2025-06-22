namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN4Phantom;

public enum OID : uint
{
    Boss = 0x30AC, // R2.4
    StuffyWraith = 0x30AD, // R2.2
    MiasmaDonut = 0x1EB0DF, // R0.5
    MiasmaRect = 0x1EB0DD, // R0.5
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    WeaveMiasma = 22435, // Boss->self, 3.0s cast, single-target, visual (create miasma markers)
    ManipulateMiasma = 22436, // Boss->self, 7.0s cast, single-target
    SwirlingMiasmaFirst = 22441, // Helper->location, 8.0s cast, range 5-19 donut
    SwirlingMiasmaRest = 22442, // Helper->location, 1.0s cast, range 5-19 donut
    CreepingMiasma = 22437, // Helper->self, 12.0s cast, range 50 width 12 rect
    MaledictionOfAgony = 22447, // Boss->self, 4.0s cast, range 70 circle
    Summon = 22443, // Boss->self, 3.0s cast, single-target
    UndyingHatred = 22444, // StuffyWraith->self, 6.0s cast, range 60 width 48 rect, knockback 30 dir forward
    Transference = 22445, // Boss->location, no cast, single-target, teleport
    VileWave = 22449 // Boss->self, 6.0s cast, range 45 120-degree cone
}
