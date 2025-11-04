namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD60AncestralMaliktender;

public enum OID : uint
{
    AncestralMaliktender = 0x49DE, // R4.0
    SabotenderAmir = 0x49DF, // R1.2
    FlowertenderAmirah = 0x49E0, // R2.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45130, // AncestralMaliktender->player, no cast, single-target

    SpineshotVisual = 44866, // AncestralMaliktender->self, 5.0s cast, single-target
    Spineshot = 44872, // Helper->self, no cast, range 60 60-degree cone

    SpinningNeedlesVisualCCW = 44867, // AncestralMaliktender->self, 5.0s cast, single-target
    SpinningNeedlesVisualCW = 44868, // AncestralMaliktender->self, 5.0s cast, single-target
    SpinningNeedlesVisual1 = 44870, // AncestralMaliktender->self, no cast, single-target
    SpinningNeedlesVisual2 = 44871, // AncestralMaliktender->self, no cast, single-target
    SpinningNeedlesVisual3 = 44869, // AncestralMaliktender->self, no cast, single-target
    SpinningNeedles = 44909, // Helper->self, no cast, range 60 60-degree cone

    BranchOut = 44857, // AncestralMaliktender->self, 3.0s cast, single-target

    LongRangeNeedlesVisual = 44865, // FlowertenderAmirah->self, 12.7s cast, single-target
    LongRangeNeedles1 = 44859, // FlowertenderAmirah->self, 5.0s cast, range 30 width 30 rect
    LongRangeNeedles2 = 44861, // FlowertenderAmirah->self, 1.0s cast, range 30 width 30 rect

    SeveralThousandNeedlesVisual = 44864, // SabotenderAmir->self, 12.7s cast, single-target
    SeveralThousandNeedles1 = 44858, // SabotenderAmir->self, 5.0s cast, range 10 width 10 rect
    SeveralThousandNeedles2 = 44860, // SabotenderAmir->self, 1.0s cast, range 10 width 10 rect
    OneStoneMarch = 44862, // AncestralMaliktender->self, 14.0s cast, single-target
    TwoStoneMarch = 44863, // AncestralMaliktender->self, 14.0s cast, single-target
}

[SkipLocalsInit]
sealed class SeveralThousandNeedles : Components.SimpleAOEs
{
    public SeveralThousandNeedles(BossModule module) : base(module, (uint)AID.SeveralThousandNeedles1, new AOEShapeRect(10f, 5f), 6)
    {
        MaxDangerColor = 3;
    }
}

[SkipLocalsInit]
sealed class LongRangeNeedles : Components.SimpleAOEs
{
    public LongRangeNeedles(BossModule module) : base(module, (uint)AID.LongRangeNeedles1, new AOEShapeRect(30f, 15f), 2)
    {
        MaxDangerColor = 1;
    }
}

