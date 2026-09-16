namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3QuickmarchTrio(UCOB module) : BossComponent(module)
{
    public WPos RelativeNorth;
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];
    private readonly WPos[] _spreadSpots = new WPos[PartyState.MaxPartySize];
    private readonly Actor _bahamut = module.BahamutPrime()!;
    private readonly UCOBConfig _config = Service.Config.Get<UCOBConfig>();
    private readonly P3BahamutPositioning _positioning = module.FindComponent<P3BahamutPositioning>()!;
    private P3Twister? _twister;

    public bool Active => RelativeNorth != default;
    private DateTime _diveAt;
    private bool _divesDone;
    private bool _earthshakersDone;
    public bool PuddleDodgeHint;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            Arena.ActorInsideBounds(RelativeNorth, (Arena.Center - RelativeNorth).ToAngle(), Colors.Object);
        }
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
        {
            Arena.ZoneCircleOutline(safespot, 1f, Colors.Safe);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (_divesDone)
        {
            return;
        }
        if (actor.OID == (uint)OID.BahamutPrime && id == 0x1E43)
        {
            RelativeNorth = actor.Position;
            _diveAt = WorldState.FutureTime(5.1f);
            var dirToNorth = Angle.FromDirection(actor.Position - Module.Center);
            var assignments = _config.P3QuickmarchTrioAssignments.Resolve(Raid);
            var count = assignments.Count;
            var center = Arena.Center;
            for (var i = 0; i < count; ++i)
            {
                var p = assignments[i];
                var left = p.group < 4;
                var order = p.group & 3;
                var offSafe = (60f + order * 20f).Degrees();

                var dirSafe = dirToNorth + (left ? offSafe : -offSafe);
                _safeSpots[p.slot] = center + 20f * dirSafe.ToDirection();
                var offSpread = (90f + (order - 1.5f) * 35f).Degrees();
                var dirSpread = dirToNorth + (left ? offSpread : -offSpread);
                _spreadSpots[p.slot] = center + 12f * dirSpread.ToDirection();
            }

            if (_positioning is { } bp)
            {
                bp.DesiredRotation = dirToNorth;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // drop twister as close to edge as possible
        if (_safeSpots[slot] != default)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(_safeSpots[slot], 1f), _diveAt);
        }

        _twister ??= Module.FindComponent<P3Twister>();
        if (_twister is { Predicted: true } or { Active: true } && _spreadSpots[slot] != default)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(_spreadSpots[slot], 1f));
        }

        if (_earthshakersDone && actor.InstanceID != _bahamut.TargetID)
        {
            hints.AddForbiddenZone(new SDHalfPlane(Arena.Center, (Arena.Center - RelativeNorth).Normalized()), DateTime.MaxValue);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MegaflarePuddle)
        {
            Array.Fill(_spreadSpots, default);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.LunarDive)
        {
            _divesDone = true;
            _diveAt = default;
            Array.Fill(_safeSpots, default);
        }
        else if (id == (uint)AID.EarthShakerAOE)
        {
            _earthshakersDone = true;
        }
    }
}

sealed class P3TwistingDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwistingDive, new AOEShapeRect(63.96f, 4f));
sealed class P3LunarDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LunarDive, new AOEShapeRect(62.55f, 4f));
sealed class P3MegaflareDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegaflareDive, new AOEShapeRect(64.2f, 6f));
sealed class P3Twister(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, (uint)AID.TwistingDive, 1.4f, predictBeforeCastEnd: 0.8f)
{
    public bool Predicted => PredictedPositions.Count > 0;
}

sealed class P3MegaflareSpreadStack : Components.UniformStackSpread
{
    private BitMask _stackTargets;
    private readonly P3QuickmarchTrio _trio;

