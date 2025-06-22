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

class LunarCry(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.LunarCry, 80f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.Icicle);
        var count = boulders.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (b.ModelState.AnimState1 != 1)
                actors.Add(b);
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}

class PillarImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarImpact, 4f);

class PillarShatter1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarShatter1, 8f);
class PillarShatter2(BossModule module) : Components.GenericAOEs(module)
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
                    _aoes.Add(new(circle, WPos.ClampToGrid(p.Position), default, WorldState.FutureTime(4.5d)));
            }
        }
        else if (spell.Action.ID == (uint)AID.PillarShatter2)
            _aoes.Clear();
    }
}

class HeavenswardRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavenswardRoar, new AOEShapeCone(50f, 30f.Degrees()));
class EclipticBite(BossModule module) : Components.SingleTargetCast(module, (uint)AID.EclipticBite);
class ThousandYearStorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThousandYearStorm);

class D263FenrirStates : StateMachineBuilder
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
public class D263Fenrir(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(1f, 65.1f), 26f, 24, 7.5f.Degrees())], [new Rectangle(new(-25.4f, 65), 1f, 20f)]);
}
