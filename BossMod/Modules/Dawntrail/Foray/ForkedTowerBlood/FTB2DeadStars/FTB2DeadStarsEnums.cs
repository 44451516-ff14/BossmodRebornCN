namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

public enum OID : uint
{
    Triton = 0x4786, // R4.0
    Phobos = 0x4788, // R4.0
    Nereid = 0x4787, // R4.0
    LiquifiedTriton = 0x478B, // R4.0
    LiquifiedNereid = 0x478C, // R4.0
    FrozenTriton = 0x4816, // R5.0
    FrozenPhobos = 0x4817, // R5.0
    GaseousNereid = 0x4814, // R5.0
    GaseousPhobos = 0x4815, // R5.0
    DeathwallHelper = 0x4866, // R0.5
    Deathwall = 0x1EBDAC, // R0.5
    DeadStars = 0x478D, // R15.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Phobos/Boss/Nereid->player, no cast, single-target
    Teleport1 = 42420, // Phobos/Nereid/Boss->location, no cast, single-target
    Teleport2 = 42486, // Phobos/Nereid/Boss->location, no cast, single-target
    Deathwall = 42504, // DeathwallHelper->self, no cast, range 30-40 donut

    DecisiveBattlePhobos = 42492, // Phobos->self, 6.0s cast, range 35 circle
    DecisiveBattleNereid = 42491, // Nereid->self, 6.0s cast, range 35 circle
    DecisiveBattleTriton = 42490, // Boss->self, 6.0s cast, range 35 circle

    SliceNDiceVisual = 42497, // Phobos/Nereid/Boss->player, 5.0s cast, single-target, tankbusters
    SliceNDice = 42498, // Helper->players, 5.0s cast, range 70 90-degree cone

    ThreeBodyProblemTeleport = 42421, // Phobos/Nereid/Boss->location, 5.3+0,7s cast, single-target
    ThreeBodyProblemVisual1 = 43453, // Nereid->self, 6.5s cast, single-target
    ThreeBodyProblemVisual2 = 42425, // Boss->self, 6.5s cast, single-target
    ThreeBodyProblemVisual3 = 42424, // Phobos/Boss->self, 6.5s cast, single-target
    ThreeBodyProblemVisual4 = 42423, // Phobos/Nereid->self, 6.5s cast, single-target
    NoisomeNuisance = 42551, // Helper->self, 7.0s cast, range 6 circle

    PrimordialChaosVisual1 = 42457, // Phobos->self, 5.0s cast, single-target, raidwide
    PrimordialChaosVisual2 = 42458, // LiquifiedTriton/LiquifiedNereid->self, no cast, single-target
    PrimordialChaos = 42460, // Helper->self, no cast, ???

    FrozenFalloutTelegraphBlue = 42464, // Helper->self, 2.5s cast, range 22 circle
    FrozenFalloutTelegraphRed = 42463, // Helper->self, 2.5s cast, range 22 circle
    FrozenFalloutVisual1 = 42461, // Phobos->self, 10.0s cast, single-target
    FrozenFalloutVisual2 = 42462, // Phobos->self, no cast, single-target
    FrozenFalloutTeleportTriton = 42465, // LiquifiedTriton->location, no cast, single-target
    FrozenFalloutTeleportNereid = 42466, // LiquifiedNereid->location, no cast, single-target
    FrozenFalloutAOEVisual = 42459, // Helper->self, 1.4s cast, range 22 circle
    FrozenFalloutRed = 42467, // Helper->self, 1.4s cast, range 22 circle
    FrozenFalloutBlue = 42468, // Helper->self, 1.4s cast, range 22 circle

    NoxiousNovaVisual = 42469, // Phobos->self, 5.0s cast, single-target, raidwide
    NoxiousNova = 42470, // Helper->self, 5.8s cast, ???

    ChangeModelState1 = 42427, // Nereid/Boss/Phobos->self, no cast, single-target
    ChangeModelState2 = 42428, // Nereid/Boss/Phobos->self, no cast, single-target
    ChangeModelState3 = 42426, // Phobos/Nereid/Boss->self, no cast, single-target

    VengefulBlizzardIII = 42430, // Nereid->self, 5.5s cast, range 60 120-degree cone
    VengefulFireIII = 42429, // Boss->self, 5.5s cast, range 60 120-degree cone
    VengefulBioIII = 42431, // Phobos->self, 5.5s cast, range 60 120-degree cone

    ExcruciatingEquilibrium1 = 42488, // Phobos/Nereid/Boss->self, no cast, single-target
    ExcruciatingEquilibrium2 = 42489, // Helper->Phobos/Nereid/Boss, 0.5s cast, single-target

    DeltaAttackVisual1 = 42493, // Phobos/Nereid/Boss->self, 5.0s cast, single-target
    DeltaAttackVisual2 = 42495, // Helper->self, 5.0s cast, single-target
    DeltaAttackVisual3 = 42494, // Phobos/Nereid/Boss->self, no cast, single-target
    DeltaAttackVisual4 = 42496, // Helper->self, no cast, single-target
    DeltaAttackFirst = 42558, // Helper->self, 5.5s cast, ???, raidwide
    DeltaAttackRepeat = 42559, // Helper->self, 0.5s cast, ???

