namespace BossMod.Stormblood.Trial.T05Yojimbo;

class DragonsLair(BossModule module) : Components.Exaflare(module, _rect)
{
    private static readonly AOEShapeRect _rect = new(3f, 3f);
    private const float ExplosionInterval = 0.6f;
    private const int ExplosionsPerLine = 20;
    private const int MaxShownExplosions = 20;
    private const float StartDelay = 5.7f;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DragonsHead)
        {
            Lines.Add(new()
            {
                Next = actor.Position,
                Advance = 3 * actor.Rotation.ToDirection(),
                Rotation = actor.Rotation,
                NextExplosion = WorldState.FutureTime(StartDelay),
                TimeToMove = ExplosionInterval,
                ExplosionsLeft = ExplosionsPerLine,
                MaxShownExplosions = MaxShownExplosions
            });
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
