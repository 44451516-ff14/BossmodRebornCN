namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

sealed class Tornado(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Tornado, 6f);
sealed class MeteorStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MeteorStack, (uint)AID.Meteor, 10f, 5.1f, 4, 24)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Meteor) // for some reason the stack is location targeted and not player targeted
            Stacks.Clear();
    }
}

sealed class MeteorBait(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.MeteorBaitaway, (uint)AID.MeteorImpact, 18f, 8.9f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MeteorImpact)
            Spreads.Clear();
    }
}
sealed class MeteorImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpact, 18f);

sealed class AccelerationBomb(BossModule module) : Components.StayMove(module, 3f)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BaldesionArsenal, GroupID = 639, NameID = 7981, SortOrder = 5)]
public sealed class BA4ProtoOzma(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WDir[] Directions = [new(default, 1f), 120f.Degrees().ToDirection(), (-120f.Degrees()).ToDirection()];
    private static readonly WPos arenaCenter = new(-17.043f, 29.01f);
    private static readonly WPos[] vertices = [new(-41.461f, 23.856f), new(-35.261f, 25.114f), new(-35.017f, 24.387f), new(-30.37f, 27.089f), new(-25.37f, 18.429f),
    new(-30.154f, 15.639f), new(-26.262f, 12.808f), new(-22.582f, 11.188f), new(-18.653f, 10.418f), new(-15.122f, 10.418f),
    new(-11.165f, 11.273f), new(-7.626f, 12.893f), new(-4.56f, 15.181f), new(-0.37f, 10.44f), new(-4.53f, 7.37f),
    new(-9.464f, 5.228f), new(-14.389f, 4.157f), new(-19.697f, 4.157f), new(-24.572f, 5.177f), new(-29.522f, 7.32f),
    new(-33.85f, 10.507f), new(-35.698f, 12.464f), new(-46.154f, 6.429f), new(-51.154f, 15.089f), new(-40.595f, 21.181f)];
    // ozma's arena consists of 3 identical segments, so we rotate the vertices, the segments are slighly off from polygonal donut segments (check arenaslices.jpg for visualisation),
    // so we can't generate them directly if we want pixel perfectness. max error of this should be about 1/1000th of a yalm
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices), new PolygonCustom(WPos.GenerateRotatedVertices(arenaCenter, vertices, 120f)),
    new PolygonCustom(WPos.GenerateRotatedVertices(arenaCenter, vertices, 240f))]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArsenalUrolith));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ArsenalUrolith => 1,
                _ => 0
            };
        }
    }
}
