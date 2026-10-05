namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class Hatch(BossModule module) : Components.CastCounter(module, (uint)AID.Hatch)
{
    public bool Active = true;
    public override bool KeepOnPhaseChange => true;
    public int NumNeurolinkSpawns;
    public int NumTargetsAssigned;
    private readonly List<(Actor orb, DateTime moveStart)> _orbs = [];
    private readonly List<Actor> _neurolinks = module.Enemies((uint)OID.Neurolink);
    private BitMask _targets;
    private BitMask _tenstrikeUntargeted;
    private readonly Actor?[] _assignedLinks = new Actor?[PartyState.MaxPartySize];
    private readonly List<InterceptState> _intercepts = [];
    private readonly StringBuilder _targetsBuilder = new(128);
    private ulong _cachedTargetsMask;
    private string? _cachedTargetsHint;

    sealed class InterceptState(int first, int second)
    {
        public int First = first;
        public int Second = second;
        public Actor? Link = null;
        public int NumHits = 0;
    }

    public const float Radius = 8f;

    public bool Twister;

    public bool IsTarget(int slot) => _targets[slot];

    public void Reset()
    {
        _targets.Reset();
        NumTargetsAssigned = NumCasts = 0;
    }

    private string GetTargetsHint()
    {
        var targets = _targets;

        // Reuse the string when the mask hasn't changed.
        if (_cachedTargetsHint is not null && _cachedTargetsMask == targets.Raw)
        {
            return _cachedTargetsHint;
        }

        var raid = Raid.WithSlot(false, true, true);
        var builder = _targetsBuilder;
        builder.Clear();
        builder.Append("Targets: ");
        var first = true;
        var len = raid.Length;

        for (var i = 0; i < len; ++i)
        {
            var p = raid[i];

            if (!_targets[p.Item1])
            {
                continue;
            }

            if (!first)
            {
                builder.Append(", ");
            }

            builder.Append(p.Item2.Name);
            first = false;
        }

        _cachedTargetsHint = builder.ToString();
        _cachedTargetsMask = targets.Raw;

        return _cachedTargetsHint;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
        {
            return;
        }

        hints.Add(GetTargetsHint(), false);

        var inNeurolink = false;
        var count = _neurolinks.Count;
        var pos = actor.Position;
        for (var i = 0; i < count; ++i)
        {
            if (pos.InCircle(_neurolinks[i].Position, 2f))
            {
                inNeurolink = true;
                break;
            }
        }
        if (_targets[slot])
        {
            hints.Add("Go to neurolink!", !inNeurolink);
        }
        else if (inNeurolink)
        {
            hints.Add("GTFO from neurolink!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.IsTargetable)
        {
            var twintania = hints.FindEnemy(Module.PrimaryActor)!;
            switch (_neurolinks.Count)
            {
                case 0:
                    twintania.DesiredPosition = new(0f, -8f);
                    twintania.DesiredRotation = 180f.Degrees();
                    break;
                case 1:
                    twintania.DesiredPosition = new(-8f, 5f);
                    twintania.DesiredRotation = -60f.Degrees();

                    // TODO: find a melee spot that's easy to get twin out of
                    //if (_numHatches == 0)
                    //    twintania.DesiredPosition = new(-7, -8);
                    break;
                case 2:
                    twintania.DesiredPosition = new(8f, 5f);
                    twintania.DesiredRotation = 60f.Degrees();
                    break;
            }
        }

        if (!Active || _neurolinks.Count == 0)
        {
            return;
        }

        var countN = _neurolinks.Count;
        var shapeDistances = new ShapeDistance[countN];
        for (var i = 0; i < countN; ++i)
        {
            shapeDistances[i] = new SDCircle(_neurolinks[i].Position, 2f);
        }
        var linkShape = new SDUnion(shapeDistances);

        if (_targets[slot])
        {
            // tiebreaker
            var myLink = _assignedLinks[slot];
            if (myLink == null)
            {
                return;
            }

            var leewaySeconds = 10f;
            Actor? closestOrb = null;

            if (_orbs.Count > 0)
            {
                var orbs = CollectionsMarshal.AsSpan(_orbs);
                var waitMove = Math.Max(0f, (float)(orbs[0].moveStart - WorldState.CurrentTime).TotalSeconds);
                closestOrb = orbs[0].orb;
                var dist = actor.DistanceToHitbox(closestOrb);
                var count = _orbs.Count;
                for (var i = 1; i < count; ++i)
                {
                    var orb = orbs[i].orb;
                    var orbDist = actor.DistanceToHitbox(orb);

                    if (orbDist < dist)
                    {
                        closestOrb = orb;
                        dist = orbDist;
                    }
                }
                leewaySeconds = waitMove + dist * 0.2f;
            }

            if (closestOrb is { LastFrameMovement: var m } && m != default)
            {
                var src = closestOrb.Position;
                var dir = m.Normalized() * 1000f;
                hints.GoalZones.Add(p => p.InRect(src, dir, 1f) ? 1f : 0f);
            }

            hints.GoalZones.Add(AIHints.GoalSingleTarget(myLink.Position, 5f, 0.5f));

            if (Twister)
            {
                hints.AddForbiddenZone(new SDInvertedDonutSector(myLink.Position, 3f, 5f, Module.PrimaryActor.AngleTo(myLink), 90f.Degrees()), WorldState.FutureTime(leewaySeconds));
            }
            else
            {
                hints.AddForbiddenZone(new SDInvertedCircle(myLink.Position, 2f), WorldState.FutureTime(leewaySeconds));
            }
        }
        else if (_tenstrikeUntargeted[slot])
        {
            // non participating players should gtfo to give allies space to preposition
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 19f));
        }
        else
        {
            var countO = _orbs.Count;
            if (countO > 0)
            {
                var raid = Raid.WithSlot(false, true, true);
                var lenR = raid.Length;
                for (var i = 0; i < countO; ++i)
                {
                    var orb = _orbs[i].orb;
                    hints.AddForbiddenZone(new SDCircle(orb.Position, 2f));
                    if (orb.LastFrameMovement == default)
                    {
                        for (var j = 0; j < lenR; ++j)
                        {
                            var p = raid[i];
                            if (_targets[p.Item1])
                            {
                                hints.AddForbiddenZone(new SDCapsule(orb.Position, orb.AngleTo(p.Item2), 6f, 2f), WorldState.FutureTime(2d));
                            }
                        }
                    }
                    else
                    {
                        var last = orb.LastFrameMovementVec4;
                        hints.AddForbiddenZone(new SDCapsule(orb.Position, new Angle(ref last), 6f, 2f), WorldState.FutureTime(2d));
                    }
                }

                for (var i = 0; i < lenR; ++i)
                {
                    var p = raid[i];
                    if (_targets[p.Item1])
                    {
                        var tar = p.Item2;

                        var found = false;
                        var minDistSq = float.MaxValue;
                        Actor? closest = null;
                        var moveStart = DateTime.MaxValue;

                        for (var j = 0; j < countO; ++j)
                        {
                            var o = _orbs[j];
                            var distSq = (o.orb.Position - tar.Position).LengthSq();
                            if (distSq < minDistSq)
                            {
                                minDistSq = distSq;
                                closest = o.orb;
                                moveStart = o.moveStart;
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            continue;
                        }

                        var waitMove = Math.Max(0f, (float)(moveStart - WorldState.CurrentTime).TotalSeconds);
                        var toOrb = (closest!.Position - tar.Position).Normalized();

                        // radius = 8 tested extensively to work fine in P1, but first baiters get clipped by it in P3...i don't know       
                        hints.AddForbiddenZone(new SDCircle(tar.Position + toOrb, Radius + 1f), WorldState.FutureTime(waitMove + tar.DistanceToHitbox(closest) * 0.2f));
                    }
                }
            }

            hints.AddForbiddenZone(linkShape, DateTime.MaxValue);
        }

        var countI = _intercepts.Count;
        InterceptState? i0 = null;
        InterceptState? i1 = null;
        for (var i = 0; i < countI; ++i)
        {
            var intercept = _intercepts[i];
            if (i1 == null && intercept.NumHits == 1 && intercept.First == slot)
            {
                i1 = intercept;
            }
            else if (i0 == null && intercept.NumHits == 0 && intercept.Second == slot)
            {
                i0 = intercept;
            }
        }
        if (i1 is { Link: { } li })
        {
            var linkDir = (li.Position - Arena.Center).Normalized();

            // first hatch player should dodge directly backwards to wall
            hints.AddForbiddenZone(new SDInvertedRect(Arena.Center + linkDir * 15f, Arena.Center + linkDir * 22f, 1f));
        }

        if (i0 is { Link: { } link })
        {
            var linkPos = link.Position;
            var linkDir = (linkPos - Arena.Center).Normalized();
            var adj = linkDir.OrthoR() * 100f;
            hints.GoalZones.Add(p => p.InRect(linkPos, adj, 2f) ? 10f : 0f);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Active && _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
        {
            return;
        }

        var count = _orbs.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircle(_orbs[i].orb.Position, 2f, Colors.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active)
        {
            return;
        }

        var count = _neurolinks.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(_neurolinks[i].Position, 2f, _targets[pcSlot] ? Colors.Safe : default);
        }

        var raid = Raid.WithSlot(false, true, true);
        var len = raid.Length;
        var countO = _orbs.Count;
        for (var i = 0; i < len; ++i)
        {
            var p = raid[i];
            if (!_targets[p.Item1])
            {
                continue;
            }
            Actor? closestOrb = null;
            var closestDistSq = float.MaxValue;
            var pos = p.Item2.Position;
            for (var j = 0; j < countO; ++j)
            {
                var o = _orbs[j];
                var distSq = (o.orb.Position - pos).LengthSq();
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestOrb = o.orb;
                }
            }

            if (closestOrb != null)
            {
                var off = (closestOrb.Position - pos).Normalized();
                Arena.ZoneCircleOutline(pos + off, Radius);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Generate)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
            ++NumTargetsAssigned;
            if (_targets.NumSetBits() == _neurolinks.Count)
            {
                AssignLinks();
            }
        }
    }

    private void AssignLinks()
    {
        if (_targets.NumSetBits() == 3)
        {
            AssignTenstrike();
        }
        else
        {
            AssignP1();
        }
    }

    // can't use proximity for assignment during p1 because players are moving
    private void AssignP1()
    {
        Array.Fill(_assignedLinks, null);

        SortHelpers.SortActorsByID(_neurolinks);

        var targets = new List<(int Slot, Actor Player)>();

        var raid = Raid.WithSlot(false, true, true);
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = raid[i];
            var slot = p.Item1;
            if (_targets[slot])
            {
                targets.Add((slot, p.Item2));
            }
        }

        SortHelpers.SortActorsSlotByID(targets);
        var count = targets.Count;
        for (var i = 0; i < count; ++i)
        {
            _assignedLinks[targets[i].Slot] = _neurolinks[i];
        }
    }

    private void AssignTenstrike()
    {
        if (_tenstrikeUntargeted.Any())
        {
            return;
        }

        Array.Fill(_assignedLinks, null);

        List<(int slot, Actor player)> set1 = [];
        List<(int slot, Actor player)> set2 = [];

        var raid = Raid.WithSlot(true, true, true);

        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var (slot, player) = raid[i];
            (_targets[slot] ? set1 : set2).Add((slot, player));
        }

        var count = _neurolinks.Count;
        for (var i = 0; i < count; ++i)
        {
            var link = _neurolinks[i];

            var countS1 = set1.Count;
            var countS2 = set2.Count;
            if (countS1 == 0 || countS2 == 0)
            {
                ReportError("Each neurolink requires one player from each set.");
                continue;
            }

            // Find the closest targeted player
            var closestIndex = 0;
            var closestDistance = set1[0].player.DistanceToPoint(link.Position);

            for (var j = 1; j < countS1; ++j)
            {
                var distance = set1[j].player.DistanceToPoint(link.Position);
                if (distance < closestDistance)
                {
                    closestIndex = j;
                    closestDistance = distance;
                }
            }

            var closest = set1[closestIndex];
            set1.RemoveAt(closestIndex);

            // Find the closest untargeted player
            var closestFriendIndex = 0;
            var closestFriendDistance = set2[0].player.DistanceToPoint(link.Position);

            for (var j = 1; j < countS2; ++j)
            {
                var distance = set2[j].player.DistanceToPoint(link.Position);
                if (distance < closestFriendDistance)
                {
                    closestFriendIndex = j;
                    closestFriendDistance = distance;
                }
            }

            var closestFriend = set2[closestFriendIndex];
            set2.RemoveAt(closestFriendIndex);

            _assignedLinks[closest.slot] = _assignedLinks[closestFriend.slot] = link;
            _intercepts.Add(new(closest.slot, closestFriend.slot) { Link = link });
        }

        var countset2 = set2.Count;
        for (var i = 0; i < countset2; ++i)
        {
            _tenstrikeUntargeted.Set(set2[i].slot);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var count = _orbs.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_orbs[i].orb == caster)
                {
                    _orbs.RemoveAt(i);
                    return;
                }
            }
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;
            var countI = _intercepts.Count;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref targets[i];
                if (Raid.FindSlot(t.ID) is var slot && slot >= 0)
                {
                    _targets.Clear(slot);
                    for (var j = 0; j < countI; ++j)
                    {
                        var intercept = _intercepts[j];
                        if (intercept.First == slot)
                        {
                            ++intercept.NumHits;
                            _targets.Set(intercept.Second);
                        }
                        else if (intercept.Second == slot)
                        {
                            ++intercept.NumHits;
                        }
                    }
                }
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Twintania && id == 0x94)
        {
            ++NumNeurolinkSpawns;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Oviform)
        {
            _orbs.Add((actor, WorldState.FutureTime(4d)));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Oviform)
        {
            var count = _orbs.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_orbs[i].orb == actor)
                {
                    _orbs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
