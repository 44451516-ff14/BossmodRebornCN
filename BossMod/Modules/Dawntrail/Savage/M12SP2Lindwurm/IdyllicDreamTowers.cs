using Dalamud.Interface;

namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class IdyllicDreamArena(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly ArenaBoundsCustom PlatformBounds = BuildPlatformBounds();
    private static readonly AOEShapeCustom InversePlatformShape = BuildInverseShape();
    private readonly AOEInstance[] _aoe = new AOEInstance[1];

    private DateTime _activation;

    public int State { get; private set; }

    private static ArenaBoundsCustom BuildPlatformBounds()
    {
        // two 10y circles centered ±14 from arena center
        Shape[] union =
        [
            new Circle(new(-14, 0), 10),
            new Circle(new(14, 0), 10)
        ];

        return new ArenaBoundsCustom(union);
    }

    private static AOEShapeCustom BuildInverseShape()
    {
        // full 20y arena minus platform union
        Shape[] arena =
        [
            new Circle(default, 20)
        ];

        Shape[] subtract =
        [
            new Circle(new(-14, 0), 10),
            new Circle(new(14, 0), 10)
        ];

        return new AOEShapeCustom(arena, subtract);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x21)
            return;

        switch (state)
        {
            case 0x00200010:
                _activation = WorldState.FutureTime(9.1f);
                break;

            case 0x00800040:
            case 0x02000040:
                _activation = default;
                Arena.Bounds = PlatformBounds;
                State = 1;
                break;

            case 0x01000001:
                _activation = default;
                Arena.Bounds = new ArenaBoundsCircle(20);
                State = 0;
                break;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            return default;

        _aoe[0] = new(InversePlatformShape, Arena.Center, default, _activation);
        return _aoe.AsSpan(0, 1);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;

        var pos = actor.Position;
        if (!pos.InCircle(new(114, 100), 10) && !pos.InCircle(new(86, 100), 10))
            hints.Add("Go to platform!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
        {
            hints.AddForbiddenZone(
                new SDOutsideOfUnion(
                [
                    new SDInvertedCircle(new(114, 100), 10),
                    new SDInvertedCircle(new(86, 100), 10)
                ]),
                _activation
            );
        }
    }

    public void Predict(float seconds)
    {
        _activation = WorldState.FutureTime(seconds);
    }
}

class ArcadianArcanum(BossModule module) : Components.UniformStackSpread(module, 0, 6)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArcadianArcanumCast)
        {
            foreach (var p in Raid.WithoutSlot())
                AddSpread(p, Module.CastFinishAt(spell, 1.4f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArcadianArcanum)
        {
            NumCasts++;
            Spreads.Clear();
        }
    }
}
sealed class IdyllicDreamElementalMeteor(BossModule module) : Components.GenericTowers(module)
{
    internal readonly record struct Meteor(Actor Actor, Element Element)
    {
        public WPos Position => Actor.Position;
    }

    internal readonly List<Meteor> Meteors = [];

    BitMask _lightVuln;
    public bool DrawIcons;

    public const uint ColorWind = 0xFF9ABB81;
    public const uint ColorDark = 0xFFE67AD2;
    public const uint ColorEarth = 0xFF81A1AD;
    public const uint ColorFire = 0xFF2A2DD5;

    // --------------------------------------------------------
    // Meteor Detection
    // --------------------------------------------------------

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00010002)
            return;

        var element = (OID)actor.OID switch
        {
            OID.MeteorWind => Element.Wind,
            OID.MeteorDark => Element.Dark,
            OID.MeteorEarth => Element.Earth,
            OID.MeteorFire => Element.Fire,
            _ => default
        };

        if (element != default)
            Meteors.Add(new(actor, element));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.LightResistanceDownII)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _lightVuln.Set(slot);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.CosmicKiss)
            return;

        NumCasts++;
        Towers.Clear();
        Meteors.Clear();
        _lightVuln = default;
        DrawIcons = false;
    }

    // --------------------------------------------------------
    // Tower Creation
    // --------------------------------------------------------

    public void CreateTowers()
    {
        DrawIcons = true;

        var activation = WorldState.FutureTime(8.4f);
        var meteors = CollectionsMarshal.AsSpan(Meteors);

        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];

            var forbidden = m.Element is Element.Wind or Element.Dark
                ? _lightVuln
                : ~_lightVuln;

            Towers.Add(new(m.Position, 3, forbiddenSoakers: forbidden, activation: activation));
        }
    }

    // --------------------------------------------------------
    // Drawing
    // --------------------------------------------------------

    void DrawElement(Element el, WPos p)
    {
        var (icon, color) = el switch
        {
            Element.Wind => (FontAwesomeIcon.Tornado, ColorWind),
            Element.Dark => (FontAwesomeIcon.StarOfLife, ColorDark),
            Element.Earth => (FontAwesomeIcon.Gem, ColorEarth),
            Element.Fire => (FontAwesomeIcon.Fire, ColorFire),
            _ => default
        };

        Arena.AddCircleFilled(p, 1.5f, Colors.Background);
        Arena.IconWorld(p, icon, color);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (!DrawIcons)
            return;

        var meteors = CollectionsMarshal.AsSpan(Meteors);
        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];

            var center = m.Position.X > 100
                ? new WPos(114, 100)
                : new WPos(86, 100);

            DrawElement(m.Element, m.Position + (m.Position - center));
        }
    }

    // --------------------------------------------------------
    // Suppress default tower hints
    // --------------------------------------------------------

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // intentionally suppress GenericTowers hints
    }
}