    public P3MegaflareSpreadStack(BossModule module) : base(module, 5f, 5f, 3, 3)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(2.6d));
        _trio = module.FindComponent<P3QuickmarchTrio>()!;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            _stackTargets.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MegaflareSpread:
                Spreads.Clear();
                if (Stacks.Count > 0)
                {
                    return;
                }
                var stackTarget = Raid.WithSlot(false, true, true).IncludedInMask(_stackTargets).FirstOrDefault().Item2; // random target
                if (stackTarget != null)
                {
                    AddStack(stackTarget, WorldState.FutureTime(4d), ~_stackTargets);
                }
                break;
            case (uint)AID.MegaflareStack:
                Stacks.Clear();
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count > 0)
        {
            ref var stack = ref Stacks.Ref(0);
            var isTarget = stack.Target == actor || !stack.ForbiddenPlayers[slot];

            if (isTarget)
            {
                var safeDir = (_trio.RelativeNorth - Arena.Center).ToAngle() + 135f.Degrees();
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + safeDir.ToDirection() * 5f, 2));
            }

            // everyone else should avoid the stack, it will kill healers and do ~50% to tanks
            else if (actor.Class.IsSupport())
            {
                var raid = Raid.WithSlot(false, true, true);
                var act = stack.Activation;
                var len = raid.Length;
                for (var i = 0; i < len; ++i)
                {
                    var p = raid[i];
                    if (stack.ForbiddenPlayers[p.Item1])
                    {
                        continue;
                    }
                    hints.AddForbiddenZone(new SDCircle(p.Item2.Position, StackRadius), act);
                }
            }

            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class P3MegaflarePuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegaflarePuddle, 6f);
sealed class P3TempestWing(BossModule module) : Components.TankbusterTether(module, (uint)AID.TempestWing, (uint)TetherID.TempestWing, 5f, 7.3d)
{
    // determines whether non-tank players should try to avoid tanks
    // disabled by default, since we assume the tethers spawn on random players, meaning non-tanks should plant and let tanks grab tethers from them; should be set to true after some appropriate amount of delay
    public bool EnableRaidHints;

    public DateTime TetherDeadline;

    // tethers disappear more than a full second before the AOE goes off
    private BitMask _lastTethered;
    private BitMask Targets => _tetheredPlayers.Any() ? _tetheredPlayers : _lastTethered;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
        {
            return;
        }

        if (actor.Role == Role.Tank)
        {
            if (!_tetheredPlayers[slot])
            {
                hints.Add("Grab the tether!");
                return;
            }
            if (!EnableRaidHints)
            {
                return;
            }
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            var pos = actor.Position;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p == actor)
                {
                    continue;
                }

