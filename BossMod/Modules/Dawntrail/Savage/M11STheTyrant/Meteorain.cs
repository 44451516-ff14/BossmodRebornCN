namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class CometTethers(BossModule module) : Components.TankbusterTether(module, (uint)AID.ForegoneFatality, (uint)TetherID.CometTether, 0f);
sealed class CosmicKiss(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CosmicKissIcon, (uint)AID.CosmicKiss, 4f, 5d);
sealed class Tether1(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.ShortTether, 40f, new AOEShapeRect(60f, 5f));
sealed class Tether2(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.LongTether, 40f, new AOEShapeRect(60f, 5f));
sealed class FireBreath(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 3f), (uint)IconID.CosmicKissIcon, (uint)AID.FireBreath1);
sealed class MajesticMeteor : Components.SimpleAOEs
{
    public MajesticMeteor(BossModule module) : base(module, (uint)AID.MajesticMeteorBaits, 3f, 24)
    {
        MaxDangerColor = 8;
    }
}

sealed class MeteorainComets(BossModule module) : BossComponent(module)
{
    private static readonly WPos InvalidPos = new(100f, 100f);
    private const float InvalidPosEpsilonSq = 0.01f;

    private bool _active = true;
    private readonly List<ulong> _consumed = new(4);
    public bool IsValidComet(Actor c)
    {
        return !c.IsDead
            && (c.Position - InvalidPos).LengthSq() >= InvalidPosEpsilonSq
            && !_consumed.Contains(c.InstanceID);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;

        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;

        for (var i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (IsValidComet(c))
                Arena.AddCircle(c.Position, c.HitboxRadius, Colors.Object);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Comet_Explosion:
            case (uint)AID.Comet_Destroyed:
                _consumed.Add(caster.InstanceID);
                break;
            case (uint)AID.TripleTyrannhilationEnd:
                _active = false;
                break;
        }
    }
}

sealed class TripleTyrannhilation(BossModule module)
    : Components.CastLineOfSightAOE(module, (uint)AID.TripleTyrannhilation2, 60f)
{
    private MeteorainComets? _comets;
    private readonly List<Actor> _blockers = new(4);
    private readonly List<(WPos Center, float Radius)> _blockerData = new(4);

    private bool _active;
    private WPos? _origin;

    private MeteorainComets? Comets => _comets ??= Module.FindComponent<MeteorainComets>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _active = true;
            _origin = spell.LocXZ;
        }

        base.OnCastStarted(caster, spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && _active)
            RebuildSafezone();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Shockwave:
                if (_active)
                    RebuildSafezone();
                break;

            case AID.TripleTyrannhilationEnd:
                _active = false;
                _origin = null;
                Safezones.Clear();
                break;
        }
    }

    public override ReadOnlySpan<Actor> BlockerActors()
    {
        if (!_active)
            return default;

        var cometsComp = Comets;
        if (cometsComp == null)
            return default;

        var comets = Module.Enemies((uint)OID.Comet);
        int count = comets.Count;
        if (count == 0)
            return default;

        _blockers.Clear();

        for (var i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (cometsComp.IsValidComet(c))
                _blockers.Add(c);
        }

        return CollectionsMarshal.AsSpan(_blockers);
    }

    private void RebuildSafezone()
    {
        if (_origin == null)
            return;

        var blockers = BlockerActors();
        var len = blockers.Length;

        _blockerData.Clear();

        for (var i = 0; i < len; ++i)
        {
            ref readonly var b = ref blockers[i];
            _blockerData.Add((b.Position, b.HitboxRadius));
        }

        Modify(_origin, _blockerData, WorldState.FutureTime(2));

        Safezones.Clear();
        AddSafezone(WorldState.FutureTime(2));
    }
}

