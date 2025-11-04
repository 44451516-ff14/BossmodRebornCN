namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class ColorClash(BossModule module) : Components.GenericStackSpread(module, raidwideOnResolve: false)
{
    private bool? partnerStack;
    private DateTime activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (partnerStack is bool stack)
            hints.Add($"Stored: {(stack ? "Partner" : "Light party")} stack");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ColorClashVisual1 or (uint)AID.ColorClashVisual2)
        {
            partnerStack = spell.Action.ID == (uint)AID.ColorClashVisual1;
            activation = WorldState.FutureTime(21.1d);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (Stacks.Count == 0 && status.ID == (uint)SID.Stun && partnerStack is bool kind)
        {
            var party = Raid.WithoutSlot(true, false, false);
            var len = party.Length;
            if (kind)
            {
                for (var i = 0; i < len; ++i)
                {
                    var p = party[i];
                    if (p.Class.IsSupport())
                    {
                        Stacks.Add(new(p, 6f, 2, 2, activation));
                    }
                }
            }
            else
            {
                for (var i = 0; i < len; ++i)
                {
                    var p = party[i];
                    if (p.Role == Role.Healer)
                    {
                        Stacks.Add(new(p, 6f, 4, 4, activation));
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ColorClash1 or (uint)AID.ColorClash2)
        {
            partnerStack = null;
            Stacks.Clear();
        }
    }
}
