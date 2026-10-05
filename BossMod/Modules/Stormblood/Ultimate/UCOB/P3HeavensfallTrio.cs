namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3HeavensfallPreposition(UCOB module) : Components.CastCounter(module, (uint)AID.HeavensfallTrio)
{
    // heavensfall cast start to dive bait
    private readonly DateTime _diveAt = module.WorldState.FutureTime(8.5d);
    private readonly Actor _bahamut = module.BahamutPrime()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_bahamut.IsTargetable)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 5f), _diveAt.AddSeconds(-1d));
        }
        else
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 1f), _diveAt);
        }
    }
}

sealed class P3HeavensfallTrio(BossModule module) : BossComponent(module)
{
    private Actor? _nael;
    private Actor? _twin;
    private Actor? _baha;
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];
    private readonly UCOBConfig _config = Service.Config.Get<UCOBConfig>();

    public bool Active => _nael != null;
    private bool _divesActive;
    private bool _divesDone;
    private bool _puddlesActive;
    private P3Twister? _twister;

    private readonly Angle[] _offsetsNaelCenter = [10f.Degrees(), 80f.Degrees(), 100f.Degrees(), 170f.Degrees()];
    private readonly Angle[] _offsetsNaelSide = [60f.Degrees(), 80f.Degrees(), 100f.Degrees(), 120f.Degrees()];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_nael, Colors.Object, true, true);
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
        {
            Arena.ZoneCircleOutline(safespot, 1f, Colors.Safe);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID is var oid && oid == (uint)OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
            InitIfReady();
        }
        else if (oid == (uint)OID.Twintania && id == 0x1E44)
        {
            _twin = actor;
            InitIfReady();
        }
        else if (oid == (uint)OID.BahamutPrime && id == 0x1E43)
        {
            _baha = actor;
            InitIfReady();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_divesActive)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(_safeSpots[slot], 1f));
        }
        if (!_puddlesActive)
        {
            if (_twister == null)
            {
                var comp = Module.FindComponent<P3Twister>();
                if (comp != null)
                {
                    _twister = comp;
                }
                else
                {
                    return;
                }
            }
            if (_twister.Predicted || _twister.Active)
            {
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 8f));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.MegaflareDive)
        {
            _divesActive = true;
        }
        else if (id == (uint)AID.MegaflarePuddle)
        {
            _puddlesActive = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TwistingDive)
        {
            _divesActive = false;
            _divesDone = true;
            Array.Fill(_safeSpots, default);
        }
    }

    private void InitIfReady()
    {
        if (_nael == null || _twin == null || _baha == null || _divesDone)
        {
            return;
        }

        var dirToNael = Angle.FromDirection(_nael.Position - Arena.Center);
        var dirToTwin = Angle.FromDirection(_twin.Position - Arena.Center);
        var dirToBaha = Angle.FromDirection(_baha.Position - Arena.Center);

        var twinRel = (dirToTwin - dirToNael).Normalized();
        var bahaRel = (dirToBaha - dirToNael).Normalized();
        var (offsetSymmetry, offsets) = twinRel.Rad * bahaRel.Rad < 0f // twintania & bahamut are on different sides => nael is in center
            ? (default, _offsetsNaelCenter)
            : ((twinRel + bahaRel) * 0.5f, _offsetsNaelSide);
        var dirSymmetry = dirToNael + offsetSymmetry;
        var assignments = _config.P3QuickmarchTrioAssignments.Resolve(Raid);
        var count = assignments.Count;
        for (var i = 0; i < count; ++i)
        {
            var p = assignments[i];
            var left = p.group < 4;
            var order = p.group & 3;
            var offset = offsets[order];
            var dir = dirSymmetry + (left ? offset : -offset);
            _safeSpots[p.slot] = Arena.Center + 20f * dir.ToDirection();
        }
    }
}

sealed class P3Heavensfall(BossModule module) : Heavensfall(module)
{
    // no hints, handled by towers component
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}

