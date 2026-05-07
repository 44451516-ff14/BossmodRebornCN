namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

//TODO: This needs a way to move the source to the other side of the target.
sealed class NaughtGrowsWildCharge(BossModule module) : Components.InverseWildCharge(module, 3f, 6f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Source = null;
        switch (iconID)
        {
            case (uint)IconID.NaughtGrowsWildChargeSingle:
                Source = actor;
                var party = Raid.WithoutSlot(true, false, false);
                for (var i = 0; i < party.Length; i++)
                {
                    var p = party[i];
                    if (p.InstanceID != targetID)
                    {
                        if (p.Role == Role.Tank)
                        {
                            PlayerRoles[Raid.FindSlot(p.InstanceID)] = PlayerRole.Share;
                        }
                        else
                        {
                            PlayerRoles[Raid.FindSlot(p.InstanceID)] = PlayerRole.ShareNotFirst;
                        }
                    }
                }
                PlayerRoles[Raid.FindSlot(targetID)] = PlayerRole.TargetNotFirst;
                break;
            //NOTE: This won't work if a healer is dead or unavailable.
            //There's probably a better way?
            case (uint)IconID.NaughtGrowsWildChargeDouble:
                Source = actor;
                var partya = Raid.WithoutSlot(true, false, false);
                for (var i = 0; i < partya.Length; i++)
                {
                    var p = partya[i];
                    PlayerRoles[Raid.FindSlot(p.InstanceID)] = p.Role switch
                    {
                        Role.Healer => PlayerRole.TargetNotFirst,
                        Role.Tank => PlayerRole.Share,
                        _ => PlayerRole.ShareNotFirst,
                    };
                }
                break;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GreatReturnToNothing)
        {
            for (var i = 0; i < PlayerRoles.Length; i++)
            {
                PlayerRoles[i] = default;
            }
        }
        if (spell.Action.ID == (uint)AID.ReturnToNothing)
        {
            for (var i = 0; i < PlayerRoles.Length; i++)
            {
                PlayerRoles[i] = default;
            }
        }
    }
}

sealed class NaughtGrowsDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsDonut, new AOEShapeDonut(40f, 60f));

sealed class NaughtGrowsCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsCircle, new AOEShapeCircle(40f));

sealed class NaughtGrowsBossDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsBossDonut, new AOEShapeDonut(6f, 20f));

sealed class NaughtGrowsBossCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsBossCircle, new AOEShapeCircle(12f));
