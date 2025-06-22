namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

public enum OID : uint
{
    Boss = 0x4799, // R4.0
    Painting = 0x47B4, // R1.0
    SweetShot = 0x479D, // R1.5
    ThrowUpTarget = 0x479C, // R1.5
    HeavenBomb = 0x479B, // R0.8
    PaintBomb = 0x479A, // R0.8
    TempestPiece = 0x479E, // R0.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 42931, // Boss->player, no cast, single-target
    Teleport = 42611, // Boss->location, no cast, single-target

    MousseMural = 42607, // Boss->self, 5.0s cast, range 100 circle

    SingleStyle1 = 42580, // Boss->self, 7.0+0,9s cast, single-target
    SingleStyle2 = 42582, // Boss->self, 7.0+0,9s cast, single-target
    SingleStyle3 = 42584, // Boss->self, 9.0+0,9s cast, single-target
    SingleStyle4 = 42586, // Boss->self, 5.0+0,9s cast, single-target

    Burst1 = 42581, // PaintBomb->self, 1.5s cast, range 15 circle
    Burst2 = 42583, // HeavenBomb->location, 1.5s cast, range 15 circle
    Rush = 42585, // SweetShot->location, 1.5s cast, width 7 rect charge

    PuddingPartyVisual = 42605, // Boss->self, 4.0+1,0s cast, single-target, stack, first time 5 hits, after 6 hits
    PuddingParty = 42606, // Helper->players, no cast, range 6 circle

    ColorRiot = 42608, // Boss->self, 3.0+2,0s cast, single-target, tankbusters
    WarmBomb = 42609, // Helper->player, 5.0s cast, range 4 circle
    CoolBomb = 42610, // Helper->player, 5.0s cast, range 4 circle

    Sugarscape1 = 42600, // Boss->self, 1.0+7,0s cast, single-target
    Sugarscape2 = 42595, // Boss->self, 1.0+7,0s cast, single-target
    Layer1 = 42601, // Boss->self, 1.0+6,0s cast, single-target
    Layer2 = 42602, // Boss->self, 1.0+6,0s cast, single-target
    Layer3 = 42604, // Boss->self, 1.0+6,0s cast, single-target
    Layer4 = 42596, // Boss->self, 1.0+6,0s cast, single-target
    Layer5 = 42598, // Boss->self, 1.0+6,0s cast, single-target

    SprayPain = 42603, // Helper->self, 7.0s cast, range 10 circle
    MousseTouchUpVisual = 42612, // Boss->self, 3.0s cast, single-target, spread
    MousseTouchUp = 42613, // Helper->player, 5.0s cast, range 6 circle

    DoubleStyle1 = 42591, // Boss->self, 8.0+0,9s cast, single-target
    DoubleStyle2 = 42592, // Boss->self, 8.0+0,9s cast, single-target
    DoubleStyle3 = 42593, // Boss->self, 8.0+0,9s cast, single-target
    DoubleStyle4 = 42594, // Boss->self, 8.0+0,9s cast, single-target

    TasteOfThunderVisual = 42589, // Boss->self, 8.0+1,0s cast, single-target, spread (outside river)
    TasteOfThunder = 42590, // Helper->player, 6.0s cast, range 6 circle
    TasteOfFireVisual = 42587, // Boss->self, 8.0+1,0s cast, single-target, stack (inside river)
    TasteOfFire = 42588, // Helper->players, 6.0s cast, range 6 circle

    LightningBolt = 42597, // Helper->location, 3.0s cast, range 4 circle
    Highlightning = 42599, // TempestPiece->self, 1.5s cast, range 21 circle
}

public enum TetherID : uint
{
    ActivateMechanicSingleStyle = 324, // PaintBomb/HeavenBomb/SweetShot/ThrowUpTarget->Boss
    ActivateMechanicDoubleStyle1 = 319, // HeavenBomb/PaintBomb/ThrowUpTarget->Boss
    ActivateMechanicDoubleStyle2 = 320 // PaintBomb/SweetShot->Boss
}

public enum IconID : uint
{
    PuddingParty = 305 // player->self
}
