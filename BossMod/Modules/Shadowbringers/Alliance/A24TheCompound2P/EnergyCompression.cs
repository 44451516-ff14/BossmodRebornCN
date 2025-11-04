namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class EnergyCompression(BossModule module) : Components.GenericTowers(module)
{
    private readonly List<(WPos source, Actor target)> _tethers = new(4); // tether target can teleport after tether got applied (in the same frame), leading to incorrect locations if used directly in OnTethered
    private bool first = true; // used to not draw 2nd set of towers early so we can wait for teleport
    private int movedTowers;

    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        if (first || movedTowers != 0)
        {
            return CollectionsMarshal.AsSpan(Towers);
        }
        return [];
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Transfer1 && WorldState.Actors.Find(tether.Target) is Actor t)
        {
            _tethers.Add((source.Position, t));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (Towers.Count != 6 && actor.OID == (uint)OID.Towers1)
        {
            Towers.Add(new(actor.Position.Quantized(), 5f, activation: WorldState.FutureTime(9.9d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
        {
            Towers.Clear();
            movedTowers = 0;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ForcedTransfer1 or (uint)AID.ForcedTransfer2)
        {
            _tethers.Clear(); // other mechanics use the same tether ID
        }
    }

    public override void Update()
    {
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var tower = ref towers[i];
            var countT = _tethers.Count - 1;
            for (var j = countT; j >= 0; --j)
            {
                var t = _tethers[j];
                if (t.target.Position.AlmostEqual(tower.Position, 1f))
                {
                    tower.Position = t.source;
                    tower.Activation = WorldState.FutureTime(10.2d);
                    _tethers.RemoveAt(j);
                    first = true; // not sure if and how towers repeat later, no replay is long enough
                    ++movedTowers;
                    break;
                }
            }
        }
    }
}
