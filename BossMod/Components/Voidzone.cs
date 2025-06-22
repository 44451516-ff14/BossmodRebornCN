// TODO: revise and refactor voidzone components;
namespace BossMod.Components;

// voidzone (circle aoe that stays active for some time) centered at each existing object with specified OID, assumed to be persistent voidzone center
// for moving 'voidzones', the hints can mark the area in front of each source as dangerous
// TODO: typically sources are either eventobj's with eventstate != 7 or normal actors that are non dead; other conditions are much rarer
public class Voidzone(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, float moveHintLength = 0) : GenericAOEs(module, default, "GTFO from voidzone!")
{
    public readonly AOEShape Shape = moveHintLength == 0 ? new AOEShapeCircle(radius) : new AOEShapeCapsule(radius, moveHintLength);
    public readonly Func<BossModule, IEnumerable<Actor>> Sources = sources;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        foreach (var source in Sources(Module))
        {
            aoes.Add(new(Shape, WPos.ClampToGrid(source.Position), source.Rotation));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Sources(Module).Any())
            return;
        if (moveHintLength == 0)
        {
            var forbidden = new List<Func<WPos, float>>();
            foreach (var s in Sources(Module))
                forbidden.Add(ShapeDistance.Circle(WPos.ClampToGrid(s.Position), radius));
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden));
        }
        else
        {
            var forbiddenImminent = new List<Func<WPos, float>>();
            var forbiddenFuture = new List<Func<WPos, float>>();
            foreach (var s in Sources(Module))
            {
                forbiddenFuture.Add(ShapeDistance.Capsule(s.Position, s.Rotation, moveHintLength, radius));
                forbiddenImminent.Add(ShapeDistance.Circle(s.Position, radius));
            }
            hints.AddForbiddenZone(ShapeDistance.Union(forbiddenFuture), WorldState.FutureTime(1.5d));
            hints.AddForbiddenZone(ShapeDistance.Union(forbiddenImminent));
        }
    }
}

// voidzone that appears with some delay at cast target
// note that if voidzone is predicted by cast start rather than cast event, we have to account for possibility of cast finishing without event (e.g. if actor dies before cast finish)
// TODO: this has problems when target moves - castevent and spawn position could be quite different
// TODO: this has problems if voidzone never actually spawns after castevent, eg because of phase changes
public class VoidzoneAtCastTarget(BossModule module, float radius, uint aid, Func<BossModule, IEnumerable<Actor>> sources, float castEventToSpawn) : GenericAOEs(module, aid, "GTFO from voidzone!")
{
    public readonly AOEShapeCircle Shape = new(radius);
    public readonly Func<BossModule, IEnumerable<Actor>> Sources = sources;
    public readonly float CastEventToSpawn = castEventToSpawn;
    protected readonly List<(WPos pos, DateTime time, ulong InstanceID)> _predicted = [];

    public bool HaveCasters => _predicted.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        var count = _predicted.Count;
        if (count != 0)
            for (var i = 0; i < count; ++i)
            {
                var p = _predicted[i];
                aoes.Add(new(Shape, p.pos, default, p.time));
            }

        foreach (var z in Sources(Module))
            aoes.Add(new(Shape, WPos.ClampToGrid(z.Position)));

        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void Update()
    {
        if (_predicted.Count != 0)
            foreach (var s in Sources(Module))
            {
                var count = _predicted.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (_predicted[i].pos.InCircle(s.Position, 2f))
                    {
                        _predicted.RemoveAt(i);
                        break;
                    }
                }
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _predicted.Add(new(spell.LocXZ, Module.CastFinishAt(spell, CastEventToSpawn), caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _predicted.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_predicted[i].InstanceID == id)
                {
                    _predicted.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
            _predicted.Add(new(spell.TargetXZ, WorldState.FutureTime(CastEventToSpawn), caster.InstanceID));
    }
}

public class VoidzoneAtCastTargetGroup(BossModule module, float radius, uint[] aids, Func<BossModule, IEnumerable<Actor>> sources, float castEventToSpawn) : VoidzoneAtCastTarget(module, radius, default, sources, castEventToSpawn)
{
    private readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
            {
                _predicted.Add(new(spell.LocXZ, Module.CastFinishAt(spell, CastEventToSpawn), caster.InstanceID));
                return;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // we probably dont need to check for AIDs here since actorID should already be unique to any active spell
        var count = _predicted.Count;
        var id = caster.InstanceID;
        for (var i = 0; i < count; ++i)
        {
            if (_predicted[i].InstanceID == id)
            {
                _predicted.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
            {
                ++NumCasts;
                _predicted.Add(new(spell.TargetXZ, WorldState.FutureTime(CastEventToSpawn), caster.InstanceID));
                return;
            }
        }
    }
}

// these are normal voidzones that could be 'inverted' (e.g. when you need to enter a voidzone at specific time to avoid some mechanic)
// TODO: i'm not sure whether these should be considered actual voidzones (if so, should i merge them with base component? what about cast prediction?) or some completely other type of mechanic (maybe drawing differently)
// TODO: might want to have per-player invertability
public class PersistentInvertibleVoidzone(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, uint aid = default) : CastCounter(module, aid)
{
    public readonly AOEShapeCircle Shape = new(radius);
    public readonly Func<BossModule, IEnumerable<Actor>> Sources = sources;
    public DateTime InvertResolveAt;

    public bool Inverted => InvertResolveAt != default;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var inVoidzone = Sources(Module).Any(s => Shape.Check(actor.Position, s));
        if (Inverted)
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        else if (inVoidzone)
            hints.Add("GTFO from voidzone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shapes = new List<Func<WPos, float>>();

        foreach (var source in Sources(Module))
        {
            var shape = Shape.Distance(WPos.ClampToGrid(source.Position), source.Rotation);
            shapes.Add(shape);
        }
        if (shapes.Count == 0)
            return;

        hints.AddForbiddenZone(Inverted ? ShapeDistance.InvertedUnion(shapes) : ShapeDistance.Union(shapes), InvertResolveAt);
    }

    // TODO: reconsider - draw foreground circles instead?
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var color = Inverted ? Colors.SafeFromAOE : default;
        foreach (var s in Sources(Module))
            Shape.Draw(Arena, s.Position, s.Rotation, color);
    }
}

// invertible voidzone that is inverted when specific spell is being cast; resolved when cast ends
public class PersistentInvertibleVoidzoneByCast(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, uint aid) : PersistentInvertibleVoidzone(module, radius, sources, aid)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            InvertResolveAt = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            InvertResolveAt = default;
    }
}
