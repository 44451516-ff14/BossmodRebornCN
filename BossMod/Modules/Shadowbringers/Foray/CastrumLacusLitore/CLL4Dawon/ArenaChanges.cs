namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donutDawon = new(30f, 35f);
    private readonly AOEShapeDonut donutLyon = new(20f, 25f);
    private AOEInstance[] _aoe = [];
    private bool lyonDeathwall;
    private bool dawonDeathwall;
    private readonly WPos topCenter = new(80f, -874f);
    private readonly WPos bottomCenter = new(80f, -813f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.MoltingPlumage && !dawonDeathwall)
        {
            AddAOE(donutDawon, new(80f, -813f), 0.2d, 0);
        }
        else if (id == (uint)AID.RagingWindsVisual1 && !lyonDeathwall)
        {
            AddAOE(donutLyon, new(80f, -874f), 1.2d, 1);
        }
        void AddAOE(AOEShapeDonut shape, WPos origin, double delay, int layer)
        {
            _aoe = [new(shape, origin, default, Module.CastFinishAt(spell, delay), shapeDistance: shape.Distance(origin, default), arenaProjectionLayer: layer, restrictToArenaProjectionLayer: true)];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is var id && id == (uint)OID.DeathwallDawon)
        {
            var bottom = new Polygon(bottomCenter, 30f, 96);
            var top = new Polygon(topCenter, 24.5f, 48);
            var combinedCenter = new WPos(80f, -840.75f);
            var polybottom = new RelSimplifiedComplexPolygon(bottom.Contour(combinedCenter));
            var polytop = new RelSimplifiedComplexPolygon(top.Contour(combinedCenter));
            var arena = new ArenaBoundsCustom([bottom, top], WorldProjectionLayers: [new(polybottom, 254.5f, borderY: 254.5f), new(polytop, 258.7f, borderY: 258.7f)]);
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
            dawonDeathwall = true;
        }
        else if (id == (uint)OID.DeathwallLyon)
        {
            var bottom = new Polygon(bottomCenter, 30f, 96);
            var top = new Polygon(topCenter, 20f, 48);
            var combinedCenter = new WPos(80f, -838.5f);
            var polybottom = new RelSimplifiedComplexPolygon(bottom.Contour(combinedCenter));
            var polytop = new RelSimplifiedComplexPolygon(top.Contour(combinedCenter));
            var arena = new ArenaBoundsCustom([bottom, top], WorldProjectionLayers: [new(polybottom, 254.5f, borderY: 254.5f), new(polytop, 258.7f, borderY: 258.7f)]);
            lyonDeathwall = true;
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
        }
    }
}
