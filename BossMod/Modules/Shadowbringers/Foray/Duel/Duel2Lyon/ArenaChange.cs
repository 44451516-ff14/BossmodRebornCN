namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(20f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RagingWinds1)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.2d))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            // arena turns into a pulsating donut aoe
            Arena.Bounds = new ArenaBoundsCircle(20f);
            Arena.Center = Arena.Center.Quantized();
            _aoe = [];
        }
    }
}
