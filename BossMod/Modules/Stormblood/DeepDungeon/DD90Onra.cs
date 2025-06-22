namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD90Onra;

public enum OID : uint
{
    Boss = 0x23EF, // R3.0
    SandSphere = 0x2413, // R1.0
    Helper = 0x22A1
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    BurningRave = 12213, // Boss->location, 3.5s cast, range 8 circle
    KnucklePress = 12216, // Boss->self, 3.0s cast, range 6+R circle
    AncientQuaga = 12214, // Boss->self, 5.0s cast, range 60+R circle
    Subduction = 12210, // SandSphere->self, 3.0s cast, range 8+R circle
    MeteorImpactVisual = 12211, // Boss->self, 3.0s cast, single-target
    MeteorImpact = 12212, // Helper->location, 6.0s cast, range 60 circle, proximity AOE, optimal range around 20
    AuraCannon = 12215 // Boss->self, 3.0s cast, range 60+R width 10 rect
}

class BurningRave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningRave, 8f);
class KnucklePress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KnucklePress, 9f);
class AuraCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AuraCannon, new AOEShapeRect(63f, 5f));
class AncientQuaga(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientQuaga);
class MeteorImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpact, 20f);

class Subduction(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(9f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SandSphere)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(3.2d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Subduction)
        {
            var count = _aoes.Count;
            var pos = caster.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 0.1f))
                {
                    if (++aoe.ActorID == 3u)
                    {
                        _aoes.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor) // rarely a sphere only seems to do 2 instead of 3 hits
    {
        if (actor.OID == (uint)OID.SandSphere)
        {
            var count = _aoes.Count;
            var pos = actor.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoes[i].Origin.AlmostEqual(pos, 0.1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class DD90OnraStates : StateMachineBuilder
{
    public DD90OnraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Subduction>()
            .ActivateOnEnter<BurningRave>()
            .ActivateOnEnter<AncientQuaga>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<KnucklePress>()
            .ActivateOnEnter<AuraCannon>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 548, NameID = 7493)]
public class DD90Onra(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(HoHArenas.ArenaCenter, 25f, 48)], HoHArenas.Entrance);
}
