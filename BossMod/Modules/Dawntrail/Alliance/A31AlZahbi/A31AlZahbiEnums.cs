namespace BossMod.Dawntrail.Alliance.A31AlZahbi;

public enum OID : uint
{
    Acrolith = 0x4DB5, // R3.000, x3
    LamiaRover1 = 0x4DB4, // R1.100, x2
    LamiaRover2 = 0x4DA3, // R1.100, x2

    LamiaJaeger = 0x4DA5,
    QutrubForayer = 0x4DA7,
    PiningAbazohn = 0x4DA6,

    AssaultBhoot = 0x4DAA,
    NemeanLion = 0x4DA9,
    LamiaNo2 = 0x4DA8,

    Medusa = 0x4DAB, // R2.250, x1

    Helper = 0x233C, // R0.500, x22, Helper type
    Alxaal = 0x4D66, // R0.500, x1
    Teleporter = 0x1EBFF7, // R0.500, x1, EventObj type
    LeatherBoundMemo = 0x1EC000, // R0.500, x1, EventObj type
    SpatialRift = 0x1EC014, // R0.500, x1, EventObj type
    SalaheemsSentinel = 0x4DB3, // R0.500, x1
    SalaheemsSentinel1 = 0x4DB2, // R0.500, x1
    SalaheemsSentinel2 = 0x4DB0, // R0.500, x1
    SalaheemsSentinel3 = 0x4DAE, // R0.500, x1
    SalaheemsSentinel4 = 0x4DAF, // R0.500, x1
    SalaheemsSentinel5 = 0x4DB1, // R0.500, x1
    SalaheemsSentinel6 = 0x4DAC, // R0.500, x1
    SalaheemsSentinel7 = 0x4DAD, // R0.500, x1
}

public enum AID : uint
{
    AutoAttack1 = 870, // LamiaRover1/SalaheemsSentinel7/SalaheemsSentinel4/Medusa->player/SalaheemsSentinel4/SalaheemsSentinel6/Acrolith/LamiaRover1, no cast, single-target
    AutoAttack2 = 872, // SalaheemsSentinel6/Acrolith/SalaheemsSentinel1->LamiaRover1/SalaheemsSentinel1/SalaheemsSentinel7/SalaheemsSentinel5/Acrolith, no cast, single-target
    UnknownSpell1 = 50507, // SalaheemsSentinel3->Acrolith, 2.0s cast, single-target
    Attack3 = 871, // SalaheemsSentinel2->Acrolith, no cast, single-target
    UnknownSpell2 = 50512, // SalaheemsSentinel/SalaheemsSentinel5->SalaheemsSentinel1/SalaheemsSentinel2, 2.0s cast, single-target
    UnknownWeaponskill1 = 50503, // SalaheemsSentinel6->LamiaRover1, no cast, single-target
    UnknownSpell3 = 50508, // SalaheemsSentinel3->Acrolith, 2.0s cast, single-target
    UnknownSpell4 = 50515, // SalaheemsSentinel/SalaheemsSentinel5->self, 3.0s cast, single-target
    UnknownSpell5 = 50509, // SalaheemsSentinel3->Acrolith, 2.0s cast, single-target
    UnknownWeaponskill2 = 50504, // SalaheemsSentinel1/SalaheemsSentinel7->Acrolith, no cast, single-target
    DanceToDust = 50096, // Helper->self, 5.0s cast, range 7 circle
    DanceToDust1 = 50095, // Medusa->self, 5.0s cast, single-target
    DanceToDust2 = 50097, // Helper->self, no cast, range 7 circle
    UnknownAbility1 = 50506, // SalaheemsSentinel1/SalaheemsSentinel7->self, no cast, single-target
    LeftShadowSlash = 50099, // Medusa->self, 5.0s cast, range 60 180.000-degree cone
    SavageBlade = 50389, // Alxaal->Medusa, no cast, single-target
    VorpalBlade = 50390, // Alxaal->Medusa, no cast, single-target
    BellowingGrunt = 50103, // Medusa->self, 4.0s cast, range 60 circle
    SpiritsWithin = 50391, // Alxaal->Medusa, no cast, single-target
    UrielBlade = 50392, // Alxaal->Medusa, no cast, single-target
    Disregard = 50100, // Medusa->self, 4.0s cast, range 60 circle
    Disregard1 = 50101, // Helper->self, 4.0s cast, range 55 width 10 rect
    Petrifaction = 50102, // Medusa->self, 5.0s cast, range 60 circle
    RightShadowSlash = 50098, // Medusa->self, 5.0s cast, range 60 180.000-degree cone
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // LamiaNo2/Medusa->player, extra=0x1/0x2/0x3
    Petrification = 1511, // Medusa->player, extra=0x0
}
