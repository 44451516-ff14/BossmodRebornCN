namespace BossMod.Components;

// generic 'chasing AOE' component - these are AOEs that follow the target for a set amount of casts
public class GenericChasingAOEs(BossModule module, float moveDistance, uint aid = default, string warningText = "GTFO from chasing aoe!") : GenericAOEs(module, aid, warningText)
{
    private readonly float MoveDistance = moveDistance;

    public sealed class Chaser(AOEShape shape, Actor target, WPos prevPos, float moveDist, int numRemaining, DateTime nextActivation, double secondsBetweenActivations, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    {
        public int? ArenaProjectionLayer = arenaProjectionLayer;
        public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
        public AOEShape Shape = shape;
        public Actor Target = target;
        public WPos PrevPos = prevPos;
        public float MoveDist = moveDist;
        public int NumRemaining = numRemaining;
        public DateTime NextActivation = nextActivation;
        public double SecondsBetweenActivations = secondsBetweenActivations;

        // Null keeps the target-following default; explicit IDs and all-layer mode take precedence.
        public int? ResolveArenaProjectionLayer(BossModule module)
            => module.ResolveTargetArenaProjectionLayer(Target, ArenaProjectionLayer, RestrictToArenaProjectionLayer);

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
            var chaser = Chasers[i];
            if (actor != null && chaser.Target == actor && ArenaProjectionLayerParticipantApplies(actor, chaser.ResolveArenaProjectionLayer(Module), chaser.RestrictToArenaProjectionLayer))
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
        {
            return [];
        }

        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            var pos = c.PredictedPosition();
            var off = pos - c.PrevPos;
            aoes[i] = new(c.Shape, pos, off.LengthSq() > 0f ? Angle.FromDirection(off) : default, c.NextActivation, arenaProjectionLayer: c.ResolveArenaProjectionLayer(Module), restrictToArenaProjectionLayer: c.RestrictToArenaProjectionLayer);
        }
        return aoes;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            if (c.Target == player && ArenaProjectionLayerApplies(pc, c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer)
                && ArenaProjectionLayerParticipantApplies(player, c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer))
            {
                return PlayerPriority.Interesting;
            }
        }
        return PlayerPriority.Irrelevant;
    }

    // Return false if no chaser matched. Pass the event's physical layer to distinguish overlapping sequences.
    public bool Advance(WPos pos, float moveDistance, DateTime currentTime, bool removeWhenFinished = true, int? arenaProjectionLayer = null)
    {
        ++NumCasts;
        Chaser? c = null;
        var minDistSq = float.MaxValue;

        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            var chaser = Chasers[i];
            var layer = chaser.ResolveArenaProjectionLayer(Module);
            if (arenaProjectionLayer.HasValue && layer.HasValue && layer != arenaProjectionLayer)
            {
                continue;
            }
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
        var count = Chasers.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chasers[i];
            if (!ArenaProjectionLayerApplies(actor, c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer))
            {
                continue;
            }

            var predicted = c.PredictedPosition();
            ShapeDistance distance;
            if (c.Shape is AOEShapeCircle circle)
            {
                // Offset the target's circle to avoid equal-distance paths trapping it inside the AOE.
                var isTarget = c.Target == actor && ArenaProjectionLayerParticipantApplies(actor, c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer);
                var radius = circle.Radius;
                var radiusAdj = isTarget ? MoveDistance + radius : radius + 1f;
                var position = isTarget ? predicted - radius * actor.Rotation.ToDirection() : predicted;
                distance = new SDCircle(position, radiusAdj);
            }
            else
            {
                var offset = predicted - c.PrevPos;
                distance = c.Shape.Distance(predicted, offset.LengthSq() > 0f ? Angle.FromDirection(offset) : default);
            }
            hints.AddForbiddenZone(distance, c.NextActivation, arenaProjectionLayer: ArenaProjectionLayerForAI(c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer));
        }
    }
}

// standard chasing aoe; first cast is long - assume it is baited on the nearest allowed target; successive casts are instant
public class StandardChasingAOEs(BossModule module, AOEShape shape, uint actionFirst, uint actionRest, float moveDistance, double secondsBetweenActivations, int maxCasts, bool resetTargets = false, uint icon = default, double activationDelay = 5.1d, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : GenericChasingAOEs(module, moveDistance)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public StandardChasingAOEs(BossModule module, float radius, uint actionFirst, uint actionRest, float moveDistance, double secondsBetweenActivations, int maxCasts, bool resetTargets = false, uint icon = default, double activationDelay = 5.1d, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    : this(module, new AOEShapeCircle(radius), actionFirst, actionRest, moveDistance, secondsBetweenActivations, maxCasts, resetTargets, icon, activationDelay, arenaProjectionLayer, restrictToArenaProjectionLayer) { }

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
            using (Arena.WorldProjectionLayer(c.ResolveArenaProjectionLayer(Module), c.RestrictToArenaProjectionLayer))
            {
                Arena.AddLine(c.PrevPos, c.Target.Position);
            }
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
                if (!ArenaProjectionLayerParticipantApplies(t, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    continue;
                }
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
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations, arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer)); // initial cast does not move anywhere
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == ActionFirst || id == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position.Quantized() : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos, MoveDistance, WorldState.CurrentTime, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
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
