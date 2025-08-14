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

sealed class HighTensionDischarger(BossModule module) : Components.RaidwideCast(module, (uint)AID.HighTensionDischarger);
sealed class RailCannon(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RailCannon);
sealed class HighGravity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HighGravity, 8f);
sealed class ShatteredCrystal(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShatteredCrystal, 5f);

sealed class TemporalParadoxMagitekRay(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rectShort = new(10f, 4f);
    private static readonly AOEShapeRect rectLong = new(40f, 4f);
    private readonly List<AOEInstance> _aoes = new(12);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        void AddAOE(AOEShapeRect shape, double delay) => _aoes.Add(new(shape, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(delay)));
        if (actor.OID == (uint)OID.Lasers)
        {
            AddAOE(rectShort, 6.5d);
            AddAOE(rectLong, 14.3d); // actual activation time varies a lot, depending on which mechanic it gets paired with. this is the lowest time i found in my log.
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TemporalParadox or (uint)AID.MagitekRay)
        {
            var count = _aoes.Count - 1;
            var pos = caster.Position;
            for (var i = count; i >= 0; --i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                }
            }
        }
    }
}

sealed class D073TheTycoonStates : StateMachineBuilder
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
public sealed class D073TheTycoon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(default, -329f), 19.5f * CosPI.Pi36th, 36)], [new Rectangle(new(default, -309f), 20, 1.25f)]);
}
