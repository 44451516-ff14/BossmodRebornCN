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
    HeavenswardRoar = 29593, // Boss->self, 5.0s cast, range 50 60-degree cone
}

class LunarCry(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.LunarCry), 80)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Icicle).Where(x => x.ModelState.AnimState1 != 1);
}

class PillarImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarImpact), new AOEShapeCircle(4));

class PillarShatter1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarShatter1), new AOEShapeCircle(8));
class PillarShatter2(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LunarCry)
            foreach (var p in Module.Enemies(OID.Icicle).Where(x => x.ModelState.AnimState1 != 1))
                _aoes.Add(new(circle, p.Position, default, WorldState.FutureTime(4.5f)));
        else if ((AID)spell.Action.ID == AID.PillarShatter2)
            _aoes.Clear();
    }
}

class HeavenswardRoar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeavenswardRoar), new AOEShapeCone(50, 30.Degrees()));
class EclipticBite(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.EclipticBite));
class ThousandYearStorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ThousandYearStorm));

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
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(1, 65.1f), 26, 24, 7.5f.Degrees())], [new Rectangle(new(-25.4f, 65), 20, 1, 89.977f.Degrees())]);
}