sealed class IdyllicDreamWindTower(BossModule module) : Components.GenericKnockback(module, maxCasts: int.MaxValue)
{
    readonly List<Knockback> _knockbacks = [];
    IdyllicDreamElementalMeteor? _meteor;
    bool _initialized;

    public override void Update()
    {
        if (_initialized)
            return;

        _meteor ??= Module.FindComponent<IdyllicDreamElementalMeteor>();
        if (_meteor == null)
            return;

        if (_meteor.Towers.Count == 0 || _meteor.Meteors.Count == 0)
            return;

        var activation = _meteor.Towers[0].Activation;
        var meteors = CollectionsMarshal.AsSpan(_meteor.Meteors);

        for (var i = 0; i < meteors.Length; ++i)
        {
            ref var m = ref meteors[i];
            if (m.Element != Element.Wind)
                continue;

            _knockbacks.Add(new Knockback(
                origin: m.Position,
                distance: 23.5f,
                activation: activation,
                shape: new AOEShapeCircle(3),
                kind: Kind.AwayFromOrigin
            ));
        }

        _initialized = true;
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_knockbacks);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.LindwurmsDarkII)
            return;

        _knockbacks.Clear();
        _initialized = false;
    }
}

sealed class IdyllicDreamLindwurmsDarkII(BossModule module) : Components.GenericBaitAway(module)
{
    readonly List<(Actor Source, DateTime Activation)> _sources = [];
    IdyllicDreamElementalMeteor? _meteor;
    bool _initialized;

    static readonly AOEShapeRect DarkRect = new(50, 5);

    public override void Update()
    {
        if (!_initialized)
        {
            _meteor ??= Module.FindComponent<IdyllicDreamElementalMeteor>();
            if (_meteor == null)
                return;

            if (_meteor.Towers.Count == 0 || _meteor.Meteors.Count == 0)
                return;

            var activation = _meteor.Towers[0].Activation;
            var meteors = CollectionsMarshal.AsSpan(_meteor.Meteors);

            for (var i = 0; i < meteors.Length; ++i)
            {
                ref var m = ref meteors[i];
                if (m.Element != Element.Dark)
                    continue;

                _sources.Add((m.Actor, activation));
            }

            _initialized = true;
        }

        CurrentBaits.Clear();

        var count = _sources.Count;
        for (var i = 0; i < count; ++i)
        {
            var (src, activation) = _sources[i];

            // closest player in 3y circle
            var target = Raid.WithoutSlot().Closest(src.Position);
            if (target == null)
                continue;

            if (!target.Position.InCircle(src.Position, 3))
                continue;

            CurrentBaits.Add(new(src, target, DarkRect, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.LindwurmsDarkII)
            return;

        _sources.Clear();
        _initialized = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // intentionally suppress default bait hints
    }
}

sealed class IdyllicDreamDoom(BossModule module) : BossComponent(module)
{
    readonly BitMask _doomSlots;

    // --------------------------------------------------------
    // Status Tracking
    // --------------------------------------------------------

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID != SID.Doom)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _doomSlots.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID != SID.Doom)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _doomSlots.Clear(slot);
    }

    // --------------------------------------------------------
    // Player Hints
    // --------------------------------------------------------

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Class.CanEsuna())
            return;

        var mask = _doomSlots;
        if (mask.None())
            return;

        for (var i = 0; i < PartyState.MaxAllies; ++i)
        {
            if (!mask[i])
                continue;

            var target = Raid[i];
            if (target == null || target.PendingDispels.Count != 0)
                continue;

            hints.Add($"Cleanse {target.Name}!");
            break; // only need one warning
        }
    }

    // --------------------------------------------------------
    // AI Hints
    // --------------------------------------------------------

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var mask = _doomSlots;
        if (mask.None())
            return;

        for (var i = 0; i < PartyState.MaxAllies; ++i)
        {
            if (!mask[i])
                continue;

            var target = Raid[i];
            if (target == null || target.PendingDispels.Count != 0)
                continue;

            hints.ShouldCleanse.Set(i);
        }
    }

    // --------------------------------------------------------
    // Priority Highlighting
    // --------------------------------------------------------

    public override PlayerPriority CalcPriority(
        int pcSlot,
        Actor pc,
        int playerSlot,
        Actor player,
        ref uint customColor)
    {
        if (!pc.Class.CanEsuna())
            return PlayerPriority.Normal;

        if (!_doomSlots[playerSlot])
            return PlayerPriority.Normal;

        if (player.PendingDispels.Count != 0)
            return PlayerPriority.Normal;

        return PlayerPriority.Critical;
    }
}

