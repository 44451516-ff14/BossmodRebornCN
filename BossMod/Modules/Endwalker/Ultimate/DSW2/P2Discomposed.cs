namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P2Discomposed(BossModule module) : BossComponent(module)
{
    public bool Applied;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Discomposed)
            Applied = true;
    }
}
