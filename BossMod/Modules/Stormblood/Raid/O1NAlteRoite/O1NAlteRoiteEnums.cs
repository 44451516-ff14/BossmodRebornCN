namespace BossMod.Stormblood.Raid.O1NAlteRoite;

public enum OID : uint
{
    Boss = 0x1A6F,
    Helper = 0x233C,
    BallOfFire = 0x1A71, // R1.000, x0 (spawn during fight)
    Gen_AlteRoite = 0x18D6, // R0.500, x13, mixed types
    Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1e969d = 0x1E969D, // R0.500, x0 (spawn during fight), EventObj type

}
public enum AID : uint
{
    Attack = 872, // Boss->player, no cast, single-target
    WyrmTail = 9174, // Boss->player, no cast, single-target - Not required
    Flame = 9181, // Boss->self, no cast, ???
    Roar = 9180, // Boss->self, 4.0s cast, range 100 circle - Working
    Burn = 9173, // BallOfFire->self, 1.0s cast, range 8 circle
    BreathWing = 9182, // Boss->self, 5.0s cast, single-target
    TwinBolt = 9175, // Boss->self, 5.0s cast, Self AOE
    TwinBolt1 = 9176, // 18D6->player, Ranged damage mechanic of Twinbolt is handled by 'Helper' actor rather than the boss itself, the bost casts twinbold and generates an aoe around him of 5f radius. Twinbolt1 9176 
    Clamp = 9186, // Boss->self, 3.0s cast, range 9+R width 10 rect
    FlashFreeze = 9183, // Boss->self, no cast, single-target
    Levinbolt = 9177, // Boss->self, 5.0s cast, single-target
    Levinbolt1 = 9178, // 18D6->self, no cast, range 6 circle
    Blaze = 9185, // Boss->player, 5.0s cast, range 6 circle
    TheClassicalElements = 9184, // Boss->self, 5.0s cast, single-target
    Turbulence = 9603, // 18D6->self, no cast, range 5 circle
    Downburst = 7896, // Boss->self, 5.0s cast, single-target
    Downburst2 = 7898, // 18D6->self, no cast, ???
    Downburst1 = 7897, // 18D6->self, no cast, range 5 circle
    Charybdis = 9179, // Boss->self, 5.0s cast, range 100 circle
    Burn1 = 9188, // BallOfFire->self, 10.0s cast, range 8 circle
}

public enum SID : uint
{
    ThinIce = 911, // none->player, extra=0x96
    VulnerabilityUp = 202, // BallOfFire->player, extra=0x4/0x1/0x3/0x2
}

public enum TetherID : uint
{
    TwinBolt = 21, // player->player
}

public enum IconID : uint
{
    LevinSpread = 108, // player->self
    StackBlaze = 62, // player->self
}

