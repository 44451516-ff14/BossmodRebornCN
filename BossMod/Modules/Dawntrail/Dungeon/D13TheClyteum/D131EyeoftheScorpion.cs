namespace BossMod.Dawntrail.Dungeon.D13TheClyteum.D131EyeOfTheScorpion;

public enum OID : uint
{
    EyeOfTheScorpion = 0x4C2C,
    MotionScanner = 0x4C2D, // R1.000, x2
    Helper = 0x233C
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
    MotionTracker = 5191, // none->41EF/41F0/41F1/player, extra=0x0
}

[SkipLocalsInit]

sealed class PetrifyingBeam(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PetrifyingBeam, (uint)AID.PetrifyingBeam2], new AOEShapeCone(70f, 50f.Degrees()), maxCasts: 2);

sealed class EyesOnMe(BossModule module) : Components.RaidwideCast(module, (uint)AID.EyesOnMe);

sealed class PenetratorMissile(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PenetratorMissile, 6f, 4);

sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 5f);

sealed class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AntiPersonnelMissile, 5f);

sealed class MotionTracker(BossModule module) : Components.StayMove(module)
{
    public Actor? TrackingBeam;
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

    public override void OnActorRenderflagsChange(Actor actor, int renderflags)
    {
        if (actor.OID == (uint)OID.MotionScanner)
        {
            if (renderflags == 0)
            {
                TrackingBeam = actor;
            }
            else if (renderflags == 16384)
            {
                TrackingBeam = null;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (TrackingBeam != null)
        {
            var _rect = new AOEShapeRect(9f, 20f, 9f);
            _rect.Draw(Arena, TrackingBeam.Position, TrackingBeam.Rotation);
            if (pc.Position.InRect(TrackingBeam.Position, TrackingBeam.Rotation, 10f, 10f, 20f))
            {
                PlayerStates[pcSlot] = new(Requirement.Stay, WorldState.CurrentTime);
            }
            else
                PlayerStates[pcSlot] = default; // In theory the status should have fallen off by this point but better safe than sorry.
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (TrackingBeam != null)
        {
            hints.Add("Stop everything while in beam!");
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

[ModuleInfo(BossModuleInfo.Maturity.Verified,
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