    FirestrikeVisual = 42499, // Phobos/Nereid/Boss->self, 5.0s cast, single-target, line stack
    FirestrikeMarker1 = 42500, // Helper->player, no cast, single-target
    FirestrikeMarker2 = 42503, // Helper->player, no cast, single-target
    Firestrike = 42502, // Phobos/Nereid/Boss->players, no cast, range 70 width 10 rect

    IceboundBuffoon = 42550, // Helper->self, 7.0s cast, range 6 circle
    SnowballFlight = 42446, // Nereid->self, 7.0s cast, single-target
    SnowBoulderVisual = 42447, // Helper->location, 7.0s cast, width 10 rect charge
    SnowBoulder = 42448, // FrozenTriton/FrozenPhobos->location, no cast, width 10 rect charge, knockback 10, dir forward
    ChillingCollisionVisual1 = 42422, // Nereid->self, 5.0s cast, single-target
    ChillingCollisionVisual2 = 42451, // Helper->self, 5.0s cast, range 40 circle, knockback 21, away from source
    ChillingCollision = 42452, // Helper->self, no cast, ???

    AvalaunchVisual = 43162, // FrozenTriton/FrozenPhobos->self, 5.0s cast, single-target
    AvalaunchMarker = 42449, // Helper->player, 8.0s cast, single-target, stack
    Avalaunch = 42450, // FrozenTriton/FrozenPhobos->location, no cast, range 8 circle

    ToTheWinds1 = 42453, // Nereid->self, 13.0s cast, single-target
    ToTheWinds2 = 42437, // Boss->self, 7.0s cast, single-target

    SliceNStrike = 42501, // Phobos/Nereid/Boss->player, 5.0s cast, single-target

    BlazingBelligerent = 42549, // Helper->self, 7.0s cast, range 6 circle
    ElementalImpact1 = 42432, // GaseousNereid/GaseousPhobos->self, 3.0s cast, range 5 circle, tower
    ElementalImpact2 = 42433, // GaseousNereid/GaseousPhobos->self, 3.0s cast, range 5 circle, tower
    ElementalImpact3 = 42434, // GaseousNereid/GaseousPhobos->self, 5.0s cast, range 5 circle, tower
    ElementalImpactFailVisual = 42435, // Helper->self, no cast, single-target
    ElementalImpactFail = 42436, // Helper->self, no cast, ???, tower explodes
    FireSpread = 43272, // Helper->players, no cast, range 60 120-degree cone

    GeothermalRuptureVisual = 42441, // Boss->self, 5.0s cast, single-target
    GeothermalRupture = 42442, // Helper->location, 3.0s cast, range 8 circle

    FlameThrowerVisual = 42443, // Boss->self, 5.0s cast, single-target
    FlameThrowerMarker = 42444, // Helper->player, no cast, single-target, line stack
    FlameThrower = 42445, // Helper->self, no cast, range 40 width 8 rect

    SixHandedFistfightVisual1 = 42471, // Phobos/Nereid/Boss->location, 9.4+0,6s cast, single-target
    SixHandedFistfightVisual2 = 42472, // Helper->self, 10.0s cast, range 12 circle
    SixHandedFistfight = 42473, // Helper->self, 10.5s cast, ???

    Bodied = 42474, // Helper->self, no cast, range 12 circle, inner deathwall after sixhandedfirstfight
    BodiedVisual1 = 42475, // Boss/Phobos/Nereid->self, no cast, single-target
    BodiedVisual2 = 42476, // Nereid/Phobos/Boss->self, no cast, single-target

    CollateralGasJet = 42480, // Helper->self, 5.0s cast, range 40 60-degree cone
    CollateralHeatJet = 42478, // Helper->self, 5.0s cast, range 40 60-degree cone
    CollateralColdJet = 42479, // Helper->self, 5.0s cast, range 40 60-degree cone

    CollateralDamage = 42477, // DeadStars->self, 5.0s cast, single-target
    CollateralIceball = 42482, // Helper->player, no cast, range 4 circle
    CollateralBioball = 42483, // Helper->player, no cast, range 4 circle
    CollateralFireball = 42481, // Helper->player, no cast, range 4 circle

    Return = 42487, // Phobos/Nereid/Boss->self, 6.0s cast, single-target
    SelfDestructVisual1 = 42454, // FrozenTriton/FrozenPhobos->self, 13.0s cast, single-target
    SelfDestructVisual2 = 42438, // GaseousPhobos/GaseousNereid->self, 7.0s cast, single-target
    SelfDestructVisual3 = 42455, // Helper->self, no cast, single-target
    SelfDestructVisual4 = 42439, // Helper->self, no cast, single-target
    SelfDestruct1 = 42456, // Helper->self, no cast, ???
    SelfDestruct2 = 42440, // Helper->self, no cast, ???

    FusionBurstVisual = 42484, // DeadStars->self, 10.0s cast, single-target, enrage
    FusionBurst = 42485 // Helper->self, 10.0s cast, ???
}

public enum SID : uint
{
    IceOoze = 4442, // none->player, extra=0x1/0x2/0x3
    NovaOoze = 4441, // none->player, extra=0x2/0x1/0x3
    IceboundBuffoonery = 4443 // none->FrozenTriton/FrozenPhobos, extra=0x4/0x3/0x2/0x1
}

public enum IconID : uint
{
    Avalaunch = 100 // player->self
}

public enum TetherID : uint
{
    DecisiveBattle = 249, // player->Boss/Phobos/Nereid
    AvalaunchBad = 246 // FrozenTriton/FrozenPhobos->player
}
