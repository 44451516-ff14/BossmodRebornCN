namespace BossMod.Dawntrail.Alliance.A33Awaern;

public enum OID : uint
{
    Awaern = 0x4DB6, // R4.500, x1
    Helper = 0x233C, // R0.500, x25, Helper type

    Awzdei = 0x4DB7, // R2.300, x4

    Alxaal = 0x4D66, // R0.500, x1
    LeatherBoundMemo = 0x1EC003, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x3, EventObj type
    Teleporter = 0x1EBFF9, // R0.500, x1, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x2, EventObj type
    Teleporter1 = 0x1EBFFA, // R0.500, x1, EventObj type
    ResplendentCrystal = 0x1EBFFE, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    SavageBlade = 50389, // Alxaal->Awaern, no cast, single-target
    AutoAttack1 = 50477, // Awzdei->player, no cast, single-target
    AutoAttack2 = 45307, // Awaern->player, no cast, single-target
    VorpalBlade = 50390, // Alxaal->Awaern, no cast, single-target
    GlacierSplitter = 50104, // Awaern->self, 2.9+0.6s cast, single-target
    GlacierSplitter1 = 50105, // Helper->self, 3.5s cast, range 60 30.000-degree cone
    OpticInduration = 50106, // Awzdei->self, 3.5s cast, range 60 30.000-degree cone
    StaticFilament = 50487, // Awzdei->location, 4.0s cast, range 8 circle
    AuroralWind = 50501, // Awaern->self, 5.0s cast, single-target
    AuroralWind1 = 50502, // Helper->players, 5.0s cast, range 6 circle
    SpiritsWithin = 50391, // Alxaal->Awaern, no cast, single-target
    UrielBlade = 50392, // Alxaal->Awaern, no cast, single-target
    ImpactStream = 50485, // Awaern->self, 4.0s cast, single-target
    ImpactStream1 = 50486, // Helper->self, 4.0s cast, range 80 circle
    Holy = 50393, // Alxaal->self, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Awzdei/Helper->player, extra=0x1/0x2/0x3
}

public enum IconID : uint
{
    Icon_target_ae_s5f = 139, // player->self
}
