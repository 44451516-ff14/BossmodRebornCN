namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D032Batsquatch;

public enum OID : uint
{
    Boss = 0x27B0, //R=3.2
    StalactiteBig = 0x1EAACD, //R=0.5
    StalactiteSmall1 = 0x1EAACC, //R=0.5
    StalactiteSmall2 = 0x1EAACB, //R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // 27B0->player, no cast, single-target

    RipperFang = 15505, // 27B0->player, 4.0s cast, single-target
    Soundwave = 15506, // 27B0->self, 3.0s cast, range 40 circle
    Subsonics = 15507, // 27B0->self, 6.0s cast, single-target
    Subsonics2 = 15508, // 233C->self, no cast, range 40 circle
    FallingRock = 15510, // 233C->self, 2.0s cast, range 3 circle
    FallingRock2 = 15509, // 233C->self, 2.0s cast, range 2 circle
    FallingBoulder = 15511, // 233C->self, 2.0s cast, range 4 circle
    Towerfall = 15512 // 233C->self, 3.0s cast, range 15 30.5-degree cone
}

class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(15, 15.25f.Degrees());
    public List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.StalactiteBig)
            _aoes.Add(new(cone, actor.Position, actor.Rotation, WorldState.FutureTime(12)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Soundwave)
        {
            for (var i = 0; i < _aoes.Count; ++i)
                _aoes[i] = _aoes[i] with { Activation = Module.CastFinishAt(spell, 3.7f) };
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Towerfall)
            _aoes.Clear();
    }
}

class Soundwave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Soundwave), "Raidwide + towers fall");
class Subsonics(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Subsonics), "Raidwide x11");
class RipperFang(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.RipperFang));
class FallingBoulder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingBoulder), new AOEShapeCircle(4));
class FallingRock(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingRock), new AOEShapeCircle(3));
class FallingRock2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingRock2), new AOEShapeCircle(2));

class D032BatsquatchStates : StateMachineBuilder
{
    public D032BatsquatchStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<Soundwave>()
            .ActivateOnEnter<Subsonics>()
            .ActivateOnEnter<RipperFang>()
            .ActivateOnEnter<FallingBoulder>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<FallingRock2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8232)]
public class D032Batsquatch(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(62, -35), 14.5f / MathF.Cos(MathF.PI / 28), 28)], [new Rectangle(new(61.9f, -20), 20, 2), new Rectangle(new(61.9f, -50), 20, 2)]);
}
