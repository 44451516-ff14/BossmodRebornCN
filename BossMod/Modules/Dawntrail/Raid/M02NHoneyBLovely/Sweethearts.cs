namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

abstract class Sweethearts(BossModule module, uint oid, uint aid) : Components.GenericAOEs(module)
{
    private const int Radius = 1;
    private readonly HashSet<Actor> _hearts = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _hearts.Select(a =>
        {
            var position = a.Position;
            var directionOffset = 3 * a.Rotation.ToDirection();
            var pos = position + directionOffset;
            var shapes = new Shape[]
            {
                new Circle(position, Radius),
                new Circle(pos, Radius),
                new RectangleSE(position, pos, Radius)
            };
            return new AOEInstance(new AOEShapeCustom(shapes), Arena.Center);
        });
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
        foreach (var o in _hearts)
            forbidden.Add(ShapeDistance.Capsule(o.Position, o.Rotation, 3, Radius));
        forbidden.Add(ShapeDistance.Circle(Arena.Center, Module.PrimaryActor.HitboxRadius));
        hints.AddForbiddenZone(p => forbidden.Min(f => f(p)));
    }
}

class SweetheartsN(BossModule module) : Sweethearts(module, (uint)OID.Sweetheart, (uint)AID.SweetheartTouch);
