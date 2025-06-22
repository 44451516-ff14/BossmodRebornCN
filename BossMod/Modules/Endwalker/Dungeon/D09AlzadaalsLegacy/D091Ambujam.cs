namespace BossMod.Endwalker.Dungeon.D09AlzadaalsLegacy.D091Ambujam;

public enum OID : uint
{
    Boss = 0x3879, // R=9.0
    CyanTentacle = 0x387B, // R2.400, x1
    ScarletTentacle = 0x387A, // R2.400, x1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BigWave = 28512, // Boss->self, 5.0s cast, range 40 circle

    CorrosiveFountain = 29556, // Helper->self, 7.0s cast, range 8 circle, knockback 10, away from source
    ToxicFountainVisual = 29466, // Boss->self, 4.0s cast, single-target
    ToxicFountain = 29467, // Helper->self, 7.0s cast, range 8 circle

    TentacleDig1 = 28501, // Boss->self, 3.0s cast, single-target
    TentacleDig2 = 28505, // Boss->self, 3.0s cast, single-target

    CorrosiveVenomVisual = 29157, // CyanTentacle->self, no cast, single-target
    CorrosiveVenom = 29158, // Helper->self, 2.5s cast, range 21 circle, knockback 10, away from source
    ToxinShowerVisual = 28507, // ScarletTentacle->self, no cast, single-target
    ToxinShower = 28508, // Helper->self, 2.5s cast, range 21 circle

    ModelStateChange1 = 28502, // Boss->self, no cast, single-target
    ModelStateChange2 = 28506 // Boss->self, no cast, single-target
}

class ToxinShowerCorrosiveVenom(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(21);
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly Dictionary<byte, Dictionary<uint, WPos>> _statePositions = new()
    {
        {0x11, new Dictionary<uint, WPos>
            {{ 0x00080004, new(117f, -97f) }, { 0x00800004, new(131f, -83f) },
            { 0x00200004, new(131f, -97f) }, { 0x02000004, new(117f, -83f) }}},
        {0x10, new Dictionary<uint, WPos>
            {{ 0x00200004, new(109f, -90f) }, { 0x02000004, new(139f, -90f) },
            { 0x00080004, new(124f, -75f) }, { 0x00800004, new(124f, -105f) }}}
    };

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (_statePositions.TryGetValue(index, out var statePosition) && statePosition.TryGetValue(state, out var position))
        {
            var activation = WorldState.FutureTime(10.5d);
            _aoes.Add(new(circle, WPos.ClampToGrid(position), default, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.CorrosiveVenom or (uint)AID.ToxinShower)
            _aoes.RemoveAt(0);
    }
}
class ToxicCorrosiveFountain(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ToxicFountain, (uint)AID.CorrosiveFountain], 8f, 10, 13);

class BigWave(BossModule module) : Components.RaidwideCast(module, (uint)AID.BigWave);

class D091AmbujamStates : StateMachineBuilder
{
    public D091AmbujamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ToxicCorrosiveFountain>()
            .ActivateOnEnter<ToxinShowerCorrosiveVenom>()
            .ActivateOnEnter<BigWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 844, NameID = 11241)]
public class D091Ambujam(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly ArenaBoundsComplex DefaultBounds = new([new Circle(new(124f, -90f), 19.5f)], [new Rectangle(new(124f, -110.25f), 20f, 2f), new Rectangle(new(124f, -69.5f), 20f, 2f)]);
}
