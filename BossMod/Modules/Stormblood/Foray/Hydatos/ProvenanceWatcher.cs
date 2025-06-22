namespace BossMod.Stormblood.Foray.Hydatos.ProvenanceWatcher;

public enum OID : uint
{
    Boss = 0x2686, // R12.000, x1
    Helper1 = 0x2687, // R0.500, x1
    Charybdis = 0x2768, // R1.000, x0 (spawn during fight)
    Helper3 = 0x277B, // R0.500, x0 (spawn during fight)
    Helper4 = 0x277E, // R12.000, x0 (spawn during fight)
    Icicle = 0x2769, // R2.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 14983, // Boss->player, no cast, single-target
    TheScarletPrice = 15001, // Boss->player, 5.0s cast, range 3 circle
    TheScarletWhisper = 15000, // Boss->self, 4.0s cast, range 10+R 120-degree cone
    Reforge = 14987, // Boss->self, 5.0s cast, single-target
    EuhedralSwat = 14992, // Helper4->self, 6.0s cast, range 100 width 26 rect
    Touchdown = 14993, // Boss->self, no cast, range 50 circle
    PillarImpact = 15346, // Icicle->self, 2.5s cast, range 4+R circle
    PillarPierce = 15344, // Icicle->self, 4.0s cast, range 50+R width 10 rect
    Thunderstorm = 15005, // Helper3->location, 3.0s cast, range 5 circle
    IceAndLevin = 14997, // Boss->self, 5.0s cast, single-target
    Chillstorm = 15006, // Helper3->self, no cast, range 11-40 donut
    IceAndWind = 14996, // Boss->self, 5.0s cast, single-target
    Charybdis = 15002, // Helper2->location, no cast, range 6 circle
    HotTailFirst = 14999, // Boss->self, 5.0s cast, range 65+R width 16 rect
    HotTailSecond = 15007, // Boss->self, no cast, range 65+R width 16 rect
    AkhMornFirst = 14994, // Boss->players, 5.0s cast, range 6 circle
    AkhMornRest = 14995, // Boss->players, no cast, range 6 circle
    DiffractiveBreak = 14998 // Boss->self, 4.0s cast, range 40 circle
}

class TheScarletPrice(BossModule module) : Components.BaitAwayCast(module, (uint)AID.TheScarletPrice, 3f, true);
class TheScarletWhisper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheScarletWhisper, new AOEShapeCone(22f, 60f.Degrees()));
class EuhedralSwat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EuhedralSwat, new AOEShapeRect(100f, 13f));
class Touchdown(BossModule module) : Components.RaidwideInstant(module, (uint)AID.Touchdown, 3.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (Activation == default && spell.Action.ID == (uint)AID.EuhedralSwat)
            Activation = WorldState.FutureTime(Delay);
    }
}
class PillarImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarImpact, 6.5f);
class PillarPierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarPierce, new AOEShapeRect(52.5f, 5));
class Thunderstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderstorm, 5f);

class IceAndLevin(BossModule module) : Components.GenericAOEs(module, (uint)AID.Chillstorm)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeDonut donut = new(11f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IceAndLevin)
            _aoe = new(donut, spell.LocXZ, default, Module.CastFinishAt(spell, 1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
            _aoe = null;
    }
}
class Charybdis(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);

    private readonly List<(ulong, WPos, DateTime, int)> casters = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = casters[i];
            aoes[i] = new(circle, WPos.ClampToGrid(c.Item2), default, c.Item3);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Charybdis)
            casters.Add((actor.InstanceID, WPos.ClampToGrid(actor.Position), WorldState.FutureTime(4d), 19));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Charybdis)
        {
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(casters);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var c = ref aoes[i];
                if (c.Item1 == id)
                {
                    if (--c.Item4 == 0)
                        casters.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class HotTail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HotTailFirst, new AOEShapeRect(77f, 8f));

class HotTailSecond(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(77f, 8f, 77f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    private AOEInstance? _aoe;
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HotTailFirst)
            _aoe = new(rect, caster.Position, spell.Rotation, WorldState.FutureTime(3.1d));

        else if (spell.Action.ID == (uint)AID.HotTailSecond)
            _aoe = null;
    }
}

class AkhMorn(BossModule module) : Components.GenericStackSpread(module)
{
    private int castsLeft;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMornFirst)
        {
            Stacks.Add(new(WorldState.Actors.Find(spell.TargetID)!, 6f, activation: Module.CastFinishAt(spell)));
            castsLeft = 3;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMornRest && --castsLeft == 0)
            Stacks.Clear();
    }
}

class ProvenanceWatcherStates : StateMachineBuilder
{
    public ProvenanceWatcherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheScarletPrice>()
            .ActivateOnEnter<TheScarletWhisper>()
            .ActivateOnEnter<EuhedralSwat>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<Thunderstorm>()
            .ActivateOnEnter<IceAndLevin>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<HotTail>()
            .ActivateOnEnter<HotTailSecond>()
            .ActivateOnEnter<AkhMorn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1423, Contributors = "xan", SortOrder = 10)]
public class ProvenanceWatcher(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
