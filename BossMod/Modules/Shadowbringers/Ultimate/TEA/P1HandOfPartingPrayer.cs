namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: determine when mechanic is selected; determine threshold
[SkipLocalsInit]
sealed class P1HandOfPartingPrayer(BossModule module) : BossComponent(module)
{
    private readonly TEA bossmod = (TEA)module;
    public bool Resolved;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var hint = (bossmod.LiquidHand2?.ModelState.ModelState ?? default) switch
        {
            19 => "Split boss & hand",
            20 => "Stack boss & hand",
            _ => ""
        };
        if (hint.Length > 0)
            hints.Add(hint);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HandOfParting or (uint)AID.HandOfPrayer)
        {
            Resolved = true;
        }
    }
}
