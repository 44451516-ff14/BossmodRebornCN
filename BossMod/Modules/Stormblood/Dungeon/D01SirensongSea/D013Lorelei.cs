namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D013Lorelei;

public enum OID : uint
{
    Lorelei = 0x1AFE, // R3.36
    ArenaVoidzone = 0x1EA2FF, // R2.0
    Voidzone = 0x1EA300 // R0.5
}

public enum AID : uint
{
    IllWill = 8035, // Lorelei->player, no cast, single-target

    VirginTears = 8041, // Lorelei->self, 3.0s cast, single-target
    MorbidAdvance = 8037, // Lorelei->self, 5.0s cast, range 80+R circle
    HeadButt = 8036, // Lorelei->player, no cast, single-target
    SomberMelody = 8039, // Lorelei->self, 4.0s cast, range 80+R circle
    MorbidRetreat = 8038, // Lorelei->self, 5.0s cast, range 80+R circle
    VoidWaterIII = 8040 // Lorelei->location, 3.5s cast, range 8 circle
}

[SkipLocalsInit]
sealed class VirginTearsArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(15.75f, 22f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x001)
        {
            Arena.Bounds = new ArenaBoundsCustom([new Polygon(D013Lorelei.ArenaCenter, 21.554275f, 64)], [new Rectangle(new(-44.5f, 443f), 20f, 1f)]);
            Arena.Center = D013Lorelei.ArenaCenter;
        }
        else if (state == 0x002)
        {
            _aoe = [];
            var arena = new ArenaBoundsCustom([new Polygon(new(-44.49977f, 465.00049f), 15.95688f, 24)]);
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VirginTears && Arena.Bounds.Radius > 16f)
        {
            if (++NumCasts > 3)
            {
                _aoe = [new(donut, D013Lorelei.ArenaCenter, default, Module.CastFinishAt(spell, 0.7d))];
            }
        }
    }
}

[SkipLocalsInit]
sealed class MorbidAdvance(BossModule module) : Components.ActionDrivenForcedMarch(module, (uint)AID.MorbidAdvance, 3f, default, 1f)
{
    private readonly Voidzone _aoe = module.FindComponent<Voidzone>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }

        ref var move0 = ref state.PendingMoves.Ref(0);
        var act = move0.activation;
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        var pos = actor.Position;
        var moveDir = move0.dir.ToDirection();
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            var origin = aoe.Origin;
            var d = origin - pos;
            var dist = d.Length();

            if (dist is <= 7f or >= 24f) // inside mine or max distance 3s * 6 + 7 radius + 1 safety margin
            {
                continue; // inside mine or impossible to run into this mine from current position
            }

            var forward = d.Dot(moveDir);
            var sideways = d.Dot(moveDir.OrthoL());

            hints.ForbiddenDirections.Add(new(Angle.Atan2(sideways, forward), Angle.Asin(7f / dist), act));
        }
        hints.AddForbiddenZone(new SDInvertedDonut(Arena.Center, 3f, 5f));
    }
}

[SkipLocalsInit]
sealed class MorbidRetreat(BossModule module) : Components.ActionDrivenForcedMarch(module, (uint)AID.MorbidRetreat, 3f, 180f.Degrees(), 1f)
{
    private readonly Voidzone _aoe = module.FindComponent<Voidzone>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }

        ref var move0 = ref state.PendingMoves.Ref(0);
        var act = move0.activation;
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        var pos = actor.Position;
        var moveDir = move0.dir.ToDirection();
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            var origin = aoe.Origin;
            var d = origin - pos;
            var dist = d.Length();

            if (dist is <= 7f or >= 24f) // inside mine or max distance 3s * 6 + 7 radius + 1 safety margin
            {
                continue; // inside mine or impossible to run into this mine from current position
            }

            var forward = d.Dot(moveDir);
            var sideways = d.Dot(moveDir.OrthoL());

            hints.ForbiddenDirections.Add(new(Angle.Atan2(sideways, forward), Angle.Asin(7f / dist), act));
        }
        hints.AddForbiddenZone(new SDInvertedDonut(Arena.Center, 3f, 5f));
    }
}

[SkipLocalsInit]
sealed class SomberMelody(BossModule module) : Components.RaidwideCast(module, (uint)AID.SomberMelody);
[SkipLocalsInit]
sealed class VoidWaterIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidWaterIII, 8f);
[SkipLocalsInit]
sealed class Voidzone(BossModule module) : Components.Voidzone(module, 7f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

[SkipLocalsInit]
sealed class D013LoreleiStates : StateMachineBuilder
{
    public D013LoreleiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VirginTearsArenaChange>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<MorbidAdvance>()
            .ActivateOnEnter<MorbidRetreat>()
            .ActivateOnEnter<SomberMelody>()
            .ActivateOnEnter<VoidWaterIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(D013LoreleiStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.Lorelei,
Contributors = "The Combat Reborn Team (Malediktus), erdelf",
Expansion = BossModuleInfo.Expansion.Stormblood,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 238u,
NameID = 6074u,
SortOrder = 3,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D013Lorelei : BossModule
{
    public D013Lorelei(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }
    public static readonly WPos ArenaCenter = new(-44.5f, 465f);

    private D013Lorelei(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(ArenaCenter, 21.554275f, 64)], [new Rectangle(new(-44.5f, 443f), 20f, 1f)]);
        return (arena.Center, arena);
    }
}
