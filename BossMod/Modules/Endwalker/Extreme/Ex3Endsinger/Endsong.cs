namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

class Endsong(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _active = [];

    private static readonly AOEShapeCircle _aoe = new(15f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_active.Any(a => _aoe.Check(actor.Position, a)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var a in _active)
            _aoe.Draw(Arena, a);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.EndsongFirst or (uint)TetherID.EndsongNext)
            _active.Add(source);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        _active.Remove(source);
    }
}
