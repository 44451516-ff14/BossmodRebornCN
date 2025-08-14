namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class SilkenPuff(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 3f, Length = 5f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> puffs = new(7);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = puffs.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = puffs[i];
            aoes[i] = new(capsule, h.Position, h.Rotation);
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1 && actor.OID == (uint)OID.SilkenPuff)
        {
            var cotton = Module.Enemies((uint)OID.Cotton);
            if (cotton.Count == 0)
            {
                puffs.Clear();
            }
            else
            {
                puffs.Add(actor);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = puffs.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenImminent = new Func<WPos, float>[count];
        var forbiddenFuture = new Func<WPos, float>[count];
        var forbiddenFarFuture = new Func<WPos, float>[count];
        for (var i = 0; i < count; ++i)
        {
            var h = puffs[i];
            forbiddenFarFuture[i] = ShapeDistance.Capsule(h.Position, h.Rotation, 50f, Radius);
            forbiddenFuture[i] = ShapeDistance.Capsule(h.Position, h.Rotation, Length, Radius);
            forbiddenImminent[i] = ShapeDistance.Circle(h.Position, Radius);
        }
        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenFarFuture), DateTime.MaxValue);
        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenFuture), WorldState.FutureTime(1.1d));
        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenImminent));
    }
}
