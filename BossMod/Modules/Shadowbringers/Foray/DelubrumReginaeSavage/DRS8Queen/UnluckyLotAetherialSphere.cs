namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

sealed class UnluckyLotAetherialSphere(BossModule module) : Components.GenericAOEs(module, (uint)AID.UnluckyLotAetherialSphere)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OptimalOffensiveMoveSphere)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 2.6d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnluckyLotAetherialSphere)
        {
            _aoe = [];
        }
    }
}
