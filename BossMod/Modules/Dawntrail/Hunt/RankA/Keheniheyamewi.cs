namespace BossMod.Dawntrail.Hunt.RankA.Keheniheyamewi;

public enum OID : uint
{
    Boss = 0x43DC // R8.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Scatterscourge1 = 39807, // Boss->self, 4.0s cast, range 10-40 donut
    Scatterscourge2 = 38650, // Boss->self, 1.5s cast, range 10-40 donut
    SlipperyScatterscourge = 38648, // Boss->self, 5.0s cast, range 20 width 10 rect
    WildCharge = 39559, // Boss->self, no cast, range 20 width 10 rect
    PoisonGas = 38652, // Boss->self, 5.0s cast, range 60 circle
    BodyPress1 = 40063, // Boss->self, 4.0s cast, range 15 circle
    BodyPress2 = 38651, // Boss->self, 4.0s cast, range 15 circle
    MalignantMucus = 38653, // Boss->self, 5.0s cast, single-target
    PoisonMucus = 38654 // Boss->location, 1.0s cast, range 6 circle
}

public enum SID : uint
{
    RightFace = 2164,
    LeftFace = 2163,
    ForwardMarch = 2161,
    AboutFace = 2162
}

sealed class BodyPress(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BodyPress1, (uint)AID.BodyPress2], 15f);
sealed class Scatterscourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scatterscourge1, new AOEShapeDonut(10f, 40f));

sealed class SlipperyScatterscourge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(20f, 5f);
    private static readonly AOEShapeDonut donut = new(10f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SlipperyScatterscourge)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 0.2f)));
            _aoes.Add(new(donut, WPos.ClampToGrid(caster.Position + 20f * spell.Rotation.ToDirection()), default, Module.CastFinishAt(spell, 2.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.WildCharge or (uint)AID.Scatterscourge2)
            _aoes.RemoveAt(0);
    }
}

sealed class PoisonGas(BossModule module) : Components.RaidwideCast(module, (uint)AID.PoisonGas);

sealed class PoisonGasMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 5f)
{
    private readonly SlipperyScatterscourge _aoe = module.FindComponent<SlipperyScatterscourge>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = ForcedMovements(actor);
        var count = movements.Count;
        if (count == 0)
            return;
        var last = movements[count - 1];
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim for green safe spot!");
    }
}

sealed class MalignantMucus(BossModule module) : Components.CastInterruptHint(module, (uint)AID.MalignantMucus);
sealed class PoisonMucus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonMucus, 6f);

sealed class KeheniheyamewiStates : StateMachineBuilder
{
    public KeheniheyamewiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BodyPress>()
            .ActivateOnEnter<Scatterscourge>()
            .ActivateOnEnter<SlipperyScatterscourge>()
            .ActivateOnEnter<PoisonGas>()
            .ActivateOnEnter<PoisonGasMarch>()
            .ActivateOnEnter<MalignantMucus>()
            .ActivateOnEnter<PoisonMucus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13401)]
public sealed class Keheniheyamewi(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
