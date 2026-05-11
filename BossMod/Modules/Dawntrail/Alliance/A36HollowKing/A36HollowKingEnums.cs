namespace BossMod.Dawntrail.Alliance.A36HollowKing;

public enum OID : uint
{
    HollowKing = 0x4D96, // R25.000, x1
    Helper = 0x233C, // R0.500, x24, Helper type

    ShinryuParadox = 0x4D92, // R25.000, x1
    UnknownActor = 0x4EB3, // R2.000, x5
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    ArcaneSphere = 0x4D97, // R1.000, x1
    ArcaneSphere1 = 0x4DCD, // R1.000, x1
    Alxaal = 0x4D98, // R0.500, x1
    Prishe = 0x4D99, // R0.465, x1
    ShinryuParadox1 = 0x4D9A, // R0.000, x3, Part type
    ShinryuParadox2 = 0x4D93, // R25.000, x1, Part type
    GuloolJaJa = 0x4E53, // R5.000, x1
    HollowKing1 = 0x4D9B, // R0.000, x3, Part type
}

public enum AID : uint
{
    AutoAttack1 = 50763, // Prishe->HollowKing, no cast, single-target
    AuroralUppercut = 50383, // Prishe->HollowKing, no cast, single-target
    CelestialTrail = 49139, // HollowKing->self, no cast, single-target
    NullifyingDropkick = 50384, // Prishe->HollowKing, no cast, width 5 rect charge
    UnknownWeaponskill1 = 49150, // Alxaal->location, no cast, single-target
    UnknownWeaponskill2 = 49148, // Alxaal->self, no cast, single-target
    KnuckleSandwich = 50385, // Prishe->self, 5.0s cast, range 8 circle
    CelestialTrail1 = 49140, // Helper->self, 8.0s cast, range 2 circle
    CelestialTrail2 = 49141, // HollowKing->self, no cast, single-target
    CelestialTrail3 = 49142, // Helper->player/Alxaal, no cast, single-target
    Banish = 50386, // Prishe->HollowKing, 2.0s cast, single-target
    CelestialTrail4 = 49143, // Helper->player/Alxaal, no cast, single-target
    UnknownAbility = 50388, // Prishe->location, no cast, single-target
    CelestialTrail5 = 49144, // HollowKing->self, no cast, single-target
    Cure = 50387, // Prishe->Alxaal, 2.0s cast, single-target
    AutoAttack2 = 50721, // Alxaal->HollowKing, no cast, single-target
    AutoAttack3 = 49180, // HollowKing->self, no cast, single-target
    AutoAttack4 = 49181, // HollowKing1->player, no cast, single-target
    UnknownWeaponskill3 = 49149, // Alxaal->self, no cast, single-target
    SavageBlade = 50389, // Alxaal->HollowKing, no cast, single-target
    EmptyProclamation = 49179, // HollowKing->self, 4.0s cast, range 60 circle
    RightSwordscross = 49151, // HollowKing->self, 8.0+1.0s cast, single-target
    RightSwordscross1 = 49153, // Helper->self, 9.0s cast, range 60 width 30 rect
    RightSwordscross2 = 49155, // Helper->self, 9.0s cast, range 70 width 36 rect
    VorpalBlade = 50390, // Alxaal->HollowKing, no cast, single-target
    TwinBlaze = 49157, // HollowKing->self, 5.0+1.0s cast, single-target
    TwinBlaze1 = 49159, // Helper->self, 6.0s cast, range ?-60 donut
    TwinBlaze2 = 49160, // Helper->self, 6.0s cast, range 35 ?-degree cone
    CataclysmicBlade = 49162, // Helper->self, 7.0s cast, range 60 45.000-degree cone
    CataclysmicBlade1 = 49161, // HollowKing->self, 7.0s cast, single-target
    CataclysmicBlade2 = 49163, // Helper->player, no cast, single-target
    Burst = 49170, // HollowKing->self, 3.0s cast, single-target
    SpiritsWithin = 50391, // Alxaal->HollowKing, no cast, single-target
    Burst1 = 49171, // Helper->self, 5.0s cast, range 10 circle
    Burst2 = 49172, // Helper->self, 7.0s cast, range ?-20 donut
    Burst3 = 49173, // Helper->self, 9.0s cast, range ?-30 donut
    TwinBlaze3 = 49158, // HollowKing->self, 5.0+1.0s cast, single-target
    CosmicFlame = 49168, // Helper->self, 5.0s cast, range 6 circle
    CosmicFlame1 = 49166, // HollowKing->self, 5.0s cast, single-target
    CosmicFlame2 = 49169, // Helper->self, no cast, range 6 circle
    AtomicRay = 49164, // HollowKing->self, 3.0s cast, single-target
    AtomicRay1 = 49165, // ArcaneSphere/ArcaneSphere1->self, 1.5s cast, range 60 width 15 rect
    SuperNova = 49182, // HollowKing->self, 5.0s cast, single-target
    SuperNova1 = 49183, // Helper->players, no cast, range 6 circle
}

public enum SID : uint
{
    UnknownStatus1 = 2056, // none->HollowKing/Alxaal/ArcaneSphere/ArcaneSphere1, extra=0x474/0x48E/0x497/0x496
    Clashing = 1271, // none->Alxaal/player, extra=0x317A/0x1836
    UnknownStatus2 = 2552, // none->player, extra=0x48F
    HPRecoveryDown = 2852, // Helper->player/Alxaal, extra=0x0
    DownForTheCount = 774, // none->Alxaal, extra=0x3177
    UnknownStatus3 = 2160, // none->Alxaal, extra=0x3931
    VulnerabilityUp = 1789, // Helper/ArcaneSphere1/ArcaneSphere->player, extra=0x1/0x2/0x3/0x4
}

public enum IconID : uint
{
    Icon_z6r3_b4_lock_mv_7s_c0k2 = 683, // player/Alxaal->self
    Icon_z6r3_b4_lock_no_mv_7s_c0k2 = 682, // player/Prishe->self
    Icon_z6r3_b4_lock_no_lk_7s_c0k2 = 680, // player/Prishe->self
    Icon_z6r3_b4_lock_lk_7s_c0k2 = 681, // player->self
    Icon_m0489trg_a0c = 136, // player/Alxaal/Prishe->self
    Icon_m0489trg_b0c = 137, // player->self
    Icon_z6r3_b4_com_s5count3k2 = 720, // ArcaneSphere/ArcaneSphere1->self
    Icon_com_share4a1 = 305, // player->self
}

public enum TetherID : uint
{
    Tether_chn_fire001f = 5, // UnknownActor->HollowKing
}
