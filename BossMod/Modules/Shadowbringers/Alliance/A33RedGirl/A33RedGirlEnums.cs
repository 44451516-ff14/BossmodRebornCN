namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

public enum OID : uint
{
    Boss = 0x32BB, // R7.5
    BossP2 = 0x32BD, // R12.25
    WhiteLance = 0x32E3, // R1.0
    BlackLance = 0x32E4, // R1.0
    RedSphere = 0x32E9, // R4.0
    RedGirl1 = 0x32BC, // R2.25
    RedGirl2 = 0x32BE, // R12.25
    WhiteWall = 0x32EB, // R1.0
    BlackWall = 0x32EC, // R1.0
    BlackPylon = 0x32E8, // R1.5
    WhitePylon = 0x32E7, // R1.5
    BlackPylonP2 = 0x32E5, // R1.0
    RedSphereHelper = 0x32EA,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24597, // Helper->player, no cast, single-target
    Teleport = 24605, // Helper->location, no cast, single-target

    CrueltyVisualP1 = 24594, // Boss->self, 5.0s cast, single-target
    CrueltyVisualP2 = 24595, // BossP2->self, 5.0s cast, single-target
    Cruelty = 24596, // Helper->location, no cast, range 75 circle

    Shockwave = 24590, // Boss->self, 2.0s cast, single-target
    ShockWhiteBait = 24591, // Helper->players, no cast, range 5 circle
    ShockWhiteAOE = 24592, // Helper->location, 4.0s cast, range 5 circle
    ShockBlackBait = 24593, // Helper->players, no cast, range 5 circle
    ShockBlackAOE = 24972, // Helper->location, 4.0s cast, range 5 circle

    GenerateBarrierVisualP1 = 24580, // Boss->self, 4.0s cast, single-target
    GenerateBarrierVisualP2 = 24581, // BossP2->self, 4.0s cast, single-target
    GenerateBarrierVisual1 = 24584, // Helper->self, 4.0s cast, range 18 width 3 rect
    GenerateBarrierVisual2 = 24585, // Helper->self, 4.0s cast, range 24 width 3 rect
    GenerateBarrierVisual3 = 24583, // Helper->self, 4.0s cast, range 12 width 3 rect
    GenerateBarrierVisual4 = 24582, // Helper->self, 4.0s cast, range 6 width 3 rect
    GenerateBarrier1 = 25362, // Helper->self, no cast, range 18 width 3 rect
    GenerateBarrier2 = 25363, // Helper->self, no cast, range 24 width 3 rect
    GenerateBarrier3 = 25361, // Helper->self, no cast, range 12 width 3 rect
    GenerateBarrier4 = 25360, // Helper->self, no cast, range 6 width 3 rect

    PointWhite1 = 24607, // WhiteLance->self, no cast, range 50 width 6 rect
    PointWhite2 = 24609, // WhiteLance->self, no cast, range 24 width 6 rect
    PointBlack1 = 24608, // BlackLance->self, no cast, range 50 width 6 rect
    PointBlack2 = 24610, // BlackLance->self, no cast, range 24 width 6 rect

    ManipulateEnergyVisualP1 = 24600, // Boss->self, 4.0s cast, single-target
    ManipulateEnergyVisualP2 = 24601, // BossP2->self, 4.0s cast, single-target
    ManipulateEnergy = 24602, // Helper->players, no cast, range 3 circle

    DiffuseEnergyFirst = 24611, // RedGirl1->self, 5.0s cast, range 12 120-degree cone
    DiffuseEnergyRest = 24662, // RedGirl1->self, no cast, range 12 120-degree cone

    SublimeTranscendenceVisual = 25098, // Boss->self, 5.0s cast, single-target
    SublimeTranscendence = 25099, // Helper->location, no cast, range 75 circle

    Vortex = 24599, // Helper->location, no cast, ???, pull into center for p1 meteor
    RecreateMeteor = 24903, // Boss->self, 2.0s cast, single-target
    ReplicateP1 = 24586, // Boss->self, 3.0s cast, single-target
    ReplicateP2 = 24587, // BossP2->self, 3.0s cast, single-target

    BigExplosion = 24615, // BlackPylon/WhitePylon->self, 6.0s cast, range 50 circle
    TeleportSphere = 24606, // RedSphereHelper->location, no cast, single-target
    WaveBlack = 24974, // RedSphere->self, 8.0s cast, range 22 circle
    WaveWhite = 24973, // RedSphere->self, 8.0s cast, range 22 circle

    ChildsPlay1 = 24612, // BossP2/RedGirl2->self, 10.0s cast, single-target, forced march north for players, pull towards tether target for rotations
    ChildsPlay2 = 24613, // BossP2/RedGirl2->self, 10.0s cast, single-target, forced march east, pull left from tether target for rotations
    Explosion = 24614, // BlackPylonP2->self, 15.0s cast, range 9 circle

    WipeWhite = 24588, // Helper->self, 13.0s cast, range 75 circle
    WipeBlack = 24589, // Helper->self, 13.0s cast, range 75 circle
}

public enum SID : uint
{
    ProgramFFFFFFF = 2632, // none->player, extra=0x1AB
    Program000000 = 2633, // none->player, extra=0x1AC
    PayingThePiper = 1681 // none->player, extra=0x4/0x2
}

public enum IconID : uint
{
    ShockWhiteSlow = 262, // player
    ShockBlack = 263, // player
    ShockWhiteFast = 264, // player
    ManipulateEnergy = 218, // player
    RotateCW = 167, // RedGirl1
    RotateCCW = 168 // RedGirl1
}

public enum TetherID : uint
{
    ChildsPlay = 149 // player/RedGirl1->BossP2/RedGirl3
}
