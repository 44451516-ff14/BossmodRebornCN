namespace BossMod.Stormblood.Ultimate.UCOB;

class P5Teraflare(BossModule module) : Components.CastCounter(module, (uint)AID.Teraflare)
{
    public bool DownForTheCountAssigned;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.DownForTheCount)
            DownForTheCountAssigned = true;
    }
}

class P5FlamesOfRebirth(BossModule module) : Components.CastCounter(module, (uint)AID.FlamesOfRebirth);
