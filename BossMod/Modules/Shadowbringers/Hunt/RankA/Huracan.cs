namespace BossMod.Shadowbringers.Hunt.RankA.Huracan;

public enum OID : uint
{
    Boss = 0x28B5 // R=4.9
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WindsEnd = 17494, // Boss->player, no cast, single-target
    WinterRain = 17497, // Boss->location, 4.0s cast, range 6 circle
    Windburst = 18042, // Boss->self, no cast, range 80 width 10 rect
    SummerHeat = 17499, // Boss->self, 4.0s cast, range 40 circle
    DawnsEdge = 17495, // Boss->self, 3.5s cast, range 15 width 10 rect
    SpringBreeze = 17496, // Boss->self, 3.5s cast, range 80 width 10 rect
    AutumnWreath = 17498 // Boss->self, 4.0s cast, range 10-20 donut
}

class SpringBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpringBreeze, new AOEShapeRect(80f, 5f));
class SummerHeat(BossModule module) : Components.RaidwideCast(module, (uint)AID.SummerHeat);

class Combos(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(10f, 20f);
    private static readonly AOEShapeCircle circle = new(6f);
    private static readonly AOEShapeRect rect = new(15f, 5f);
    private static readonly AOEShapeRect rect2 = new(40f, 5f, 40f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        aoes[0].Risky = true;
        if (count > 1)
        {
            aoes[0].Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.AutumnWreath => donut,
            (uint)AID.DawnsEdge => rect,
            (uint)AID.WinterRain => circle,
            _ => null
        };
        if (shape != null)
        {
            AddAOE(shape);
            AddAOE(rect2, 180f.Degrees(), 3.1d);
        }
        void AddAOE(AOEShape shape, Angle offset = default, double delay = default) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay), risky: false));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Windburst or (uint)AID.AutumnWreath or (uint)AID.DawnsEdge or (uint)AID.WinterRain)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class HuracanStates : StateMachineBuilder
{
    public HuracanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpringBreeze>()
            .ActivateOnEnter<SummerHeat>()
            .ActivateOnEnter<Combos>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8912)]
public class Huracan(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
