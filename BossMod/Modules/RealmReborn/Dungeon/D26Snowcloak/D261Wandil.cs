namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D261Wandil;

public enum OID : uint
{
    Boss = 0xD07, // R3.23
    FrostBomb = 0xD09, // R0.9
    Voidzone = 0x1E9661, // R2.0
    Helper = 0xD2E
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/D09->player, no cast, single-target

    SnowDriftVisual = 3080, // Boss->self, 5.0s cast, single-target
    SnowDrift = 3079, // Helper->self, no cast, range 80+R circle
    IceGuillotine = 3084, // Boss->player, no cast, range 8+R ?-degree cone
    ColdWaveVisual = 3083, // Boss->self, 3.0s cast, ???
    ColdWave = 3111, // Helper->location, 4.0s cast, range 8 circle
    Tundra = 3082, // Boss->self, 3.0s cast, single-target
    HypothermalCombustion = 3085 // FrostBomb->self, 3.0s cast, range 80+R circle
}

class TundraArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom donut = new([new Circle(D261Wandil.ArenaCenter, 20f)], D261Wandil.Polygon);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && actor.OID == (uint)OID.Voidzone)
        {
            Arena.Bounds = D261Wandil.SmallArena;
            Arena.Center = D261Wandil.ArenaCenter;
            _aoe = null;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Tundra)
            _aoe = new(donut, D261Wandil.ArenaCenter, default, Module.CastFinishAt(spell, 2f));
    }
}

class IceGuillotine(BossModule module) : Components.Cleave(module, (uint)AID.IceGuillotine, new AOEShapeCone(11.23f, 60f.Degrees()), activeWhileCasting: false);
class SnowDrift(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.SnowDriftVisual, (uint)AID.SnowDrift, 2f);
class ColdWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ColdWave, 8f);

class D261WandilStates : StateMachineBuilder
{
    public D261WandilStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SnowDrift>()
            .ActivateOnEnter<ColdWave>()
            .ActivateOnEnter<IceGuillotine>()
            .ActivateOnEnter<TundraArenaChange>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27, NameID = 3038, SortOrder = 1)]
public class D261Wandil(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArena.Center, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(56.168f, -88.335f);

    private static readonly WPos[] vertices = [new(55.82f, -107), new(65.68f, -103.86f), new(66.18f, -103.62f), new(70.16f, -99.65f), new(70.44f, -99.22f),
    new(71.53f, -97.13f), new(71.6f, -96.6f), new(71.52f, -96), new(71.8f, -95.56f), new(72.61f, -94.76f),
    new(73.07f, -94.45f), new(73.42f, -93.94f), new(74.29f, -88.56f), new(74.27f, -87.98f), new(73.5f, -83.07f),
    new(73.35f, -82.53f), new(70.97f, -77.85f), new(70.63f, -77.41f), new(67.17f, -73.95f), new(66.73f, -73.57f),
    new(62.26f, -71.29f), new(61.71f, -71.05f), new(56.35f, -70.2f), new(55.82f, -70.23f), new(54.78f, -70.39f),
    new(51.34f, -70.9f), new(50.76f, -70.96f), new(42.58f, -76.02f), new(42.18f, -76.42f), new(42.04f, -76.98f),
    new(39.18f, -82.29f), new(38.94f, -82.8f), new(38.12f, -87.95f), new(38.11f, -88.52f), new(38.94f, -93.76f),
    new(39.15f, -94.28f), new(41.39f, -98.67f), new(41.7f, -99.13f), new(45.25f, -102.67f), new(45.67f, -103.03f),
    new(50.41f, -105.45f), new(50.93f, -105.65f), new(55.56f, -107.02f)];
    public static readonly ArenaBoundsComplex DefaultArena = new([new PolygonCustom(vertices)]);
    public static readonly Polygon[] Polygon = [new Polygon(ArenaCenter, 12f, 20)];
    public static readonly ArenaBoundsComplex SmallArena = new(Polygon);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.FrostBomb => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.FrostBomb));
    }
}
