namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D033Maulskull;

public enum OID : uint
{
    Boss = 0x41C7, // R19.98
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36678, // Boss->player, no cast, single-target

    StonecarverVisual1 = 36668, // Boss->self, 8.0s cast, single-target
    StonecarverVisual2 = 36669, // Boss->self, 8.0s cast, single-target
    StonecarverVisual3 = 36672, // Boss->self, no cast, single-target
    StonecarverVisual4 = 36673, // Boss->self, no cast, single-target
    StonecarverVisual5 = 36699, // Boss->self, no cast, single-target
    StonecarverVisual6 = 36700, // Boss->self, no cast, single-target
    Stonecarver1 = 36670, // Helper->self, 9.0s cast, range 40 width 20 rect
    Stonecarver2 = 36671, // Helper->self, 11.5s cast, range 40 width 20 rect
    Stonecarver3 = 36696, // Helper->self, 11.1s cast, range 40 width 20 rect
    Stonecarver4 = 36697, // Helper->self, 13.6s cast, range 40 width 20 rect

    Impact1 = 36677, // Helper->self, 7.0s cast, range 60 circle, knockback 18, away from origin
    Impact2 = 36667, // Helper->self, 9.0s cast, range 60 circle, knockback 18, away from origin
    Impact3 = 36707, // Helper->self, 8.0s cast, range 60 circle, knockback 20, away from origin

    SkullCrushVisual1 = 36674, // Boss->self, no cast, single-target
    SkullcrushVisual2 = 36675, // Boss->self, 5.0+2.0s cast, single-target
    SkullcrushVisual3 = 38664, // Boss->self, no cast, single-target
    Skullcrush1 = 36676, // Helper->self, 7.0s cast, range 10 circle
    Skullcrush2 = 36666, // Helper->self, 9.0s cast, range 10 circle

    Charcore = 36708, // Boss->self, no cast, single-target
    DestructiveHeat = 36709, // Helper->players, 7.0s cast, range 6 circle

    MaulworkFirstCenter = 36679, // Boss->self, 5.0s cast, single-target
    MaulworkFirstSides = 36681, // Boss->self, 5.0s cast, single-target
    MaulworkSecondSides = 36682, // Boss->self, 5.0s cast, single-target
    MaulworkSecondCenter = 36680, // Boss->self, 5.0s cast, single-target
    ShatterCenter = 36684, // Helper->self, 3.0s cast, range 40 width 20 rect
    ShatterLR1 = 36685, // Helper->self, 3.0s cast, range 45 width 22 rect
    ShatterLR2 = 36686, // Helper->self, 3.0s cast, range 45 width 22 rect
    Landing = 36683, // Helper->location, 3.0s cast, range 8 circle

    DeepThunderTower1 = 36688, // Helper->self, 9.0s cast, range 6 circle
    DeepThunderTower2 = 36689, // Helper->self, 11.0s cast, range 6 circle
    DeepThunderVisual1 = 36687, // Boss->self, 6.0s cast, single-target
    DeepThunderVisual2 = 36691, // Boss->self, no cast, single-target
    DeepThunderVisual3 = 36692, // Boss->self, no cast, single-target
    DeepThunder2 = 36690, // Helper->self, no cast, range 6 circle

    RingingBlows1 = 36694, // Boss->self, 7.0+2.0s cast, single-target
    RingingBlows2 = 36695, // Boss->self, 7.0+2.0s cast, single-target

    WroughtFireVisual = 39121, // Boss->self, 4.0+1.0s cast, single-target
    WroughtFire = 39122, // Helper->player, 5.0s cast, range 6 circle

    ColossalImpactVisual1 = 36704, // Boss->self, 6.0+2.0s cast, single-target
    ColossalImpactVisual2 = 36705, // Boss->self, 6.0+2.0s cast, single-target
    ColossalImpact = 36706, // Helper->self, 8.0s cast, range 10 circle

    BuildingHeat = 36710, // Helper->players, 7.0s cast, range 6 circle

    AshlayerVisual = 36711, // Boss->self, 3.0+2.0s cast, single-target
    Ashlayer = 36712 // Helper->self, no cast, range 60 circle
}

class StayInBounds(BossModule module) : BossComponent(module)
{
    private static readonly WDir offset = new(0, 19);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center + offset, Arena.Center - offset, 19));
    }
}

class Stonecarver(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 10);
    private static readonly HashSet<AID> aids = [AID.Stonecarver1, AID.Stonecarver2, AID.Stonecarver3, AID.Stonecarver4];
    private static readonly WDir offset = new(0, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (aids.Contains((AID)spell.Action.ID))
        {
            _aoes.Add(new(rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && aids.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center + offset, Arena.Center - offset, 2), _aoes[0].Activation);
    }
}

class Shatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect rectCenter = new(40, 10);
    private static readonly AOEShapeRect rectSides = new(42, 11, 4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _aoes)
            yield return new(c.Shape, c.Origin, c.Rotation, c.Activation, Risky: c.Activation.AddSeconds(-6) <= WorldState.CurrentTime);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 15.1f);
        switch ((AID)spell.Action.ID)
        {
            case AID.MaulworkFirstCenter:
            case AID.MaulworkSecondCenter:
                _aoes.Add(new(rectCenter, caster.Position, spell.Rotation, activation));
                break;
            case AID.MaulworkFirstSides:
            case AID.MaulworkSecondSides:
                _aoes.Add(new(rectSides, new(91.564f, caster.Position.Z), -17.004f.Degrees(), activation));
                _aoes.Add(new(rectSides, new(108.436f, caster.Position.Z), 16.999f.Degrees(), activation));
                break;
            case AID.ShatterCenter:
            case AID.ShatterLR1:
            case AID.ShatterLR2:
                _aoes.Clear();
                break;
        }
    }
}

abstract class Impact(BossModule module, AID aid, int distance) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(aid), distance, stopAfterWall: true);

class Impact1(BossModule module) : Impact(module, AID.Impact1, 18)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Origin, 10, 12, default, 30.Degrees()), source.Activation);
    }
}

class Impact2(BossModule module) : Impact(module, AID.Impact2, 18)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Stonecarver>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation) && z.Risky) ?? false) || !Arena.InBounds(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Origin, 10, 12, default, 20.Degrees()), source.Activation);
    }
}

class Impact3(BossModule module) : Impact(module, AID.Impact3, 20)
{
    private static readonly Angle halfAngle = 10.Degrees();
    private static readonly Angle direction = 135.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
        {
            if (source.Origin.X == 90)
                hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Origin, 10, 15, direction, halfAngle), source.Activation);
            else if (source.Origin.X == 110)
                hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Origin, 10, 15, -direction, halfAngle), source.Activation);
        }
    }
}

abstract class Crush(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(10));
class ColossalImpact(BossModule module) : Crush(module, AID.ColossalImpact);
class Skullcrush1(BossModule module) : Crush(module, AID.Skullcrush1);
class Skullcrush2(BossModule module) : Crush(module, AID.Skullcrush2);

class DestructiveHeat(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DestructiveHeat), 6)
{
    private WPos origin;
    private readonly Impact1 _kb1 = module.FindComponent<Impact1>()!;
    private readonly Impact2 _kb2 = module.FindComponent<Impact2>()!;
    private readonly Impact3 _kb3 = module.FindComponent<Impact3>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveSpreads.Any())
        {
            var source1 = _kb1.Sources(slot, actor).FirstOrDefault();
            var source2 = _kb2.Sources(slot, actor).FirstOrDefault();
            var source3 = _kb3.Sources(slot, actor).FirstOrDefault();
            var knockback = source1 != default || source2 != default || source3 != default;
            if (source1 != default)
                origin = actor.Role is Role.Melee or Role.Ranged ? new(100, -400) : source1.Origin;
            else if (source2 != default)
                origin = source2.Origin;
            else if (source3 != default)
                origin = source3.Origin;
            if (!knockback)
            {
                base.AddAIHints(slot, actor, assignment, hints);
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(origin, 15), ActiveSpreads.FirstOrDefault().Activation);
            }
            else
            { }
        }
    }
}

class Landing(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Landing), 8);

abstract class DeepThunder(BossModule module, AID aid) : Components.CastTowers(module, ActionID.MakeSpell(aid), 6, 4, 4);
class DeepThunder1(BossModule module) : DeepThunder(module, AID.DeepThunderTower1);
class DeepThunder2(BossModule module) : DeepThunder(module, AID.DeepThunderTower2);

class WroughtFire(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.WroughtFire), new AOEShapeCircle(6), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class BuildingHeat(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BuildingHeat), 6, 4);
class Ashlayer(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Ashlayer));

class D033MaulskullStates : StateMachineBuilder
{
    public D033MaulskullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StayInBounds>()
            .ActivateOnEnter<Stonecarver>()
            .ActivateOnEnter<Impact1>()
            .ActivateOnEnter<Impact2>()
            .ActivateOnEnter<Impact3>()
            .ActivateOnEnter<ColossalImpact>()
            .ActivateOnEnter<Skullcrush1>()
            .ActivateOnEnter<Skullcrush2>()
            .ActivateOnEnter<DestructiveHeat>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<DeepThunder1>()
            .ActivateOnEnter<DeepThunder2>()
            .ActivateOnEnter<WroughtFire>()
            .ActivateOnEnter<BuildingHeat>()
            .ActivateOnEnter<Ashlayer>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12728)]
public class D033Maulskull(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, -430), new ArenaBoundsSquare(20));
