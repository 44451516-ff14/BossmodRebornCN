namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class Dragonfall(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8)
{
    public int NumCasts;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Dragonfall)
        {
            AddStack(source, WorldState.FutureTime(9.5d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DragonfallAOE)
        {
            ++NumCasts;
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            var forbidden = Raid.WithSlot(false, false, true).WhereActor(a => spell.Targets.Any(t => t.ID == a.InstanceID)).Mask();
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers |= forbidden;
        }
    }
}
