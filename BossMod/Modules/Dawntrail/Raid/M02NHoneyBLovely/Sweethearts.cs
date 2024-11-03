namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

abstract class Sweethearts(BossModule module, uint oid, uint aid) : Components.GenericAOEs(module)
{
    private const int Radius = 1, Length = 3;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly HashSet<Actor> _hearts = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var h in _hearts)
            yield return new(capsule, h.Position, h.Rotation);
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
        if (_hearts.Count == 0)
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var h in _hearts)
            forbidden.Add(ShapeDistance.Capsule(h.Position, h.Rotation, Length, Radius)); // merging all forbidden zones into one to make pathfinding less demanding
        forbidden.Add(ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius));
        hints.AddForbiddenZone(p => forbidden.Min(f => f(p)));
    }
}

class SweetheartsN(BossModule module) : Sweethearts(module, (uint)OID.Sweetheart, (uint)AID.SweetheartTouch);
