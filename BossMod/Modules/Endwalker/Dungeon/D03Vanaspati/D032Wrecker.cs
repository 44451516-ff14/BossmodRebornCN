namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D032Wrecker;

public enum OID : uint
{
    Boss = 0x33E9, // R=6.0
    QueerBubble = 0x3731, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AetherSiphonFire = 25145, // Boss->self, 3.0s cast, single-target
    AetherSiphonWater = 25146, // Boss->self, 3.0s cast, single-target
    AetherSprayFire = 25147, // Boss->location, 7.0s cast, range 30, raidwide, be in bubble
    AetherSprayWater = 25148, // Boss->location, 7.0s cast, range 30, knockback 13 away from source
    MeaninglessDestruction = 25153, // Boss->self, 5.0s cast, range 100 circle
    PoisonHeartVisual = 25151, // Boss->self, 5.0s cast, single-target
    PoisonHeartStack = 27851, // Helper->players, 5.0s cast, range 6 circle
    TotalWreck = 25154, // Boss->player, 5.0s cast, single-target
    UnholyWater = 27852, // Boss->self, 3.0s cast, single-target, spawns bubbles
    Withdraw = 27847 // 3731->player, 1.0s cast, single-target, pull 30 between centers
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 30f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MeaninglessDestruction && Arena.Bounds == D032Wrecker.StartingArena)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x06)
        {
            Arena.Bounds = D032Wrecker.DefaultArena;
            Arena.Center = D032Wrecker.DefaultArena.Center;
            _aoe = null;
        }
    }
}
class QueerBubble(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AetherSprayFire _aoe = module.FindComponent<AetherSprayFire>()!;
    public readonly List<Actor> AOEs = [];
    private static readonly AOEShapeCircle circle = new(2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var color = Colors.SafeFromAOE;
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var b = AOEs[i];
            if (!b.IsDead)
                aoes[index++] = new(circle, b.Position, Color: _aoe.Active ? color : 0);
        }
        return aoes.AsSpan()[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.QueerBubble)
            AOEs.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (AOEs.Count != 0 && actor.OID == (uint)OID.QueerBubble)
            AOEs.Remove(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == (uint)AID.Withdraw)
            AOEs.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Active)
        {
            var forbidden = new List<Func<WPos, float>>(6);
            var count = AOEs.Count;
            for (var i = 0; i < count; ++i)
                forbidden.Add(ShapeDistance.InvertedCircle(AOEs[i].Position, 2.5f));
            if (forbidden.Count != 0)
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), Module.CastFinishAt(_aoe.Casters[0].CastInfo));
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class MeaninglessDestruction(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MeaninglessDestruction));
class PoisonHeartStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.PoisonHeartStack), 6f, 4, 4);
class TotalWreck(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TotalWreck));
class AetherSprayWater(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AetherSprayWater));
class AetherSprayFire(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AetherSprayFire), "Go into a bubble! (Raidwide)");

class AetherSprayWaterKB(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.AetherSprayWater), 13f)
{
    private readonly QueerBubble _aoe = module.FindComponent<QueerBubble>()!;
    private static readonly Angle a60 = 60f.Degrees(), a10 = 10f.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && _aoe.AOEs.Count != 0)
        {
            var source = Casters[0];
            var forbidden = new List<Func<WPos, float>>(7)
            {
                ShapeDistance.InvertedCircle(Arena.Center, 7f)
            };
            var bubbles = Module.Enemies(OID.QueerBubble);
            for (var i = 0; i < 6; ++i)
            {
                var bcount = bubbles.Count;
                for (var j = 0; j < bcount; ++j)
                {
                    var b = bubbles[j];
                    if (b.Position.AlmostEqual(WPos.RotateAroundOrigin(i * 60f, Arena.Center, b.Position), 1f) && _aoe.AOEs.Contains(b))
                    {
                        forbidden.Add(ShapeDistance.Cone(Arena.Center, 20f, i * a60, a10));
                        break;
                    }
                }
            }
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class D032WreckerStates : StateMachineBuilder
{
    public D032WreckerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<AetherSprayFire>()
            .ActivateOnEnter<QueerBubble>()
            .ActivateOnEnter<AetherSprayWater>()
            .ActivateOnEnter<AetherSprayWaterKB>()
            .ActivateOnEnter<TotalWreck>()
            .ActivateOnEnter<PoisonHeartStack>()
            .ActivateOnEnter<MeaninglessDestruction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10718)]
public class D032Wrecker(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArena.Center, StartingArena)
{
    private static readonly WPos arenaCenter = new(-295f, -354f);
    public static readonly ArenaBoundsComplex StartingArena = new([new Polygon(arenaCenter, 24.5f, 36)],
    [new Rectangle(new(-295f, -328f), 20f, 2.5f), new Rectangle(new(-295f, -379f), 20f, 1.32f)]);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Polygon(arenaCenter, 20, 36)]);
}