sealed class CosmicKissTowers(BossModule module) : Components.CastTowers(module, (uint)AID.CosmicKissTower, 4f, 1, 1, AIHints.PredictedDamageType.Tankbuster);
sealed class WeightyImpactTowers(BossModule module) : Components.CastTowers(module, (uint)AID.WeightyImpactTower, 4f, 2, 2, AIHints.PredictedDamageType.Shared);
sealed class HeartBreakerTower(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HeartbreakKickTower)
        {
            Towers.Add(new(spell.LocXZ, 4f, 8, 8, activation: Module.CastFinishAt(spell)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HeartbreakKickDamage:
                ++NumCasts;
                break;
            case (uint)AID.HeartBreakKickEnd:
            case (uint)AID.EnrageCast:
                Towers.Clear();
                break;
            default: break;
        }
    }
}
sealed class ExplosionTowers(BossModule module) : Components.CastTowers(module, (uint)AID.ExplosionTower, radius: 4f, minSoakers: 2, maxSoakers: 2);
sealed class ExplosionTowerKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly ExplosionTowers? _towers = module.FindComponent<ExplosionTowers>();
    private static readonly AOEShapeCircle Shape = new(4f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_towers == null)
            return [];

        var towers = _towers.Towers;
        var count = towers.Count;
        if (count == 0)
            return [];

        Span<Knockback> sources = new Knockback[count];
        for (var i = 0; i < count; ++i)
        {
            var t = towers[i];
            sources[i] = new(
                t.Position,
                distance: 20f,
                activation: t.Activation,
                shape: Shape
            );
        }
        return sources;
    }
}
sealed class MeteorainPortals(BossModule module) : Components.GenericAOEs(module)
{
    // Meteorain line: length 60, width 10 (half-width = 5)
    private static readonly AOEShapeRect Shape = new(60f, 5f);
    // Expose portals so hints can get AOE info
    internal IEnumerable<byte> ActivePortalIndices => _activePortals.Keys;

    // MapEffect index -> portal position
    private static readonly Dictionary<byte, WPos> PortalPositions = new()
    {
        { 0x16, new(79f, 75f) },
        { 0x17, new(89f, 75f) },
        { 0x18, new(111f, 75f) },
        { 0x19, new(121f, 75f) },
    };

    // Portals that have spawned this cycle
    private readonly Dictionary<byte, WPos> _activePortals = [];

    // Active AOEs once armed
    private readonly List<AOEInstance> _aoes = [];

    private bool _armed;

    // Capture portal spawns
    public override void OnMapEffect(byte index, uint state)
    {
        // Portal spawn event
        if (state == 0x00200010 && PortalPositions.TryGetValue(index, out var pos))
        {
            _activePortals[index] = pos;
        }
    }

    // Arm AOEs when Fire Breath begins
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FireBreath)
        {
            _armed = true;
            _aoes.Clear();

            var activation = Module.CastFinishAt(spell);

            foreach (var p in _activePortals.Values)
            {
                // If later needed, rotation can be adjusted here
                _aoes.Add(new AOEInstance(
                    Shape,
                    p,
                    default,
                    activation
                ));
            }
        }
    }

    // Clear once MajesticMeteorain resolves
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MajesticMeteorain)
        {
            _armed = false;
            _aoes.Clear();
            _activePortals.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _armed ? CollectionsMarshal.AsSpan(_aoes) : [];
}
internal class TyrantFireballLines : Components.GenericBaitStack
{
    private readonly int _numTargets;
    private static readonly AOEShapeRect _shape = new(60f, 3f);
    private DateTime _activation;
    private bool _active;

    public TyrantFireballLines(BossModule module, uint aid, int numTargets)
        : base(module, aid)
    {
        _numTargets = numTargets;
        CurrentBaits.Capacity = 4;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != WatchedAction)
            return;

        _activation = Module.CastFinishAt(spell);
        _active = true;
    }

    public override void Update()
    {
        if (!_active)
            return;

        var boss = Module.PrimaryActor;

        CurrentBaits.Clear();

        int count = 0;
        foreach (var target in Raid.WithoutSlot(false, true, true)
                                   .SortedByRange(boss.Position))
        {
            CurrentBaits.Add(new(boss, target, _shape, _activation));
            if (++count == _numTargets)
                break;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _active = false;
            CurrentBaits.Clear();
        }
    }
}
sealed class TwoWayFireball(BossModule module)
    : TyrantFireballLines(module, (uint)AID.TwoWayFireballStart, 2);

sealed class FourWayFireball(BossModule module)
    : TyrantFireballLines(module, (uint)AID.FourWayFireballStart, 4);

sealed class MeteorMechanicHints(BossModule module) : BossComponent(module)
{
    private readonly MeteorainPortals _meteorain = module.FindComponent<MeteorainPortals>()!;
    private readonly Tether1 _t1 = module.FindComponent<Tether1>()!;
    private readonly Tether2 _t2 = module.FindComponent<Tether2>()!;
    private readonly FireBreath _icons = module.FindComponent<FireBreath>()!;

    // ---------------- FIRE SAFE SPOTS ----------------

    private static readonly WPos[] Fire_Left_LeftSafe =
    [
        new(83f, 88f),
        new(83f, 112f),
    ];

