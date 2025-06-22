namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

public enum OID : uint
{
    Boss = 0x2EBD, // R6.9
    LyonTheBeastKing = 0x2EBB, // R0.6
    DeathwallDawon = 0x1EB03C, // R0.5
    DeathwallLyon = 0x1EB02F, // R0.5
    VerdantPlume = 0x2EBE, // R0.5
    ScarletPlume = 0x2EBF, // R0.5
    FrigidPulseJump = 0x1EB03D, // R0.5
    FervidPulseJump = 0x1EB03E, // R0.5
    TamedCarrionCrow = 0x2EC3, // R3.6
    PassageToMajestysPlace = 0x1EB03F, // R0.5
    TamedCoeurl = 0x2EC1, // R3.15
    TamedManticore = 0x2EC2, // R3.0
    TamedBeetle = 0x2EC0, // R2.2
    Helper4 = 0x1EB033, // R0.5
    Helper3 = 0x1EB035, // R0.5
    Helper2 = 0x1EB034, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 6497, // Boss/TamedCoeurl/TamedManticore/LyonTheBeastKing->player, no cast, single-target
    AutoAttackAdd1 = 6499, // TamedBeetle->player, no cast, single-target

    MoltingPlumage = 20858, // Boss->self, 4.0s cast, range 60 circle, raidwide
    Burn = 20865, // ScarletPlume->self, 1.5s cast, range 10 circle
    Explosion = 20866, // VerdantPlume->self, 1.5s cast, range 3-12 donut
    Scratch = 20859, // Boss->player, 5.0s cast, single-target, tankbuster

    Ready = 20881, // LyonTheBeastKing->location, no cast, single-target
    Obey1 = 20860, // Boss->self, 10.0s cast, single-target
    Obey2 = 20861, // Boss->self, 12.0s cast, single-target

    FervidPulse = 20857, // Boss->self, 3.5s cast, range 50 width 14 cross
    SwoopingFrenzy = 20853, // Boss->location, 3.0s cast, range 12 circle
    FrigidPulse = 20856, // Boss->self, 5.0s cast, range 12-60 donut
    SwoopingFrenzyJump = 20862, // Boss->location, no cast, range 12 circle
    FrigidPulseJump = 20863, // Boss->self, no cast, range 12-60 donut
    FervidPulseJump = 20864, // Boss->self, no cast, range 50 width 14 cross

    CallBeast1 = 20882, // LyonTheBeastKing->self, no cast, single-target
    CallBeast2 = 20923, // Helper->self, no cast, single-target
    Flutter = 20880, // TamedCarrionCrow->self, 4.5s cast, range 30 width 30 rect, knockbacks plumes, distance 28, dir forward
    PentagustVisual = 20854, // Boss->self, 3.0s cast, single-target
    Pentagust = 20855, // Helper->self, 3.5s cast, range 50 20-degree cone

    RagingWindsVisual1 = 20836, // LyonTheBeastKing->self, 3.0s cast, single-target
    RagingWindsVisual2 = 20837, // Helper->self, 1.0s cast, range 50 circle
    WindsPeak = 20847, // LyonTheBeastKing->self, 3.0s cast, range 5 circle
    WindsPeakKB = 20848, // Helper->self, 4.0s cast, range 50 circle, knockback 10, away from source
    UnfetteredFerocity = 20849, // LyonTheBeastKing->location, no cast, single-target
    HeartOfNature = 20830, // LyonTheBeastKing->self, 3.0s cast, range 80 circle
    NaturesPulse1 = 20833, // Helper->self, 4.0s cast, range 10 circle
    NaturesPulse2 = 20834, // Helper->self, 5.5s cast, range 10-20 donut
    NaturesPulse3 = 20835, // Helper->self, 7.0s cast, range 20-30 donut
    TasteOfBlood = 20851, // LyonTheBeastKing->self, 4.0s cast, range 40 180-degree cone
    NaturesBloodFirst = 20831, // Helper->self, 7.0s cast, range 4 circle, exaflare, 7 hits total, 1.1s between casts, moves 6y per hit
    NaturesBloodRest = 20832, // Helper->self, no cast, range 4 circle
    TwinAgonies = 20852, // LyonTheBeastKing->player, 4.0s cast, single-target
    TheKingsNotice = 20846, // LyonTheBeastKing->self, 5.0s cast, range 50 circle, gaze

    Defend = 20867, // TamedBeetle->Boss, 1.0s cast, single-target
    CriticalRip = 20869, // TamedCoeurl->player, no cast, single-target
    CrackleHiss = 20870, // TamedCoeurl->self, 3.0s cast, range 25 120-degree cone
    RipperClaw = 20872, // TamedManticore->self, 4.0s cast, range 9 90-degree cone
    SpikeFlail = 20871, // TamedCoeurl->self, 3.0s cast, range 25 60-degree cone

    LeftHammerVisual1 = 20878, // TamedManticore->self, no cast, single-target
    RightHammerVisual1 = 20875, // TamedManticore->self, no cast, single-target
    RightHammer1 = 20877, // TamedManticore->self, 5.0s cast, range 20 180-degree cone
    LeftHammer1 = 20874, // TamedManticore->self, 5.0s cast, range 20 180-degree cone
    LeftHammer2 = 20879, // Helper->self, 7.0s cast, range 20 180-degree cone
    RightHammer2 = 20876 // Helper->self, 7.0s cast, range 20 180-degree cone
}
