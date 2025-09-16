namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL3Adrammelech;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HolyIV && Arena.Bounds != CLL3Adrammelech.DefaultArena)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.2d))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = CLL3Adrammelech.DefaultArena;
            Arena.Center = CLL3Adrammelech.DefaultArena.Center;
        }
    }
}
