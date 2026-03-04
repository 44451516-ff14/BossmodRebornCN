namespace BossMod.Dawntrail.Savage.M12SLindwurm;

sealed class BurstingGrotesquerieAct1(BossModule module) : Components.UniformStackSpread(module, default, 6f)
{
    private const float ShowThreshold = 6f;
    private readonly List<Actor> _targets = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BurstingGrotesquerie)
            _targets.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BurstingGrotesquerie)
        {
            _targets.Remove(actor);
            Spreads.RemoveAll(s => s.Target == actor);
        }
    }

    public override void Update()
    {
        foreach (var actor in _targets)
        {
            var status = actor.FindStatus((uint)SID.BurstingGrotesquerie);
            if (status == null)
                continue;

            var remaining = (status.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds;
            if (remaining <= ShowThreshold && remaining > 0 && !Spreads.Any(s => s.Target == actor))
                AddSpread(actor, status.Value.ExpireAt);
        }
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DramaticLysis0)
        {
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            _targets.RemoveAll(t => t.InstanceID == spell.MainTargetID);
        }
    }
}

sealed class SharedGrotesquerieAct1(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4)
{
    private const float ShowThreshold = 6f;
    private Actor? _target;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SharedGrotesquerie)
            _target = actor;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SharedGrotesquerie && _target == actor)
        {
            _target = null;
            Stacks.Clear();
        }
    }

    public override void Update()
    {
        if (_target != null)
        {
            var status = _target.FindStatus((uint)SID.SharedGrotesquerie);
            if (status != null)
            {
                var remaining = (status.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds;
                if (remaining <= ShowThreshold && remaining > 0 && !Stacks.Any(s => s.Target == _target))
                    AddStack(_target, status.Value.ExpireAt);
            }
        }
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FourthWallFusion0)
        {
            Stacks.Clear();
            _target = null;
        }
    }
}

sealed class DirectedGrotesquerieAct1(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private const int ForwardDir = 0x40C;
    private const int LeftDir = 0x40F;
    private const float ShowThreshold = 6f;

    private record struct PendingCone(Actor Actor, ushort DirectionExtra);
    private readonly List<PendingCone> _pending = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_Direction)
        {
            var extra = status.Extra;
            if (extra is >= ForwardDir and <= LeftDir)
                _pending.Add(new(actor, extra));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_Direction)
        {
            _pending.RemoveAll(p => p.Actor == actor);
            CurrentBaits.RemoveAll(b => b.Target == actor);
        }
    }

    public override void Update()
    {
        foreach (var pending in _pending)
        {
            var status = pending.Actor.FindStatus((uint)SID.DirectedGrotesquerie);
            if (status == null)
                continue;

            var remaining = (status.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds;
            if (remaining <= ShowThreshold && remaining > 0 && !CurrentBaits.Any(b => b.Target == pending.Actor))
            {
                var rotation = ((pending.DirectionExtra - ForwardDir) * -90f).Degrees();
                AOEShapeCone cone = new(60f, 15f.Degrees(), rotation);
                CurrentBaits.Add(new(pending.Actor, pending.Actor, cone, status.Value.ExpireAt));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HemorrhagicProjection0 or (uint)AID.HemorrhagicProjection1 or (uint)AID.HemorrhagicProjection2)
        {
            CurrentBaits.Clear();
            _pending.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count > 0)
            hints.Add("Bait away!", false);
    }
}
