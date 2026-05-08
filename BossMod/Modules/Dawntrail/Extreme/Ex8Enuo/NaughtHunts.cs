namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

// May want some additional signaling to the secondary target?  But this seems to work.

sealed class NaughtHunts(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6f), (uint)AID.EndlessChaseCast, (uint)AID.EndlessChaseInstant, 2.9f, 0.7d, 13, icon: (uint)IconID.NaughtHuntTarget, activationDelay: 6d)
{
    private int _tethercount = 0;
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.NaughtHuntFirst)
        {
            var p = WorldState.Actors.Find(tether.Target);
            if (_tethercount == 2)
            {
                Chasers.Clear();
            }
            if (p != null)
            {
                Chasers.Add(new(Shape, p, source.Position, 0, MaxCasts, WorldState.FutureTime(ActivationDelay), SecondsBetweenActivations));
                ++_tethercount;
            }
            if (_tethercount == 4)
            {
                _tethercount = 0;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.NaughtHuntChaser)
        {
            Chasers.Clear(); // This'll happen twice per cycle but who cares?
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionFirst)
        {
            var pos = spell.LocXZ;
            var minDistance = float.MaxValue;

            var count = Targets.Count;
            for (var i = 0; i < count; ++i)
            {
                var t = Targets[i];
                var distanceSq = (t.Position - pos).LengthSq();
                if (distanceSq < minDistance)
                {
                    minDistance = distanceSq;
                }
            }
            // Overriding to remove the 'add' behaviour here.
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell) // Don't count the first action so that we can have the same count between the two.
    {
        if (spell.Action.ID is var id && id == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position.Quantized() : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos, MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetTargets)
            {
                Targets.Clear();
                NumCasts = 0;
            }
        }
    }
}

// If we don't need to try to track who it's hopping to for the purposes of the actual mechanic, why don't we just...

sealed class NaughtHuntsJumps(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _targets = [];
    private readonly List<Actor> _sources = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.NaughtHuntJump)
        {
            var p = WorldState.Actors.Find(tether.Target);
            if (p != null)
            {
                _targets.Add(p);
                _sources.Add(source);
            }
        }
    }
    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.NaughtHuntJump)
        {
            var p = WorldState.Actors.Find(tether.Target);
            if (p != null)
            {
                for (var i = 0; i < _targets.Count; i++)
                {
                    _targets.Clear();
                    _sources.Clear();
                }
            }
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_targets.Count() > 0 && _sources.Count > 0)
        {
            for (var i = 0; i < _targets.Count; i++)
            {
                var s = _sources[i];
                var t = _targets[i];
                Arena.AddLine(s.Position, t.Position);
            }
        }
    }
}
