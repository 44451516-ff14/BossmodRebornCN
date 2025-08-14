namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

class Analysis(BossModule module) : BossComponent(module)
{
    public Angle[] SafeDir = new Angle[4];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        Angle? offset = status.ID switch
        {
            (uint)SID.FrontUnseen => new Angle(),
            (uint)SID.BackUnseen => 180f.Degrees(),
            (uint)SID.LeftUnseen => 90f.Degrees(),
            (uint)SID.RightUnseen => -90f.Degrees(),
            _ => null
        };
        if (offset != null && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < SafeDir.Length)
            SafeDir[slot] = offset.Value;
    }
}

class AnalysisRadiance(BossModule module) : Components.GenericGaze(module)
{
    private readonly Analysis? _analysis = module.FindComponent<Analysis>();
    private readonly ArcaneArray? _pulse = module.FindComponent<ArcaneArray>();
    private readonly List<Actor> _globes = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var (nextGlobe, activation) = NextGlobe();
        if (_analysis != null && nextGlobe != null && activation != default)
            return new Eye[1] { new(nextGlobe.Position, activation, _analysis.SafeDir[slot], inverted: true) };
        return [];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NArcaneGlobe or (uint)OID.SArcaneGlobe)
            _globes.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NRadiance1 or (uint)AID.SRadiance1)
        {
            ++NumCasts;
            _globes.Remove(caster);
        }
    }

    private DateTime GlobeActivation(Actor globe) => _pulse?.AOEs.FirstOrDefault(aoe => aoe.Check(globe.Position)).Activation.AddSeconds(0.2d) ?? default;
    private (Actor? actor, DateTime activation) NextGlobe() => _globes.Select(g => (g, GlobeActivation(g))).MinBy(ga => ga.Item2);
}

class TargetedLight(BossModule module) : Components.GenericGaze(module)
{
    public bool Active;
    private readonly Analysis? _analysis = module.FindComponent<Analysis>();
    private readonly Angle[] _rotation = new Angle[4];
    private readonly Angle[] _safeDir = new Angle[4];
    private readonly int[] _rotationCount = new int[4];
    private DateTime _activation;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (Active)
            return new Eye[1] { new(Arena.Center, _activation, _safeDir[slot], inverted: true) };
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_rotation[slot] != default)
            hints.Add($"Rotation: {(_rotation[slot].Rad < 0 ? "CW" : "CCW")}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var count = status.ID switch
        {
            (uint)SID.TimesThreePlayer => -1,
            (uint)SID.TimesFivePlayer => 1,
            _ => 0
        };
        if (count != 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _rotationCount.Length)
            _rotationCount[slot] = count;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var rot = iconID switch
        {
            (uint)IconID.PlayerRotateCW => -90f.Degrees(),
            (uint)IconID.PlayerRotateCCW => 90f.Degrees(),
            _ => default
        };
        if (rot != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _rotation.Length)
        {
            _rotation[slot] = rot * _rotationCount[slot];
            if (_analysis != null)
                _safeDir[slot] = _analysis.SafeDir[slot] + _rotation[slot];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NTargetedLightAOE or (uint)AID.STargetedLightAOE)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NTargetedLightAOE or (uint)AID.STargetedLightAOE)
            ++NumCasts;
    }
}
