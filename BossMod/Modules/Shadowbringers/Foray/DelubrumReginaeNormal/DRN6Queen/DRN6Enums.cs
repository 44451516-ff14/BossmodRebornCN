namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

public enum OID : uint
{
    Boss = 0x310B, // R7.92
    QueensKnight = 0x310C, // R2.8
    QueensWarrior = 0x310D, // R2.8
    QueensSoldier = 0x3110, // R4.0
    QueensGunner = 0x3112, // R4.0
    SoldierAvatar = 0x3111, // R4.0
    AutomaticTurret = 0x3113, // R3.0
    AetherialBolt = 0x310E, // R0.6
    AetherialBurst = 0x310F, // R1.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 23499, // Boss->location, no cast, single-target

    EmpyreanIniquity = 22984, // Boss->self, 5.0s cast, range 60 circle raidwide
    NorthswainsGlow = 22979, // Boss->self, 3.0s cast, single-target, visual (three fire lines with aoes on intersections)
    NorthswainsGlowAOE = 22980, // Helper->self, 10.0s cast, range 20 circle
    CleansingSlash = 22981, // Boss->player, 5.0s cast, single-target tankbuster

    QueensWill = 22969, // Boss->self, 5.0s cast, single-target, visual (easy chess start)
    QueensEdict = 22974, // Boss->self, 5.0s cast, single-target, visual (super chess start)
    QueensJustice = 22975, // Helper->self, no cast, range 60 circle raidwide (hitting players who failed their movement edict)

    BeckAndCallToArmsWill = 23449, // Boss->self, 5.0s cast, single-target, visual (tether knight & warrior and have them move)

    AboveBoard = 22993, // QueensWarrior->self, 6.0s cast, range 60 circle, visual (throw up)
    AboveBoardExtra = 23437, // Helper->self, 6.0s cast, range 60 circle, visual (???)
    Bombslinger = 23358, // QueensWarrior->self, 3.0s cast, single-target, visual (spawn bombs)
    DoubleGambit = 23001, // QueensSoldier->self, 3.0s cast, single-target, visual (summon 4 pawns)
    GodsSaveTheQueen = 22985, // Boss->self, 5.0s cast, range 60 circle, raidwide

    HeavensWrath = 22982, // Boss->self, 3.0s cast, single-target, single-target, visual (preparation for knockback)
    HeavensWrathKnockback = 22983, // Helper->self, 7.0s cast, range 60 width 100 rect, knockback 20, source left/right

    JudgmentBladeVisualL = 22977, // Boss->location, 5.0s cast, single-target, visual (half arena cleave)
    JudgmentBladeVisualR = 22978, // Boss->location, 5.0s cast, single-target, visual (half arena cleave)
    JudgmentBladeL = 23426, // Helper->self, 5.3s cast, range 70 width 30 rect
    JudgmentBladeR = 23427, // Helper->self, 5.3s cast, range 70 width 30 rect
    LotsCastBigShort = 23431, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallShort = 22995, // AetherialBolt->location, no cast, range 10 circle
    LotsCastBigLong = 22994, // AetherialBurst->location, no cast, range 10 circle visual
    LotsCastSmallLong = 23430, // AetherialBolt->location, no cast, range 10 circle visual

    OptimalPlayVisual = 22990, // Helper->self, 5.0s cast, single-target
    OptimalPlaySword = 22988, // QueensKnight->self, 6.0s cast, range 10 circle
    OptimalPlayShield = 22989, // QueensKnight->self, 6.0s cast, range 5-60 donut
    ShieldOmen = 22987, // QueensKnight->self, 3.0s cast, single-target
    SwordOmen = 22986, // QueensKnight->self, 3.0s cast, single-target

    PawnOffReal = 23002, // SoldierAvatar->self, 5.0s cast, range 20 circle
    PawnOffFake = 23003, // SoldierAvatar->self, 5.0s cast, range 20 circle

    RelentlessPlay = 23036, // Boss->self, 5.0s cast, single-target, visual (mechanic start)
    ReversalOfForces = 22996, // QueensWarrior->self, 4.0s cast, single-target, visual (tethers/icons for players)
    ReversalOfForcesExtra = 23360, // Helper->self, 4.0s cast, single-target

    SecondPhaseModelChange = 21928, // Boss->self, no cast, single-target, visual
    SecretsRevealed = 23434, // QueensSoldier->self, 5.0s cast, single-target
    SecretsRevealedExtra = 23436, // SoldierAvatar->self, no cast, single-target

    EndsKnight = 22970, // QueensKnight->self, 1.0s cast, range 60 width 10 cross
    EndsSoldier = 22972, // QueensSoldier->self, 1.0s cast, range 60 width 10 cross
    MeansWarrior = 22971, // QueensWarrior->self, 1.0s cast, range 60 width 10 cross
    MeansGunner = 22973, // QueensGunner->self, 1.0s cast, range 60 width 10 cross

    AutomaticTurret = 23006, // QueensGunner->self, 3.0s cast, single-target
    TurretsTour = 23007, // QueensGunner->self, 5.0s cast, single-target, visual (unseen statuses)
    TurretsTourFirst = 23008, // Helper->location, 5.0s cast, width 6 rect charge
    TurretsTourRest1 = 23009, // AutomaticTurret->location, no cast, width 6 rect charge
    TurretsTourRest2 = 23010 // AutomaticTurret->self, no cast, range 50 width 6 rect
}

public enum SID : uint
{
    Doom = 910, // Boss->player, extra=0x0
    MovementIndicator = 2056, // Boss->QueensKnight/QueensWarrior/Boss/AutomaticTurret, extra=0xE1/0xE2/0xE4/0xE3/0x111
    MovementEdict2 = 2474, // none->player, extra=0x109
    MovementEdict4 = 2476, // none->player, extra=0x10B
    MovementEdict3 = 2475, // none->player, extra=0x10A
    YourMove2Squares = 2480, // none->player, extra=0x0
    YourMove4Squares = 2482, // none->player, extra=0x0
    YourMove3Squares = 2481 // none->player, extra=0x0
}
