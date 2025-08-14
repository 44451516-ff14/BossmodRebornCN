namespace BossMod.Dawntrail.Quest.Job.Pictomancer.SomewhereOnlySheKnows.JanquetilaquesPortrait;

public enum OID : uint
{
    Boss = 0x4298, // R4.000, x1
    AFlowerInTheSun = 0x4299, // R2.72
    OerTheAncientArbor = 0x429A, // R2.6
    TheHallowedPeak = 0x429B, // R6.24
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 37542, // Boss->player, no cast, single-target
    Teleport = 37541, // Boss->location, no cast, single-target

    FlowerMotif = 37524, // Boss->self, 5.0s cast, single-target
    BloodyCaress = 37527, // AFlowerInTheSun->self, 5.0s cast, range 60 180-degree cone

    FloodInBlueVisual = 37534, // Boss->self, 5.0s cast, single-target
    FloodInBlueFirst = 37535, // Helper->self, 5.0s cast, range 50 width 10 rect
    FloodInBlueRest = 37536, // Helper->self, no cast, range 50 width 5 rect

    BlazeInRed = 37539, // Boss->location, 6.0s cast, range 40 circle

    ArborMotif = 37525, // Boss->self, 5.0s cast, single-target
    TornadoInGreen = 37538, // Boss->self, 5.0s cast, range 10-40 donut
    NineIviesVisual1 = 37528, // OerTheAncientArbor->self, 3.0s cast, single-target
    NineIviesVisual2 = 39744, // OerTheAncientArbor->self, no cast, single-target
    NineIvies = 37529, // Helper->self, 3.0s cast, range 50 20-degree cone

    SculptureCast = 37537, // Boss->self, 5.0s cast, range 45 circle, gaze
    MountainMotif = 37526, // Boss->self, 5.0s cast, single-target
    EarthquakeVisual = 37530, // TheHallowedPeak->self, 5.0s cast, single-target
    Earthquake1 = 37531, // Helper->self, 5.0s cast, range 10 circle
    Earthquake2 = 37532, // Helper->self, 7.0s cast, range 10-20 donut
    Earthquake3 = 37533, // Helper->self, 9.0s cast, range 20-30 donut

    FreezeInCyan = 37540 // Boss->self, 5.0s cast, range 40 45-degree cone
}

sealed class FreezeInCyan(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FreezeInCyan, new AOEShapeCone(40f, 22.5f.Degrees()));
sealed class NineIvies(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NineIvies, new AOEShapeCone(50f, 10f.Degrees()), 9);
sealed class TornadoInGreen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TornadoInGreen, new AOEShapeDonut(10f, 40f));
sealed class SculptureCast(BossModule module) : Components.CastGaze(module, (uint)AID.SculptureCast);
sealed class BlazeInRed(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlazeInRed);

sealed class BloodyCaress(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AFlowerInTheSun)
            _aoe = new(cone, actor.Position, actor.Rotation, WorldState.FutureTime(9.8f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BloodyCaress)
            _aoe = null;
    }
}

sealed class Earthquake(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Earthquake1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Earthquake1 => 0,
                (uint)AID.Earthquake2 => 1,
                (uint)AID.Earthquake3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class FloodInBlueFirst : Components.SimpleAOEs
{
    public FloodInBlueFirst(BossModule module) : base(module, (uint)AID.FloodInBlueFirst, new AOEShapeRect(50f, 5f)) { Color = Colors.Danger; }
}

sealed class FloodInBlueRest(BossModule module) : Components.Exaflare(module, new AOEShapeRect(25f, 2.5f, 25f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddLine(WPos first, WDir dir, Angle offset) => Lines.Add(new(first, dir, Module.CastFinishAt(spell), 2d, 5, 2, spell.Rotation + offset));
        if (spell.Action.ID == (uint)AID.FloodInBlueFirst)
        {
            var advance = spell.Rotation.ToDirection().OrthoR();
            var advance1 = advance * 7.5f;
            var advance2 = advance * 5f;
            var pos = caster.Position;
            AddLine(pos + advance1, advance2, default);
            AddLine(pos - advance1, -advance2, 180f.Degrees());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FloodInBlueRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 3f))
                {
                    AdvanceLine(line, caster.Position + 2.5f * line.Rotation.ToDirection().OrthoR());
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

sealed class JanquetilaquesPortraitStates : StateMachineBuilder
{
    public JanquetilaquesPortraitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<FreezeInCyan>()
            .ActivateOnEnter<NineIvies>()
            .ActivateOnEnter<TornadoInGreen>()
            .ActivateOnEnter<SculptureCast>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<BlazeInRed>()
            .ActivateOnEnter<FloodInBlueFirst>()
            .ActivateOnEnter<FloodInBlueRest>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70395, NameID = 13037)]
public sealed class JanquetilaquesPortrait(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -340f), new ArenaBoundsSquare(24.5f));