                if (pos.InCircle(p.Position, 5f))
                {
                    hints.Add("GTFO from raid!");
                    return;
                }
            }
        }
        else if (EnableRaidHints)
        {
            if (Targets[slot])
            {
                hints.Add("Hit by tankbuster");
            }
            var party = Raid.WithSlot(false, true, true);
            var len = party.Length;
            var pos = actor.Position;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (Targets[p.Item1])
                {
                    if (pos.InCircle(p.Item2.Position.Quantized(), 5f))
                    {
                        hints.Add("GTFO from tankbuster!");
                        break;
                    }
                }
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);

        if (tether.ID == (uint)TetherID.TempestWing)
        {
            if (TetherDeadline == default)
            {
                TetherDeadline = activation.AddSeconds(-1.4d);
            }
            _lastTethered.Reset();
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        // if tether count is >2 it means that this tether just got passed and the gain event fired before the lose event
        if (tether.ID == (uint)TetherID.TempestWing && _tethers.Count <= 2)
        {
            _lastTethered.Set(Raid.FindSlot(source.InstanceID));
        }
        base.OnUntethered(source, tether);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (Targets[playerSlot])
        {
            return PlayerPriority.Danger;
        }

        // for tanks, other players are interesting, since tank should not clip them
        if (pc.Role == Role.Tank)
        {
            return PlayerPriority.Normal;
        }

        // for non-tanks, other players are irrelevant
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        using var projection = Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer);
        // show tethered targets with circles
        var count = _tethers.Count;
        var isnottethered = !_tetheredPlayers[pcSlot];
        var istank = pc.Role == Role.Tank;
        for (var i = 0; i < count; ++i)
        {
            var side = _tethers[i];
            var color = istank && side.Player.Role != Role.Tank && isnottethered ? Colors.Safe : default;

            // thick yellow line if pov player should pass this tether; thick green line is only used in encounters with specific tether priority (in this case it's random)
            var thickness = side.Player == pc && !istank ? 2f : 1f;

            var playerPos = side.Player.Position;
            var enemyPos = side.Enemy.Position;
            Arena.AddLine(enemyPos, playerPos, side.Player.Role == Role.Tank ? Colors.Safe : default, thickness);

            Arena.ZoneCircleOutline(side.Player.Position, 5f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
        {
            Actor? tetherSource = null;

            var count = _tethers.Count;
            for (var i = 0; i < count; ++i)
            {
                var tether = _tethers[i];
                if (tether.Player == actor)
                {
                    tetherSource = tether.Enemy;
                    break;
                }
            }

            if (tetherSource != null)
            {
                var raid = Raid.WithSlot(false, true, true);
                var len = raid.Length;
                for (var i = 0; i < len; ++i)
                {
                    var p = raid[i];
                    var ally = p.Item2;
                    if (ally == actor)
                    {
                        continue;
                    }

                    hints.AddForbiddenZone(new SDCircle(ally.Position, 5f), activation);

                    // if we walk behind another player, it will pass the tether to them the cone width doesn't really matter here; as long as the pixels are blocked,
                    // pathfinder won't try to go through them
                    if (ally.Role != Role.Tank && !_tetheredPlayers[p.Item1])
                    {
                        hints.AddForbiddenZone(new SDDonutSector(tetherSource.Position, (ally.Position - tetherSource.Position).Length(), 60f, tetherSource.AngleTo(ally), 2f.Degrees()));
                    }
                }
            }
            else if (Targets[slot])
            {
                var raid = Raid.WithoutSlot(false, true, true);
                var len = raid.Length;
                for (var i = 0; i < len; ++i)
                {
                    var p = raid[i];
                    if (p == actor)
                    {
                        continue;
                    }
                    hints.AddForbiddenZone(new SDCircle(p.Position, 5f), activation);
                }
            }
            else
            {
                List<ShapeDistance> goal = [];

                for (var i = 0; i < count; ++i)
                {
                    var side = _tethers[i];
                    if (side.Player.Role != Role.Tank)
                    {
                        goal.Add(new SDPrecisePosition(WPos.Lerp(side.Player.Position, side.Enemy.Position, 0.5f), new(0f, 1f), 0.5f, actor.Position, 0.1f));
                    }
                }

                if (goal.Count > 0)
                {
                    hints.AddForbiddenZone(new SDIntersection([.. goal]), TetherDeadline);
                }
            }
        }
        else
        {
            // non tanks need to avoid stealing tethers
            var count = _tethers.Count;
            for (var i = 0; i < count; ++i)
            {
                var side = _tethers[i];
                // don't steal from tank
                if (side.Player.Role == Role.Tank)
                {
                    hints.AddForbiddenZone(new SDRect(side.Enemy.Position, side.Player.Position, 1f), TetherDeadline);
                }

                // don't move too close to source, or tank will be unable to grab tether
                if (side.Player == actor)
                {
                    hints.AddForbiddenZone(new SDCircle(side.Enemy.Position, 2f));
                }
            }
            if (EnableRaidHints)
            {
                var raid = Raid.WithSlot(false, true, true);
                var len = raid.Length;
                for (var i = 0; i < len; ++i)
                {
                    var p = raid[i];
                    if (Targets[p.Item1])
                    {
                        hints.AddForbiddenZone(new SDCircle(p.Item2.Position, 5f), activation);
                    }
                }
            }
        }

        if (Targets.Any())
        {
            hints.AddPredictedDamage(Targets, activation, AIHints.PredictedDamageType.Tankbuster);
        }
    }
}
