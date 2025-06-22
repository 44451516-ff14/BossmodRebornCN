namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to brightened fire mechanic
// this helper relies on waymarks 1-4
class BrightenedFire(BossModule module) : Components.CastCounter(module, (uint)AID.BrightenedFireAOE)
{
    private readonly int[] _playerOrder = new int[8]; // 0 if unknown, 1-8 otherwise

    private const float _aoeRange = 7;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerOrder[slot] <= NumCasts)
            return;

        var pos = PositionForOrder(_playerOrder[slot]);
        if (!actor.Position.InCircle(pos, 5))
        {
            hints.Add($"Get to correct position {_playerOrder[slot]}!");
        }

        var numHitAdds = Module.Enemies(OID.DarkenedFire).InRadius(actor.Position, _aoeRange).Count();
        if (numHitAdds < 2)
        {
            hints.Add("Get closer to adds!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_playerOrder[pcSlot] <= NumCasts)
            return;

        var pos = PositionForOrder(_playerOrder[pcSlot]);
        Arena.AddCircle(pos, 1, Colors.Safe);

        // draw all adds
        var addIndex = 0;
        foreach (var fire in Module.Enemies(OID.DarkenedFire).SortedByRange(pos))
        {
            Arena.Actor(fire, addIndex++ < 2 ? Colors.Danger : Colors.PlayerGeneric);
        }

        // draw range circle
        Arena.AddCircle(pc.Position, _aoeRange, Colors.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= 268 and <= 275)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerOrder[slot] = (int)iconID - 267;
        }
    }

    private WPos PositionForOrder(int order)
    {
        // TODO: consider how this can be improved...
        var markID = (int)Waymark.N1 + (order - 1) % 4;
        var wm = WorldState.Waymarks[markID];
        return wm != null ? new(wm.Value.XZ()) : Arena.Center;
    }
}
