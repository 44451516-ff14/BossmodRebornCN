namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash1;

class Hydroshot(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _caster;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
            return new Knockback[1] { new(_caster.Position, 10f, Module.CastFinishAt(_caster.CastInfo)) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NHydroshot or (uint)AID.SHydroshot)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_caster == caster)
            _caster = null;
    }
}

class C030MonkStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030MonkStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Hydroshot>()
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        Hydroshot(id, 9.1f);
        CrossAttack(id + 0x10000, 3.4f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Hydroshot(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SHydroshot : (uint)AID.NHydroshot, delay, 5, "Single-target 1");
        Cast(id + 0x10, _savage ? (uint)AID.SHydroshot : (uint)AID.NHydroshot, 1.5f, 5, "Single-target 2");
        Cast(id + 0x20, _savage ? (uint)AID.SHydroshot : (uint)AID.NHydroshot, 1.5f, 5, "Single-target 3");
    }

    private void CrossAttack(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SCrossAttack : (uint)AID.NCrossAttack, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }
}
class C030NMonkStates(BossModule module) : C030MonkStates(module, false);
class C030SMonkStates(BossModule module) : C030MonkStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NMonk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12631, SortOrder = 4)]
public class C030NMonk(WorldState ws, Actor primary) : C030Trash1(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SMonk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12631, SortOrder = 4)]
public class C030SMonk(WorldState ws, Actor primary) : C030Trash1(ws, primary);
