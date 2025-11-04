namespace BossMod.Components;

// generic 'chasing AOE' component - these are AOEs that follow the target for a set amount of casts

[SkipLocalsInit]
public class GenericChasingAOEs(BossModule module, float moveDistance, uint aid = default, string warningText = "GTFO from chasing aoe!") : GenericAOEs(module, aid, warningText)
{
    private readonly float MoveDistance = moveDistance;

    public sealed class Chaser(AOEShape shape, Actor target, WPos prevPos, float moveDist, int numRemaining, DateTime nextActivation, double secondsBetweenActivations)
    {
        public AOEShape Shape = shape;
        public Actor Target = target;
        public WPos PrevPos = prevPos;
        public float MoveDist = moveDist;
        public int NumRemaining = numRemaining;
        public DateTime NextActivation = nextActivation;
        public double SecondsBetweenActivations = secondsBetweenActivations;

        public WPos PredictedPosition()
        {
            var loc = Target.Position;
            var offset = loc - PrevPos;
            var distance = offset.Length();
            var pos = distance > MoveDist ? PrevPos + MoveDist * offset / distance : loc;
            return pos.Quantized();
        }
    }

    public List<Chaser> Chasers = [];

    public bool IsChaserTarget(Actor? actor)
    {
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            if (Chasers[i].Target == actor)
            {
                return true;
            }
        }
        return false;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Chasers.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            var pos = c.PredictedPosition();
            var off = pos - c.PrevPos;
            aoes[i] = new(c.Shape, pos, off.LengthSq() > 0f ? Angle.FromDirection(off) : default, c.NextActivation);
        }
        return aoes;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            if (c.Target == player)
            {
                return PlayerPriority.Interesting;
            }
        }
        return PlayerPriority.Irrelevant;
    }

    // return false if chaser was not found
    public bool Advance(WPos pos, float moveDistance, DateTime currentTime, bool removeWhenFinished = true)
    {
        ++NumCasts;
        Chaser? c = null;
        var minDistSq = float.MaxValue;

        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            var chaser = Chasers[i];
            var predicted = chaser.PredictedPosition();
            var distSq = (predicted - pos).LengthSq();

            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                c = chaser;
            }
        }

        if (c == null)
        {
            return false;
        }

        if (--c.NumRemaining <= 0 && removeWhenFinished)
        {
            Chasers.Remove(c);
        }
        else
        {
            c.PrevPos = pos;
            c.MoveDist = moveDistance;
            c.NextActivation = currentTime.AddSeconds(c.SecondsBetweenActivations);
        }
        return true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Chasers.Count > 0)
        {
            AddForbiddenZones(actor, hints, true);
            AddForbiddenZones(actor, hints, false);
        }
    }

    private void AddForbiddenZones(Actor actor, AIHints hints, bool isTarget)
    {
        // sort of a hack to prevent the AI from getting "stuck" inside the AOE because all paths to safety have equal distance
        var count = Chasers.Count;
        var circle = (AOEShapeCircle)Chasers[0].Shape;
        var radius = circle.Radius;
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            if (c.Target == actor == isTarget)
            {
                var radiusAdj = isTarget ? MoveDistance + radius : radius + 1f;
                var predicted = c.PredictedPosition();
                var position = isTarget ? predicted - radius * actor.Rotation.ToDirection() : predicted;
                hints.AddForbiddenZone(new SDCircle(position, radiusAdj), c.NextActivation);
            }
        }
    }
}

// standard chasing aoe; first cast is long - assume it is baited on the nearest allowed target; successive casts are instant
[SkipLocalsInit]
public class StandardChasingAOEs(BossModule module, AOEShape shape, uint actionFirst, uint actionRest, float moveDistance, double secondsBetweenActivations, int maxCasts, bool resetTargets = false, uint icon = default, double activationDelay = 5.1d) : GenericChasingAOEs(module, moveDistance)
{
    public StandardChasingAOEs(BossModule module, float radius, uint actionFirst, uint actionRest, float moveDistance, double secondsBetweenActivations, int maxCasts, bool resetTargets = false, uint icon = default, double activationDelay = 5.1d)
    : this(module, new AOEShapeCircle(radius), actionFirst, actionRest, moveDistance, secondsBetweenActivations, maxCasts, resetTargets, icon, activationDelay) { }

    public readonly AOEShape Shape = shape;
    public readonly uint ActionFirst = actionFirst;
    public readonly uint ActionRest = actionRest;
    public readonly float MoveDistance = moveDistance;
    public readonly double SecondsBetweenActivations = secondsBetweenActivations;
    public int MaxCasts = maxCasts;
    public readonly uint Icon = icon;
    public readonly double ActivationDelay = activationDelay;
    public readonly bool ResetTargets = resetTargets;
    public readonly List<Actor> Targets = [];
    public BitMask TargetsMask; // to keep track of the icon before mechanic starts for handling custom forbidden zones
    public DateTime Activation;

    public override void Update()
    {
        var count = Chasers.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var c = Chasers[i];
            if ((c.Target.IsDestroyed || c.Target.IsDead) && c.NumRemaining < MaxCasts)
            {
                Chasers.RemoveAt(i);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            Arena.AddLine(c.PrevPos, c.Target.Position);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionFirst)
        {
            var pos = spell.LocXZ;
            Actor? target = null;
            var minDistance = float.MaxValue;

            var count = Targets.Count;
            for (var i = 0; i < count; ++i)
            {
                var t = Targets[i];
                var distanceSq = (t.Position - pos).LengthSq();
                if (distanceSq < minDistance)
                {
                    minDistance = distanceSq;
                    target = t;
                }
            }
            if (target != null)
            {
                Targets.Remove(target);
                TargetsMask.Clear(Raid.FindSlot(target.InstanceID));
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations)); // initial cast does not move anywhere
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == ActionFirst || id == ActionRest)
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

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon)
        {
            Activation = WorldState.FutureTime(ActivationDelay);
            Targets.Add(actor);
            TargetsMask.Set(Raid.FindSlot(targetID));
        }
    }
}
