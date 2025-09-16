namespace BossMod.Dawntrail.Unreal.UnSeiryu;

sealed class SerpentAscending(BossModule module) : Components.GenericTowers(module)
{
    private BitMask forbidden;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Tower)
        {
            Towers.Add(new(actor.Position.Quantized(), 3f, activation: WorldState.FutureTime(7.8d), forbiddenSoakers: forbidden));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SerpentsFang or (uint)AID.SerpentsJaws)
        {
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SerpentDescending)
        {
            forbidden.Set(Raid.FindSlot(targetID));
        }
    }
}
