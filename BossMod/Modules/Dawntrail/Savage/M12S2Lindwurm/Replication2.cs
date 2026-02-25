namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

class Replication2Staging(BossModule module)
    : StagingAssignment<Replication2Role>(module, playerGroupSize: 2, cloneGroupSize: 6, hasBossTether: true)
{
    readonly M12S2LindwurmConfig _config = Service.Config.Get<M12S2LindwurmConfig>();
    readonly List<WurmClone> _sortedClones = [];

    protected override Replication2Role? DeterminePlayerRole(PlayerClone c)
    {
        var eff = _config.GetReplication2();

        var relNorth = eff.RelativeNorth;
        var angleAdj = (c.Position - relNorth.Angle + 180.Degrees()).Normalized();

        return eff.GetRole(Clockspot.GetClosest(angleAdj));
    }
    protected override Replication2Role DetermineCloneRole(WurmClone w)
    {
        if (w.Shape == CloneShape.Boss)
            return Replication2Role.Boss;

        var northAngle = _config.GetReplication2().RelativeNorth.Angle;

        _sortedClones.Clear();

        var clones = CollectionsMarshal.AsSpan(WurmClones);
        for (var i = 0; i < clones.Length; ++i)
        {
            if (clones[i].Shape != CloneShape.Boss)
                _sortedClones.Add(clones[i]);
        }

        var span = CollectionsMarshal.AsSpan(_sortedClones);
        var count = span.Length;
        var center = Module.Center;

        // selection sort descending by adjusted angle
        for (var i = 0; i < count - 1; ++i)
        {
            var maxIdx = i;

            var maxVal = ((span[i].Actor.Position - center).ToAngle() - northAngle + 180.Degrees()).Normalized().Rad;

            for (var j = i + 1; j < count; ++j)
            {
                var val = ((span[j].Actor.Position - center).ToAngle() - northAngle + 180.Degrees()).Normalized().Rad;

                if (val > maxVal)
                {
                    maxVal = val;
                    maxIdx = j;
                }
            }

            if (maxIdx != i)
                (span[i], span[maxIdx]) = (span[maxIdx], span[i]);
        }

        Replication2Role[] roles =
        [
            Replication2Role.Boss,
            Replication2Role.Cone1,
            Replication2Role.Defam1,
            Replication2Role.Stack1
        ];

        for (var i = 0; i < count; ++i)
        {
            ref var c = ref span[i];
            var idx = (int)c.Shape!;
            var role = roles[idx]++;
            if (c.Actor == w.Actor)
                return role;
        }

        return Replication2Role.None;
    }
}

class Replication2FirefallSplash : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2FirefallSplash(BossModule module)
        : base(module, 0, 5, includeDeadTargets: true)
    {
        var tethers = module.FindComponent<Replication2Staging>()!;
        var activation = WorldState.FutureTime(5.9f);

        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var worm = tethers.WurmsBySlot[slot];
            if (worm != null && worm.AssignedRole == Replication2Role.Boss)
                AddSpread(player, activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FirefallSplash)
        {
            ++NumCasts;

            // remove only spreads with radius 5 (defensive-safe)
            for (var i = Spreads.Count - 1; i >= 0; --i)
            {
                if (Spreads[i].Radius == 5)
                    Spreads.RemoveAt(i);
            }
        }
    }
}

class Replication2ScaldingWaves : Components.GenericBaitProximity
{
    private Actor? _source;
    private WPos? _sourcePos;
    private readonly DateTime _activation;

    public BitMask Targets;

    public static readonly AOEShapeCone Shape = new(50, 5.Degrees());

    public Replication2ScaldingWaves(BossModule module)
        : base(module)
    {
        var staging = module.FindComponent<Replication2Staging>()!;
        _activation = WorldState.FutureTime(6.2f);

        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var clone = staging.WurmsBySlot[slot];
            if (clone != null && clone.AssignedRole == Replication2Role.Boss)
            {
                _source = player;
                break;
            }
        }
    }

    // jump target should not be warned
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == _source)
            return;

        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor == _source)
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = (AID)spell.Action.ID;

        if (aid == AID.FirefallSplash)
        {
            _sourcePos = spell.TargetXZ;
            return;
        }

        if (aid == AID.ScaldingWaves)
        {
            _source = null;
            _sourcePos = null;
            ++NumCasts;

            // determine closest target to cone centerline
            var party = Raid.WithSlot();
            var len = party.Length;

            var bestSlot = -1;
            var bestDiff = float.MaxValue;

            for (var i = 0; i < len; ++i)
            {
                ref var entry = ref party[i];
                var slot = entry.Item1;
                var player = entry.Item2;

                if (Targets[slot])
                    continue;

                var angle = (player.Position - caster.Position).ToAngle();
                var diff = MathF.Abs(angle.Rad - spell.Rotation.Rad);

                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestSlot = slot;
                }
            }

            if (bestSlot >= 0)
                Targets.Set(bestSlot);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var pos = _sourcePos ?? _source?.Position;
        if (pos == null)
            return;

        // select 4 nearest players excluding source
        CurrentBaits.Add(new Bait(
            pos.Value,
            Shape,
            _activation,
            numTargets: 4,
            nearest: true
        ));
    }
}

