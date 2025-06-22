namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA2Raiden;

sealed class LancingBlowSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.LancingBlow, 10f, 6f)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.StreakLightning)
            Spreads.Clear();
    }
}

sealed class LancingBlowAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    public readonly List<AOEInstance> AOEs = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.StreakLightning)
            AOEs.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LancingBlow)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}
