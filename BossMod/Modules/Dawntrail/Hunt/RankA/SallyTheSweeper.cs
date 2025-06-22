namespace BossMod.Dawntrail.Hunt.RankA.SallyTheSweeper;

public enum OID : uint
{
    Boss = 0x4395 // R6.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ExecutionModelCircleTelegraph = 38454, // Boss->self, 2.0s cast, range 10 circle
    ExecutionModelDonutTelegraph = 38455, // Boss->self, 2.0s cast, range 10-40 donut
    ExecutionModelCrossTelegraph = 38456, // Boss->self, 2.0s cast, range 40 width 10 cross
    CodeExecutionCircleFirst = 38457, // Boss->self, 5.0s cast, range 10 circle
    CodeExecutionDonutFirst = 38458, // Boss->self, 5.0s cast, range 10-40 donut
    CodeExecutionDonutRest = 38462, // Boss->self, no cast, range 10-40 donut
    CodeExecutionCrossRest = 38463, // Boss->self, no cast, range 40 width 10 cross
    CodeExecutionCircleRest = 38461, // Boss->self, no cast, range 10 circle
    TargetedAdvance = 38466, // Boss->location, 7.0s cast, range 18 circle
    ReverseCodeVisual = 40056, // Boss->self, 5.0s cast, single-target, visual (???)
    ReverseCodeCrossFirst = 38460, // Boss->self, 5.0s cast, range 40 width 10 cross
    ReverseCodeCircleFirst = 38459 // Boss->self, 5.0s cast, range 10 circle
}

sealed class TargetedAdvance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TargetedAdvance, 18f);

sealed class ExecutionModel(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(10f, 40f);
    private static readonly AOEShapeCross cross = new(40f, 5f, 45f.Degrees());
    private readonly List<AOEInstance> _aoes = new(3);
    private readonly List<AOEShape> _shapes = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else if (aoe.Shape != cross)
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, float delay = default) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, delay)));
        switch (spell.Action.ID)
        {
            case (uint)AID.ExecutionModelCircleTelegraph:
                _shapes.Add(circle);
                break;
            case (uint)AID.ExecutionModelDonutTelegraph:
                _shapes.Add(donut);
                break;
            case (uint)AID.ExecutionModelCrossTelegraph:
                _shapes.Add(cross);
                break;
            case (uint)AID.CodeExecutionCircleFirst:
            case (uint)AID.CodeExecutionDonutFirst:
                var count = _shapes.Count;
                for (var i = 0; i < count; ++i)
                {
                    var shape = _shapes[i];
                    AddAOE(shape, i * 2.6f);
                }
                _shapes.Clear();
                break;
            case (uint)AID.ReverseCodeCircleFirst:
            case (uint)AID.ReverseCodeCrossFirst:
                var countI = _shapes.Count - 1;
                for (var i = countI; i >= 0; --i)
                {
                    var shape = _shapes[i];
                    AddAOE(shape, 5.2f - 2.6f * i);
                }
                _shapes.Clear();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.CodeExecutionCircleFirst:
                case (uint)AID.CodeExecutionCircleRest:
                case (uint)AID.CodeExecutionCrossRest:
                case (uint)AID.CodeExecutionDonutFirst:
                case (uint)AID.CodeExecutionDonutRest:
                case (uint)AID.ReverseCodeCircleFirst:
                case (uint)AID.ReverseCodeCrossFirst:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

sealed class SallyTheSweeperStates : StateMachineBuilder
{
    public SallyTheSweeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ExecutionModel>()
            .ActivateOnEnter<TargetedAdvance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13435)]
public sealed class SallyTheSweeper(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
