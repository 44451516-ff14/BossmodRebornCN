namespace BossMod.Endwalker.Hunt.RankS.Ophioneus;

public enum OID : uint
{
    Boss = 0x35DC // R5.875, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    RightMaw = 27350, // Boss->self, 5.0s cast, range 30 180-degree cone
    LeftMaw = 27351, // Boss->self, 5.0s cast, range 30 180-degree cone
    PyricCircle = 27352, // Boss->self, 5.0s cast, range 5-40 donut
    PyricBurst = 27353, // Boss->self, 5.0s cast, range 40 circle with ? falloff
    LeapingPyricCircle = 27341, // Boss->location, 6.0s cast, width 0 rect charge, visual
    LeapingPyricBurst = 27342, // Boss->location, 6.0s cast, width 0 rect charge, visual
    LeapingPyricCircleAOE = 27346, // Boss->self, 1.0s cast, range 5-40 donut
    LeapingPyricBurstAOE = 27347, // Boss->self, 1.0s cast, range 40 circle with ? falloff
    Scratch = 27348 // Boss->player, 5.0s cast, single-target
}

sealed class Maw(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightMaw, (uint)AID.LeftMaw], new AOEShapeCone(30f, 90f.Degrees()));

sealed class PyricCircleBurst(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeCircle circle = new(10f); // TODO: verify falloff
    private static readonly AOEShapeDonut donut = new(5f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.PyricCircle or (uint)AID.LeapingPyricCircleAOE or (uint)AID.LeapingPyricCircle => donut,
            (uint)AID.PyricBurst or (uint)AID.LeapingPyricBurstAOE or (uint)AID.LeapingPyricBurst => circle,
            _ => null
        };
        if (shape != null)
        {
            _aoe = [new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, spell.Action.ID <= (uint)AID.LeapingPyricBurst ? 5d : default))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PyricCircle or (uint)AID.PyricBurst or (uint)AID.LeapingPyricCircleAOE or (uint)AID.LeapingPyricBurstAOE)
        {
            _aoe = [];
        }
    }
}

sealed class Scratch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Scratch);

sealed class OphioneusStates : StateMachineBuilder
{
    public OphioneusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Maw>()
            .ActivateOnEnter<PyricCircleBurst>()
            .ActivateOnEnter<Scratch>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10621u)]
public sealed class Ophioneus(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
