namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D152Prometheus;

public enum OID : uint
{
    Boss = 0x2515, // R7.8
    TunnelHelper = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15046, // Boss->player, no cast, single-target

    Nitrospin = 13397, // Boss->self, 4.5s cast, range 50 circle
    NeedleGun = 13402, // Boss->self, 4.5s cast, range 40+R 90-degree cone
    OilShower = 13398, // Boss->self, 4.5s cast, range 40+R 270-degree cone
    Tunnel = 13399, // Boss->self, no cast, single-target
    Heat = 13400, // Helper->self, no cast, range 100+R width 16 rect
    UnbreakableCermetDrill = 13401, // Boss->player, 3.0s cast, single-target
    FreezingMissileVisual = 13403, // Boss->self, 3.0s cast, single-target
    FreezingMissile = 13404, // Helper->location, 3.5s cast, range 40 circle
}

class NitrospinArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom donut = new([new Circle(D152Prometheus.ArenaCenter, 50)], D152Prometheus.Polygon);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x001)
        {
            Arena.Bounds = D152Prometheus.DefaultArena;
            Arena.Center = D152Prometheus.ArenaCenter;
            _aoe = null;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Nitrospin && Arena.Bounds == D152Prometheus.StartingArena)
        {
            _aoe = new(donut, D152Prometheus.ArenaCenter, default, Module.CastFinishAt(spell, 0.8d));
        }
    }
}

class Heat(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(107.8f, 8f);

    private static readonly Dictionary<Angle, (WPos origin, Angle rotation)> aoeSources = new()
    {
        {68f.Degrees(), (new(79.481f, -57.598f), 67.482f.Degrees())},
        {-113f.Degrees(), (new(188.498f, -12.441f), -112.488f.Degrees())},
        {113f.Degrees(), (new(79.481f, -12.441f), 112.477f.Degrees())},
        {23f.Degrees(), (new(111.411f, -89.528f), 22.498f.Degrees())},
        {157f.Degrees(), (new(111.411f, 19.489f), 157.483f.Degrees())},
        {-157f.Degrees(), (new(156.568f, 19.489f), -157.505f.Degrees())},
        {-23f.Degrees(), (new(156.568f, -89.528f), -22.487f.Degrees())},
        {-68f.Degrees(), (new(188.498f, -57.598f), -67.482f.Degrees())},
    };

    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && actor.OID == (uint)OID.TunnelHelper)
        {
            foreach (var r in aoeSources.Keys)
                if (actor.Rotation.AlmostEqual(r, Angle.DegToRad))
                {
                    _aoe = new(rect, aoeSources[r].origin, aoeSources[r].rotation, WorldState.FutureTime(6.7d));
                    break;
                }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Heat)
        {
            if (++NumCasts == 5)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }
}

class Nitrospin(BossModule module) : Components.RaidwideCast(module, (uint)AID.Nitrospin);
class UnbreakableCermetDrill(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.UnbreakableCermetDrill);
class OilShower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OilShower, new AOEShapeCone(47.8f, 135f.Degrees()));
class NeedleGun(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NeedleGun, new AOEShapeCone(47.8f, 45f.Degrees()));
class FreezingMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FreezingMissile, 8f);

class D152PrometheusStates : StateMachineBuilder
{
    public D152PrometheusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NitrospinArenaChange>()
            .ActivateOnEnter<Heat>()
            .ActivateOnEnter<OilShower>()
            .ActivateOnEnter<NeedleGun>()
            .ActivateOnEnter<Nitrospin>()
            .ActivateOnEnter<FreezingMissile>()
            .ActivateOnEnter<UnbreakableCermetDrill>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7856, SortOrder = 2)]
