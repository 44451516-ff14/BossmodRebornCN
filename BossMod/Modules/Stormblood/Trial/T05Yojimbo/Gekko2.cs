namespace BossMod.Stormblood.Trial.T05Yojimbo;

class Gekko2(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(5f);
    private readonly List<Actor> _targets = [];
    private readonly List<AOEInstance> _aoes = [];
    private const double _gekko2Delay = 4.165d;

    private bool _yukikazeActive;
    private DateTime _yukikazeExpiry;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        foreach (var t in _targets)
        {
            if (!t.IsDestroyed)
                _aoes.Add(new(_circle, t.Position, default, WorldState.FutureTime(_gekko2Delay)));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == 144)
            _targets.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Gekko2)
        {
            _targets.Clear();
            _aoes.Clear();
        }
    }

    public override void Update()
    {
        if (Module.Enemies((uint)OID.Embodiment).All(e => e.IsDeadOrDestroyed))
        {
            _targets.Clear();
            _aoes.Clear();
        }

        if (_yukikazeActive && WorldState.CurrentTime >= _yukikazeExpiry)
            _yukikazeActive = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Contains(actor))
            hints.Add("Spread out!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Yukikaze2 && !_yukikazeActive)
        {
            _yukikazeActive = true;
            _yukikazeExpiry = Module.CastFinishAt(spell, 3.5f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor))
        {
            var overlapCount = _targets
                .Where(t => !t.IsDeadOrDestroyed && t.InstanceID != actor.InstanceID)
                .Count(t => _circle.Check(t.Position, aoe.Origin));

            if (_circle.Check(actor.Position, aoe.Origin) && overlapCount >= 1 && !_yukikazeActive)
                hints.AddForbiddenZone(_circle.Distance(aoe.Origin, aoe.Rotation), aoe.Activation);
        }
    }
}
