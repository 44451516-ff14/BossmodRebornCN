namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME2SwordDancer;

[SkipLocalsInit]
sealed class Steelsbreath(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Steelsforge _steelsforge = module.FindComponent<Steelsforge>()!;
    private readonly List<Knockback> _knockbacks = [];
    private readonly double _startDelay = 12.8d;
    private int _steelsforgeCount = 0;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var count = _knockbacks.Count;
        if (count == 0)
        {
            return [];
        }

        var kbs = CollectionsMarshal.AsSpan(_knockbacks);
        return kbs[..1];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.LeapingLift && status.Extra is 0x47B or 0x495)
        {
            // 12.8s between 1st status and resolve, status 1-1.5s between each
            // 0 -> 1 - ~12.8s after status
            // 1 -> 2 - ~2.57s (steelsforge 1.5s after)
            // 2 -> 3 - ~4.93s
            // 3 -> 4 - ~2.49s (steelsforge 1.5s after)
            // 4 -> 5 - ~5s
            // so roughly 2.5s, 5s if after steelsforge
            // need to adjust timing but too short better than too long
            _steelsforgeCount += status.Extra == 0x495 ? 1 : 0;
            var count = _knockbacks.Count;
            var delay = status.Extra == 0x47B ? 0d : 2.5d * _steelsforgeCount;
            var act = WorldState.FutureTime(_startDelay + delay + 1.1d * count);
            _knockbacks.Add(new(actor.Position, 26f, act, actorID: actor.InstanceID));
        }
        base.OnStatusGain(actor, ref status);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_knockbacks.Count != 0 && spell.Action.ID == (uint)AID.Steelsbreath)
        {
            ++NumCasts;
            _knockbacks.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // need to avoid Steelsforge if it's coming up, but close enough to position into next knockback
        // knockback pending actually clears as expected in EX unlike normal
        // verify if enough time to move away from RushLong after KB, otherwise need to move for that too

        var kbs = CollectionsMarshal.AsSpan(_knockbacks);
        var count = kbs.Length;
        if (count != 0)
        {
            ref var kb = ref kbs[0];
            var act = kb.Activation;
            var isImmune = IsImmune(slot, act);
            if (!isImmune)
            {
                if (count == 1)
                {
                    // slightly bigger to avoid sus knockback
                    hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, kb.Origin, 28f, 24f), act);
                }
                else
                {
                    ref var kb1 = ref kbs[1];
                    // try to land near but outside Steelsforge to position for next knockback
                    if (_steelsforge.GetSteelsforgeId() == kb1.ActorID)
                    {
                        hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginIntoDonut(Arena.Center, kb.Origin, 26f, 24f, kb1.Origin, 15f, 18f), act);
                    }
                    else
                    {
                        hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginIntoCircle(Arena.Center, kb.Origin, 28f, 24f, kb1.Origin, 7f), act);
                    }
                }
            }
        }
    }
    /*
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_knockbacks.Count != 0)
        {
            var kbs = CollectionsMarshal.AsSpan(_knockbacks);
            var count = kbs.Length;
            for (var i = 0; i < count; i++)
            {
                Arena.TextWorld(kbs[i].Origin, $"{i + 1}", Colors.CardinalS, 12);
            }
        }
        base.DrawArenaForeground(pcSlot, pc);
    }
    */
    private sealed class SDKnockbackInCircleAwayFromOriginIntoDonut(WPos Center, WPos Origin, float Distance, float Radius, WPos DonutOrigin, float InnerRadius, float OuterRadius) : ShapeDistance
    {
        private readonly WPos center = Center;
        private readonly WPos origin = Origin;
        private readonly float radius = Radius;
        private readonly float distance = Distance;
        private readonly WPos donutOrigin = DonutOrigin;
        private readonly float innerRadius = InnerRadius;
        private readonly float outerRadius = OuterRadius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Contains(in WPos p)
        {
            var projected = p + distance * (p - origin).Normalized();
            if (!projected.InCircle(center, radius))
            {
                return true;
            }
            return !projected.InDonut(donutOrigin, innerRadius, outerRadius);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
    }
}

[SkipLocalsInit]
sealed class Steelsforge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly double _startDelay = 12.8d;
    private readonly AOEShapeCircle _circle = new(13f);
    private int _counter = 0;

    public ulong GetSteelsforgeId() => _aoes.Count == 0 ? 0 : _aoes[0].ActorID;

    public ReadOnlySpan<AOEInstance> ActiveCasters() => CollectionsMarshal.AsSpan(_aoes);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(_aoes)[..1];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.LeapingLift && status.Extra is 0x47B or 0x495)
        {
            if (status.Extra == 0x495)
            {
                var act = WorldState.FutureTime(_startDelay + 1.1d * _counter);
                _aoes.Add(new(_circle, actor.Position, default, act, actorID: actor.InstanceID));
            }
            ++_counter;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Steelsforge)
        {
            _aoes.RemoveAt(0);
        }
    }
    /*
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_aoes.Count != 0)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var count = aoes.Length;
            for (var i = 0; i < count; i++)
            {
                Arena.ZoneCircle(aoes[i].Origin, 2f, Colors.CardinalN);
            }
        }
    }
    */
}
