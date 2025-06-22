namespace BossMod.Shadowbringers.Hunt.RankA.Rusalka;

public enum OID : uint
{
    Boss = 0x2853 // R=3.6
}

public enum AID : uint
{
    AutoAttack = 17364, // Boss->player, no cast, single-target
    Hydrocannon = 17363, // Boss->location, 3.5s cast, range 8 circle
    AetherialSpark = 17368, // Boss->self, 2.5s cast, range 12 width 4 rect
    AetherialPull = 17366, // Boss->self, 4.0s cast, range 30 circle, pull 30 between centers
    Flood = 17369 // Boss->self, no cast, range 8 circle
}

class Hydrocannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrocannon, 8f);
class AetherialSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialSpark, new AOEShapeRect(12f, 2f));

class AetherialPull(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AetherialPull, 30f, shape: new AOEShapeCircle(30f), kind: Kind.TowardsOrigin)
{
    private readonly Flood _aoe = module.FindComponent<Flood>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _aoe.AOE is Components.GenericAOEs.AOEInstance aoe && aoe.Check(pos);
}

class Flood(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE;
    private static readonly AOEShapeCircle circle = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AetherialPull)
            AOE = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 3.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Flood)
            AOE = null;
    }
}

class RusalkaStates : StateMachineBuilder
{
    public RusalkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<AetherialSpark>()
            .ActivateOnEnter<Flood>()
            .ActivateOnEnter<AetherialPull>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8896)]
public class Rusalka(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
