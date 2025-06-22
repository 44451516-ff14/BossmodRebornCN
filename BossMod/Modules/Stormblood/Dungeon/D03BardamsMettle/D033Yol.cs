namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D033Yol;

public enum OID : uint
{
    Boss = 0x1AA6, // R2.8
    YolFeather = 0x1AA8, // R0.5
    CorpsecleanerEagle = 0x1AA7, // R2.52
    LeftWing = 0x1C0B, // R0.8
    RightWing = 0x1C0A, // R0.8
    Helper = 0x19A
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // CorpsecleanerEagle->player, no cast, single-target
    Feathercut = 7945, // Boss->player, no cast, single-target

    WindUnbound = 7946, // Boss->self, 3.0s cast, range 40+R circle, raidwide
    Pinion = 7953, // YolFeather->self, 2.5s cast, range 40+R width 2 rect
    FlutterfallSpreadVisual = 7947, // Boss->self, 3.5s cast, single-target
    FlutterfallSpread = 7948, // Helper->player, no cast, range 6 circle, spread
    FlutterfallVisual = 7952, // Boss->self, 3.0s cast, single-target
    Flutterfall = 7954, // Helper->location, 2.5s cast, range 6 circle
    EyeOfTheFierce = 7949, // Boss->self, 4.5s cast, range 40+R circle, gaze
    FeatherSquall = 7950, // Boss->self, no cast, range 40+R width 6 rect, charge during add phase
    Wingbeat = 7951 // Boss->self, no cast, range 40+R 90-degree cone, bait away icon
}

public enum IconID : uint
{
    Flutterfall = 23, // player
    Wingbeat = 16 // player
}

class Flutterfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flutterfall, 6f);
class Pinion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pinion, new AOEShapeRect(40.5f, 1f));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, (uint)AID.EyeOfTheFierce);
class WindUnbound(BossModule module) : Components.RaidwideCast(module, (uint)AID.WindUnbound);
class Wingbeat(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(42.8f, 45f.Degrees()), (uint)IconID.Wingbeat, (uint)AID.Wingbeat);
class FlutterfallSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Flutterfall, (uint)AID.FlutterfallSpread, 6f, 5.4f);

class FeatherSquall(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(42.8f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor == Module.PrimaryActor && !actor.Position.AlmostEqual(new(24f, -475.5f), 1f))
            _aoe = new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(6.7d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FeatherSquall)
            _aoe = null;
    }
}

class D033YolStates : StateMachineBuilder
{
    public D033YolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flutterfall>()
            .ActivateOnEnter<Pinion>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<Wingbeat>()
            .ActivateOnEnter<FlutterfallSpread>()
            .ActivateOnEnter<FeatherSquall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6155)]
public class D033Yol(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(24.09f, -494.71f), new(26.37f, -494.25f), new(26.91f, -494.23f), new(28.47f, -494.12f), new(29.03f, -494.01f),
    new(29.52f, -493.81f), new(30.02f, -493.58f), new(31.4f, -492.64f), new(32.91f, -491.81f), new(33.52f, -491.82f),
    new(34.05f, -491.81f), new(35.46f, -491.02f), new(37.03f, -489.31f), new(37.91f, -488.51f), new(39.15f, -487.43f),
    new(39.42f, -486.91f), new(39.64f, -486.41f), new(41.24f, -484.25f), new(42.09f, -482.24f), new(42.26f, -481.74f),
    new(42.19f, -481.18f), new(42.08f, -480.65f), new(42.02f, -480.15f), new(42.33f, -477.97f), new(42.48f, -477.45f),
    new(42.95f, -475.99f), new(43.37f, -475.01f), new(43.4f, -474.41f), new(42.76f, -471.71f), new(42.74f, -471.21f),
    new(42.64f, -470.08f), new(42.53f, -469.53f), new(42.16f, -469.07f), new(41.78f, -468.69f), new(41.73f, -468.11f),
    new(41.52f, -467.54f), new(40.03f, -464.43f), new(39.76f, -463.99f), new(38.34f, -462.29f), new(37.82f, -462.01f),
    new(37.25f, -461.82f), new(35.86f, -460.77f), new(35.48f, -460.37f), new(35.14f, -459.88f), new(34.74f, -459.4f),
    new(33.15f, -458.06f), new(31.57f, -457.32f), new(31.02f, -457.09f), new(29.9f, -456.82f), new(18.28f, -456.73f),
    new(17.79f, -457.09f), new(17.33f, -457.41f), new(16.77f, -457.65f), new(15.84f, -458.13f), new(15.3f, -458.36f),
    new(13.18f, -459.06f), new(12.73f, -459.42f), new(11.92f, -460.2f), new(11.58f, -460.67f), new(10.79f, -462.24f),
    new(10.05f, -463.09f), new(9.2f, -463.8f), new(8.95f, -464.25f), new(8.43f, -464.5f), new(7.91f, -464.79f),
    new(7.79f, -465.35f), new(7.35f, -466.38f), new(6.04f, -468.41f), new(5.35f, -469.25f), new(5.09f, -469.78f),
    new(5.09f, -470.29f), new(5.18f, -470.86f), new(5, -471.96f), new(4.73f, -472.46f), new(4.59f, -472.94f),
    new(4.72f, -475.18f), new(4.67f, -475.73f), new(4.59f, -476.32f), new(4.64f, -477.94f), new(4.77f, -478.5f),
    new(5.18f, -479.54f), new(5.35f, -480.07f), new(5.74f, -481.67f), new(5.93f, -482.21f), new(6.63f, -483.78f),
    new(6.95f, -484.28f), new(8.59f, -485.85f), new(9.93f, -487.61f), new(10.23f, -488.08f), new(10.78f, -489.12f),
    new(11.14f, -489.57f), new(12.51f, -490.54f), new(13.31f, -491.39f), new(13.76f, -491.83f), new(14.34f, -491.8f),
    new(14.83f, -491.96f), new(15.35f, -492.1f), new(17.65f, -493.47f), new(18.15f, -493.73f), new(20.83f, -494.52f),
    new(21.4f, -494.52f), new(22.55f, -494.49f), new(23.63f, -494.68f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
    private static readonly uint[] adds = [(uint)OID.LeftWing, (uint)OID.RightWing, (uint)OID.CorpsecleanerEagle];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(adds));
    }
}
