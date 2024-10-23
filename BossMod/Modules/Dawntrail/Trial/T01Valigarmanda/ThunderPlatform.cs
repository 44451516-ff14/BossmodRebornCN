namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class ThunderPlatform(BossModule module) : Components.GenericAOEs(module)
{
    private BitMask requireLevitating;
    private BitMask requireHint;
    private BitMask levitating;
    private DateTime activation;

    private static readonly AOEShapeRect rect = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (requireHint[slot])
        {
            var highlightLevitate = requireLevitating[slot];
            for (var x = 0; x < 2; ++x)
            {
                for (var z = 0; z < 3; ++z)
                {
                    var cellLevitating = ((x ^ z) & 1) != 0;
                    if (cellLevitating != highlightLevitate)
                    {
                        yield return new(rect, Arena.Center + new WDir(-5 - 10 * x, -10 + 10 * z), default, activation);
                        yield return new(rect, Arena.Center + new WDir(+5 + 10 * x, -10 + 10 * z), default, activation);
                    }
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThunderousBreath)
        {
            foreach (var (i, _) in Raid.WithSlot(true))
                requireHint[i] = requireLevitating[i] = true;
            activation = Module.CastFinishAt(spell);
        }
        else if ((AID)spell.Action.ID == AID.BlightedBoltVisual)
        {
            foreach (var (i, _) in Raid.WithSlot(true))
            {
                requireHint[i] = true;
                requireLevitating[i] = false;
            }
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderousBreath or AID.BlightedBolt2)
        {
            requireHint.Reset();
            requireLevitating.Reset();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (requireHint[slot])
            hints.Add(requireLevitating[slot] ? "Levitate" : "Stay on ground", requireLevitating[slot] != levitating[slot]);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Levitate)
            levitating.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Levitate)
            levitating.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class BlightedBolt1(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ThunderPlatform _levitate = module.FindComponent<ThunderPlatform>()!;
    private static readonly AOEShapeCircle circle = new(3);
    private bool active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (active)
            foreach (var p in Raid.WithSlot().Exclude(actor))
            {
                var pos = p.Item2.Position;
                if (_levitate.ActiveAOEs(slot, actor).Any(c => c.Check(pos)))
                    yield return new(circle, pos);
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BlightedBoltVisual)
            active = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BlightedBoltVisual)
            active = false;
    }
}

class BlightedBolt2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BlightedBolt2), 7);