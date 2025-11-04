namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: assign positions?
[SkipLocalsInit]
sealed class P3Inception4Cleaves(BossModule module) : Components.GenericBaitAway(module, (uint)AID.AlphaSwordP3)
{
    private readonly AOEShapeCone _shape = new(30f, 45f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        var source = ((TEA)Module).CruiseChaser();
        if (source != null)
            CurrentBaits.AddRange(Raid.WithoutSlot(false, true, true).SortedByRange(source.Position).Take(3).Select(t => new Bait(source, t, _shape)));
    }
}
