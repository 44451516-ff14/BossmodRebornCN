namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN3QueensGuard;

public enum OID : uint
{
    QueensWarrior = 0x30BC, // R2.8
    QueensKnight = 0x30BA, // R2.8
    QueensSoldier = 0x30BF, // R4.0
    QueensGunner = 0x30C1, // R4.0
    SoldierAvatar = 0x30C0, // R4.0
    AetherialBurst = 0x30BE, // R1.2
    AetherialBolt = 0x30BD, // R0.6
    AetherialWard = 0x30BB, // R3.0
    AutomaticTurret = 0x30C2, // R4.0
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackOthers = 6497, // QueensSoldier/QueensKnight/QueensWarrior->player, no cast, single-target
    AutoAttackGunner = 22615, // QueensGunner->player, no cast, single-target

    DoubleGambit = 22533, // QueensSoldier->self, 3.0s cast, single-target
    SecretsRevealed = 23406, // QueensSoldier->self, 5.0s cast, single-target, visual (tether 2 of 4 avatars that will be activated)
    SecretsRevealedExtra = 23408, // SoldierAvatar->self, no cast, single-target, visual (untether after secrets revealed?)
    ReversalOfForces = 22527, // QueensWarrior->self, 4.0s cast, single-target, visual (remove tethers and apply statuses)
    ReversalOfForcesExtra = 23187, // Helper->self, 4.0s cast, single-target, visual (??? cast together with ReversalOfForces)
    PawnOffFake = 22535, // SoldierAvatar->self, 5.0s cast, range 20 circle fake aoe
    PawnOffReal = 22534, // SoldierAvatar->self, 5.0s cast, range 20 circle aoe
    Bombslinger = 23293, // QueensWarrior->self, 3.0s cast, single-target
    AvatarJump = 22584, // SoldierAvatar->location, no cast, single-target

    AboveBoardExtra = 23409, // Helper->self, 6.0s cast, range 60 circle
    AboveBoard = 22524, // QueensWarrior->self, 6.0s cast, range 60 circle

    LotsCastBigShort = 23403, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallShort = 22526, // AetherialBolt->location, no cast, range 10 circle
    LotsCastBigLong = 22525, // AetherialBurst->location, no cast, range 10 circle
    LotsCastSmallLong = 23402, // AetherialBolt->location, no cast, range 10 circle

    RapidSeverKnight = 22523, // QueensKnight->player, 5.0s cast, single-target, tankbuster
    RapidSeverWarrior = 22529, // QueensWarrior->player, 5.0s cast, single-target, tankbuster
    RapidSeverSoldier = 22537, // QueensSoldier->player, 5.0s cast, single-target, tankbuster
    ShotInTheDark = 22545, // QueensGunner->player, 5.0s cast, single-target, tankbuster

    ShieldOmen = 22513, // QueensKnight->self, 3.0s cast, single-target
    SwordOmen = 22512, // QueensKnight->self, 3.0s cast, single-target

    OptimalPlay = 22516, // Helper->self, 5.0s cast, single-target
    OptimalPlayShield = 22515, // QueensKnight->self, 6.0s cast, range 5-60 donut
    OptimalPlaySword = 22514, // QueensKnight->self, 6.0s cast, range 10 circle

    AutomaticTurret = 22539, // QueensGunner->self, 3.0s cast, single-target
    Reading = 22540, // QueensGunner->self, 5.0s cast, single-target
    TurretsTourFirst = 22541, // Helper->location, 5.0s cast, width 6 rect charge
    TurretsTourRest1 = 22542, // GunTurret->location, no cast, width 6 rect charge
    TurretsTourRest2 = 22543, // GunTurret->self, no cast, range 50 width 6 rect

    StrongpointDefense = 22517, // Knight->self, 5.0s cast, single-target
    CoatOfArmsLR = 22519, // AetherialWard->self, 4.0s cast, single-target
    CoatOfArmsFB = 22518, // AetherialWard->self, 4.0s cast, single-target
    Counterplay = 22520, // Helper->player, no cast, single-target

    BloodAndBoneKnight = 22522, // QueensKnight->self, 5.0s cast, range 60 circle
    BloodAndBoneSoldier = 22536, // QueensSoldier->self, 5.0s cast, range 60 circle
    BloodAndBoneWarrior = 22528, // QueensWarrior->self, 5.0s cast, range 60 circle
    QueensShotUnseen = 22544, // QueensGunner->self, 5.0s cast, range 60 circle

    BloodAndBoneWarriorEnrage = 22530, // QueensWarrior->self, 60.0s cast, range 60 circle
    BloodAndBoneKnightEnrage = 22521, // QueensKnight->self, 60.0s cast, range 60 circle
    BloodAndBoneSoldierEnrage = 22538, // QueensSoldier->self, 60.0s cast, range 60 circle
    QueensShotEnrage = 22546, // QueensGunner->self, 60.0s cast, range 60 circle
    QueensShotEnrageRepeat = 23456, // QueensGunner->self, no cast, range 60 circle
    BloodAndBoneSoldierEnrageRepeat = 23455, // QueensSoldier->self, no cast, range 60 circle
    BloodAndBoneWarriorEnrageRepeat = 23454, // QueensWarrior->self, no cast, range 60 circle
    BloodAndBoneKnightEnrageRepeat = 23453 // QueensKnight->self, no cast, range 60 circle
}
