namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class Exodus(BossModule module) : Components.RaidwideInstant(module, (uint)AID.Exodus, 7.2f)
{
    private int _numCrystalsDestroyed;

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.CrystalOfLight)
        {
            if (++_numCrystalsDestroyed == 6)
                Activation = WorldState.FutureTime(7.2f);
        }
    }
}
