namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

sealed class NaughtGrowsWildCharge(BossModule module) : Components.GenericWildCharge(module, 3f, (uint)AID.GreatReturnToNothing)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.NaughtGrowsWildChargeSingle:
                Source = Module.PrimaryActor;
                PlayerRoles[Raid.FindSlot(targetID)] = PlayerRole.TargetNotFirst;
                break;
        }
    }
}
