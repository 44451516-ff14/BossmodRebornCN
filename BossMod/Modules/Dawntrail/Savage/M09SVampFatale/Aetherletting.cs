namespace BossMod.Dawntrail.Savage.M09SVampFatale;

sealed class AetherlettingCone : Components.SimpleAOEs
{
    public AetherlettingCone(BossModule module) : base(module, (uint)AID.AetherlettingCone, new AOEShapeCone(40f, 22.5f.Degrees()), 4)
    {
        MaxDangerColor = 2;
    }
}
// TODO: draw bait areas for each role (every 45 degrees, edges of cone)
sealed class AetherlettingPuddle(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AetherlettingPuddle, 6f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var len = spreads.Length;

        for (var i = 0; i < len; i++)
        {
            if (spreads[i].Target.InstanceID == actor.InstanceID)
                hints.Add("Bait puddle to edge", false);
        }
    }
}
sealed class AetherlettingCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherlettingCross, new AOEShapeCross(40f, 5f), 2)
{
    private readonly AetherlettingPuddle _puddles = module.FindComponent<AetherlettingPuddle>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_puddles.NumFinishedSpreads < 8)
            return [];
        return base.ActiveAOEs(slot, actor);
    }
}
