namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD80Kajigakaka;

public enum OID : uint
{
    Boss = 0x23EC, // R4.5
    IcePillar = 0x23EE, // R2.0
    IceBoulder = 0x23ED // R1.5
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    HeavenswardHowl = 11985, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    EclipticBite = 11986, // Boss->player, no cast, single-target
    HowlingMoon = 11988, // Boss->self, no cast, single-target
    PillarImpact = 11990, // IcePillar->self, 2.5s cast, range 4+R circle
    PillarPierce = 11989, // IcePillar->self, 2.5s cast, range 80+R width 4 rect
    SphereShatter = 11992, // IceBoulder->self, no cast, range 8 circle
    LunarCry = 11987 // Boss->self, 3.0s cast, range 80+R circle
}

class HeavenswardHowl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavenswardHowl, new AOEShapeCone(12.5f, 60f.Degrees()));
class PillarImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarImpact, 6.5f);
class PillarPierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarPierce, new AOEShapeRect(82f, 2f));
class LunarCry(BossModule module) : Components.RaidwideCast(module, (uint)AID.LunarCry);

class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private readonly List<AOEInstance> _aoes = new(10);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.IceBoulder)
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(8.5d), actorID: actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
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

class DD80KajigakakaStates : StateMachineBuilder
{
    public DD80KajigakakaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavenswardHowl>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<LunarCry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 547, NameID = 7490)]
public class DD80Kajigakaka(WorldState ws, Actor primary) : HoHArena2(ws, primary);