sealed class IdyllicDreamHotBlooded : Components.StayMove
{
    readonly List<Actor> _sources = [];
    readonly DateTime _activation;
    bool _resolved;

    const int PriorityHotBlooded = 1;

    public IdyllicDreamHotBlooded(BossModule module) : base(module)
    {
        var meteorComp = module.FindComponent<IdyllicDreamElementalMeteor>();
        if (meteorComp == null)
            return;

        if (meteorComp.Towers.Count > 0)
            _activation = meteorComp.Towers[0].Activation;

        var meteors = CollectionsMarshal.AsSpan(meteorComp.Meteors);
        for (var i = 0; i < meteors.Length; ++i)
        {
            if (meteors[i].Element == Element.Fire)
                _sources.Add(meteors[i].Actor);
        }
    }

    public override void Update()
    {
        if (_resolved || _activation == default)
            return;

        var sources = CollectionsMarshal.AsSpan(_sources);
        var sourceCount = sources.Length;

        foreach (var (slot, player) in Raid.WithSlot())
        {
            var pos = player.Position;

            var inside = false;
            for (var i = 0; i < sourceCount; ++i)
            {
                var delta = pos - sources[i].Position;
                if (delta.LengthSq() <= 9f) // radius 3 squared
                {
                    inside = true;
                    break;
                }
            }

            if (inside)
            {
                var state = new PlayerState(
                    Requirement.Stay2,
                    _activation,
                    PriorityHotBlooded
                );

                SetState(slot, ref state);
            }
            else
            {
                ClearState(slot, PriorityHotBlooded);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID != SID.HotBlooded)
            return;

        _resolved = true;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            ClearState(slot, PriorityHotBlooded);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID != SID.HotBlooded)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            ClearState(slot, PriorityHotBlooded);
    }
}

sealed class LindwurmsStoneIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LindwurmsStoneIII, 4);

sealed class LindwurmsPortent(BossModule module) : Components.GenericBaitAway(module)
{
    struct Portent
    {
        public Actor Source;
        public bool Far;
        public DateTime Expire;
    }

    static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    readonly List<Portent> _sources = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FarawayPortent:
                _sources.Add(new Portent
                {
                    Source = actor,
                    Far = true,
                    Expire = status.ExpireAt
                });
                break;

            case SID.NearbyPortent:
                _sources.Add(new Portent
                {
                    Source = actor,
                    Far = false,
                    Expire = status.ExpireAt
                });
                break;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var sources = CollectionsMarshal.AsSpan(_sources);
        var len = sources.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var s = ref sources[i];

            var sourcePos = s.Source.Position;
            Actor? target = null;

            if (s.Far)
            {
                float maxDist = -1;
                foreach (var (_, player) in Raid.WithSlot())
                {
                    var dist = (player.Position - sourcePos).LengthSq();
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        target = player;
                    }
                }
            }
            else
            {
                float minDist = float.MaxValue;
                foreach (var (_, player) in Raid.WithSlot())
                {
                    if (player == s.Source)
                        continue;

                    var dist = (player.Position - sourcePos).LengthSq();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        target = player;
                    }
                }
            }

            if (target != null)
            {
                CurrentBaits.Add(new(
                    s.Source,
                    target,
                    _shape,
                    s.Expire
                ));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.LindwurmsThunderII)
            return;

        NumCasts++;
        _sources.Clear();
    }
}