public class D152Prometheus(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArena.Center, StartingArena)
{
    public static readonly WPos ArenaCenter = new(134f, -35f);
    private static readonly WPos[] vertices = [new(126.83f, -71.81f), new(130.07f, -71.6f), new(130.64f, -71.54f), new(131.12f, -71.34f), new(131.69f, -71.16f),
    new(132.23f, -71.05f), new(133.32f, -70.96f), new(133.83f, -71.2f), new(134.32f, -71.37f), new(134.88f, -71.38f),
    new(138.75f, -71.64f), new(139.81f, -71.43f), new(141.47f, -70.96f), new(141.98f, -71.04f), new(142.53f, -71.21f),
    new(143.09f, -71.32f), new(147.87f, -69.89f), new(148.4f, -69.7f), new(154.63f, -66.37f), new(155.1f, -66.18f),
    new(157.81f, -66.17f), new(158.31f, -65.84f), new(164.82f, -59.19f), new(164.99f, -58.68f), new(164.81f, -57.04f),
    new(165.18f, -55.98f), new(165.4f, -55.49f), new(166.79f, -52.97f), new(167.08f, -52.48f), new(168.52f, -49.85f),
    new(168.77f, -49.34f), new(169.84f, -45.59f), new(170.01f, -45.04f), new(170.15f, -44.48f), new(170.34f, -43.9f),
    new(170.64f, -42.82f), new(170.78f, -42.25f), new(170.44f, -39.56f), new(169.8f, -30.56f), new(169.89f, -29.97f),
    new(170.04f, -29.38f), new(169.93f, -27.7f), new(170.06f, -27.19f), new(170.21f, -26.68f), new(170.32f, -26.12f),
    new(168.74f, -20.79f), new(165.47f, -14.63f), new(165.15f, -14.16f), new(160.84f, -10.99f), new(160.45f, -10.62f),
    new(160.07f, -10.18f), new(159.3f, -9.31f), new(156.39f, -5.71f), new(155.96f, -5.35f), new(155.52f, -5.04f),
    new(154.36f, -4.4f), new(153.89f, -4.06f), new(153.52f, -3.64f), new(153.36f, -3.14f), new(152.99f, -2.79f),
    new(148.57f, -0.38f), new(148.08f, -0.17f), new(142.03f, 1.64f), new(141.2f, 2.35f), new(140.81f, 2.82f),
    new(129.15f, 3.13f), new(128.6f, 3.1f), new(128.08f, 2.93f), new(127.63f, 2.56f), new(127.3f, 2.13f),
    new(124.49f, 1.03f), new(119.72f, -0.26f), new(114.43f, -3.06f), new(113.95f, -3.44f), new(111.44f, -6.58f),
    new(107.15f, -10.81f), new(106.82f, -11.19f), new(105.41f, -12.98f), new(103.99f, -14.68f), new(103.6f, -15.09f),
    new(102.17f, -16.03f), new(101.72f, -16.29f), new(101.4f, -16.75f), new(101.16f, -17.2f), new(100.84f, -17.76f),
    new(99.27f, -20.7f), new(97.39f, -27.14f), new(97.4f, -27.69f), new(97.72f, -31.86f), new(97.8f, -32.42f),
    new(97.9f, -32.96f), new(97.95f, -33.52f), new(97.54f, -37.34f), new(97.42f, -38.97f), new(97.5f, -39.6f),
    new(97.66f, -40.13f), new(97.76f, -40.63f), new(98, -42.28f), new(97.99f, -42.83f), new(97.92f, -43.35f),
    new(97.73f, -43.86f), new(97.8f, -44.38f), new(99.29f, -49.42f), new(100.11f, -50.92f), new(100.39f, -51.4f),
    new(100.64f, -51.95f), new(102.58f, -55.52f), new(102.96f, -56.02f), new(106.95f, -60.07f), new(107.37f, -60.38f),
    new(107.86f, -60.7f), new(113.62f, -65.44f), new(113.96f, -65.87f), new(114.52f, -66.88f), new(115.01f, -67.22f),
    new(119.29f, -69.57f), new(119.76f, -69.79f), new(126.44f, -71.78f)];
    public static readonly ArenaBoundsComplex StartingArena = new([new PolygonCustom(vertices)]);
    public static readonly Polygon[] Polygon = [new(ArenaCenter, 20f, 24)];
    public static readonly ArenaBoundsComplex DefaultArena = new(Polygon);
}