sealed class P3HeavensfallTowers(UCOB module) : Components.CastTowers(module, (uint)AID.MegaflareTower, 3f)
{
    private readonly UCOBConfig _config = Service.Config.Get<UCOBConfig>();
    private readonly Actor _nael = module.Nael()!;
    bool _knockbackDone;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action.ID == (uint)AID.Heavensfall)
        {
            _knockbackDone = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == WatchedAction && Towers.Count == 8)
        {
            var center = Arena.Center;
            var dirToNael = Angle.FromDirection(_nael.Position - center);

            var towers = CollectionsMarshal.AsSpan(Towers);
            RefSort.Sort(towers, new TowerComparer(dirToNael, center));

            var assignments = _config.P3HeavensfallTrioTowers.Resolve(Raid);
            var count = assignments.Count;
            for (var i = 0; i < count; ++i)
            {
                var p = assignments[i];
                towers[p.group].ForbiddenSoakers = new(~(1ul << p.slot));
            }
        }
    }

    // order towers from Nael's position CW
    private readonly struct TowerComparer(Angle reference, WPos center) : IRefComparer<Tower>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Tower a, ref Tower b)
        {
            var aAngle = Angle.FromDirection(a.Position - center);
            var bAngle = Angle.FromDirection(b.Position - center);

            var aDist = (reference - aAngle).Normalized().Deg;
            var bDist = (reference - bAngle).Normalized().Deg;

            // towers are ~22.5 degrees apart; tolerate slight offset around reference
            if (aDist < -5f)
            {
                aDist += 360f;
            }

            if (bDist < -5f)
            {
                bDist += 360f;
            }

            return aDist.CompareTo(bDist);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_knockbackDone)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var index = -1;
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;
        for (var i = 0; i < len; ++i)
        {
            if (!towers[i].ForbiddenSoakers[slot])
            {
                index = i;
                break;
            }
        }
        if (index >= 0)
        {
            var center = Arena.Center;
            ref var myTower = ref towers[index];
            var dir = myTower.Position - Arena.Center;
            var mySpot = Arena.Center + dir.Normalized() * 9f;

            hints.GoalZones.Add(AIHints.GoalProximity(mySpot, 10f, 5f));

            hints.AddForbiddenZone(new SDPrecisePosition(mySpot, new(0f, 1f), 0.5f, actor.Position, 0.1f), myTower.Activation);
        }
    }
}

sealed class P3HeavensfallFireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, (uint)AID.Fireball, 4f, 5.3d, 8, 8)
{
    int _numHypernovas;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count > 0)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 2f), Stacks.Ref(0).Activation);
            return;
        }

        // last hypernova to stack going off is 6 seconds
        switch (_numHypernovas)
        {
            case 3:
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 8f), DateTime.MaxValue);
                break;
            case 2:
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 15f), DateTime.MaxValue);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action.ID == (uint)AID.Hypernova)
        {
            ++_numHypernovas;
        }
    }
}

sealed class P3ThermionicBurst(BossModule module) : ThermionicBurst(module)
{
    private readonly Angle[] _startingSlice = new Angle[PartyState.MaxPartySize];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action.ID == (uint)AID.MegaflareTower)
        {
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;
            var angle = (spell.TargetXZ - Arena.Center).ToAngle() - 11.25f.Degrees();
            for (var i = 0; i < len; ++i)
            {
                if (Raid.FindSlot(targets[i].ID) is var slot && slot >= 0)
                {
                    _startingSlice[slot] = angle;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var numAoes = 0;
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Activation > WorldState.CurrentTime || NumCasts < 2)
            {
                hints.AddForbiddenZone(aoe.ShapeDistance!, aoe.Activation);
                if (++numAoes >= 2)
                {
                    break;
                }
            }
        }

        if (NumCasts < 16 && _startingSlice[slot] != default)
        {
            hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, _startingSlice[slot], 40f, -2f, 1.5f));
        }
    }
}

sealed class P3HeavensfallHypernova(BossModule module) : Hypernova(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var events = CollectionsMarshal.AsSpan(_predictedByEvent);
        var len = events.Length;
        for (var i = 0; i < len; ++i)
        {
            var event_ = events[i];
            var pos = event_.pos;
            var dir = pos - Arena.Center;
            hints.AddForbiddenZone(new SDRect(pos, dir.ToAngle(), 50f, 0f, 5f), event_.time);
        }
    }
}
