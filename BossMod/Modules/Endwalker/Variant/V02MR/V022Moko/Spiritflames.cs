namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

class Spiritflame(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Spiritflame), 6);
class Spiritflames(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 2.4f;
    private readonly HashSet<Actor> _flames = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _flames.Select(a =>
        {
            var position = a.Position;
            var directionOffset = 6 * a.Rotation.ToDirection();
            var pos = position + directionOffset;
            var shapes = new Shape[]
            {
                new Circle(position, Radius),
                new Circle(pos, Radius),
                new RectangleSE(a.Position, pos, Radius)
            };
            return new AOEInstance(new AOEShapeCustom(shapes), Arena.Center);
        });
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Spiritflame)
        {
            if (id == 0x1E46)
                _flames.Add(actor);
            else if (id == 0x1E3C)
                _flames.Remove(actor);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_flames.Count == 0)
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var o in _flames)
            forbidden.Add(ShapeDistance.Capsule(o.Position, o.Rotation, 6, Radius));
        hints.AddForbiddenZone(p => forbidden.Min(f => f(p)));
    }
}
