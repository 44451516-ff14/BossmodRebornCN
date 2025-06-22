namespace BossMod.Dawntrail.Unreal.UnSuzaku;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(3.5f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScarletFever && Module.Arena.Bounds == UnSuzaku.Phase1Bounds)
            _aoe = new(circle, Arena.Center, default, Module.CastFinishAt(spell, 7f));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.RapturousEchoPlatform && state == 0x00040008)
        {
            Arena.Bounds = UnSuzaku.Phase2Bounds;
            _aoe = null;
        }
    }
}
