namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class SoulGrasp(BossModule module) : Components.GenericSharedTankbuster(module, (uint)AID.SoulGraspAOE, 4f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SoulGrasp)
        {
            Source = Module.PrimaryActor;
            Target = actor;
            Activation = WorldState.FutureTime(5.8d);
        }
    }
}