    private static readonly WPos[] Fire_Left_RightSafe =
    [
        new(85f, 88f),
        new(85f, 112f),
    ];

    private static readonly WPos[] Fire_Right_LeftSafe =
    [
        new(115f, 88f),
        new(115f, 112f),
    ];

    private static readonly WPos[] Fire_Right_RightSafe =
    [
        new(117f, 88f),
        new(117f, 112f),
    ];

    // ---------------- TETHER SAFE SPOTS ----------------

    private static readonly WPos[] Tether_Left_LeftSafe =
    [
        new(83f, 80.5f),
        new(83f, 119.5f),
    ];

    private static readonly WPos[] Tether_Left_RightSafe =
    [
        new(85f, 80.5f),
        new(85f, 119.5f),
    ];

    private static readonly WPos[] Tether_Right_LeftSafe =
    [
        new(115f, 80.5f),
        new(115f, 119.5f),
    ];

    private static readonly WPos[] Tether_Right_RightSafe =
    [
        new(117f, 80.5f),
        new(117f, 119.5f),
    ];

    // ---------------- DRAWING ----------------

    public override void DrawArenaForeground(int slot, Actor pc)
    {
        DeterminePlatformSafeSides(out var leftPlatformLeftSafe, out var rightPlatformLeftSafe);

        var onLeftPlatform = pc.Position.X < 100f;
        var leftSafe = onLeftPlatform ? leftPlatformLeftSafe : rightPlatformLeftSafe;

        var tetherSource = GetTetherSourceOn(pc);

        //  Tether player, only show ONE spot
        if (tetherSource != null)
        {
            var spot = ChooseTetherSpot(tetherSource, leftPlatformLeftSafe, rightPlatformLeftSafe);
            Arena.AddCircle(spot, 1f, Colors.Safe);
            return;
        }

        // Fire breath player, show both safe spots on platform
        if (HasFireIcon(pc))
        {
            var spots = GetFireSpots(onLeftPlatform, leftSafe);
            for (var i = 0; i < spots.Length; ++i)
                Arena.AddCircle(spots[i], 1f, Colors.Safe);
        }
    }

    // ---------------- LOGIC ----------------

    private void DeterminePlatformSafeSides(out bool leftPlatformLeftSafe, out bool rightPlatformLeftSafe)
    {
        leftPlatformLeftSafe = false;
        rightPlatformLeftSafe = false;

        foreach (var idx in _meteorain.ActivePortalIndices)
        {
            switch (idx)
            {
                case 0x16: leftPlatformLeftSafe = false; break;
                case 0x17: leftPlatformLeftSafe = true; break;
                case 0x18: rightPlatformLeftSafe = false; break;
                case 0x19: rightPlatformLeftSafe = true; break;
            }
        }
    }

    private static WPos[] GetFireSpots(bool leftPlatform, bool leftSafe)
    {
        if (leftPlatform)
            return leftSafe ? Fire_Left_LeftSafe : Fire_Left_RightSafe;
        else
            return leftSafe ? Fire_Right_LeftSafe : Fire_Right_RightSafe;
    }

    private static WPos ChooseTetherSpot(Actor src, bool leftPlatformLeftSafe, bool rightPlatformLeftSafe)
    {
        var centerZ = 100f;

        var wantSouth = src.Position.Z < centerZ;
        var wantWest = src.Position.X > 125f;

        var leftSpots = leftPlatformLeftSafe ? Tether_Left_LeftSafe : Tether_Left_RightSafe;
        var rightSpots = rightPlatformLeftSafe ? Tether_Right_LeftSafe : Tether_Right_RightSafe;

        var spots = wantWest ? leftSpots : rightSpots;

        for (var i = 0; i < spots.Length; ++i)
        {
            var s = spots[i];
            if ((s.Z > centerZ) == wantSouth)
                return s;
        }

        return spots[0];
    }

    // ---------------- DATA ACCESS ----------------

    private Actor? GetTetherSourceOn(Actor pc)
    {
        var baits = _t1.CurrentBaits;
        for (var i = 0; i < baits.Count; ++i)
            if (baits[i].Target == pc)
                return baits[i].Source;

        baits = _t2.CurrentBaits;
        for (var i = 0; i < baits.Count; ++i)
            if (baits[i].Target == pc)
                return baits[i].Source;

        return null;
    }

    private bool HasFireIcon(Actor pc)
    {
        var baits = _icons.CurrentBaits;
        for (var i = 0; i < baits.Count; ++i)
            if (baits[i].Target == pc)
                return true;

        return false;
    }
}
