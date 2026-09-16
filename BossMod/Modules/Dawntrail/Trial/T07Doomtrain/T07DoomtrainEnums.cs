namespace BossMod.Dawntrail.Trial.T07Doomtrain;

public enum OID : uint
{
    _Gen_Aether = 0x233C, // R0.500, x?, Helper type
    _Gen_Doomtrain = 0x4A33, // R1.000, x?, Doomtrain Helper
    Doomtrain = 0x4A30, // R19.040, x? - First train visual probably
    LevinSignal = 0x4A31, // R1.000, x? - The portals from north that shoot rays from north to south on upper and lower levels
    KinematicTurret = 0x4A32, // R1.200, turrets that shoot from sides
    AetherIntermission = 0x4A34, // R1.500, x The aether enemy during intermission
    DoomtrainIntermission = 0x4B7E, // R19.040, x? - Doomtrain during the intermission
    GhostTrain = 0x4B80, // R2.720, x?
    ArcaneRevelation = 0x4A36, // R1.000, x?  - Large red outlined circle
}

public enum AID : uint
{
    _Ability_ = 45662, // 233C->player, no cast, single-target
    // tank buster spreads with red icon
    LightningBurstVisual = 45660, // 4A30->self, 5.0s cast, single-target
    LightningBurst = 45661, // 233C->player, 0.5s cast, range 5 circle
    // Lightning Express is ramming the car and knocking everybody back
    LightningExpress = 45618, // 4A30->self, 6.0s cast, range 70 width 70 rect
    // Beams that shoot out of portal visuals.
    // 'Levin signal readies plasma beam.'/ 'levin signal uses plasma beam'
    PlasmaBeamUpper = 45620, // 4A31->self, 1.0s cast, range 30 width 5 rect :
    PlasmaBeamLower = 45619, // 4A31->self, 1.0s cast, range 30 width 5 rect :
    PlasmaBeamMedium = 45621, // 4A31->self, 1.0s cast, range 20 width 5 rect
    PlasmaBeamShort = 45622, // 4A31->self, 1.0s cast, range 10 width 5 rect
    // Windpipe knockback.  Draw in and rectangle AOE (Blastpipe) at front of car.
    WindpipeVisual = 45625, // 4A30->self, 6.0+1.0s cast, single-target
    WindpipeDrawIn = 45667, // 233C->self, 7.0s cast, range 30 width 20 rect
    BlastpipeVisual = 45626, // 4A30->self, no cast, single-target
    Blastpipe = 45627, // 233C->self, 3.0s cast, range 10 width 20 rect
    UnlimitedExpressVisual = 45623, // 4A30->self, 5.0s cast, single-target : Switch to car 2 on first cast/ switch to car 3 on second cast / switch to car 4 on third cast
    UnlimitedExpress = 45624, // 233C->self, 5.9s cast, range 70 width 70 rect
    _Ability_1 = 45641, // 4A30->location, no cast, single-target
    _Ability_TurretCrossing = 45628, // 4A30->self, 3.0s cast, single-target
    ElectrayLong = 45629, // 4A32->self, 5.0s cast, range 25 width 5 rect : lower deck
    ElectrayShort = 45633, // 4A32->self, 5.0s cast, range 10 width 5 rect : lower deck
    ElectrayMedium = 45632, // 4A32->self, 5.0s cast, range 15 width 5 rect : lower deck
    Electray3 = 45631, // 4A32->self, 5.0s cast, range 20 width 5 rect
    ElectrayUpper = 45630, // 4A32->self, 5.0s cast, range 25 width 5 rect : Upper Deck
    HeadOnEmissionVisual = 45634, // 4A30->self, 6.0+1.0s cast, single-target : Ground floor?
    ThunderousBreathLowerDeck = 45635, // 233C->self, 7.0s cast, range 70 width 70 rect
    HeadOnEmissionVisual1 = 45636, // 4A30->self, 6.0+1.0s cast, single-target : Uppder deck
    HeadlightUpperDeck = 45637, // 4A33->self, 7.0s cast, range 30 width 20 rect
    RunawayTrain = 45638, // 4A30->self, 5.0s cast, single-target : sends to circle arena
    _Ability_Overdraught = 45639, // 4A34->self, no cast, single-target
    AetherSurgeVisual = 45642, // 4A34->self, 6.0s cast, single-target
    AetherSurge = 45643, // 233C->self, 6.0s cast, range 30 45.000-degree cone
    AetherialRay = 45640, // 233C->self, no cast, range 50 ?-degree cone
    RunawayTrainVisual = 45644, // 4B7E->self, no cast, single-target
    RunawayTrainRaidwide = 45645, // 233C->self, no cast, range 20 circle
    ShockwaveVisual = 45646, // 4A30->self, no cast, single-target
    Shockwave = 45647, // 233C->self, no cast, range 50 circle
    ArcaneRevelationVisual = 47527, // 4A30->self, 2.0+1.0s cast, single-target
    _Ability_HailOfThunder = 45656, // 4A30->self, no cast, single-target - indicator moves 2
    _Ability_HailOfThunder2 = 45657, // 4A30->self, no cast, single-target - indicator moves 3
    HailOfThunder = 45659, // 233C->location, 3.2s cast, range 16 circle
    DerailmentSiege = 45648, // 4A30->self, 6.0+1.0s cast, single-target
    DerailmentSiegeStack = 45650, // 233C->self, no cast, range 5 circle
    _Ability_DerailmentSiege2 = 45651, // 233C->self, 0.5s cast, range 5 circle
    DerailmentSiegeCircle = 45649, // 233C->self, 10.0s cast, range 5 circle
    Derail = 45654, // 233C->self, 10.0s cast, range 30 width 20 rect
    DerailVisual = 45653, // 4A30->self, 10.1s cast, single-target
    _Ability_2 = 45655, // 4A30->location, no cast, single-target
    _Ability_BatteringArms = 47529, // 4A30->self, 6.0+1.0s cast, single-target
    _Ability_BatteringArms1 = 47236, // 233C->self, no cast, range 5 circle
    _Ability_BatteringArms2 = 47237, // 233C->self, 0.5s cast, range 5 circle
}

public enum SID : uint
{
    Distance = 4541, // none->GhostTrain, extra=0x578 (170 degree rotation)/0x960 (106 degree rotation) Aetherial Ray Ghost train?
    Stop = 4176, // none->GhostTrain, extra=0x0
    DesignatedConductor = 4719, // none->player, extra=0x0
}

public enum IconID : uint
{
    LightningBurstIcon = 343, // player->self
    Horn = 642, // IntermissionTrain->self
    AetherialRayIcon = 412, // player->self
    DoubleToot = 637, // IntermissionTrain->self
    TripleToot = 638, // IntermissionTrain->self
    Plummet = 499, // player->self
}
