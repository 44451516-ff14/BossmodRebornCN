namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

sealed class BoxSpawn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoxSpawn, new AOEShapeRect(8f, 4f));

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    private readonly List<Square> squaresInflated = new(4);
    public readonly List<Square> Squares = new(4);
    private static readonly Square inflatedSquare = new(default, 4.5f);
    private static readonly Square square = new(default, 4f);
    private DateTime lastUpdate;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoxSpawn)
        {
            var pos = caster.Position;
            squaresInflated.Add(inflatedSquare with { Center = pos }); // squares adjusted for player hitbox radius
            Squares.Add(square with { Center = pos });
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures)
        {
            if (state == 0x00010002u)
            {
                Arena.Bounds = new ArenaBoundsComplex(A31KnaveofHearts.BaseSquare, [.. squaresInflated]);
                squaresInflated.Clear();
                lastUpdate = WorldState.CurrentTime;
            }
            else if (state == 0x00040008u && WorldState.CurrentTime > lastUpdate.AddSeconds(1d)) // clearing old squares can happen in the same frame as new squares got added
            {
                Arena.Bounds = A31KnaveofHearts.DefaultArena;
            }
        }
    }
}
