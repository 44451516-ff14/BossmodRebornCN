namespace BossMod.Dawntrail.Foray.FATE.RoughWaters;

public enum OID : uint
{
    Boss = 0x4718, // R5.5
    Helper = 0x4719
}

public enum AID : uint
{
    AutoAttack = 41768, // Boss->player, no cast, single-target

    TidelineVisual = 41770, // Boss->self, 3.0s cast, single-target
    TidelineFirst = 41771, // Helper->location, 5.0s cast, range 50 width 10 rect
    TidelineRest = 41772, // Helper->location, 1.0s cast, range 50 width 5 rect

    RightTwinTentacle = 41781, // Boss->self, 5.0s cast, range 60 180-degree cone
    LeftTentacle = 41780, // Boss->self, no cast, range 60 180-degree cone
    LeftTwinTentacle = 41779, // Boss->self, 5.0s cast, range 60 180-degree cone
    RightTentacle = 41782, // Boss->self, no cast, range 60 180-degree cone

    VoidWaterIIIVisual = 41783, // Boss->self, 3.0s cast, single-target
    VoidWaterIII = 41784, // Helper->location, 3.0s cast, range 6 circle
    VoidWaterIVVisual = 41785, // Boss->self, 5.0s cast, single-target
    VoidWaterIV = 41786, // Helper->location, 5.0s cast, range 40 circle

    RecedingTwinTides = 41773, // Boss->self, 5.0s cast, single-target
    NearTide1 = 41774, // Helper->location, 5.0s cast, range 10 circle
    FarTide1 = 41778, // Helper->location, 7.0s cast, range 10-40 donut
    EncroachingTwinTides = 41776, // Boss->self, 5.0s cast, single-target
    FarTide2 = 41777, // Helper->location, 5.0s cast, range 10-40 donut
    NearTide2 = 41775 // Helper->location, 7.0s cast, range 10 circle
}

sealed class Tideline(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(9)];
    private readonly AOEShapeRect rect1 = new(50f, 5f), rect2 = new(50f, 2.5f);
    private readonly EncroachingTwinTides inout = module.FindComponent<EncroachingTwinTides>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count == 9 ? inout.Sequences.Count != 0 && inout.Sequences.Ref(0).NumCastsDone == 0 ? 1 : 3 : count > 3 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        return aoes;
    }

    private void UpdateAOEs()
    {
        var count = _aoes.Count;
        var max = count == 9 ? 3 : count > 3 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        var isFourAOEs = max == 4;
        var isThreeAOEs = max == 3;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];

            var shouldBeDanger = isFourAOEs && i < 2 || isThreeAOEs && i == 0;
            var shouldBeRisky = shouldBeDanger || max == 2 && i < 2;

            if (shouldBeDanger)
            {
                aoe.Color = Colors.Danger;
            }

            if (shouldBeRisky)
            {
                aoe.Risky = true;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TidelineFirst)
        {
            var rot = spell.Rotation;
            var pos = caster.Position;
            var activation = Module.CastFinishAt(spell);
            AddAOE(rect1, activation, spell.LocXZ, rot);

            var a180 = 180f.Degrees();
            var dir1 = (rot + a180).Round(1f).ToDirection();
            var dir2 = rot.Round(1f).ToDirection();
            var dirOrtho = (rot + a180 + 90f.Degrees()).Round(1f).ToDirection();

            for (var i = 0; i < 4; ++i)
            {
                var act = activation.AddSeconds(2d + 2d * i);
                var dirOrthoAdj = (7.5f + 5f * i) * dirOrtho;
                AddAOE(rect2, act, (pos - 25f * dir1 + dirOrthoAdj).Quantized(), rot + a180);
                AddAOE(rect2, act, (pos - 25f * dir2 + -dirOrthoAdj).Quantized(), rot);
            }
            UpdateAOEs();
        }
        void AddAOE(AOEShapeRect shape, DateTime act, WPos position, Angle rotation) => _aoes.Add(new(shape, position, rotation, act, risky: false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count is var count && count != 0 && spell.Action.ID is (uint)AID.TidelineFirst or (uint)AID.TidelineRest)
        {
            _aoes.RemoveAt(0);
            if (count < 4)
            {
                return;
            }
            UpdateAOEs();
        }
    }
}

sealed class TwinTentacle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly AOEShapeCone cone = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftTwinTentacle or (uint)AID.RightTwinTentacle)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var act = Module.CastFinishAt(spell);
            AddAOE();
            AddAOE(180f.Degrees(), 2.1d);

            void AddAOE(Angle offset = default, double delay = default)
            {
                var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
                var rot2 = rot + offset;
                _aoes.Add(new(cone, pos, rot2, delay != default ? act.AddSeconds(delay) : act, shapeDistance: cone.Distance(pos, rot2)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count is var count && count != 0 && spell.Action.ID is (uint)AID.LeftTwinTentacle or (uint)AID.RightTwinTentacle or (uint)AID.LeftTentacle or (uint)AID.RightTentacle)
        {
            _aoes.RemoveAt(0);
            if (count == 2)
            {
                ref var aoe2 = ref _aoes.Ref(0);
                var rot = aoe2.Rotation;
                aoe2.Origin -= 5f * rot.ToDirection();
                aoe2.ShapeDistance = cone.Distance(aoe2.Origin, rot);
            }
        }
    }
}

sealed class RecedingTwinTides(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 40f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NearTide1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.NearTide1 => 0,
                (uint)AID.FarTide1 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class EncroachingTwinTides(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(10f, 40f), new AOEShapeCircle(10f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FarTide2)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.FarTide2 => 0,
                (uint)AID.NearTide2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class VoidWaterIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidWaterIII, 6f);
sealed class VoidWaterIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoidWaterIV);

sealed class RoughWatersStates : StateMachineBuilder
{
    public RoughWatersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TwinTentacle>()
            .ActivateOnEnter<VoidWaterIII>()
            .ActivateOnEnter<VoidWaterIV>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018u, NameID = 1962u)]
public sealed class RoughWaters : OpenWorldFate
{
    public RoughWaters(WorldState ws, Actor primary) : base(ws, primary)
    {
        ActivateComponent<TwinTentacle>();
        ActivateComponent<EncroachingTwinTides>();
        ActivateComponent<RecedingTwinTides>();
        ActivateComponent<Tideline>();
    }
}
