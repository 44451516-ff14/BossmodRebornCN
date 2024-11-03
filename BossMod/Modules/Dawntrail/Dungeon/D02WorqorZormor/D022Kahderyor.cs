namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D022Kahderyor;

public enum OID : uint
{
    Boss = 0x415D, // R7.0
    CrystallineDebris = 0x415E, // R1.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WindUnbound = 36282, // Boss->self, 5.0s cast, range 60 circle

    CrystallineCrushVisual = 36285, // Boss->location, 5.0+1.0s cast, single-target
    CrystallineCrush = 36153, // Helper->self, 6.3s cast, range 6 circle, tower

    WindShotVisual1 = 36284, // Boss->self, 5.5s cast, single-target
    WindshotVisual2 = 36300, // Helper->player, no cast, single-target
    WindShot = 36296, // Helper->players, 6.0s cast, range 5-10 donut, stack

    EarthenShotVisual1 = 36283, // Boss->self, 5.0+0.5s cast, single-target
    EarthenShotVisual2 = 36299, // Helper->player, no cast, single-target
    EarthenShot = 36295, // Helper->player, 6.0s cast, range 6 circle, spread

    CrystallineStormVisual = 36286, // Boss->self, 3.0+1.0s cast, single-target
    CrystallineStorm = 36290, // Helper->self, 4.0s cast, range 50 width 2 rect

    SeedCrystalsVisual = 36291, // Boss->self, 4.5+0.5s cast, single-target
    SeedCrystals = 36298, // Helper->player, 5.0s cast, range 6 circle, spread

    SharpenedSights = 36287, // Boss->self, 3.0s cast, single-target
    EyeOfTheFierce = 36297, // Helper->self, 5.0s cast, range 60 circle

    StalagmiteCircleVisual = 36288, // Boss->self, 5.0s cast, single-target
    StalagmiteCircle = 36293, // Helper->self, 5.0s cast, range 15 circle

    CyclonicRingVisual = 36289, // Boss->self, 5.0s cast, single-target
    CyclonicRing = 36294 // Helper->self, 5.0s cast, range 8-40 donut
}

public enum IconID : uint
{
    Stackmarker = 62, // Helper
    WindShot = 511, // player
    EarthenShot = 169, // player
    SeedCrystals = 311 // player
}

class WindEarthShot(BossModule module) : Components.GenericAOEs(module)
{
    private const string risk2Hint = "Walk into a crystal line!";
    private const string stayHint = "Stay inside crystal line!";
    private static readonly AOEShapeDonut donut = new(8, 50);
    private static readonly AOEShapeCircle circle = new(15);
    private static readonly Angle am120 = -119.997f.Degrees();
    private static readonly Angle am30 = -29.996f.Degrees();
    private static readonly Angle a80 = 80.001f.Degrees();
    private static readonly Angle am100 = -99.996f.Degrees();
    private static readonly WPos pos1 = new(-43, -57);
    private static readonly WPos pos2 = new(-63, -57);
    private static readonly WPos pos3 = new(-53, -47);
    private static readonly WPos pos4 = new(-53, -67);
    private const int Length = 50;
    private const uint State = 0x00800040;
    private static readonly AOEShapeCustom ENVC21Inverted = CreateShape(pos2, pos3, pos4, am30, am120, a80, 1, true);
    private static readonly AOEShapeCustom ENVC21 = CreateShape(pos2, pos3, pos4, am30, am120, a80, 7);
    private static readonly AOEShapeCustom ENVC20Inverted = CreateShape(pos1, pos3, pos4, am30, am100, am120, 1, true);
    private static readonly AOEShapeCustom ENVC20 = CreateShape(pos1, pos3, pos4, am30, am100, am120, 7);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        var activation = WorldState.FutureTime(5.9f);

