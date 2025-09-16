namespace BossMod.Dawntrail.Extreme.Ex5Necron;

public enum OID : uint
{
    Necron = 0x490B, // R20.0
    NecronHelper1 = 0x4945, // R1.0
    NecronHelper2 = 0x49B2, // R0.0
    LoomingSpecter1 = 0x49DD, // R3.0
    LoomingSpecter2 = 0x4910, // R15.75
    BeckoningHands = 0x4912, // R5.5
    AzureAether1 = 0x4949, // R1.0
    AzureAether2 = 0x4914, // R1.0
    AzureAether3 = 0x49B0, // R1.0
    IcyHands1 = 0x490C, // R3.575
    IcyHands2 = 0x4911, // R3.3
    IcyHands3 = 0x490D, // R3.575
    IcyHands4 = 0x4913, // R3.575
    IcyHands5 = 0x490F, // R3.575
    IcyHands6 = 0x490E, // R3.575
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44615, // NecronHelper2->player, no cast, single-target

    BlueShockwaveVisual1 = 44592, // Necron->self, 6.0+1,0s cast, single-target
    BlueShockwaveVisual2 = 44594, // Necron->self, no cast, single-target
    BlueShockwave = 44593, // Helper->self, no cast, range 100 100-degree cone

    FearOfDeath = 44550, // Necron->self, 5.0s cast, range 100 circle
    FearOfDeathAOE1 = 44551, // Helper->location, 3.0s cast, range 3 circle
    FearOfDeathAOE2 = 44577, // Helper->location, 3.0s cast, range 3 circle

    ChokingGraspBait = 44552, // IcyHands1->self, no cast, range 24 width 6 rect
    ChokingGraspAOE1 = 44567, // IcyHands1->self, 3.0s cast, range 24 width 6 rect
    ChokingGraspAOE2 = 44579, // IcyHands2->self, 3.0s cast, range 24 width 6 rect
    ChokingGraspAOE3 = 44584, // IcyHands3->self, 3.0s cast, range 24 width 6 rect
    ChokingGraspTB = 44587, // IcyHands5->self/player, 5.0s cast, range 24 width 6 rect

    ColdGripVisual1 = 44553, // Necron->self, 5.0+1,0s cast, single-target, right
    ColdGripVisual2 = 44554, // Necron->self, 5.0+1,0s cast, single-target, left
    ColdGrip = 44612, // Helper->self, 6.0s cast, range 100 width 12 rect
    ExistentialDread = 44555, // Helper->self, 1.0s cast, range 100 width 24 rect

    MementoMori1 = 44566, // Necron->self, 5.0s cast, range 100 width 12 rect
    MementoMori2 = 44565, // Necron->self, 5.0s cast, range 100 width 12 rect

    SmiteOfGloomVisual = 44601, // Necron->self, 4.0s cast, single-target
    SmiteOfGloom = 44602, // Helper->player, 4.0s cast, range 10 circle

    TwofoldBlight = 44557, // Necron->self, 5.0s cast, single-target
    FourfoldBlight = 44558, // Necron->self, 5.0s cast, single-target
    TheSecondSeason = 45167, // Necron->self, 8.0s cast, single-target
    TheFourthSeason = 45168, // Necron->self, 8.0s cast, single-target
    ShockwaveLP = 44559, // Helper->self, no cast, range 100 30-degree cone, light party stack
    ShockwavePS = 44560, // Helper->self, no cast, range 100 20-degree cone, partner stack

