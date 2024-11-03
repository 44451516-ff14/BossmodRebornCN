namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

class Spiritflame(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Spiritflame), 6);
class Spiritflames(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 2.4f;
    private const int Length = 6;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly HashSet<Actor> _flames = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var f in _flames)
            yield return new(capsule, f.Position, f.Rotation);
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
        foreach (var f in _flames)
            forbidden.Add(ShapeDistance.Capsule(f.Position, f.Rotation, Length, Radius));
        hints.AddForbiddenZone(p => forbidden.Min(f => f(p)));
    }
}