class Replication2ManaBurst : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2ManaBurst(BossModule module)
        : base(module, 0, 20, includeDeadTargets: true)
    {
        var staging = module.FindComponent<Replication2Staging>()!;

        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var clone = staging.WurmsBySlot[slot];
            if (clone == null || clone.AssignedRole.IsDefam)
                AddSpread(player, WorldState.FutureTime(8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.ManaBurstAOE)
            return;

        // find closest spread target to explosion location
        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var count = spreads.Length;

        var bestIndex = -1;
        var bestDist = float.MaxValue;
        var targetPos = spell.TargetXZ;

        for (var i = 0; i < count; ++i)
        {
            ref var spread = ref spreads[i];
            var dist = (spread.Target.Position - targetPos).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
            Spreads.RemoveAt(bestIndex);

        ++NumCasts;
    }
}
class Replication2HeavySlam : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2HeavySlam(BossModule module)
        : base(module, 5, 0, minStackSize: 3, includeDeadTargets: true)
    {
        var staging = module.FindComponent<Replication2Staging>()!;

        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var clone = staging.WurmsBySlot[slot];
            if (clone != null && clone.AssignedRole.IsStack)
                AddStack(player, WorldState.FutureTime(7));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.HeavySlam)
            return;

        var stacks = CollectionsMarshal.AsSpan(Stacks);
        var count = stacks.Length;

        var bestIndex = -1;
        var bestDist = float.MaxValue;
        var targetPos = spell.TargetXZ;

        for (var i = 0; i < count; ++i)
        {
            ref var stack = ref stacks[i];
            var dist = (stack.Target.Position - targetPos).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
            Stacks.RemoveAt(bestIndex);

        ++NumCasts;
    }
}

sealed class Replication2HemorrhagicProjection : Components.GenericBaitAway
{
    private readonly DateTime _activation;
    private BitMask _targets;

    public static readonly AOEShapeCone Shape = new(50, 25.Degrees());

    public Replication2HemorrhagicProjection(BossModule module)
        : base(module, centerAtTarget: true)
    {
        var staging = module.FindComponent<Replication2Staging>()!;
        _activation = WorldState.FutureTime(8.8f);

        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;

            var clone = staging.WurmsBySlot[slot];
            if (clone != null && clone.AssignedRole.IsCone)
                _targets.Set(slot);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var party = Raid.WithSlot();
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            if (!_targets[i])
                continue;

            var target = Raid[i];
            if (target == null)
                continue;

            CurrentBaits.Add(new Bait(
                Module.PrimaryActor,
                target,
                Shape,
                _activation,
                forbidden: default,
                customRotation: target.Rotation,
                maxCasts: 1
            ));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var count = baits.Length;

        var isTarget = false;
        for (var i = 0; i < count; ++i)
        {
            if (baits[i].Target == actor)
            {
                isTarget = true;
                break;
            }
        }

        if (!isTarget)
            return;

        var forbidden = new ArcList(actor.Position, 60);

        var raid = Raid.WithoutSlot();
        var rlen = raid.Length;

        for (var i = 0; i < rlen; ++i)
        {
            var ally = raid[i];
            if (ally == actor)
                continue;

            var angle = actor.AngleTo(ally);
            forbidden.ForbidInfiniteCone(actor.Position, angle, 28.Degrees());
        }

        var segments = forbidden.Forbidden.Segments;
        var slen = segments.Count;

        for (var i = 0; i < slen; ++i)
        {
            var (from, to) = segments[i];
            var center = (to + from) * 0.5f;
            var width = (to - from) * 0.5f;

            hints.ForbiddenDirections.Add((center.Radians(), width.Radians(), _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.HemorrhagicProjection)
            return;

        ++NumCasts;
        _targets.Reset();
    }
}
class Replication2ReenactmentOrder(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor Understudy, CloneShape Shape, int Order)> Replay = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.Reenactment)
            return;

        Replay.Clear();