    SoulReaping = 44556, // Necron->self, 4.0s cast, single-target
    RelentlessReaping = 44564, // Necron->self, 15.0s cast, single-target
    CropRotation = 44610, // Necron->self, 3.0s cast, single-target
    AetherblightVisualRect1 = 44607, // Necron->self, no cast, single-target
    AetherblightRect1 = 44608, // Helper->self, 1.0s cast, range 100 width 12 rect
    AetherblightVisualRect2 = 44563, // Necron->self, no cast, single-target
    AetherblightRect2 = 45185, // Helper->self, 1.0s cast, range 100 width 12 rect
    AetherblightVisualCircle = 44561, // Necron->self, no cast, single-target
    AetherblightCircle = 45183, // Helper->self, 1.0s cast, range 20 circle
    AetherblightVisualDonut = 44562, // Necron->self, no cast, single-target
    AetherblightDonut = 45184, // Helper->self, 1.0s cast, range 16-60 donut

    TheEndsEmbraceVisual = 44597, // Necron->self, 4.0s cast, single-target
    TheEndsEmbrace = 44598, // Helper->player, no cast, range 3 circle

    ArenaChangeVisual = 44604, // Helper->location, 7.0s cast, range 9-60 donut
    GrandCross = 44568, // Necron->location, 7.0s cast, range 50 circle
    GrandCrossBait = 44571, // Helper->location, 3.0s cast, range 3 circle
    GrandCrossRect = 44569, // Helper->self, 0.5s cast, range 100 width 4 rect
    GrandCrossSpread = 44572, // Helper->player, 5.0s cast, range 3 circle

    Shock = 44573, // Helper->self, 5.0s cast, range 3 circle, tower
    Electrify = 44574, // Helper->self, no cast, range 50 circle, tower fail

    GrandCrossProximity = 44570, // Helper->self, 4.0s cast, range 100 width 100 rect
    NeutronRingVisual = 44575, // Necron->location, 7.0s cast, single-target
    NeutronRing = 44576, // Helper->self, no cast, range 50 circle

    DarknessOfEternityVisual = 44580, // Necron->self, 10.0s cast, single-target
    DarknessOfEternity = 44581, // Helper->location, no cast, range 50 circle
    Inevitability = 44583, // Helper->self, no cast, range 50 circle

    SpecterOfDeath = 44606, // Necron->self, 5.0s cast, single-target
    InvitationVisual = 44591, // LoomingSpecter2->self, 4.3+0,7s cast, range 36 width 10 rect
    Invitation = 44818, // Helper->self, 5.0s cast, range 36 width 10 rect

    CircleOfLivesVisual = 44599, // Necron->self, 4.0s cast, single-target
    CircleOfLives = 44600, // AzureAether3->self, 7.0s cast, range 3-50 donut
    MassMacabre = 44595, // Necron->self, 4.0s cast, single-target
    MacabreMark = 44819, // Helper->location, no cast, range 3 circle

    MutedStruggle = 44578, // BeckoningHands->player, 3.0s cast, range 24 width 6 rect
    SpreadingFear1 = 44585, // IcyHands3->self, 10.0s cast, range 50 circle
    SpreadingFear2 = 44586, // IcyHands5->self, 5.0s cast, range 50 circle
    SpreadingFear3 = 44596, // IcyHands4->self, no cast, range 50 circle
    NecroticPulse = 44588, // IcyHands6->self, no cast, range 24 width 6 rect
    ChillingFingers = 44589, // IcyHands6->self/player, 5.0s cast, range 24 width 6 rect
    ChokingGraspHealer = 44590, // IcyHands6->self/player, 4.0s cast, range 24 width 6 rect

    DarknessOfEternityEnrage = 44613, // Necron->self, 10.0s cast, range 50 circle
    DarknessOfEternityEnrageRepeat = 44582 // Helper->location, no cast, range 50 circle

}

public enum IconID : uint
{
    AetherblightCircle = 604, // Necron->self
    AetherblightDonut = 605, // Necron->self
    AetherblightRectDouble = 606, // Necron->self
    AetherblightRectSingle = 607, // Necron->self
    TheEndsEmbrace = 614, // player->self
    GrandCross = 611 // player->self
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Doom = 4683, // none->player, extra=0x358
    Slow = 3464 // 490E/player->player, extra=0x0
}
