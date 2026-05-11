namespace BossMod.Dawntrail.Alliance.A30Shantoto;

sealed class AeroDynamics(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private enum Direction : uint
    {
        None,
        West = 90,
        East = 270,
    }

    private class StatusKB(int slot, Direction direction)
    {
        public int Slot => slot;
        public Direction Direction => direction;
    }

    private readonly List<StatusKB> _statuskbs = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.WesterlyWinds)
        {
            var p = Raid.FindSlot(actor.InstanceID);
            if (p != null)
            {
                _statuskbs.Add(new(p, Direction.West));
                //Service.Log($"Adding West: {p}");
            }
        }
        if (status.ID == (uint)SID.EasterlyWinds)
        {
            var p = Raid.FindSlot(actor.InstanceID);
            if (p != null)
            {
                _statuskbs.Add(new(p, Direction.East));
                //Service.Log($"Adding East: {p}");
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is ((uint)SID.EasterlyWinds) or ((uint)SID.WesterlyWinds))
        {
            var p = Raid.FindSlot(actor.InstanceID);
            if (p != null)
            {
                _statuskbs.Clear();
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_statuskbs.Count == 0)
        {
            return [];
        }
        var kb = _statuskbs.Find(k => k.Slot == slot);
        if (kb != null)
        {
            var _knockbacks = new Knockback[1];
            if (kb.Direction != Direction.None)
            {
                _knockbacks[0] = new Knockback(actor.Position, 40f, direction: ((float)kb.Direction).Degrees(), kind: Kind.DirForward);
                return _knockbacks;
            }
        }
        return [];
    }
}

sealed class AeroDynamicsDangerWall(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeRect _rect = new(24f, 1f, 24f);
    private readonly WPos _west = new(-24f, -720f);
    private readonly WPos _east = new(24f, -720f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AeroDynamics)
        {
            _aoe = [new(_rect, _west, activation: WorldState.CurrentTime), new(_rect, _east, activation: WorldState.CurrentTime)];
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AeroDynamics1)
        {
            _aoe = [];
        }
    }
}
