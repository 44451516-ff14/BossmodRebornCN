namespace BossMod.Dawntrail.Dungeon.D13TheClyteum.D131EyeOfTheScorpion;

public enum OID : uint
{
    EyeOfTheScorpion = 0x4C2C,
    Helper = 0x233C,
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    MotionScanner = 0x4C2D, // R1.000, x2
}

public enum AID : uint
{
    AutoAttack = 50170, // 4EB6/4DD2->4C09/4C0A, no cast, single-target
    AutoAttack_Attack = 45128, // 4C09/4C0A->4EB6/4DD2, no cast, single-target
    AutoAttack_1 = 50110, // EyeOfTheScorpion->player, no cast, single-target
    AutoAttack_2 = 50428, // 4C09/4C0A->4EB6/4DD2, no cast, single-target
    EyesOnMe = 48896, // EyeOfTheScorpion->self, 5.0s cast, range 35 circle
    PetrifyingBeamCastBar = 50175, // EyeOfTheScorpion->self, 8.0+0.5s cast, single-target
    PetrifyingBeam = 50177, // Helper->self, 8.5s cast, range 70 100.000-degree cone
    PetrifyingBeamCastBar2 = 50176, // EyeOfTheScorpion->self, 8.0+0.5s cast, single-target
    PetrifyingBeam2 = 50178, // Helper->self, 8.5s cast, range 70 100.000-degree cone
    MotionScanner = 48893, // EyeOfTheScorpion->self, 4.0s cast, single-target
    BallisticMissile = 48897, // EyeOfTheScorpion->self, no cast, single-target
    PenetratorMissile = 48901, // Helper->players, 5.0s cast, range 6 circle
    SurfaceMissile = 48898, // Helper->location, 3.0s cast, range 5 circle
    Launch = 48895, // Helper->player, no cast, single-target
    AntiPersonnelMissile = 48899, // Helper->player, 5.0s cast, range 6 circle
}
public enum SID : uint
{
    DirectionalDisregard = 3808, // none->EyeOfTheScorpion, extra=0x0
    MotionTracker = 5191, // none->41EF/41F0/41F1/player, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
}

[SkipLocalsInit]

sealed class PetrifyingBeam(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PetrifyingBeam, (uint)AID.PetrifyingBeam2], new AOEShapeCone(70f, 50f.Degrees()));

sealed class EyesOnMe(BossModule module) : Components.RaidwideCast(module, (uint)AID.EyesOnMe);

sealed class PenetratorMissile(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PenetratorMissile, 6f, 4);

sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 5f);

sealed class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AntiPersonnelMissile, 5f);

sealed class MotionTracker(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MotionTracker && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MotionTracker && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
    }
    //TODO: Figure out best way to display the beam.  This works but doesn't communicate very well to the player what should happen. Likely a voidzone on the actor with a hint?
}

sealed class D131EyeOfTheScorpionStates : StateMachineBuilder
{
    public D131EyeOfTheScorpionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PetrifyingBeam>()
            .ActivateOnEnter<EyesOnMe>()
            .ActivateOnEnter<PenetratorMissile>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<MotionTracker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(D131EyeOfTheScorpionStates),
ConfigType = null, // replace null with typeof(EyeOfTheScorpionConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = null, // replace null with typeof(SID) if applicable
TetherIDType = null, // replace null with typeof(TetherID) if applicable
IconIDType = null, // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.EyeOfTheScorpion,
Contributors = "HerStolenLight",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1011u,
NameID = 14716u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D131EyeOfTheScorpion(WorldState ws, Actor primary) : BossModule(ws, primary, new(-615f, 575f), new ArenaBoundsSquare(20f));
