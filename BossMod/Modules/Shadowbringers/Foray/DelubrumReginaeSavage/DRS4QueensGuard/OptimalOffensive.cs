namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class OptimalOffensiveSword(BossModule module) : Components.ChargeAOEs(module, (uint)AID.OptimalOffensiveSword, 2.5f);
sealed class OptimalOffensiveShield(BossModule module) : Components.ChargeAOEs(module, (uint)AID.OptimalOffensiveShield, 2.5f);

// note: there are two casters (as usual in bozja content for raidwides)
sealed class OptimalOffensiveShieldKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.OptimalOffensiveShieldKnockback, 10f, true, 1);

sealed class UnluckyLot : Components.GenericAOEs
{
    public UnluckyLot(BossModule module) : base(module)
    {
        circle = new(20f);
        _aoe = [new(circle, module.Center, default, module.WorldState.FutureTime(7.6d))];
    }
    private readonly AOEShapeCircle circle;
    private AOEInstance[] _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OptimalOffensiveShieldMoveSphere)
        {
            _aoe = [new(circle, caster.Position, default, WorldState.FutureTime(8.6d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnluckyLot)
        {
            _aoe = [];
        }
    }
}
