namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA2Raiden;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeDonut donut = new(30f, 35f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Thundercall)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 3.4d))];
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u && actor.OID == (uint)OID.Electricwall)
        {
            Arena.Bounds = BA2Raiden.DefaultArena;
            _aoe = [];
        }
    }
}
