namespace BossMod.Dawntrail.Advanced.Ad01MerchantsTale.Ad013LoneSwordmaster;

public enum OID : uint
{
    LoneSwordmaster = 0x4B17,
    Helper = 0x233C,
    _Gen_AetherialFlow = 0x1EBFA4, // R2.000, x1, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    _Gen_Actor1ebf71 = 0x1EBF71, // R0.500, x1, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_TheMerchantsTaleAbridged = 0x1EBFAB, // R0.500, x1, EventObj type
    SteelsbreathPuddles = 0x1EBF72, // R0.500, x1, EventObj type
    ForceOfWillSource = 0x4B19, // R0.500, x0 (spawn during fight)
    ForceOfWillInter = 0x4B1E, // R1.000, x0 (spawn during fight)
    EchoingHushPuddle = 0x1EBF73, // R0.500, x0 (spawn during fight), EventObj type
    ForceOfWill = 0x4C3D, // R2.000, x0 (spawn during fight)
    _Gen_ForceOfWill3 = 0x4B18, // R1.250, x0 (spawn during fight)
}
public enum AID : uint
{
    _AutoAttack_Attack = 45128, // LoneSwordmaster->player, no cast, single-target
    SteelsbreathRelease = 48136, // LoneSwordmaster->self, 5.0s cast, range 60 circle
    _Weaponskill_MaleficQuartering = 46647, // LoneSwordmaster->self, 3.0s cast, single-target
    _Weaponskill_MaleficInfluence = 46649, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence1 = 46648, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence2 = 46651, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence3 = 46650, // Helper->player, no cast, single-target
    MaleficPortentCast = 46652, // LoneSwordmaster->self, 7.0s cast, single-target
    MaleficPortent = 46653, // Helper->player, no cast, single-target
    _Ability_ = 46607, // LoneSwordmaster->location, no cast, single-target
    LashOfLight = 46655, // Helper->self, 4.0s cast, range 40 90.000-degree cone
    _Weaponskill_ShiftingHorizon = 46654, // LoneSwordmaster->self, 4.0s cast, single-target
    UnyieldingWillVisual = 48652, // 4B19->4B1E, 6.9s cast, width 4 rect charge
    UnyieldingWill1 = 46656, // 4B19->location, no cast, width 4 rect charge
    UnyieldingWill2 = 46657, // 4B19->player, no cast, width 4 rect charge
    HeavensConfluenceIn1 = 46662, // Helper->self, 7.0s cast, range 8 circle
    HeavensConfluenceIn2 = 46664, // Helper->self, 9.0s cast, range 8 circle
    HeavensConfluenceOut1 = 46663, // Helper->self, 7.0s cast, range 8-60 donut
    HeavensConfluenceOut2 = 46665, // Helper->self, 9.0s cast, range 8-60 donut
    NearToHeavenSoloCast = 47562, // LoneSwordmaster->self, 7.0s cast, single-target
    NearToHeavenSolo = 46658, // LoneSwordmaster->player, no cast, range 5 circle
    NearToHeavenMultiCast = 47564, // LoneSwordmaster->self, 7.0s cast, single-target
    NearToHeavenMulti = 46660, // LoneSwordmaster->players, no cast, range 5 circle
    FarFromHeavenSoloCast = 47563, // LoneSwordmaster->self, 7.0s cast, single-target
    FarFromHeavenSolo = 46659, // LoneSwordmaster->player, no cast, range 5 circle
    FarFromHeavenMultiCast = 47565, // LoneSwordmaster->self, 7.0s cast, single-target
    FarFromHeavenMulti = 46661, // LoneSwordmaster->players, no cast, range 5 circle
    _Weaponskill_EchoingHeat = 46666, // LoneSwordmaster->self, 5.0s cast, single-target
    EchoingHush = 46747, // Helper->location, 7.5s cast, range 8 circle
    EchoingHush1 = 46667, // Helper->location, 4.0s cast, range 8 circle
    _Weaponskill_WolfsCrossing = 46668, // LoneSwordmaster->self, 4.0s cast, single-target
    WolfsCrossing = 46669, // Helper->self, 5.0s cast, range 40 width 8 cross
    _Weaponskill_EchoingEight = 46670, // LoneSwordmaster->self, 5.0s cast, single-target
    EchoingEight = 46671, // Helper->self, 3.0s cast, range 40 width 8 cross
    StingOfTheScorpion = 46646, // LoneSwordmaster->player, 5.0s cast, single-target
    _Weaponskill_MaleficAlignment = 46672, // LoneSwordmaster->self, 3.0+1.0s cast, single-target
    MaleficAlignment = 46673, // Helper->self, 4.0s cast, range 40 ?-degree cone
    _Weaponskill_CardinalHorizons = 46674, // LoneSwordmaster->self, 4.0s cast, single-target
    WillOfTheUnderworld = 47762, // 4C3D->self, 6.0s cast, range 40 width 20 rect
    WaitingWounds = 46676, // Helper->location, 6.0s cast, range 10 circle
    _Weaponskill_WaitingWounds1 = 46675, // LoneSwordmaster->self, 6.0s cast, single-target
    _Weaponskill_SilentEight = 46677, // LoneSwordmaster->self, 4.0s cast, single-target
    ResoundingSilence = 46678, // Helper->player, no cast, range 8 circle
    _Weaponskill_MawOfTheWolf = 46679, // LoneSwordmaster->self, 3.4+1.6s cast, single-target
    _Weaponskill_MawOfTheWolf1 = 46680, // Helper->self, 5.0s cast, range 80 width 80 rect
    SteelsbreathReleaseArena = 46681, // LoneSwordmaster->self, 5.0s cast, range 60 circle
    _Weaponskill_VanishingHorizon = 46682, // LoneSwordmaster->self, 4.0s cast, single-target
    _Weaponskill_SteelsbreathBonds = 46684, // LoneSwordmaster->self, no cast, single-target
    WillOfTheUnderworld1 = 46683, // 4B18->self, 6.0s cast, range 40 width 10 rect
}
public enum SID : uint
{
    MaleficE = 4773, // none->player, extra=0x0
    MaleficW = 4774, // none->player, extra=0x0
    MaleficEW = 4775, // none->player, extra=0x0
    MaleficS = 4776, // none->player, extra=0x0
    MaleficSE = 4777, // none->player, extra=0x0
    MaleficSW = 4778, // none->player, extra=0x0
    MaleficSEW = 4779, // none->player, extra=0x0
    MaleficN = 4780, // none->player, extra=0x0
    MaleficNE = 4781, // none->player, extra=0x0
    MaleficNW = 4782, // none->player, extra=0x0
    MaleficNEW = 4783, // none->player, extra=0x0
    MaleficNS = 4784, // none->player, extra=0x0
    MaleficNSE = 4785, // none->player, extra=0x0
    MaleficNSW = 4786, // none->player, extra=0x0
    MaleficNSEW = 4787, // none->player, extra=0x0
    Bind = 2518, // none->player, extra=0x0 
    _Gen_VulnerabilityUp = 1789, // Helper/LoneSwordmaster->player, extra=0x1/0x2/0x3/0x4
    _Gen_Bleeding = 3077, // none->player, extra=0x0
    _Gen_Bleeding1 = 3078, // none->player, extra=0x0
    _Gen_PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0
}
public enum IconID : uint
{
    Checkmark = 136, // player->self
    RedX = 137, // player->self
    Tankbuster = 218, // player->self
    Lockon = 648, // player->self
    SilentEight = 499, // player->self
    Chains = 653, // player->self
}
public enum TetherID : uint
{
    MaleficN = 357, // player->LoneSwordmaster
    MaleficE = 358, // player->LoneSwordmaster
    MaleficW = 359, // player->LoneSwordmaster
    MaleficS = 360, // player->LoneSwordmaster
    UnyieldingWill = 371, // 4B19/4B1E->4B1E/player
    Chains = 163, // player->player
}
