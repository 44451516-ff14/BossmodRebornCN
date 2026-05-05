namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

sealed class MeltdownAoE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Meltdown1, new AOEShapeCircle(5f));

sealed class MeltdownSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Meltdown2, 5f);

sealed class MeltdownWait(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ChainsOfCondemnation)
        {
            PlayerStates[Raid.FindSlot(actor.InstanceID)] = new(Requirement.Stay2, WorldState.CurrentTime, finish: status.ExpireAt);
        }
    }
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ChainsOfCondemnation)
        {
            PlayerStates[Raid.FindSlot(actor.InstanceID)] = default;
        }
    }
}
