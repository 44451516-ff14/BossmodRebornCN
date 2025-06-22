namespace BossMod;

public sealed class DemoModule : BossModule
{
    private sealed class DemoComponent(BossModule module) : BossComponent(module)
    {
        public override void AddHints(int slot, Actor actor, TextHints hints)
        {
            hints.Add("Hint", false);
            hints.Add("Risk");
        }

        public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
        {
            movementHints.Add(actor.Position, actor.Position + new WDir(10f, 10f), Colors.Danger);
        }

        public override void AddGlobalHints(GlobalHints hints)
        {
            hints.Add("Global");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc)
        {
            Arena.ZoneCircle(Arena.Center, 10f, Colors.AOE);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            Arena.Actor(Arena.Center, default, Colors.PC);
        }
    }

    public DemoModule(WorldState ws, Actor primary) : base(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
    {
        ActivateComponent<DemoComponent>();
    }
}
