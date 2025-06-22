namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class HailOfFeathers : Components.SimpleAOEGroups
{
    public HailOfFeathers(BossModule module) : base(module, [(uint)AID.HailOfFeathersAOE1, (uint)AID.HailOfFeathersAOE2, (uint)AID.HailOfFeathersAOE3,
    (uint)AID.HailOfFeathersAOE4, (uint)AID.HailOfFeathersAOE5, (uint)AID.HailOfFeathersAOE6], 20f, 2, 6)
    {
        MaxDangerColor = 1;
    }
}

sealed class FeatherOfRuin(BossModule module) : Components.Adds(module, (uint)OID.FeatherOfRuin);

sealed class BlightedBolt : Components.GenericAOEs
{
    private readonly List<Actor> _targets = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(8);

    public BlightedBolt(BossModule module) : base(module)
    {
        var platform = module.FindComponent<ThunderPlatform>();
        if (platform != null)
        {
            var party = module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var slot = party[i].Item1;
                platform.RequireHint[slot] = true;
                platform.RequireLevitating[slot] = false;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _targets.Count;
        if (count == 6)
            return [];
        var targetsspan = CollectionsMarshal.AsSpan(_targets);
        Span<AOEInstance> aoes = new AOEInstance[count];
        var index = 0;
        var hasDead = false;

        for (var i = 0; i < count; ++i)
        {
            ref readonly var span = ref targetsspan[i];
            if (span.IsDead)
                hasDead = true;
            else
                aoes[index++] = new(_shape, span.Position, default, _activation);
        }
        return hasDead ? aoes[..index] : [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var count = _targets.Count;

        if (count == 6)
        {
            for (var i = 0; i < 6; ++i)
            {
                if (_targets[i].IsDead)
                    return;
            }
            hints.Add("Kill a feather!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlightedBoltAOE)
        {
            _targets.AddIfNonNull(WorldState.Actors.Find(spell.TargetID));
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlightedBoltAOE)
        {
            ++NumCasts;
            var count = _targets.Count;
            var id = spell.TargetID;
            for (var i = 0; i < count; ++i)
            {
                if (_targets[i].InstanceID == id)
                {
                    _targets.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
