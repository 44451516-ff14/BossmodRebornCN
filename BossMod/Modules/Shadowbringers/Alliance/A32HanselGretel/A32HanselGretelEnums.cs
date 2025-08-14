namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

public enum OID : uint
{
    Gretel = 0x31A4, // R7.0
    Hansel = 0x31A5, // R7.0
    MagicBullet = 0x31A7, // R1.0
    MagicalConfluence = 0x31A6, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Hansel/Boss->player, no cast, single-target
    Teleport1 = 23678, // Boss->location, no cast, single-target
    Teleport2 = 23679, // Hansel->location, no cast, single-target

    UpgradedLance1 = 23656, // Boss->self, 4.0s cast, single-target
    UpgradedLance2 = 23658, // Hansel->self, 4.0s cast, single-target
    UpgradedShield1 = 23659, // Hansel->self, 4.0s cast, single-target
    UpgradedShield2 = 23657, // Boss->self, 4.0s cast, single-target

    Wail1 = 23671, // Hansel->self, 5.0s cast, range 50 circle
    Wail2 = 23670, // Boss->self, 5.0s cast, range 50 circle

    CripplingBlow1 = 23672, // Boss->player, 5.0s cast, single-target
    CripplingBlow2 = 23673, // Hansel->player, 5.0s cast, single-target

    TandemAssaultBloodySweep1 = 25016, // Boss->self, 5.0s cast, single-target
    TandemAssaultBloodySweep2 = 25017, // Hansel->self, 5.0s cast, single-target

    BloodySweepVisual1 = 23636, // Boss->self, 8.0s cast, single-target
    BloodySweepVisual2 = 23637, // Hansel->self, 8.0s cast, single-target
    BloodySweepVisual3 = 23638, // Boss->self, 13.0s cast, single-target
    BloodySweepVisual4 = 23639, // Hansel->self, 13.0s cast, single-target
    BloodySweep1 = 23661, // Gretel->self, 8.6s cast, range 50 width 25 rect
    BloodySweep2 = 23660, // Gretel->self, 8.6s cast, range 50 width 25 rect
    BloodySweep3 = 23663, // Gretel->self, 13.6s cast, range 50 width 25 rect
    BloodySweep4 = 23662, // Gretel->self, 13.6s cast, range 50 width 25 rect

    SeedOfMagicAlphaVisual = 23674, // Boss->self, 5.0s cast, single-target
    SeedOfMagicAlpha = 23649, // Gretel->players, 5.0s cast, range 5 circle

    RiotOfMagicVisual = 23675, // Hansel->self, 5.0s cast, single-target
    RiotOfMagic = 23651, // Gretel->players, 5.0s cast, range 5 circle

    TandemAssaultPassingLance1 = 25020, // Boss->self, 5.0s cast, single-target
    TandemAssaultPassingLance2 = 25021, // Hansel->self, 5.0s cast, single-target

    PassingLanceVisual1 = 23652, // Boss->self, 8.0s cast, single-target
    PassingLanceVisual2 = 23653, // Hansel->self, 8.0s cast, single-target
    PassingLance = 23654, // HanselGretel->self, 8.4s cast, range 50 width 24 rect
    Explosion = 23655, // MagicBullet->self, 1.0s cast, range 4 width 50 rect

    WanderingTrail1 = 23642, // Boss->self, 5.0s cast, single-target, spawns yellow disks
    WanderingTrail2 = 23643, // Hansel->self, 5.0s cast, single-target
    UnevenFooting = 23647, // HanselGretel->self, 9.3s cast, range 50 circle, proximity AOE

    HungryLance1 = 23665, // Boss->self, 5.0s cast, range 40 120-degree cone
    HungryLance2 = 23666, // Hansel->self, 5.0s cast, range 40 120-degree cone

    Repay = 23664, // Hansel/Gretel->player, no cast, single-target, knockback 5, punishment for attacking parry side

    Tandem1 = 23640, // Boss->self, no cast, single-target
    Tandem2 = 23641, // Hansel->self, no cast, single-target
    Transference1 = 23794, // Gretel->location, no cast, single-target
    Transference2 = 23793, // Gretel->location, no cast, single-target
    TandemAssaultBreakthrough1 = 25018, // Boss->self, 5.0s cast, single-target
    TandemAssaultBreakthrough2 = 25019, // Hansel->self, 5.0s cast, single-target
    Impact = 23644, // Gretel->self, no cast, range 3 circle
    BreakthroughVisual1 = 23645, // Boss->location, 9.0s cast, single-target
    BreakthroughVisual2 = 23646, // Hansel->location, 9.0s cast, single-target
    Breakthrough = 21939, // HanselGretel->self, 9.0s cast, range 53 width 32 rect
    SeedOfMagicBetaVisual1 = 23676, // Boss->self, 5.0s cast, single-target
    SeedOfMagicBetaVisual2 = 23677, // Hansel->self, 5.0s cast, single-target
    SeedOfMagicBeta = 23669, // Gretel->location, 5.0s cast, range 5 circle
    Lamentation1 = 23667, // Gretel->self, 8.0s cast, range 50 circle, raidwides as soon as Hansel dies
    Lamentation2 = 23668 // Hansel->self, 8.0s cast, range 50 circle, raidwides as soon as Gretel dies
}

public enum SID : uint
{
    StrongerTogether = 2539 // none->Boss/Hansel, extra=0x0
}
