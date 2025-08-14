namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

public enum OID : uint
{
    Boss = 0x31AB, // R9.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    AmbientPulsationVisual = 23693, // Boss->self, 5.0s cast, single-target
    AmbientPulsation = 23694, // Helper->self, 8.0s cast, range 12 circle

    PutrifiedSoul1 = 23695, // Boss->self, 5.0s cast, ???, raidwide, always casted together with PutrifiedSoul2
    PutrifiedSoul2 = 23696, // Helper->self, 5.0s cast, ???
    BurgeoningDread1 = 23688, // Boss->self, 5.0s cast, ???, raidwide, always casted together with BurgeoningDread2, applies forced march debuffs
    BurgeoningDread2 = 23689, // Helper->self, 5.0s cast, ???
    GhastlyAura1 = 24909, // Boss->self, 5.0s cast, ???, raidwide, always casted together with GhastlyAura2, applies temporary misdirection
    GhastlyAura2 = 24910, // Helper->self, 5.0s cast, ???

    FellFlow = 23691, // Boss->self, 5.0s cast, range 50 120-degree cone
    FellFlowBait = 23692, // Helper->self, no cast, range 50 30-degree cone

    FleshyNecromassVisual = 23682, // Boss->self, 8.0s cast, single-target
    FleshyNecromassJump1 = 23683, // Boss->location, no cast, single-target
    FleshyNecromassJump2 = 24953, // Boss->location, no cast, single-target
    FleshyNecromass1 = 23684, // Helper->self, no cast, range 12 circle, always casted together with FleshyNecromass2
    FleshyNecromass2 = 23685, // Helper->self, no cast, range 12 circle

    MightOfMalice = 23698, // Boss->player, 5.0s cast, single-target

    NecroticBillowVisual = 23686, // Boss->self, 5.0s cast, single-target
    NecroticBillow = 23687 // Helper->self, 4.0s cast, range 8 circle
}

public enum SID : uint
{
    AboutFace = 2162, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    ForwardMarch = 2161, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x2/0x1/0x8/0x4
    Gelatinous = 2543 // none->player, extra=0x1AD
}

public enum IconID : uint
{
    FellFlow = 40 // player
}
