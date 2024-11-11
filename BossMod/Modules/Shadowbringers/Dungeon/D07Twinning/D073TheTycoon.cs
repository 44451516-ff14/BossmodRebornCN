namespace BossMod.Shadowbringers.Dungeon.D07Twinning.D073TheTycoon;

public enum OID : uint
{
    Boss = 0x2805, // R7.2
    Lasers = 0x1EAC39, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15869, // Boss->player, no cast, single-target

    MagitekCrossray = 15864, // Boss->self, 5.0s cast, single-target
    TemporalParadoxVisual = 15862, // Helper->self, no cast, range 10 width 8 rect, explodes when entering rect
    TemporalParadox = 15863, // Helper->player, no cast, range 40 circle
    MagitekRay = 15859, // Helper->self, no cast, range 40 width 8 rect
    TemporalFlow = 15861, // Boss->self, no cast, single-target

    DefensiveArray = 15858, // Boss->self, 5.0s cast, single-target
    ArtificialGravity = 15865, // Boss->self, 1.0s cast, single-target
    HighGravity = 15866, // Helper->location, 6.0s cast, range 1 circle, expanding AOE, max size 8
    RailCannon = 15867, // Boss->player, 3.0s cast, single-target
    Magicrystal = 15884, // Boss->self, 1.0s cast, single-target
    ShatteredCrystal = 17306, // Helper->player, 6.0s cast, range 5 circle, spread
    HighTensionDischarger = 15868, // Boss->self, 3.0s cast, range 40 circle
}

class HighTensionDischarger(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HighTensionDischarger));
class RailCannon(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.RailCannon));
class HighGravity(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HighGravity), new AOEShapeCircle(8));
class ShatteredCrystal(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ShatteredCrystal), 5);

class TemporalParadoxMagitekRay(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rectShort = new(10, 4);
    private static readonly AOEShapeRect rectLong = new(40, 4);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Lasers)
        {
            _aoes.Add(new(rectShort, actor.Position, actor.Rotation, WorldState.FutureTime(6.5f)));
            _aoes.Add(new(rectLong, actor.Position, actor.Rotation, WorldState.FutureTime(14.3f))); // actual activation time varies a lot, depending on which mechanic it gets paired with. this is the lowest time i found in my log.
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TemporalParadox or AID.MagitekRay)
            _aoes.RemoveAll(x => x.Origin == caster.Position);
    }
}

class D073TheTycoonStates : StateMachineBuilder
{
    public D073TheTycoonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HighTensionDischarger>()
            .ActivateOnEnter<RailCannon>()
            .ActivateOnEnter<HighGravity>()
            .ActivateOnEnter<ShatteredCrystal>()
            .ActivateOnEnter<TemporalParadoxMagitekRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 655, NameID = 8167)]
public class D073TheTycoon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(0, -329), 19.5f / MathF.Cos(MathF.PI / 36), 36)], [new Rectangle(new(0, -309), 20, 1.25f)]);
}
