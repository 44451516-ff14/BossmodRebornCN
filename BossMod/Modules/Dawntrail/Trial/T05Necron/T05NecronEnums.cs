namespace BossMod.Dawntrail.Trial.T05Necron;

public enum OID : uint
{
    Necron = 0x4870, // R20.0
    NecronHelper1 = 0x4945, // R1.0
    NecronHelper2 = 0x49B1, // R0.0-0.5
    LoomingSpecter1 = 0x49DD, // R3.0
    LoomingSpecter2 = 0x4907, // R15.75
    AzureAether1 = 0x4948, // R1.0
    AzureAether2 = 0x490A, // R1.0
    IcyHands1 = 0x4903, // R3.575
    IcyHands2 = 0x4908, // R4.400
    IcyHands3 = 0x4904, // R3.575
    IcyHands4 = 0x4905, // R3.575
    IcyHands5 = 0x4906, // R3.575
    IcyHands6 = 0x4909, // R3.575
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44614, // NecronHelper2->player, no cast, single-target

    FearOfDeath = 44521, // Necron->self, 5.0s cast, range 100 circle
    FearOfDeathAOE1 = 44522, // Helper->location, 3.0s cast, range 3 circle
    FearOfDeathAOE2 = 44540, // Helper->location, 3.0s cast, range 3 circle

    ChokingGrasp = 44523, // IcyHands1/IcyHands2/IcyHands3/IcyHands4/IcyHands5->self, 3.0s cast, range 24 width 6 rect
    ColdGripVisual1 = 44524, // Necron->self, 5.0+1,0s cast, single-target
    ColdGripVisual2 = 44525, // Necron->self, 5.0+1,0s cast, single-target
    ColdGrip = 44611, // Helper->self, 6.0s cast, range 30 width 12 rect
    ExistentialDread = 44526, // Helper->self, 1.0s cast, range 30 width 24 rect

    MementoMori = 44532, // Necron->self, 5.0s cast, range 37 width 12 rect
    BlueShockwaveVisual = 44546, // Necron->self, 6.0+1,0s cast, single-target
    BlueShockwave = 44547, // Helper->self, no cast, range 100 100-degree cone

    SoulReaping = 44527, // Necron->self, 6.0s cast, single-target
    RelentlessReaping = 44531, // Necron->self, 15.0s cast, single-target
    CropRotation = 44609, // Necron->self, 3.0s cast, single-target
    SeasonsOfBlight = 45166, // Necron->self, 10.0s cast, single-target
    Aetherblight = 44528, // Necron->self, 5.0s cast, single-target
    AetherblightVisualDonut = 44530, // Necron->self, no cast, single-target
    AetherblightDonut = 45182, // Helper->self, 1.0s cast, range 16-60 donut
    AetherblightVisualCircle = 44529, // Necron->self, no cast, single-target
    AetherblightCircle = 45181, // Helper->self, 1.0s cast, range 20 circle

    GrandCross = 44533, // Necron->location, 7.0s cast, range 50 circle
    ArenaChangeVisual = 44603, // Helper->location, 7.0s cast, range 9-60 donut
    GrandCrossBait = 44536, // Helper->location, 3.0s cast, range 3 circle
    GrandCrossRect = 44534, // Helper->self, 0.5s cast, range 100 width 4 rect
    GrandCrossProximity = 44535, // Helper->self, 5.0s cast, range 100 width 100 rect
    NeutronRingVisual = 44538, // Necron->location, 7.0s cast, single-target
    NeutronRing = 44539, // Helper->self, no cast, range 50 circle

    DarknessOfEternityVisual = 44541, // Necron->self, 10.0s cast, single-target
    DarknessOfEternity = 44542, // Helper->location, no cast, range 50 circle
    Inevitability = 44544, // Helper->self, no cast, range 50 circle

    SpecterOfDeath = 44605, // Necron->self, 5.0s cast, single-target
    InvitationVisual = 44545, // LoomingSpecter2->self, 4.3+0,7s cast, range 36 width 10 rect
    Invitation = 44817, // Helper->self, 5.0s cast, range 36 width 10 rect

    MassMacabre = 44548, // Necron->self, 4.0s cast, single-target
    SpreadingFear = 44549 // IcyHands6->self, 8.0s cast, range 50 circle
}

public enum IconID : uint
{
    AetherblightCircle1 = 604, // Necron->self
    AetherblightCircle2 = 621, // Necron->self
    AetherblightDonut1 = 605, // Necron->self
    AetherblightDonut2 = 622, // Necron->self
    BlueShockwave = 615 // Necron->player
}
