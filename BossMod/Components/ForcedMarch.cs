namespace BossMod.Components;

// generic component dealing with 'forced march' mechanics
// these mechanics typically feature 'march left/right/forward/backward' debuffs, which rotate player and apply 'forced march' on expiration
// if there are several active march debuffs, we assume they are chained together
public class GenericForcedMarch(BossModule module, float activationLimit = float.MaxValue) : BossComponent(module)
{
    public class PlayerState
    {
        public List<(Angle dir, float duration, DateTime activation)> PendingMoves = [];
        public DateTime ForcedEnd; // zero if forced march not active

        public bool Active(BossModule module) => ForcedEnd > module.WorldState.CurrentTime || PendingMoves.Count > 0;
    }

    public bool OverrideDirection;
    public int NumActiveForcedMarches;
    public readonly Dictionary<ulong, PlayerState> State = []; // key = instance ID
    public float MovementSpeed = 6; // default movement speed, can be overridden if necessary
    public readonly float ActivationLimit = activationLimit; // do not show pending moves that activate later than this limit

    // called to determine whether we need to show hint
    public virtual bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = ForcedMovements(actor);
        var count = movements.Count;
        if (count == 0)
            return;
        var last = movements[count - 1];
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim for safe spot!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var movements = ForcedMovements(pc);
        var count = movements.Count;
        for (var i = 0; i < count; ++i)
        {
            var m = movements[i];
            Arena.ActorProjected(m.from, m.to, m.dir, Colors.Danger);
            Arena.AddLine(m.from, m.to);
        }
    }

    public void AddForcedMovement(Actor player, Angle direction, float duration, DateTime activation)
    {
        var moves = State.GetOrAdd(player.InstanceID).PendingMoves;
        moves.Add((direction, duration, activation));
        moves.SortBy(x => x.activation);
    }

    public bool HasForcedMovements(Actor player) => State.GetValueOrDefault(player.InstanceID)?.Active(Module) ?? false;

    public void ActivateForcedMovement(Actor player, DateTime expiration)
    {
        State.GetOrAdd(player.InstanceID).ForcedEnd = expiration;
        ++NumActiveForcedMarches;
    }

    public void DeactivateForcedMovement(Actor player)
    {
        State.GetOrAdd(player.InstanceID).ForcedEnd = default;
        --NumActiveForcedMarches;
    }

    public List<(WPos from, WPos to, Angle dir)> ForcedMovements(Actor player)
    {
        var state = State.GetValueOrDefault(player.InstanceID);
        if (state == null)
            return [];

        var from = player.Position;
        var dir = !OverrideDirection ? player.Rotation : default;
        var movements = new List<(WPos, WPos, Angle)>();

        if (state.ForcedEnd > WorldState.CurrentTime)
        {
            // note: as soon as player starts marching, he turns to desired direction
            // TODO: would be nice to use non-interpolated rotation here...
            dir = player.Rotation;
            var to = from + MovementSpeed * (float)(state.ForcedEnd - WorldState.CurrentTime).TotalSeconds * dir.ToDirection();
            movements.Add((from, to, dir));
            from = to;
        }

        var limit = ActivationLimit < float.MaxValue ? WorldState.FutureTime(ActivationLimit) : DateTime.MaxValue;
        var count = state.PendingMoves.Count;

        for (var i = 0; i < count; ++i)
        {
            var move = state.PendingMoves[i];
            if (move.activation > limit)
                break;

            dir += move.dir;
            var to = from + MovementSpeed * move.duration * dir.ToDirection();
            movements.Add((from, to, dir));
            from = to;
        }

        return movements;
    }
}

// typical forced march is driven by statuses
public class StatusDrivenForcedMarch(BossModule module, float duration, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight, uint statusForced = 1257, uint statusForcedNPCs = 3629, float activationLimit = float.MaxValue) : GenericForcedMarch(module, activationLimit)
{
    public float Duration = duration;
    public readonly uint[] Statuses = [statusForward, statusLeft, statusBackward, statusRight, statusForced, statusForcedNPCs]; // 5 elements: fwd, left, back, right, forced, forcedNPCs

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 4)
        {
            ActivateForcedMovement(actor, status.ExpireAt);
        }
        else if (statusKind >= 0)
        {
            AddForcedMovement(actor, statusKind * 90f.Degrees(), Duration, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 4)
        {
            DeactivateForcedMovement(actor);
        }
        else if (statusKind >= 0)
        {
            var dir = statusKind * 90f.Degrees();
            var pendingMoves = State.GetOrAdd(actor.InstanceID).PendingMoves;
            var count = pendingMoves.Count;
            for (var i = 0; i < count; ++i)
            {
                if (pendingMoves[i].dir == dir)
                {
                    pendingMoves.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

// action driven forced march
public class ActionDrivenForcedMarch(BossModule module, ActionID aid, float duration, Angle rotation, float actioneffectdelay, uint statusForced = 1257, uint statusForcedNPCs = 3629, float activationLimit = float.MaxValue) : GenericForcedMarch(module, activationLimit)
{
    public readonly float Duration = duration;
    public readonly float Actioneffectdelay = actioneffectdelay;
    public readonly Angle Rotation = rotation;
    public readonly uint StatusForced = statusForced;
    public readonly uint StatusForcedNPCs = statusForcedNPCs;
    public readonly ActionID Aid = aid;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusForced || status.ID == StatusForcedNPCs)
        {
            var pendingMoves = State.GetOrAdd(actor.InstanceID).PendingMoves;
            var count = pendingMoves.Count;
            for (var i = 0; i < count; ++i)
            {
                if (pendingMoves[i].dir == Rotation)
                {
                    pendingMoves.RemoveAt(i);
                    break;
                }
            }
            ActivateForcedMovement(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusForced || status.ID == StatusForcedNPCs)
            DeactivateForcedMovement(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == Aid)
        {
            var party = Module.Raid.WithoutSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
                AddForcedMovement(party[i], Rotation, Duration, Module.CastFinishAt(spell, Actioneffectdelay));
        }
    }
}
