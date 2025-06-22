namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class OptimalOffensiveSword(BossModule module) : Components.ChargeAOEs(module, (uint)AID.OptimalOffensiveSword, 2.5f);
sealed class OptimalOffensiveShield(BossModule module) : Components.ChargeAOEs(module, (uint)AID.OptimalOffensiveShield, 2.5f);

// note: there are two casters (as usual in bozja content for raidwides)
sealed class OptimalOffensiveShieldKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.OptimalOffensiveShieldKnockback, 10, true, 1);

sealed class UnluckyLot(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(20f);
    private AOEInstance? _aoe = new(circle, module.Center, default, module.WorldState.FutureTime(7.6d));

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OptimalOffensiveShieldMoveSphere)
            _aoe = new(circle, caster.Position, default, WorldState.FutureTime(8.6f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnluckyLot)
            _aoe = null;
    }
}
