namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P2SuperJump(BossModule module) : Components.GenericBaitAway(module, (uint)AID.SuperJumpAOE, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(10f);

    public override void Update()
    {
        CurrentBaits.Clear();
        var source = ((TEA)Module).BruteJustice();
        var target = source != null ? Raid.WithoutSlot(false, true, true).Farthest(source.Position) : null;
        if (source != null && target != null)
            CurrentBaits.Add(new(source, target, _shape));
    }
}
