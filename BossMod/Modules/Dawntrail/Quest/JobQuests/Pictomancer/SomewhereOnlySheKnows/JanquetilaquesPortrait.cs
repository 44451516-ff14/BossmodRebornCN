namespace BossMod.Dawntrail.Quest.JobQuests.Pictomancer.SomewhereOnlySheKnows.JanquetilaquesPortrait;

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

    FreezeInCyan = 37540, // Boss->self, 5.0s cast, range 40 45-degree cone
}

class FreezeInCyan(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FreezeInCyan), new AOEShapeCone(40, 22.5f.Degrees()));
class NineIvies(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NineIvies), new AOEShapeCone(50, 10.Degrees()), 9);
class TornadoInGreen(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TornadoInGreen), new AOEShapeDonut(10, 40));
class SculptureCast(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.SculptureCast));
class BlazeInRed(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BlazeInRed));

class BloodyCaress(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.AFlowerInTheSun)
            _aoe = new(cone, actor.Position, actor.Rotation, WorldState.FutureTime(9.8f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BloodyCaress)
            _aoe = null;
    }
}

class Earthquake(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Earthquake1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.Earthquake1 => 0,
                AID.Earthquake2 => 1,
                AID.Earthquake3 => 2,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
        }
    }
}

class FloodInBlueFirst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FloodInBlueFirst), new AOEShapeRect(25, 5, 25), color: Colors.Danger);

class FloodInBlueRest(BossModule module) : Components.Exaflare(module, new AOEShapeRect(25, 2.5f, 25))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FloodInBlueFirst)
        {
            var advance1 = spell.Rotation.ToDirection().OrthoR() * 7.5f;
            var advance2 = spell.Rotation.ToDirection().OrthoR() * 5;
            Lines.Add(new() { Next = caster.Position + advance1, Advance = advance2, Rotation = spell.Rotation, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = 5, MaxShownExplosions = 2 });
            Lines.Add(new() { Next = caster.Position - advance1, Advance = -advance2, Rotation = (spell.Rotation + 180.Degrees()).Normalized(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = 5, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FloodInBlueRest)
        {
            ++NumCasts;
            var index = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 3));
            if (index >= 0)
            {
                AdvanceLine(Lines[index], caster.Position + 2.5f * Lines[index].Rotation.ToDirection().OrthoR());
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}

class JanquetilaquesPortraitStates : StateMachineBuilder
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
            .ActivateOnEnter<FloodInBlueRest>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70395, NameID = 13037)]
public class JanquetilaquesPortrait(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -340), new ArenaBoundsSquare(24.5f));
