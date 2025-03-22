namespace BossMod.Stormblood.Extreme.Ex6Byakko;

public enum OID : uint
{
    Boss = 0x20F3, // R4.3
    Hakutei = 0x20F4, // R4.75 - tiger
    AratamaForce = 0x20F5, // R0.7 - orbs from center
    IntermissionHakutei = 0x2161, // R4.75
    AramitamaSoul = 0x20F6, // R1.0
    AratamaPuddle = 0x1E8EA9, // R0.5
    IntermissionHelper = 0x1EA87E, // R0.5
    VacuumClaw = 0x1EA957, // R0.5
    ArenaFeatures = 0x1EA1A1, // R2.0
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttackBoss = 872, // Boss->player, no cast, single-target
    AutoAttackAdd = 870, // Hakutei->player, no cast, single-target
    TeleportBoss = 10071, // Boss->location, no cast, single-target
    TeleportAdd = 9822, // Hakutei->location, no cast, single-target
    StormPulse = 10204, // Boss->self, 4.0s cast, range 100 circle, raidwide
    StormPulseRepeat = 10564, // Boss->self, no cast, range 100 circle, raidwide
    HeavenlyStrike = 10202, // Boss->players, 4.0s cast, range 3 circle tankbuster

    StateOfShock = 10208, // Boss->player, 4.0s cast, single-target, stun target to grab & throw it
    StateOfShockSecond = 10070, // Boss->player, no cast, single-target, stun second target to grab & throw it
    Clutch = 10209, // Boss->player, no cast, single-target, grab target
    HighestStakes = 10210, // Boss->location, 5.0s cast, single-target, jump
    HighestStakesAOE = 10211, // Helper->location, no cast, range 6 circle tower

    UnrelentingAnguish = 10221, // Boss->self, 3.0s cast, single-target, visual (orbs)
    UnrelentingAnguishAratama = 10230, // AratamaForce->self, no cast, range 2 circle, orb explosion
    OminousWind = 10219, // Boss->self, no cast, single-target, apply bubbles
    OminousWindAOE = 10220, // Helper->self, no cast, range 6 circle, if bubbles touch
    FireAndLightningBoss = 10201, // Boss->self, 4.0s cast, range 50+R width 20 rect

    DanceOfTheIncomplete = 9681, // Boss->self, no cast, single-target, visual (split off tiger)
    AddAppear = 9679, // Hakutei->self, no cast, single-target, visual (start appear animation)
    AratamaPuddle = 9821, // Helper->location, no cast, range 4 circle, puddle drop
    SteelClaw = 10207, // Hakutei->self, no cast, range 13+R ?-degree cone, cleave
    WhiteHerald = 10234, // Hakutei->self, no cast, range 50 circle with ? falloff
    DistantClap = 10205, // Boss->self, 5.0s cast, range 4-25 donut
    FireAndLightningAdd = 10206, // Hakutei->self, 4.0s cast, range 50+R width 20 rect

    VoiceOfThunder = 10231, // Hakutei->self, no cast, single-target, visual (physical damage down orbs)
    VoiceOfThunderAratama = 10659, // AramitamaSoul->self, no cast, range 2 circle, orb explosion if soaked
    VoiceOfThunderAratamaFail = 10232, // AramitamaSoul->Hakutei, no cast, single-target, orb explosion if it reaches the tiger, heals
    RoarOfThunder = 10233, // Hakutei->self, 20.0s cast, range 100 circle, raidwide scaled by remaining hp
    RoarOfThunderEnd1 = 10725, // Hakutei->Boss, no cast, single-target, visual (???)
    RoarOfThunderEnd2 = 10724, // Boss->self, no cast, single-target, visual (???)
    IntermissionOrbVisual = 10222, // Boss->self, no cast, single-target, visual (start next set of orbs)
    IntermissionOrbSpawn = 10223, // Helper->location, no cast, single-target, visual (location of next orb)
    IntermissionOrbAratama = 10224, // Helper->location, no cast, range 2 circle
    ImperialGuard = 10225, // IntermissionHakutei->self, 3.0s cast, range 40+R width 5 rect
    IntermissionSweepTheLegVisual = 10227, // Boss->self, no cast, single-target, visual (start donut)
    IntermissionSweepTheLeg = 10228, // Helper->self, 5.1s cast, range 5-25 donut
    IntermissionEnd = 10794, // Boss->self, no cast, single-target, visual (intermission end)
    FellSwoop = 10235, // Helper->self, no cast, range 100 circle, raidwide

    AnswerOnHigh = 10212, // Boss->self, no cast, single-target, visual (exaflare start)
    HundredfoldHavocFirst = 10213, // Helper->self, 5.0s cast, range 5 circle exaflare
    HundredfoldHavocRest = 10214, // Helper->self, no cast, range 5 circle exaflare
    SweepTheLegBoss = 10203, // Boss->self, 4.0s cast, range 24+R 270-degree cone

    Bombogenesis = 10215, // Boss->self, no cast, single-target, visual (3 baited puddle icons)
    GaleForce = 10216, // Helper->self, no cast, range 6 circle, baited puddle
    VacuumClaw = 10217, // Helper->self, no cast, range ? circle, growing voidzone aoe
    VacuumBlade = 10218, // Helper->self, no cast, range 100 circle, ??? (raidwide with vuln, happens if vacuum claws intersect)

    StormPulseEnrage = 10761 // Boss->self, 8.0s cast, range 100 circle
}

public enum SID : uint
{
    Stun = 201, // Boss->player, extra=0x0
    OminousWind = 1481 // none->player, extra=0x0
}

public enum IconID : uint
{
    HighestStakes = 62, // Helper->self
    AratamaPuddle = 4, // player->self
    WhiteHerald = 87, // player->self
    Bombogenesis = 101 // player->self
}
