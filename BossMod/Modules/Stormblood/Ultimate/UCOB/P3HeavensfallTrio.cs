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
    private bool _divesStarted;

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
        if (_divesStarted)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(_safeSpots[slot], 1f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MegaflareDive)
        {
            _divesStarted = true;
        }
    }

    private void InitIfReady()
    {
        if (_nael == null || _twin == null || _baha == null)
            return;

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
        if (!EnableHints)
        {
            return;
        }

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
            hints.AddForbiddenZone(new SDInvertedCone(center, 30f, dir.ToAngle(), 5f.Degrees()), myTower.Activation.AddSeconds(-2.5d));
        }
    }
}

sealed class P3HeavensfallFireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, (uint)AID.Fireball, 4f, 5.3f, 8, 8);
