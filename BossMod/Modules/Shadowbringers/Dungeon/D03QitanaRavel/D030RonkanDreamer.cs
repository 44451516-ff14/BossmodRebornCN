namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D030RonkanDreamer;

public enum OID : uint
{
    Boss = 0x2A40, //R=1.8
    Helper = 0x2E8, //R=0.5
    //trash that can be pulled into miniboss room
    RonkanVessel = 0x28DD, //R=3.0
    RonkanIdol = 0x28DC, //R=2.04
    RonkanThorn = 0x28E3 //R=2.4
}

public enum AID : uint
{
    WrathOfTheRonka = 17223, // 2A40->self, 6.0s cast, single-target
    WrathOfTheRonkaLong = 15918, // 28E8->self, no cast, range 35 width 8 rect
    WrathOfTheRonkaShort = 15916, // 28E8->self, no cast, range 12 width 8 rect
    WrathOfTheRonkaMedium = 15917, // 28E8->self, no cast, range 22 width 8 rect
    RonkanFire = 17433, // 2A40->player, 1.0s cast, single-target
    RonkanAbyss = 17387, // 2A40->location, 3.0s cast, range 6 circle
    AutoAttack = 872, // 28DD/28DC->player, no cast, single-target
    AutoAttack2 = 17949, // 28E3->player, no cast, single-target
    BurningBeam = 15923 // 28E3->self, 3.0s cast, range 15 width 4 rect
}

public enum TetherID : uint
{
    StatueActivate = 37 // 28E8->Boss
}

class RonkanFire(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.RonkanFire));
class RonkanAbyss(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RonkanAbyss), 6);

class WrathOfTheRonka(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rectShort = new(12, 4);
    private static readonly AOEShapeRect rectMedium = new(22, 4);
    private static readonly AOEShapeRect rectLong = new(35, 4);
    private static readonly (WPos Position, AOEShapeRect Shape)[] aoeMap =
        [(new(-17, 627), rectMedium), (new(17, 642), rectMedium),
        (new(-17, 436), rectMedium), (new(17, 421), rectMedium),
        (new(-17, 642), rectShort), (new(17, 627), rectShort),
        (new(17, 436), rectShort), (new(-17, 421), rectShort)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.StatueActivate)
        {
            var aoeShape = GetAOEShape(source.Position) ?? rectLong;
            _aoes.Add(new(aoeShape, source.Position, source.Rotation, WorldState.FutureTime(6)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WrathOfTheRonkaShort or AID.WrathOfTheRonkaMedium or AID.WrathOfTheRonkaLong)
            _aoes.Clear();
    }

    private static AOEShapeRect? GetAOEShape(WPos position)
    {
        foreach (var (pos, shape) in aoeMap)
            if (position.AlmostEqual(pos, 1))
                return shape;
        return null;
    }
}

class BurningBeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningBeam), new AOEShapeRect(15, 2));

class D030RonkanDreamerStates : StateMachineBuilder
{
    public D030RonkanDreamerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RonkanFire>()
            .ActivateOnEnter<RonkanAbyss>()
            .ActivateOnEnter<WrathOfTheRonka>()
            .ActivateOnEnter<BurningBeam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8639)]
public class D030RonkanDreamer(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.Z > 550 ? arena1.Center : arena2.Center, primary.Position.Z > 550 ? arena1 : arena2)
{
    private static readonly WPos[] vertices1 = [new(-3.82f, 640.12f), new(-3.95f, 640.67f), new(-3.98f, 646.62f), new(-6.08f, 646.51f), new(-6.58f, 644.99f),
    new(-6.28f, 643.05f), new(-6.38f, 641.22f), new(-6.60f, 639.71f), new(-6.29f, 639.34f), new(-4.27f, 639.03f)];
    private static readonly WPos[] vertices2 = [new(6.61f, 625.6f), new(6.3f, 627.46f), new(6.32f, 629.1f), new(6.54f, 630.78f), new(6.28f, 631.19f),
    new(4.28f, 631.45f), new(3.83f, 630.55f), new(3.82f, 629.67f), new(3.96f, 623.91f), new(6.07f, 623.83f)];
    private static readonly WPos[] vertices3 = [new(-3.77f, 418.91f), new(-3.94f, 419.58f), new(-3.97f, 425.34f), new(-6.11f, 425.47f), new(-6.59f, 423.67f),
    new(-6.38f, 421.66f), new(-6.38f, 420.01f), new(-6.53f, 418.49f), new(-6.27f, 418.12f), new(-4, 417.82f),
    new(-3.77f, 418.91f)];
    private static readonly WPos[] vertices4 = [new(6.09f, 432.98f), new(6.64f, 434.58f), new(6.37f, 436.54f), new(6.31f, 438.11f), new(6.50f, 439.59f),
    new(6.48f, 440.24f), new(4.29f, 440.32f), new(3.81f, 439.47f), new(3.95f, 438.75f), new(4.00f, 432.74f),
    new(6.09f, 432.98f)];
    private static readonly ArenaBoundsComplex arena1 = new([new Rectangle(new(0, 640), 17.5f, 23)], [new PolygonCustom(vertices1), new PolygonCustom(vertices2)]);
    private static readonly ArenaBoundsComplex arena2 = new([new Rectangle(new(0, 434.5f), 17.5f, 24)], [new PolygonCustom(vertices3), new PolygonCustom(vertices4)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.RonkanVessel).Concat([PrimaryActor]).Concat(Enemies(OID.RonkanThorn)).Concat(Enemies(OID.RonkanIdol)));
    }
}