        var staging = Module.FindComponent<Replication2Staging>()!;
        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var assignment = staging.PlayersBySlot[slot];
            if (assignment == null)
            {
                ReportError($"No clone assigned for raid member {player}");
                continue;
            }

            var clone = staging.WurmsBySlot[slot];
            var shape = clone?.Shape ?? CloneShape.Spread;
            var order = assignment.SpawnOrder;

            Replay.Add((assignment.Actor, shape, order));
        }

        var span = CollectionsMarshal.AsSpan(Replay);
        var count = span.Length;

        for (var i = 1; i < count; ++i)
        {
            var key = span[i];
            var j = i - 1;

            while (j >= 0 && span[j].Order > key.Order)
            {
                span[j + 1] = span[j];
                --j;
            }

            span[j + 1] = key;
        }
    }
}

class Replication2ReenactmentAOEs(BossModule module)
    : Components.GenericAOEs(module)
{
    // We store nullable AOEInstance to preserve timing "holes"
    private readonly List<AOEInstance?> _predicted = [];

    public const float DefamDelay = 1.2f;
    public const float ConeDelay = 1.9f;
    public const float StackDelay = 1.6f;

    // Reusable buffer (max 4 visible at once)
    private readonly AOEInstance[] _active = new AOEInstance[4];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _predicted.Count;
        if (count == 0)
            return [];

        var span = CollectionsMarshal.AsSpan(_predicted);

        var visible = 0;
        var limit = Math.Min(4, count);

        for (var i = 0; i < limit; ++i)
        {
            var p = span[i];
            if (p == null)
                continue;

            var inst = p.Value;

            var risky = i < 2;
            inst.Risky = risky;
            inst.Color = risky ? Colors.Danger : Colors.AOE;

            _active[visible++] = inst;
        }

        return _active.AsSpan(0, visible);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.Reenactment)
            return;

        var mechStart = Module.CastFinishAt(spell, 8.2f);
        var order = Module.FindComponent<Replication2ReenactmentOrder>()!.Replay;

        var replaySpan = CollectionsMarshal.AsSpan(order);
        var len = replaySpan.Length;

        for (var i = 0; i < len; ++i)
        {
            var (actor, mechShape, spawnOrder) = replaySpan[i];
            var initialCast = mechStart.AddSeconds(4 * spawnOrder);

            AOEShape? shape = null;
            float delay = 0f;

            switch (mechShape)
            {
                case CloneShape.Boss:
                    shape = new AOEShapeCircle(5);
                    break;

                case CloneShape.Spread:
                    shape = new AOEShapeCircle(20);
                    delay = DefamDelay;
                    break;

                case CloneShape.Cone:
                    shape = Replication2HemorrhagicProjection.Shape;
                    delay = ConeDelay;
                    break;
            }

            if (shape != null)
            {
                var activation = initialCast.AddSeconds(delay);
                _predicted.Add(new AOEInstance(
                    shape,
                    actor.Position,
                    actor.Rotation,
                    activation
                ));
            }
            else
            {
                _predicted.Add(null);
            }
        }

        // Sort by activation time without LINQ
        _predicted.Sort(static (a, b) =>
        {
            if (a == null && b == null)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;
            return a.Value.Activation.CompareTo(b.Value.Activation);
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FirefallSplashReplay:
            case AID.ManaBurstReplay:
            case AID.HemorrhagicProjectionReplay:
                if (_predicted.Count > 0)
                {
                    _predicted.RemoveAt(0);
                    ++NumCasts;
                }
                break;

            case AID.HeavySlamReplay:
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
        }
    }
}
sealed class Replication2ReenactmentTowers(BossModule module)
    : Components.GenericTowers(module, (uint)AID.HeavySlamReplay)
{
    private readonly List<(WPos? Position, DateTime Activation)> _predicted = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.Reenactment)
            return;

        var mechStart = Module.CastFinishAt(spell, 8.2f);
        var replay = Module.FindComponent<Replication2ReenactmentOrder>()!.Replay;

        var span = CollectionsMarshal.AsSpan(replay);
        var len = span.Length;

        for (var i = 0; i < len; ++i)
        {
            var (actor, shape, order) = span[i];
            var initialCast = mechStart.AddSeconds(4 * order);

            if (shape == CloneShape.Stack)
            {
                _predicted.Add((
                    actor.Position,
                    initialCast.AddSeconds(Replication2ReenactmentAOEs.StackDelay)
                ));
            }
            else
            {
                _predicted.Add((null, initialCast));
            }
        }

        // Sort by activation without LINQ
        _predicted.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }

    public override void Update()
    {
        Towers.Clear();

        var count = _predicted.Count;
        if (count == 0)
            return;

        var span = CollectionsMarshal.AsSpan(_predicted);
        var limit = Math.Min(4, count);

        for (var i = 0; i < limit; ++i)
        {
            var (pos, activation) = span[i];
            if (pos == null)
                continue;

            // First two = safe color (0), later = danger color
            var color = i >= 2 ? Colors.Danger : default;

            Towers.Add(new(
                pos.Value,
                5,          // radius
                1,          // min players
                4,          // max players
                default,    // forbidden mask
                activation,
                color
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FirefallSplashReplay:
            case AID.ManaBurstReplay:
            case AID.HemorrhagicProjectionReplay:
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;

            case AID.HeavySlamReplay:
                if (_predicted.Count > 0)
                {
                    _predicted.RemoveAt(0);
                    ++NumCasts;
                }
                break;
        }
    }
}

class Replication2ReenactmentScaldingWaves(BossModule module)
    : Components.GenericBaitAway(module)
{
    private readonly List<(Actor? Source, DateTime Activation)> _predicted = [];
    private BitMask _targets;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.Reenactment)
            return;

        _targets = Module.FindComponent<Replication2ScaldingWaves>()!.Targets;

        var mechStart = Module.CastFinishAt(spell, 8.2f);
        var replay = Module.FindComponent<Replication2ReenactmentOrder>()!.Replay;

        var span = CollectionsMarshal.AsSpan(replay);
        var len = span.Length;

        for (var i = 0; i < len; ++i)
        {
            var (actor, shape, order) = span[i];
            var initialCast = mechStart.AddSeconds(4 * order);

            if (shape == CloneShape.Boss)
            {
                _predicted.Add((
                    actor,
                    initialCast.AddSeconds(Replication2ReenactmentAOEs.ConeDelay)
                ));
            }
            else
            {
                _predicted.Add((null, initialCast));
            }
        }

        // Sort by activation
        _predicted.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var count = _predicted.Count;
        if (count == 0)
            return;

        var span = CollectionsMarshal.AsSpan(_predicted);
        var limit = Math.Min(2, count);

        for (var i = 0; i < limit; ++i)
        {
            var (source, activation) = span[i];
            if (source == null)
                continue;

            var raid = Raid.WithSlot();
            var raidLen = raid.Length;

            for (var j = 0; j < raidLen; ++j)
            {
                ref var entry = ref raid[j];
                var slot = entry.Item1;

                if (!_targets[slot])
                    continue;

                CurrentBaits.Add(new Bait(
                    source,
                    entry.Item2,
                    Replication2ScaldingWaves.Shape,
                    activation
                ));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ManaBurstReplay:
            case AID.HemorrhagicProjectionReplay:
            case AID.HeavySlamReplay:
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;

            case AID.ScaldingWavesReplay:
                _predicted.Clear();
                ++NumCasts;
                break;
        }
    }
}
class Replication2TimelessSpite(BossModule module)
    : Components.UniformStackSpread(module, 6, 0, maxStackSize: 2)
{
    private DateTime _activation;
    private bool _far;
    public int NumCasts;

    public override void Update()
    {
        Stacks.Clear();

        if (_activation == default)
            return;

        var raid = Raid.WithoutSlot();
        var len = raid.Length;

        if (len == 0)
            return;

        var bossPos = Module.PrimaryActor.Position;

        // we need the 2 nearest or 2 farthest players
        // do a partial selection without full sort

        int first = -1;
        int second = -1;

        float best1 = _far ? float.MinValue : float.MaxValue;
        float best2 = _far ? float.MinValue : float.MaxValue;

        for (var i = 0; i < len; ++i)
        {
            var actor = raid[i];
            var dist = (actor.Position - bossPos).LengthSq();

            if (_far)
            {
                // looking for largest distances
                if (dist > best1)
                {
                    best2 = best1;
                    second = first;

                    best1 = dist;
                    first = i;
                }
                else if (dist > best2)
                {
                    best2 = dist;
                    second = i;
                }
            }
            else
            {
                // looking for smallest distances
                if (dist < best1)
                {
                    best2 = best1;
                    second = first;

                    best1 = dist;
                    first = i;
                }
                else if (dist < best2)
                {
                    best2 = dist;
                    second = i;
                }
            }
        }

        if (first >= 0)
            AddStack(raid[first], _activation);

        if (second >= 0)
            AddStack(raid[second], _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NetherwrathNear:
                _activation = Module.CastFinishAt(spell, 1.2f);
                _far = false;
                break;

            case AID.NetherwrathFar:
                _activation = Module.CastFinishAt(spell, 1.2f);
                _far = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TimelessSpite)
        {
            ++NumCasts;
            _activation = default;
        }
    }
}