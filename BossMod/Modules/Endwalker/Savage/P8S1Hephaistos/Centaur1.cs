namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class RearingRampageSecond(BossModule module) : Components.CastCounter(module, (uint)AID.RearingRampageSecond);
class RearingRampageLast(BossModule module) : Components.CastCounter(module, (uint)AID.RearingRampageLast);

class UpliftStompDead : Components.UniformStackSpread
{
    public int NumUplifts;
    public int NumStomps;
    public int[] OrderPerSlot = new int[PartyState.MaxPartySize]; // 0 means not yet known

    public UpliftStompDead(BossModule module) : base(module, 6, 6, 2, 2, true)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true));
    }

    public override void Update()
    {
        if (Spreads.Count == 0)
        {
            Stacks.Clear();
            if (Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position) is var target && target != null)
                AddStack(target);
        }
        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (OrderPerSlot[slot] > 0)
        {
            hints.Add($"Bait order: {OrderPerSlot[slot]}", false);
        }

        if (Spreads.Count > 0)
        {
            // default implementation is fine during uplifts
            base.AddHints(slot, actor, hints);
        }
        else
        {
            // custom hints for baiting stomps
            var isBaiting = Stacks.Any(s => actor.Position.InCircle(s.Target.Position, s.Radius));
            var shouldBait = OrderPerSlot[slot] == NumStomps + 1;
            hints.Add(shouldBait ? "Bait jump!" : "Avoid jump!", isBaiting != shouldBait);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Uplift:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                var slot = Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                {
                    OrderPerSlot[slot] = 4 - Spreads.Count / 2;
                }
                ++NumUplifts;
                break;
            case AID.StompDeadAOE:
                ++NumStomps;
                break;
        }
    }
}
