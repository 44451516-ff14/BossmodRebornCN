namespace BossMod.Stormblood.Hunt.RankA.Angada;

public enum OID : uint
{
    Boss = 0x1AC0, // R=5.4
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ScytheTail = 8190, // Boss->self, 3.0s cast, range 4+R circle, knockback 10, away from source + stun
    RockThrow = 8193, // Boss->location, 3.0s cast, range 6 circle
    Butcher = 8191, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    Rip = 8192 // Boss->self, no cast, range 6+R 120-degree cone, always happens directly after Butcher
}

class ScytheTail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScytheTail, 9.4f);
class Butcher(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Butcher, new AOEShapeCone(11.4f, 60.Degrees()));

class Rip(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(11.4f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Butcher)
        {
            _aoe = [new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Rip)
        {
            _aoe = [];
        }
    }
}

class RockThrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RockThrow, 6f);

class AngadaStates : StateMachineBuilder
{
    public AngadaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScytheTail>()
            .ActivateOnEnter<Butcher>()
            .ActivateOnEnter<Rip>()
            .ActivateOnEnter<RockThrow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 5999)]
public class Angada(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