[SkipLocalsInit]
sealed class Spineshot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeCone cone = new(60f, 30f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SpineshotVisual)
        {
            var rot = spell.Rotation;
            var loc = spell.LocXZ;
            var act = Module.CastFinishAt(spell);
            AddAOE(loc, rot, act);
            AddAOE(loc, rot, act, 180f.Degrees());
            void AddAOE(WPos position, Angle rotation, DateTime activation, Angle offset = default)
            {
                var rotAdj = rotation + offset;
                _aoes.Add(new(cone, position, rotAdj, activation, shapeDistance: cone.Distance(position, rotAdj)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Spineshot)
        {
            _aoes.Clear();
        }
    }
}

[SkipLocalsInit]
sealed class OneTwoStoneMarch(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(8);
    private readonly AOEShapeRect squareSmall = new(10f, 5f), squareBig = new(30f, 15f);
    private int distance;
    private readonly List<Actor> casters = new(8);
    private bool? isCW;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OneStoneMarch:
                distance = 1;
                break;
            case (uint)AID.TwoStoneMarch:
                distance = 2;
                break;
            case (uint)AID.SeveralThousandNeedlesVisual:
            case (uint)AID.LongRangeNeedlesVisual:
                casters.Add(caster);
                InitIfReady();
                break;
            case (uint)AID.SeveralThousandNeedles2: // update AOEs to ensure pixel perfectness
            case (uint)AID.LongRangeNeedles2:
                var count = AOEs.Count;
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                var pos = spell.LocXZ;
                var rot = spell.Rotation;
                var id = caster.InstanceID;
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.ActorID == id)
                    {
                        aoe.Origin = pos;
                        aoe.Rotation = rot;
                        aoe.ShapeDistance = aoe.Shape.Distance(pos, rot);
                        return;
                    }
                }
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            switch (state)
            {
                case 0x00020001u:
                    isCW = true;
                    break;
                case 0x00200010u:
                    isCW = false;
                    break;
                default:
                    return;
            }
            activation = WorldState.FutureTime(16.2d);
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (casters.Count == 8 && isCW is bool dir)
        {
            for (var i = 0; i < 8; ++i)
            {
                var c = casters[i];
                var pos = PredictFinalCenterDiscrete(c.Position, dir, distance, Module.Center);
                var isSmallSquare = c.OID == (uint)OID.SabotenderAmir;
                var finalPos = pos + (isSmallSquare ? 5f : 15f) * new WDir(default, -1f);
                var shape = isSmallSquare ? squareSmall : squareBig;
                var rot = c.Rotation;
                AOEs.Add(new(shape, finalPos, rot, activation, shapeDistance: shape.Distance(finalPos, rot), actorID: c.InstanceID));
            }
            casters.Clear();
            isCW = null;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LongRangeNeedles2)
        {
            AOEs.Clear();
        }
    }

    // quantizes to {±1,±3}, selects ring, rotates index CW/CCW, converts back to world position
    private static WPos PredictFinalCenterDiscrete(WPos casterPos, bool isCW, int distance, WPos arenaCenter)
    {
        // quantize: centers are at ±0.5T and ±1.5T -> scaled to integers ±1 and ±3
        const float tileWidth = 10f;
        var sx = (int)MathF.Round(2f * (casterPos.X - arenaCenter.X) / tileWidth);
        var sz = (int)MathF.Round(2f * (casterPos.Z - arenaCenter.Z) / tileWidth);

        // inner ring: both coordinates at magnitude 1
        var inner = Math.Abs(sx) == 1 && Math.Abs(sz) == 1;

        var n = inner ? 4 : 12;
        var k = inner ? InnerIndexCW(sx, sz) : OuterIndexCW(sx, sz);

        // rotate inside the ring
        var k2 = Mod(k + (isCW ? +distance : -distance), n);

        // convert back to discrete half-tile coordinates
        (var sx2, var sz2) = inner ? InnerCoordCW(k2) : OuterCoordCW(k2);

        // Back to world.
        const float tileHalfWidth = 5f;
        return new(arenaCenter.X + sx2 * tileHalfWidth, arenaCenter.Z + sz2 * tileHalfWidth);
    }

    // modulo that works for negatives
    private static int Mod(int x, int m) => (x % m + m) % m;

    // inner ring CW indices for (sx,sz) ∈ {(-1,-1),(1,-1),(1,1),(-1,1)}
    // order matches top-left → top-right → bottom-right → bottom-left
    private static int InnerIndexCW(int sx, int sz) => (sx, sz) switch
    {
        (-1, -1) => 0,
        (1, -1) => 1,
        (1, 1) => 2,
        (-1, 1) => 3,
        _ => 0
    };

    private static (int sx, int sz) InnerCoordCW(int idx)
    {
        return Mod(idx, 4) switch
        {
            0 => (-1, -1),
            1 => (1, -1),
            2 => (1, 1),
            _ => (-1, 1),
        };
    }

    // outer ring CW indices for (sx,sz) ∈ perimeter {±3,±1}
    // matches: top row L→R (0..3), right col T→B (4..6), bottom row R→L (7..9), left col B→T (10..11)
    private static int OuterIndexCW(int sx, int sz) => (sx, sz) switch
    {
        (-3, -3) => 0,
        (-1, -3) => 1,
        (1, -3) => 2,
        (3, -3) => 3,
        (3, -1) => 4,
        (3, 1) => 5,
        (3, 3) => 6,
        (1, 3) => 7,
        (-1, 3) => 8,
        (-3, 3) => 9,
        (-3, 1) => 10,
        (-3, -1) => 11,
        _ => 0
    };

    private static (int sx, int sz) OuterCoordCW(int idx)
    {
        return Mod(idx, 12) switch
        {
            0 => (-3, -3),
            1 => (-1, -3),
            2 => (1, -3),
            3 => (3, -3),
            4 => (3, -1),
            5 => (3, 1),
            6 => (3, 3),
            7 => (1, 3),
            8 => (-1, 3),
            9 => (-3, 3),
            10 => (-3, 1),
            _ => (-3, -1), // 11
        };
    }
}

[SkipLocalsInit]
sealed class SpinningNeedles(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly AOEShapeCone cone = new(60f, 30f.Degrees());
    private readonly OneTwoStoneMarch _aoe = module.FindComponent<OneTwoStoneMarch>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe.AOEs.Count == 0 ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = spell.Action.ID switch
        {
            (uint)AID.SpinningNeedlesVisualCW => -60f.Degrees(),
            (uint)AID.SpinningNeedlesVisualCCW => 60f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            AddSequence(spell, increment);
            AddSequence(spell, increment, 180f.Degrees());
        }
        void AddSequence(ActorCastInfo spell, Angle increment, Angle offset = default)
         => Sequences.Add(new(cone, spell.LocXZ, spell.Rotation + offset, increment, Module.CastFinishAt(spell, 0.5d), 1.1d, 10));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SpinningNeedles)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // encourage AI to stay close to hitbox to dodge properly
        if (Sequences.Count != 0 && _aoe.AOEs.Count == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 4f), Sequences.Ref(0).NextActivation);
        }
    }
}

[SkipLocalsInit]
sealed class DD60AncestralMaliktenderStates : StateMachineBuilder
{
    public DD60AncestralMaliktenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SeveralThousandNeedles>()
            .ActivateOnEnter<LongRangeNeedles>()
            .ActivateOnEnter<Spineshot>()
            .ActivateOnEnter<OneTwoStoneMarch>()
            .ActivateOnEnter<SpinningNeedles>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DD60AncestralMaliktenderStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.AncestralMaliktender,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1037u,
NameID = 14097u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class AncestralMaliktender(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600f, -300f), new ArenaBoundsSquare(19.5f));
