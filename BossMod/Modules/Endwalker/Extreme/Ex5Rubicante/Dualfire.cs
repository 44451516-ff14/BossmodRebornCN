namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

sealed class Dualfire(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DualfireAOE)
{
    private readonly AOEShapeCone _shape = new(60f, 60f.Degrees()); // TODO: verify angle

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Dualfire)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }
}
