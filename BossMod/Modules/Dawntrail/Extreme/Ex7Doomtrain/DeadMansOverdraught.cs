namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class DeadMansOverdraught(BossModule module) : Components.GenericStackSpread(module)
{
    private bool? partnerStack;
    public uint Counter;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (partnerStack is bool stack)
        {
            hints.Add($"Stored: {(stack ? "Partner stack" : "Spread")}");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DeadMansOverdraughtSpread or (uint)AID.DeadMansOverdraughtStack)
        {
            partnerStack = spell.Action.ID == (uint)AID.DeadMansOverdraughtStack;
        }
    }

    public void AddStackSpread(double delay)
    {
        if (partnerStack is bool kind)
        {
            var party = Raid.WithoutSlot(true, false, false);
            var len = party.Length;
            var activation = WorldState.FutureTime(delay);
            if (kind)
            {
                for (var i = 0; i < len; ++i)
                {
                    var p = party[i];
                    if (p.Class.IsSupport())
                    {
                        Stacks.Add(new(p, 5f, 2, 2, activation));
                    }
                }
            }
            else
            {
                for (var i = 0; i < len; ++i)
                {
                    Spreads.Add(new(party[i], 5f, activation));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (partnerStack != null && spell.Action.ID is (uint)AID.Plasma or (uint)AID.HyperexplosivePlasma)
        {
            partnerStack = null;
            Stacks.Clear();
            Spreads.Clear();
            ++Counter;
        }
    }
}
