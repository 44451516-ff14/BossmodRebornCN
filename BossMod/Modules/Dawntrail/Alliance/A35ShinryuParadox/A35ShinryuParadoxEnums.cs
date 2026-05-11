namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

public enum OID : uint
{
    ShinryuParadox = 0x4D92, // R25.000, x1
    ShinryuParadoxPart1 = 0x4D9A, // R0.000, x3, Part type
    ShinryuParadoxPart2 = 0x4D93, // R25.000, x1, Part type

    HollowKing = 0x4D96, // R25.000, x0 (spawn during fight)
    HollowKingPart = 0x4D9B, // R0.000, x0 (spawn during fight), Part type

    Helper = 0x233C, // R0.500, x24, Helper type

    UnkownActor = 0x4EB3, // R2.000, x5
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    ArcaneSphere = 0x4D97, // R1.000, x1
    ArcaneSphere1 = 0x4DCD, // R1.000, x1
    Alxaal = 0x4D98, // R0.500, x1
    Prishe = 0x4D99, // R0.465, x1
    GuloolJaJa = 0x4E53, // R5.000, x0 (spawn during fight)
}

public enum AID : uint
{
    SavageBlade = 50389, // Alxaal->ShinryuParadox/ShinryuParadoxPart2, no cast, single-target
    AutoAttack1 = 50721, // Alxaal->ShinryuParadox/ShinryuParadoxPart2, no cast, single-target
    AutoAttack2 = 50763, // Prishe->ShinryuParadox/ShinryuParadoxPart2/HollowKing, no cast, single-target
    AutoAttack3 = 49138, // ShinryuParadoxPart1->player, no cast, single-target
    AutoAttack4 = 49137, // ShinryuParadox->self, no cast, single-target
    NullifyingDropkick = 50384, // Prishe->ShinryuParadox/ShinryuParadoxPart2, no cast, width 5 rect charge
    CosmicBreath = 49106, // ShinryuParadoxPart2->self, 6.0+1.0s cast, single-target
    CosmicBreath1 = 49105, // ShinryuParadox->self, 6.0+1.0s cast, single-target
    AuroralUppercut = 50383, // Prishe->ShinryuParadoxPart2/ShinryuParadox/HollowKing, no cast, single-target
    CosmicBreath2 = 49107, // Helper->self, 7.0s cast, range 50 width 70 rect
    VorpalBlade = 50390, // Alxaal->ShinryuParadoxPart2/ShinryuParadox, no cast, single-target
    CosmicTail = 49109, // ShinryuParadoxPart2->self, 6.0+1.0s cast, single-target
    CosmicTail1 = 49108, // ShinryuParadox->self, 6.0+1.0s cast, single-target
    CosmicTail2 = 49110, // Helper->self, 7.0s cast, range 50 width 70 rect
    CloakOfTwilight = 49111, // ShinryuParadox->self, 3.0s cast, single-target
    CloakOfTwilight1 = 49112, // ShinryuParadoxPart2->self, 3.0s cast, single-target
    TwilightNebula = 49114, // ShinryuParadoxPart2->self, 6.0s cast, single-target
    TwilightNebula1 = 49113, // ShinryuParadox->self, 6.0s cast, single-target
    TwilightRadiance = 49115, // Helper->self, no cast, range 60 circle
    TwilightShadow = 49116, // Helper->self, no cast, range 60 circle
    Starflare = 49125, // ShinryuParadoxPart2->self, 3.0s cast, single-target
    Starflare1 = 49124, // ShinryuParadox->self, 3.0s cast, single-target
    SpiritsWithin = 50391, // Alxaal->ShinryuParadox, no cast, single-target
    Starflare2 = 49126, // Helper->self, 5.0s cast, range 60 width 10 rect
    Starflare3 = 49127, // Helper->self, 7.0s cast, range 60 width 10 rect
    CataclysmicVortex = 49121, // ShinryuParadox->self, 7.0s cast, single-target
    CataclysmicVortex1 = 49123, // Helper->player, no cast, single-target
    CataclysmicVortex2 = 49122, // ShinryuParadoxPart2->self, 7.0s cast, single-target
    UrielBlade = 50392, // Alxaal->ShinryuParadox, no cast, single-target
    KnuckleSandwich = 50385, // Prishe->self, 5.0s cast, range 8 circle
    Banish = 50386, // Prishe->none, 2.0s cast, single-target
    DarkNova = 49135, // ShinryuParadoxPart2->self, 5.0s cast, single-target
    DarkNova1 = 49134, // ShinryuParadox->self, 5.0s cast, single-target
    DarkNova2 = 49136, // Helper->player, no cast, range 6 circle
    AtomicTail = 49129, // ShinryuParadoxPart2->self, 6.0+1.0s cast, single-target
    AtomicTail1 = 49128, // ShinryuParadox->self, 6.0+1.0s cast, single-target
    AtomicTail2 = 49130, // Helper->self, 7.0s cast, range 50 width 70 rect
    GyreCharge = 49132, // ShinryuParadoxPart2->self, no cast, single-target
    GyreCharge1 = 49131, // ShinryuParadox->self, no cast, single-target
    GyreCharge2 = 49133, // Helper->self, 0.5s cast, range 60 circle
    Holy = 50393, // Alxaal->self, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    CloakOfWaxingDark = 5353, // none->player/Prishe, extra=0x0
    CloakOfWaningLight = 5352, // none->player/Alxaal, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
    DownForTheCount = 1963, // Helper->player/Alxaal/Prishe, extra=0xEC7
    UnknownStatus1 = 2202, // none->player, extra=0x0
    UnknownStatus2 = 2160, // none->Prishe, extra=0x316E

}

public enum IconID : uint
{
    Icon_z6r3_b4_lock_no_lk_7s_c0k2 = 680, // player/Alxaal->self
    Icon_z6r3_b4_lock_lk_7s_c0k2 = 681, // player->self
    Icon_z6r3_b4_lock_mv_7s_c0k2 = 683, // player/Prishe->self
    Icon_z6r3_b4_lock_no_mv_7s_c0k2 = 682, // player/Alxaal->self
    Icon_m0489trg_a0c = 136, // player/Alxaal/Prishe->self
    Icon_m0489trg_b0c = 137, // player->self
    Icon_tank_lockonae_6m_5s_01t = 344, // player->self
}
