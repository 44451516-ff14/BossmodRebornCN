namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D122Nixie;

public enum OID : uint
{
    Boss = 0x307F, // R2.4
    Icicle = 0x3081, // R1.0
    UnfinishedNixie = 0x3080, // R1.2
    Geyser = 0x1EB0C7,
    CloudPlatform = 0x1EA1A1, // R0.5s-2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 22932, // Boss->player, no cast, single-target
    Teleport = 22933, // Boss->location, no cast, single-target

    CrashSmash = 22927, // Boss->self, 3.0s cast, single-target
    CrackVisual = 23481, // Icicle->self, 5.0s cast, single-target
    Crack = 22928, // Icicle->self, no cast, range 80 width 3 rect

    ShowerPower = 22929, // Boss->self, 3.0s cast, single-target
    Gurgle = 22930, // Helper->self, no cast, range 60 width 10 rect

    PitterPatter = 22920, // Boss->self, 3.0s cast, single-target
    Sploosh = 22926, // Helper->self, no cast, range 6 circle, geysirs, no dmg, just throwing player around
    FallDamage = 22934, // Helper->player, no cast, single-target

    SinginInTheRain = 22921, // UnfinishedNixie->self, 40.0s cast, single-target
    SeaShantyVisual = 22922, // Boss->self, no cast, single-target
    SeaShanty = 22924, // Helper->self, no cast, ???
    SeaShantyEnrage = 22923, // Helper->self, no cast, ???

    SplishSplash = 22925, // Boss->self, 3.0s cast, single-target
    Sputter = 22931 // Helper->player, 5.0s cast, range 6 circle, spread
}

public enum TetherID : uint
{
    Crack = 8, // Icicle->player
    Gurgle = 3 // Boss->Helper
}

sealed class Gurgle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect rect = new(60f, 5f);
    private readonly List<AOEInstance> _aoes = [with(3)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u && index is > 0x12 and < 0x1B)
        {
            var posX = index < 0x17 ? -20f : 20f;
            var posZ = posX == -20f ? -165f + (index - 0x13) * 10f : -165f + (index - 0x17) * 10f;
            var rot = posX == -20f ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0];
            _aoes.Add(new(rect, new WPos(posX, posZ).Quantized(), rot, WorldState.FutureTime(9d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Gurgle)
        {
            _aoes.Clear();
        }
    }
}

sealed class Crack(BossModule module) : Components.GenericBaitAway(module, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private readonly AOEShapeRect rect = new(80f, 1.5f);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Crack)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target is Actor t)
            {
                CurrentBaits.Add(new(source, t, rect, WorldState.FutureTime(5.4d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Crack)
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class GeysersCloudPlatform(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = [with(5)];
    private readonly WPos bottomCenter = new(0f, -150f);
    private readonly WPos topPlatformCenter = new(0f, -175f);
    private bool active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            if (active)
            {
                ulong id = default; // id of geyer closest to the platform
                var minDistanceSq = float.MaxValue;
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    var distanceSq = (aoe.Origin - topPlatformCenter).LengthSq();
                    if (distanceSq < minDistanceSq)
                    {
                        minDistanceSq = distanceSq;
                        id = aoe.ActorID;
                    }
                }
                if (id != default)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        if (aoe.ActorID == id)
                        {
                            aoe.Shape.InvertForbiddenZone = true;
                            aoe.Color = Colors.SafeFromAOE;
                            break;
                        }
                    }
                }
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x12)
        {
            if (state == 0x00020001u)
            {
                var bottom = new Square(bottomCenter, 19.5f);
                var top = new Rectangle(topPlatformCenter, 9.5f, 5.5f);
                var combinedCenter = new WPos(0f, -155.5f);
                var polybottom = new RelSimplifiedComplexPolygon(bottom.Contour(combinedCenter));
                var polytop = new RelSimplifiedComplexPolygon(top.Contour(combinedCenter));
                var arena = new ArenaBoundsCustom([bottom, top], WorldProjectionLayers: [new(polybottom, 150f, borderY: 150f), new(polytop, 160f, borderY: 160f)]);
                active = true;
                Arena.Bounds = arena;
                Arena.Center = arena.Center;
            }
            else if (state == 0x00080004u)
            {
                active = false;
                Arena.Bounds = new ArenaBoundsSquare(19.5f) { Y = 150f, BorderY = 150f };
                Arena.Center = bottomCenter;
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Geyser)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(3.9d), actorID: actor.InstanceID, arenaProjectionLayer: 0, restrictToArenaProjectionLayer: true));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Sploosh)
        {
            var count = _aoes.Count;
            var pos = caster.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active && Module.ActorMatchesArenaProjectionLayer(actor, 0, true))
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var isRisky = true;
            var color = Colors.SafeFromAOE;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Color == color && aoe.Check(actor.Position))
                {
                    isRisky = false;
                    break;
                }
            }
            hints.Add("Go to correct geyser and wait for erruption!", isRisky);
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

sealed class Sputter(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Sputter, 6f);

sealed class D122NixieStates : StateMachineBuilder
{
    public D122NixieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gurgle>()
            .ActivateOnEnter<Crack>()
            .ActivateOnEnter<Sputter>()
            .ActivateOnEnter<GeysersCloudPlatform>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746u, NameID = 9738u)]
public sealed class D122Nixie(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -150f), new ArenaBoundsSquare(19.5f) { Y = 150f, BorderY = 150f })
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.UnfinishedNixie));
    }

    public override bool ShouldPrioritizeAllEnemies => true;
}
