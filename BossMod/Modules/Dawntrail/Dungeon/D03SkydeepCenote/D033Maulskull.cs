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
    DeepThunderRepeat = 36690, // Helper->self, no cast, range 6 circle
    BigBurst = 36693, // Helper->self, no cast, range 60 circle, tower fail

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

class Stonecarver(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeRect rect = new(40f, 10f);
    private static readonly WDir offset = new(0f, 20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count != 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Stonecarver1:
            case (uint)AID.Stonecarver2:
            case (uint)AID.Stonecarver3:
            case (uint)AID.Stonecarver4:
                AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                if (AOEs.Count == 2)
                    AOEs.SortBy(x => x.Activation);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.Stonecarver1:
                case (uint)AID.Stonecarver2:
                case (uint)AID.Stonecarver3:
                case (uint)AID.Stonecarver4:
                    AOEs.RemoveAt(0);
                    break;
            }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (AOEs.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center + offset, Arena.Center - offset, 2f), AOEs[0].Activation);
    }
}

class Shatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rectCenter = new(40f, 10f), rectSides = new(42f, 11f, 4f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var a = _aoes[i];
            aoes[i] = new(a.Shape, a.Origin, a.Rotation, a.Activation, Risky: a.Activation.AddSeconds(-6d) <= WorldState.CurrentTime);
        }
        return aoes;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, WPos pos, Angle rot) => _aoes.Add(new(shape, WPos.ClampToGrid(pos), rot, Module.CastFinishAt(spell, 15.1f)));
        switch (spell.Action.ID)
        {
            case (uint)AID.MaulworkFirstCenter:
            case (uint)AID.MaulworkSecondCenter:
                AddAOE(rectCenter, caster.Position, spell.Rotation);
                break;
            case (uint)AID.MaulworkFirstSides:
            case (uint)AID.MaulworkSecondSides:
                AddAOE(rectSides, new(91.564f, caster.Position.Z), -17.004f.Degrees());
                AddAOE(rectSides, new(108.436f, caster.Position.Z), 16.999f.Degrees());
                break;
            case (uint)AID.ShatterCenter:
            case (uint)AID.ShatterLR1:
            case (uint)AID.ShatterLR2:
                _aoes.Clear();
                break;
        }
    }
}

abstract class Impact(BossModule module, AID aid, float distance) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(aid), distance, stopAfterWall: true);

class Impact1(BossModule module) : Impact(module, AID.Impact1, 18f)
{
    private static readonly Angle halfAngle = 30f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Position, 10f, 12f, default, halfAngle), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class Impact2(BossModule module) : Impact(module, AID.Impact2, 18f)
{
    private static readonly Angle halfAngle = 20f.Degrees();
    private readonly Stonecarver _aoe = module.FindComponent<Stonecarver>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _aoe.AOEs.Count != 0 && _aoe.AOEs[0].Check(pos) || !Module.InBounds(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Position, 10f, 12f, default, halfAngle), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class Impact3(BossModule module) : Impact(module, AID.Impact3, 20f)
{
    private static readonly Angle halfAngle = 10f.Degrees(), direction = 135f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Position, 10f, 15f, (source.Position.X == 90f ? 1f : -1f) * direction, halfAngle), Module.CastFinishAt(source.CastInfo));
        }
    }
}

abstract class Crush(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 10f);
class ColossalImpact(BossModule module) : Crush(module, AID.ColossalImpact);
class Skullcrush1(BossModule module) : Crush(module, AID.Skullcrush1);
class Skullcrush2(BossModule module) : Crush(module, AID.Skullcrush2);

class DestructiveHeat(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DestructiveHeat), 6f)
{
    private WPos origin;
    private readonly Impact1 _kb1 = module.FindComponent<Impact1>()!;
    private readonly Impact2 _kb2 = module.FindComponent<Impact2>()!;
    private readonly Impact3 _kb3 = module.FindComponent<Impact3>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Spreads.Count != 0)
        {
            var source1 = _kb1.Casters.Count != 0 ? _kb1.Casters[0] : null;
            var source2 = _kb2.Casters.Count != 0 ? _kb2.Casters[0] : null;
            var source3 = _kb3.Casters.Count != 0 ? _kb3.Casters[0] : null;
            var knockback = source1 != null || source2 != null || source3 != null;
            if (source1 != default)
                origin = new(100f, -400f);
            else if (source2 != default)
                origin = source2.Position;
            else if (source3 != default)
                origin = source3.Position;
            if (!knockback)
            {
                base.AddAIHints(slot, actor, assignment, hints);
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(origin, 15f), Spreads[0].Activation);
            }
            else
            { }
        }
    }
}

class Landing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Landing), 8f);

abstract class DeepThunder(BossModule module, AID aid) : Components.CastTowers(module, ActionID.MakeSpell(aid), 6f, 4, 4);
class DeepThunder1(BossModule module) : DeepThunder(module, AID.DeepThunderTower1);
class DeepThunder2(BossModule module) : DeepThunder(module, AID.DeepThunderTower2);

class WroughtFire(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.WroughtFire), new AOEShapeCircle(6f), true, tankbuster: true);
class BuildingHeat(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BuildingHeat), 6f, 4, 4);
class Ashlayer(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Ashlayer));

class D033MaulskullStates : StateMachineBuilder
{
    public D033MaulskullStates(BossModule module) : base(module)
    {
        TrivialPhase()
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
public class D033Maulskull(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, -430f), new ArenaBoundsSquare(20f));
