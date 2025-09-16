namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class AllegiantArsenal(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(70f, 135f.Degrees());
    private static readonly AOEShapeCircle circle = new(10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.AllegiantArsenalBow or (uint)AID.AllegiantArsenalSword => cone,
            (uint)AID.AllegiantArsenalStaff => circle,
            _ => null
        };
        if (shape != null)
        {
            _aoe = [new(shape, spell.LocXZ, spell.Rotation + (spell.Action.ID == (uint)AID.AllegiantArsenalSword ? 180f.Degrees() : default), Module.CastFinishAt(spell, 8.1d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.InfernalSlash or (uint)AID.Flashvane or (uint)AID.FuryOfBozja)
        {
            _aoe = [];
        }
    }
}
