namespace BossMod.Stormblood.Quest.MSQ.TheTimeBetweenTheSeconds;

public enum OID : uint
{
    Boss = 0x1A36,
    ZenosYaeGalvus = 0x1CEE, // R0.500, x9
    DomanSignifer = 0x1A3A, // R0.500, x3
    DomanHoplomachus = 0x1A39, // R0.500, x2
    ZenosYaeGalvus1 = 0x1EBC, // R0.920, x1
    DarkReflection = 0x1A37, // R0.920, x2
    LightlessFlame = 0x1CED, // R1.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    VeinSplitter = 8987, // 1A36->self, 3.5s cast, range 10 circle
    Concentrativity = 8986, // 1A36->self, 3.0s cast, range 80 circle
    LightlessFlame = 8988, // 1CED->self, 1.0s cast, range 10+R circle
    LightlessSpark = 8985, // 1A36->self, 3.0s cast, range 40+R 90-degree cone
    ArtOfTheSword1 = 8993, // 1CEE->self, 3.0s cast, range 40+R width 6 rect
}

class ArtOfTheSword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArtOfTheSword1, new AOEShapeRect(41f, 3f));
class VeinSplitter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VeinSplitter, 10f);
class Concentrativity(BossModule module) : Components.RaidwideCast(module, (uint)AID.Concentrativity);
class LightlessFlame(BossModule module) : Components.GenericAOEs(module, (uint)AID.LightlessFlame)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(11f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LightlessFlame)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.CurrentTime.AddSeconds(7d)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightlessFlame)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                var a = _aoes[i];
                if (a.ActorID == id)
                {
                    _aoes[i] = a with { Activation = Module.CastFinishAt(spell) };
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightlessFlame)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
class LightlessSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightlessSpark, new AOEShapeCone(40.92f, 45.Degrees()));
class P2Boss(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies((uint)OID.ZenosYaeGalvus1));
        Arena.Actors(Module.Enemies((uint)OID.DarkReflection));
    }
}

class ZenosYaeGalvusStates : StateMachineBuilder
{
    public ZenosYaeGalvusStates(BossModule module) : base(module)
    {
        SimplePhase(0, id => BuildState(id, ""), "P1")
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, id => BuildState(id, "").ActivateOnEnter<ArtOfTheSword>().ActivateOnEnter<P2Boss>(), "P2")
            .Raw.Update = () => Module.Enemies((uint)OID.ZenosYaeGalvus1).Count == 0;
    }

    private State BuildState(uint id, string name, float duration = 10000)
    {
        return SimpleState(id, duration, name)
            .ActivateOnEnter<VeinSplitter>()
            .ActivateOnEnter<Concentrativity>()
            .ActivateOnEnter<LightlessFlame>()
            .ActivateOnEnter<LightlessSpark>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68034, NameID = 5954)]
public class ZenosYaeGalvus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom([new(-226.91f, 523.65f), new(-254.46f, 524.46f), new(-254.66f, 541.06f), new(-269.99f, 544.12f),
    new(-269.58f, 565.97f), new(-254.58f, 565.89f), new(-249.05f, 554.06f), new(-229.18f, 562.35f)])]);
}
