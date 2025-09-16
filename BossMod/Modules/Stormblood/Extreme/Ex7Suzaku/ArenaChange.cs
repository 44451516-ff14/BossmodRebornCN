namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(3.5f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScarletFever)
        {
            _aoe = [new(circle, Arena.Center, default, Module.CastFinishAt(spell, 7d))];
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && actor.OID == (uint)OID.RapturousEchoPlatform)
        {
            Arena.Bounds = Ex7Suzaku.Phase2Bounds;
            _aoe = [];
        }
    }
}
