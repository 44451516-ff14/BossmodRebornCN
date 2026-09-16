namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

sealed class Parhelion(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _completedParhelions = [];
    private bool _subparhelions;

    private static readonly AOEShapeRect _beacon = new(45, 3);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveParhelions().Any(p => _beacon.Check(actor.Position, p)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var p in ActiveParhelions())
            _beacon.Draw(Arena, p);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BeaconParhelion:
                _completedParhelions.Add(caster);
                _subparhelions = _completedParhelions.Count >= 15;
                break;
            case (uint)AID.BeaconSubparhelion:
                _completedParhelions.Remove(caster);
                break;
        }
    }

    private IEnumerable<Actor> ActiveParhelions()
    {
        if (_subparhelions)
            return _completedParhelions.Take(10);
        else
            return Module.Enemies((uint)OID.Parhelion).Where(p => p.CastInfo != null);
    }
}
