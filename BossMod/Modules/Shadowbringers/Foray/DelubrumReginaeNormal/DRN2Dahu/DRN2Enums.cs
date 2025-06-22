namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

public enum OID : uint
{
    Boss = 0x30A6, // R4.6
    Marchosias = 0x30A7, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss/CrownedMarchosias->player, no cast, single-target

    ReverberatingRoar = 22363, // Boss->self, no cast, single-target, visual (falling rocks)
    FallingRock = 22364, // Helper->location, 3.0s cast, range 4 circle puddle
    HotCharge = 22372, // Boss->location, 3.0s cast, width 8 rect charge

    AddAppear = 22360, // Marchosias/CrownedMarchosias->self, no cast, single-target, visual
    HeadDown = 22358, // Marchosias->location, 3.0s cast, width 4 rect charge
    LeftSidedShockwaveFirst = 22368, // Boss->self, 3.0s cast, range 20 180-degree cone
    RightSidedShockwaveFirst = 22369, // Boss->self, 3.0s cast, range 20 180-degree cone
    LeftSidedShockwaveSecond = 22370, // Boss->self, 1.0s cast, range 20 180-degree cone
    RightSidedShockwaveSecond = 22371, // Boss->self, 1.0s cast, range 20 180-degree cone
    FeralHowl = 22357, // Boss->self, 5.0s cast, single-target, visual (knockback)
    HuntersClaw = 22359, // Marchosias->self, 8.5s cast, range 8 circle puddle

    Firebreathe = 22373, // Boss->self, 5.0s cast, range 60 90-degree cone
    FirebreatheRotationVisual = 22361, // Boss->self, 5.0s cast, single-target, visual (rotating cone)
    FirebreatheRotation = 22362, // Boss->self, 0.5s cast, range 60 90-degree cone

    HeatBreath = 22374, // Boss->self/players, 4.0s cast, range 10 90-degree cone, tankbuster
    TailSwing = 22367, // Boss->self, 4.0s cast, range 10 circle
    RipperClaw = 22365 // Boss->self, 4.0s cast, range 10 90-degree cone
}

public enum IconID : uint
{
    RotateCW = 167, // Boss
    RotateCCW = 168 // Boss
}
