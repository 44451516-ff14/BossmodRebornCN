namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class GraniteGaol(BossModule module) : BossComponent(module)
{
    public BitMask PendingFetters;
    public DateTime ResolveAt;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => PendingFetters[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Fetters)
            PendingFetters.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RockThrow or (uint)AID.GaolMarkerHealer)
        {
            // this generally happens after tethers, so don't bother doing anything if targets are already known
            var slot = Raid.FindSlot(spell.MainTargetID);
            if (!PendingFetters[slot])
            {
                PendingFetters.Set(slot);
                ResolveAt = WorldState.FutureTime(2.9d);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Gaol)
        {
            // dps -> healer typically
            PendingFetters.Set(Raid.FindSlot(source.InstanceID));
            PendingFetters.Set(Raid.FindSlot(tether.Target));
            ResolveAt = WorldState.FutureTime(2.9d);
        }
    }
}
