namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D071Barreltender;

public enum OID : uint
{
    Boss = 0x4234, // R5.0
    CactusBig = 0x1EBBF1, // R0.5
    CactusSmall = 0x1EBBF0, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 37393, // Boss->location, no cast, single-target

    BarbedBellow = 37392, // Boss->self, 5.0s cast, range 50 circle, raidwide
    HeavyweightNeedlesVisual = 37384, // Boss->self, 6.0s cast, single-target
    HeavyweightNeedles = 37386, // Helper->self, 6.5s cast, range 36 50-degree cone

    TenderDrop = 37387, // Boss->self, 3.0s cast, single-target, spawns cacti
    BarrelBreaker = 37390, // Boss->location, 6.0s cast, range 50 circle, knockback 20, away from source

    NeedleSuperstorm = 37389, // Helper->self, 5.0s cast, range 11 circle
    NeedleStorm = 37388, // Helper->self, 5.0s cast, range 6 circle

    SucculentStomp = 37391, // Boss->players, 5.0s cast, range 6 circle, stack
    PricklyRight = 39154, // Boss->self, 7.0s cast, range 36 270-degree cone
    PricklyLeft = 39155, // Boss->self, 7.0s cast, range 36 270-degree cone

    TenderFury = 39242 // Boss->player, 5.0s cast, single-target, tankbuster
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D071Barreltender.ArenaCenter, 25f)], [new Square(D071Barreltender.ArenaCenter, 20f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyweightNeedlesVisual && Arena.Bounds == D071Barreltender.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u && index == 0x03u)
        {
            Arena.Bounds = D071Barreltender.DefaultBounds;
            _aoe = null;
        }
    }
}

sealed class HeavyweightNeedles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavyweightNeedles, new AOEShapeCone(36f, 25f.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        if (NumCasts < 16)
            return;
        var aoe = Casters[0];
        // stay close to the middle to switch safespots
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(aoe.Origin, 3f), aoe.Activation);
    }
}

sealed class NeedleStormSuperstorm(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(16);
    private static readonly AOEShapeCircle circleBig = new(11f), circleSmall = new(6f);

    private readonly BarrelBreaker _kb = module.FindComponent<BarrelBreaker>()!;
    private readonly HeavyweightNeedles _aoe = module.FindComponent<HeavyweightNeedles>()!;
    private bool cactiActive;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Casters.Count != 0)
            return [];
        var count = _aoes.Count;
        var max = cactiActive ? (count > 8 ? 8 : count) : 0; // next wave of cactus helpers spawns immediately after first wave starts casting, so we need to limit to 8
        if (max == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        var kb = _kb;
        var isKnockback = kb.Casters.Count != 0;
        var isKnockbackImmune = isKnockback && _kb.IsImmune(slot, Module.CastFinishAt(_kb.Casters[0].CastInfo));
        var isKnockbackButImmune = isKnockback && isKnockbackImmune;
        for (var i = 0; i < max; ++i)
        {
            aoes[i].Risky = !isKnockback || isKnockbackButImmune;
        }
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.CactusSmall => circleSmall,
            (uint)OID.CactusBig => circleBig,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, WPos.ClampToGrid(actor.Position)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TenderDrop)
        {
            cactiActive = true;
            var count = _aoes.Count;
            var max = count > 8 ? 8 : count;
            var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];

            for (var i = 0; i < max; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = Module.CastFinishAt(spell, 13.7f);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.NeedleStorm or (uint)AID.NeedleSuperstorm)
        {
            _aoes.RemoveAt(0);
            cactiActive = false;
        }
    }
}

sealed class Prickly(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PricklyRight, (uint)AID.PricklyLeft], new AOEShapeCone(36f, 165f.Degrees()));
sealed class SucculentStomp(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.SucculentStomp, 6f, 4, 4);
sealed class BarrelBreaker(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BarrelBreaker, 20f)
{
    private static readonly Angle a10 = 10f.Degrees(), a135 = 135f.Degrees(), a45 = 45f.Degrees();
    private enum Pattern { None, NESW, NWSE }
    private Pattern CurrentPattern;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.CactusSmall && state == 0x00010002u)
        {
            var add = actor.Position.X + actor.Position.Z;
            if (add == 400f) // new WPos(-55f, 455f)
                CurrentPattern = Pattern.NESW;
            else if (add == 430f) // new WPos(-55f, 485f)
                CurrentPattern = Pattern.NWSE;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (!IsImmune(slot, act))
            {
                var forbidden = new Func<WPos, float>[2];
                var pattern = CurrentPattern == Pattern.NESW;
                var castInfo = source.CastInfo;
                var pos = castInfo!.LocXZ;
                forbidden[0] = ShapeDistance.InvertedCone(pos, 4f, pattern ? a135 : -a135, a10);
                forbidden[1] = ShapeDistance.InvertedCone(pos, 4f, pattern ? -a45 : a45, a10);
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoe = Module.FindComponent<NeedleStormSuperstorm>();
        if (aoe != null)
        {
            var aoes = aoe.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(pos))
                    return true;
            }
        }
        return !Module.InBounds(pos);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.CactusSmall)
            CurrentPattern = Pattern.None;
    }
}

sealed class TenderFury(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TenderFury);
sealed class BarbedBellow(BossModule module) : Components.RaidwideCast(module, (uint)AID.BarbedBellow);

sealed class D071BarreltenderStates : StateMachineBuilder
{
    public D071BarreltenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<BarrelBreaker>()
            .ActivateOnEnter<HeavyweightNeedles>()
            .ActivateOnEnter<NeedleStormSuperstorm>()
            .ActivateOnEnter<Prickly>()
            .ActivateOnEnter<TenderFury>()
            .ActivateOnEnter<SucculentStomp>()
            .ActivateOnEnter<BarbedBellow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12889)]
public sealed class D071Barreltender(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-65f, 470f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}
