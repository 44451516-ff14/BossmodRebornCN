namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class ViciousSwipe : Components.GenericKnockback
{
    public ViciousSwipe(BossModule module) : base(module, (uint)AID.ViciousSwipe)
    {
        _kb = [new(module.PrimaryActor.Position, 15f, module.WorldState.FutureTime(module.StateMachine.ActiveState?.Duration ?? default), _shape)];
    }
    private readonly Knockback[] _kb;

    private readonly AOEShapeCircle _shape = new(8f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.ZoneCircleOutline(Module.PrimaryActor.Position, _shape.Radius);
    }
}
