namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: determine when mechanic is selected; determine threshold
sealed class P1HandOfPartingPrayer(TEA module) : BossComponent(module)
{
    public bool Resolved;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var hint = (module.LiquidHand2?.ModelState.ModelState ?? default) switch
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
