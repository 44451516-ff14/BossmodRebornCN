namespace BossMod.Endwalker.Ultimate.DSW1;

// TODO: consider adding invuln hint for tether tank?..
sealed class HolyShieldBash : Components.BaitAwayTethers
{
    public HolyShieldBash(BossModule module) : base(module, new AOEShapeRect(80f, 4f), (uint)TetherID.HolyBladedance, (uint)AID.HolyShieldBash)
    {
        BaiterPriority = PlayerPriority.Danger;
        // TODO: consider selecting specific tank rather than any
        ForbiddenPlayers = Raid.WithSlot(true, true, true).WhereActor(a => a.Role != Role.Tank).Mask();
    }
}

// note: this is not really a 'bait', but component works well enough
sealed class HolyBladedance(BossModule module) : Components.GenericBaitAway(module, (uint)AID.HolyBladedanceAOE)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HolyShieldBash && WorldState.Actors.Find(spell.MainTargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, new AOEShapeCone(16f, 45f.Degrees())));
    }
}

sealed class Heavensblaze(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Heavensblaze, 4f, 7, 7)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // bladedance target shouldn't stack
        if (spell.Action.ID == (uint)AID.HolyShieldBash)
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers.Set(Raid.FindSlot(spell.MainTargetID));
    }
}
