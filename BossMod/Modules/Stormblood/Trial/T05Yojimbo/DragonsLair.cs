namespace BossMod.Stormblood.Trial.T05Yojimbo;

class DragonsLair(BossModule module) : Components.Exaflare(module, _rect)
{
    private static readonly AOEShapeRect _rect = new(3f, 3f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DragonsHead)
        {
            Lines.Add(new(actor.Position, 3f * actor.Rotation.ToDirection(), WorldState.FutureTime(5.7d), 0.6d, 20, 20, actor.Rotation));
        }
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        foreach (var line in Lines)
        {
            if (line.ExplosionsLeft > 0 && now >= line.NextExplosion)
                AdvanceLine(line, line.Next);
        }
    }
}
