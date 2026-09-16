namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL3Adrammelech;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HolyIV && Arena.Bounds.Radius > 26f)
        {
            var origin = Arena.Center;
            _aoe = [new(donut, origin, default, Module.CastFinishAt(spell, 1.2d), shapeDistance: donut.Distance(origin, default))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            var arena = new ArenaBoundsCustom([new Polygon(Arena.Center, 25f, 48)]);
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
        }
    }
}
