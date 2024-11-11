namespace BossMod.Shadowbringers.Dungeon.D07Twinning.D072Mithridates;

public enum OID : uint
{
    Boss = 0x2803, // R3.6
    Levinball = 0x2804, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ThunderBeam = 15853, // Boss->player, 3.0s cast, single-target
    ElectricDischarge = 15856, // Boss->self, 1.0s cast, single-target
    Shock = 15857, // Levinball->self, no cast, range 6 circle

    LaserbladeVisual = 15851, // Boss->self, 5.0s cast, single-target
    Laserblade = 15852, // Helper->self, 5.5s cast, range 50 width 8 rect

    AllaganThunderVisual = 15854, // Boss->self, 4.5s cast, single-target
    AllaganThunder = 15855, // Helper->player, 5.0s cast, range 5 circle
}

class ThunderBeam(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ThunderBeam));
class Laserblade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Laserblade), new AOEShapeRect(25, 4, 25));
class AllaganThunder(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AllaganThunder), 5);

class Shock(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Levinball)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(7.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Shock)
            _aoes.Clear();
    }
}

class D072MithridatesStates : StateMachineBuilder
{
    public D072MithridatesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThunderBeam>()
            .ActivateOnEnter<Laserblade>()
            .ActivateOnEnter<AllaganThunder>()
            .ActivateOnEnter<Shock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 655, NameID = 8165)]
public class D072Mithridates(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(200, 68), 19.5f / MathF.Cos(MathF.PI / 36), 36)],
    [new Rectangle(new(200, 88), 20, 1.25f), new Rectangle(new(200, 48), 20, 1.25f)]);
}
