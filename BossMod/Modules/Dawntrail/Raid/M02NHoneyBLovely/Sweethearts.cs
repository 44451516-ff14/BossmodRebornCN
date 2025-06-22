namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

abstract class Sweethearts(BossModule module, uint oid, uint aid) : Components.GenericAOEs(module)
{
    private const float Radius = 1f, Length = 3f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _hearts = new(34);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _hearts.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _hearts[i];
            aoes[i] = new(capsule, h.Position, h.Rotation);
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == oid && id == 0x11D3)
            _hearts.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == oid)
            _hearts.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == aid)
            _hearts.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _hearts.Count;
        if (count == 0)
            return;
        var forbiddenImminent = new Func<WPos, float>[count + 1];
        var forbiddenFuture = new Func<WPos, float>[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _hearts[i];
            forbiddenFuture[i] = ShapeDistance.Capsule(h.Position, h.Rotation, Length, Radius);
            forbiddenImminent[i] = ShapeDistance.Circle(h.Position, Radius);
        }
        forbiddenImminent[count] = ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius);

        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenFuture), WorldState.FutureTime(1.5d));
        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenImminent));
    }
}

sealed class SweetheartsN(BossModule module) : Sweethearts(module, (uint)OID.Sweetheart, (uint)AID.SweetheartTouch);
