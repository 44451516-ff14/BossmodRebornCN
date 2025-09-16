namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D263Fenrir;

public enum OID : uint
{
    Boss = 0x3979, // R5.85
    Icicle = 0x397A, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 29597, // Boss->location, no cast, ???

    ThousandYearStormVisual = 29594, // Boss->self, 5.0s cast, single-target
    ThousandYearStorm = 29595, // Helper->self, 5.0s cast, range 100 circle
    HowlingMoon = 29598, // Boss->self, no cast, ???
    PillarImpact = 29600, // Icicle->self, 2.5s cast, range 4 circle

    LunarCry = 29599, // Boss->self, 8.0s cast, range 80 circle
    PillarShatter1 = 29648, // Icicle->self, 6.0s cast, range 8 circle
    PillarShatter2 = 29601, // Icicle->self, 2.0s cast, range 8 circle
    EclipticBite = 29596, // Boss->player, 5.0s cast, single-target
    HeavenswardRoar = 29593 // Boss->self, 5.0s cast, range 50 60-degree cone
}

sealed class LunarCry(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.LunarCry, 80f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.Icicle);
        var count = boulders.Count;
        if (count == 0)
        {
            return [];
        }
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (b.ModelState.AnimState1 != 1)
            {
                actors.Add(b);
            }
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}

sealed class PillarImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarImpact, 4f);

sealed class PillarShatter1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarShatter1, 8f);
sealed class PillarShatter2(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LunarCry)
        {
            var icicles = Module.Enemies((uint)OID.Icicle);
            var count = icicles.Count;
            for (var i = 0; i < count; ++i)
            {
                var p = icicles[i];
                if (p.ModelState.AnimState1 != 1)
                {
                    _aoes.Add(new(circle, p.Position.Quantized(), default, WorldState.FutureTime(4.5d)));
                }
            }
        }
        else if (spell.Action.ID == (uint)AID.PillarShatter2)
        {
            _aoes.Clear();
        }
    }
}

sealed class HeavenswardRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavenswardRoar, new AOEShapeCone(50f, 30f.Degrees()));
sealed class EclipticBite(BossModule module) : Components.SingleTargetCast(module, (uint)AID.EclipticBite);
sealed class ThousandYearStorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThousandYearStorm);

sealed class D263FenrirStates : StateMachineBuilder
{
    public D263FenrirStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LunarCry>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarShatter1>()
            .ActivateOnEnter<PillarShatter2>()
            .ActivateOnEnter<HeavenswardRoar>()
            .ActivateOnEnter<EclipticBite>()
            .ActivateOnEnter<ThousandYearStorm>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27, NameID = 3044, SortOrder = 4)]
public sealed class D263Fenrir(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom([new(4.25f, 39.50f), new(10.54f, 41.14f), new(11.16f, 41.39f), new(16.75f, 44.63f), new(21.32f, 49.20f),
    new(21.72f, 49.74f), new(24.72f, 54.95f), new(24.97f, 55.57f), new(26.62f, 61.89f), new(26.62f, 68.44f),
    new(24.95f, 74.69f), new(24.68f, 75.31f), new(21.73f, 80.43f), new(21.31f, 81f), new(16.70f, 85.60f),
    new(11.10f, 88.83f), new(10.48f, 89.06f), new(4.23f, 90.69f), new(-2.31f, 90.69f), new(-8.60f, 89.01f),
    new(-9.24f, 88.72f), new(-14.46f, 85.71f), new(-14.99f, 85.28f), new(-19.53f, 80.69f), new(-24.11f, 72.99f),
    new(-24.38f, 72.38f), new(-24.38f, 58.11f), new(-24.11f, 57.48f), new(-19.32f, 49.24f), new(-14.70f, 44.62f),
    new(-9.17f, 41.42f), new(-8.54f, 41.16f), new(-2.27f, 39.5f), new(4.25f, 39.5f)])]);
}
