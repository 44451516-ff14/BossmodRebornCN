namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class ViciousSwipe(BossModule module) : Components.GenericKnockback(module, (uint)AID.ViciousSwipe)
{
    private Knockback? _source = new(module.PrimaryActor.Position, 15, module.WorldState.FutureTime(module.StateMachine.ActiveState?.Duration ?? 0), _shape);

    private static readonly AOEShapeCircle _shape = new(8f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _source);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddCircle(Module.PrimaryActor.Position, _shape.Radius);
    }
}
