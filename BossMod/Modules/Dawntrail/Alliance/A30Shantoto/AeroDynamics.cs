namespace BossMod.Dawntrail.Alliance.A30Shantoto;

sealed class AeroDynamics(BossModule module) : Components.GenericKnockback(module, stopAtWall: false)
{
    private readonly List<SafeWall> _walls = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {//Build safewalls when we see the phase start, walk 6.5f left and right from the actors, then draw it 24 towards the edge (usually over the edge) to make sure we don't leave any gaps. It's an approximation but seems good enough in practice.
        if (spell.Action.ID == (uint)AID.SuperiorStoneII1)
        {
            var left = caster.Position + caster.Rotation.ToDirection() + 90.Degrees().ToDirection() * 6.5f;
            var leftback = left + caster.Rotation.ToDirection() * 24f;
            _walls.Add(new(left, leftback));
            var right = caster.Position + caster.Rotation.ToDirection() - 90.Degrees().ToDirection() * 6.5f;
            var rightback = right + caster.Rotation.ToDirection() * 24f;
            _walls.Add(new(right, rightback));
        }
        if (spell.Action.ID == (uint)AID.GroundbreakingQuake)
        {
            _walls.Clear();
        }
    }

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
            if (p >= 0)
            {
                _statuskbs.Add(new(p, Direction.West));
                //Service.Log($"Adding West: {p}");
            }
        }
        if (status.ID == (uint)SID.EasterlyWinds)
        {
            var p = Raid.FindSlot(actor.InstanceID);
            if (p >= 0)
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
            if (p >= 0)
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
                _knockbacks[0] = new Knockback(actor.Position, 40f, direction: ((float)kb.Direction).Degrees(), kind: Kind.DirForward, safeWalls: _walls);
                return _knockbacks;
            }
        }
        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_statuskbs.Count != 0)
        {
            var kb = _statuskbs[slot];
            if (!IsImmune(slot, WorldState.CurrentTime))
            {
                var dir = ((float)kb.Direction).Degrees().ToDirection();
                var walls = _walls;
                var len = walls.Count;
                if (len > 0)
                {
                    var swalls = new SafeWall[len];
                    for (var i = 0; i < walls.Count; i++)
                    {
                        swalls[i] = walls[i];
                    }
                    hints.AddForbiddenZone(new SDKnockbackFixedDirectionAgainstSafewalls(dir, swalls, 60f, len), WorldState.CurrentTime);
                }
            }
        }
    }
}
