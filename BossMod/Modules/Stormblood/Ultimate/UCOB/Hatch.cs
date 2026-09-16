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
    private readonly Actor?[] _assignedLinks = new Actor?[PartyState.MaxPartySize];

    public const float Radius = 8f;

    public bool Twister;

    public bool IsTarget(int slot) => _targets[slot];

    public void Reset()
    {
        _targets.Reset();
        NumTargetsAssigned = NumCasts = 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
        {
            return;
        }

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

            if (_orbs.Count > 0)
            {
                var waitMove = Math.Max(0f, (float)(_orbs[0].moveStart - WorldState.CurrentTime).TotalSeconds);
                leewaySeconds = waitMove + _orbs.Min(o => actor.DistanceToHitbox(o.orb)) * 0.2f;
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

                        var waitMove = Math.Max(0, (float)(moveStart - WorldState.CurrentTime).TotalSeconds);
                        var toOrb = (closest!.Position - tar.Position).Normalized();

                        hints.AddForbiddenZone(new SDCircle(tar.Position + toOrb, Radius), WorldState.FutureTime(waitMove + tar.DistanceToHitbox(closest) * 0.2f));
                    }
                }
            }

            hints.AddForbiddenZone(linkShape, DateTime.MaxValue);
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
        Array.Fill(_assignedLinks, null);

        // Can't use proximity for assignment because positions are different between clients (if player is moving).
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
            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref targets[i];
                _targets.Clear(Raid.FindSlot(t.ID));
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
