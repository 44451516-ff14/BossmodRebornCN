namespace BossMod.Shadowbringers.Hunt.RankA.LilMurderer;

public enum OID : uint
{
    Boss = 0x28B4 // R=2.1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    GobthunderIII = 17493, // Boss->player, 6.0s cast, range 20 circle, interruptible, applies Lightning Resistance Down II
    GoblinPunch = 17488, // Boss->player, 3.0s cast, single-target
    GobthunderII = 17492, // Boss->location, 4.0s cast, range 8 circle, applies Lightning Resistance Down II
    Gobhaste = 17491, // Boss->self, 3.0s cast, single-target
    GoblinSlash = 17489 // Boss->self, no cast, range 8 circle, sometimes boss uses Gobthunder II on itself, next attack after is this
}

class GoblinSlash(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GobthunderII && spell.LocXZ == caster.Position)
            _aoe = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 2.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GoblinSlash)
            _aoe = null;
    }
}

class GobthunderIII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.GobthunderIII, 20f);
class GobthunderIIIHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GobthunderIII);
class GoblinPunch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.GoblinPunch);
class Gobhaste(BossModule module) : Components.CastHint(module, (uint)AID.Gobhaste, "Attack speed buff");
class GobthunderII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GobthunderII, 8f);

class LilMurdererStates : StateMachineBuilder
{
    public LilMurdererStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoblinPunch>()
            .ActivateOnEnter<GoblinSlash>()
            .ActivateOnEnter<GobthunderII>()
            .ActivateOnEnter<GobthunderIII>()
            .ActivateOnEnter<GobthunderIIIHint>()
            .ActivateOnEnter<Gobhaste>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8911)]
public class LilMurderer(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