        if (state is State or 0x00200010)
        {
            AOEShape shape = state == State ? donut : circle;
            AddAOE(index, state, shape, activation);
        }
        else if (state is 0x02000001 or 0x04000004 or State or 0x08000004 or 0x01000001)
            _aoe = null;
    }

    private void AddAOE(byte index, uint state, AOEShape shape, DateTime activation)
    {
        var color = state == State ? Colors.SafeFromAOE : Colors.AOE;
        _aoe = index switch
        {
            0x1E => new(shape, pos1, default, activation),
            0x1F => new(shape, pos2, default, activation),
            0x20 => new(state == State ? ENVC20Inverted : ENVC20, Arena.Center, default, activation, color),
            0x21 => new(state == State ? ENVC21Inverted : ENVC21, Arena.Center, default, activation, color),
            _ => _aoe
        };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any(c => !(c.Shape == ENVC20Inverted || c.Shape == ENVC21Inverted)))
            base.AddHints(slot, actor, hints);
        else if (ActiveAOEs(slot, actor).Any(c => (c.Shape == ENVC20Inverted || c.Shape == ENVC21Inverted) && !c.Check(actor.Position)))
            hints.Add(risk2Hint);
        else if (ActiveAOEs(slot, actor).Any(c => (c.Shape == ENVC20Inverted || c.Shape == ENVC21Inverted) && c.Check(actor.Position)))
            hints.Add(stayHint, false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var activeAOEs = ActiveAOEs(slot, actor).ToList();
        var containsENVC20 = activeAOEs.Any(c => c.Shape == ENVC20);
        var containsENVC21 = activeAOEs.Any(c => c.Shape == ENVC21);

        if (containsENVC20 || containsENVC21)
        {
            var forbiddenZone = actor.Role != Role.Tank
                ? ShapeDistance.Rect(Arena.Center, containsENVC20 ? -40.Degrees() : 0.Degrees(), 20, containsENVC20 ? 1 : 10, 20)
                : ShapeDistance.InvertedCircle(Arena.Center, 12);

            hints.AddForbiddenZone(forbiddenZone, activeAOEs.FirstOrDefault().Activation);
        }
    }

    private static AOEShapeCustom CreateShape(WPos pos1, WPos pos2, WPos pos3, Angle angle1, Angle angle2, Angle angle3, int halfWidth, bool inverted = false)
        => new([new Rectangle(pos1, halfWidth, Length, angle1), new Rectangle(pos2, halfWidth, Length, angle2), new Rectangle(pos3, halfWidth, Length, angle3)],
        InvertForbiddenZone: inverted);
}

class WindShotStack(BossModule module) : Components.DonutStack(module, ActionID.MakeSpell(AID.WindShot), (uint)IconID.WindShot, 5, 10, 6, 4, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ActiveStacks.Any())
            return;
        var comp = Module.FindComponent<WindEarthShot>()!.ActiveAOEs(slot, actor).ToList();
        var forbidden = new List<Func<WPos, float>>();
        foreach (var c in Raid.WithoutSlot().Exclude(actor).Where(x => comp.Any(c => c.Shape is AOEShapeDonut && !c.Check(x.Position) || c.Shape is AOEShapeCustom && c.Check(x.Position))))
            forbidden.Add(ShapeDistance.InvertedCircle(c.Position, Donut.InnerRadius * 0.33f));
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), ActiveStacks.FirstOrDefault().Activation);
    }
}

class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WindUnbound));
class CrystallineCrush(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.CrystallineCrush), 6, 4, 4);
class EarthenShot(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EarthenShot), 6);
class StalagmiteCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StalagmiteCircle), new AOEShapeCircle(15));
class CrystallineStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrystallineStorm), new AOEShapeRect(25, 1, 25));
class CyclonicRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CyclonicRing), new AOEShapeDonut(8, 40));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFierce));
class SeedCrystals(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SeedCrystals), 6);

class D022KahderyorStates : StateMachineBuilder
{
    public D022KahderyorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindShotStack>()
            .ActivateOnEnter<WindEarthShot>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<CrystallineStorm>()
            .ActivateOnEnter<CrystallineCrush>()
            .ActivateOnEnter<EarthenShot>()
            .ActivateOnEnter<StalagmiteCircle>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<SeedCrystals>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12703)]
public class D022Kahderyor(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly WPos arenaCenter = new(-53, -57);
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Circle(arenaCenter, 19.5f)], [new Rectangle(new(-72.5f, -57), 20, 0.75f, 90.Degrees()), new Rectangle(new(-53, -37), 20, 1.5f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.CrystallineDebris), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.CrystallineDebris => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
