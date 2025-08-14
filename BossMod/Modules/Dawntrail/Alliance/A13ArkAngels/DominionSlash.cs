namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class DominionSlash(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u && actor.OID == (uint)OID.DominionSlashHelper)
        {
            AOEs.Add(new(_shape, actor.Position, default, WorldState.FutureTime(6.5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DivineDominion or (uint)AID.DivineDominionFail)
        {
            ++NumCasts;
            AOEs.RemoveAll(aoe => aoe.Origin == caster.Position);
        }
    }
}
