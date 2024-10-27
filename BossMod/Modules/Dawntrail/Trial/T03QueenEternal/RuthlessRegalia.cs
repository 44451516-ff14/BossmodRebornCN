namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class RuthlessRegalia(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(100, 6);
    private (Actor, DateTime)? _source;
    private readonly List<Actor> _tethered = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
        {
            var s = _source.Value;
            yield return new(rect, s.Item1.Position, s.Item1.Rotation, s.Item2);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.QueenEternal2 && id == 0x11D2)
            _source = new(actor, WorldState.FutureTime(11.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RuthlessRegalia)
            _source = null;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        _tethered.Add(source);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _tethered.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_source == null)
            return;

        if (_tethered.Count > 1 && _tethered.Contains(actor))
        {
            var s = _source.Value.Item1;
            var t = _tethered.FirstOrDefault(x => x != actor);
            hints.AddForbiddenZone(rect with { HalfWidth = 12 }, new(s.Position.X, t!.Position.Z), s.Rotation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
