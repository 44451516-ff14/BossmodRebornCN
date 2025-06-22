namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class CrushingHoof(BossModule module) : Components.GenericAOEs(module, (uint)AID.CrushingHoofAOE)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(25);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CrushingHoof)
            _aoe = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
            _aoe = null;
    }
}
