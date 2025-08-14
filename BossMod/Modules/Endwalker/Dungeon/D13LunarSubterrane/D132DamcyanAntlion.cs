namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D132DamcyanAntlion;

public enum OID : uint
{
    Boss = 0x4022, // R=7.5
    StonePillar = 0x4023, // R=3.0
    StonePillar2 = 0x3FD1, // R=1.5
    QuicksandVoidzone = 0x1EB90E,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss, no cast, single-target

    Sandblast = 34813, // Boss->self, 5.0s cast, range 60 circle
    LandslipVisual = 34818, // Boss->self, 7.0s cast, single-target
    Landslip = 34819, // Helper->self, 7.7s cast, range 40 width 10 rect, knockback dir 20 forward
    Teleport = 34824, // Boss->location, no cast, single-target
    AntilonMarchTelegraph = 35871, // Helper->location, 1.5s cast, width 8 rect charge
    AntlionMarchVisual = 34816, // Boss->self, 5.5s cast, single-target
    AntlionMarch = 34817, // Boss->location, no cast, width 8 rect charge
    Towerfall = 34820, // StonePillar->self, 2.0s cast, range 40 width 10 rect
    EarthenGeyserVisual = 34821, // Boss->self, 4.0s cast, single-target
    EarthenGeyser = 34822, // Helper->players, 5.0s cast, range 10 circle
    PoundSand = 34443 // Boss->location, 6.0s cast, range 12 circle
}

class Sandblast(BossModule module) : Components.RaidwideCast(module, (uint)AID.Sandblast);

class SandblastVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom rect = new([new Rectangle(D132DamcyanAntlion.ArenaCenter, 19.5f, 25f)], [new Rectangle(D132DamcyanAntlion.ArenaCenter, 19.5f, 20f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Sandblast && Arena.Bounds == D132DamcyanAntlion.StartingBounds)
            _aoe = new(rect, Arena.Center, default, Module.CastFinishAt(spell));
    }
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = D132DamcyanAntlion.DefaultBounds;
            _aoe = null;
        }
    }
}

class Landslip(BossModule module) : Components.GenericKnockback(module)
{
    public bool TowerDanger;
    public readonly List<Knockback> Knockbacks = new(4);
    private static readonly AOEShapeRect rect = new(40f, 5f);
    private Towerfall? _aoe = module.FindComponent<Towerfall>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(Knockbacks);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Landslip)
            Knockbacks.Add(new(spell.LocXZ, 20f, Module.CastFinishAt(spell), rect, spell.Rotation, Kind.DirForward));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Landslip)
        {
            Knockbacks.Clear();
            if (++NumCasts > 4)
                TowerDanger = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Knockbacks.Count;
        if (count == 0)
            return;
        var length = Arena.Bounds.Radius * 2; // casters are at the border, orthogonal to borders
        for (var i = 0; i < count; ++i)
        {
            var c = Knockbacks[i];
            hints.AddForbiddenZone(ShapeDistance.Rect(c.Origin, c.Direction, length, 20f - length, 5f), c.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _aoe ??= Module.FindComponent<Towerfall>();
        var aoes = _aoe!.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
    }
}

class EarthenGeyser(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.EarthenGeyser, 10f, 4, 4);
class QuicksandVoidzone(BossModule module) : Components.Voidzone(module, 10f, GetVoidzone)
{
    private static Actor[] GetVoidzone(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.QuicksandVoidzone);
        if (enemies.Count != 0 && enemies[0].EventState != 7)
            return [.. enemies];
        return [];
    }
}
class PoundSand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoundSand, 12f);

class AntlionMarch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
        {
            ref var aoe0 = ref aoes[0];
            aoe0.Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AntilonMarchTelegraph)
        {
            // actual charge is only 4 halfwidth, but the telegraphs and actual AOEs can be in different positions by upto 0.5y according to my logs
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 4.5f), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell, 4.2d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.AntlionMarch)
            _aoes.RemoveAt(0);
    }
}

class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Landslip _kb = module.FindComponent<Landslip>()!;
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var risky = _kb.TowerDanger;
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            aoe.Risky = risky;
        }
        return aoes;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index > 0x00 && state == 0x00020001u)
        {
            var posX = index < 0x05 ? -20f : 20f;
            var posZ = posX == -20f ? 35f + index * 10f : -5f + index * 10f;
            var rot = posX == -20f ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0];
            _aoes.Add(new(rect, new WPos(posX, posZ).Quantized(), rot, WorldState.FutureTime(13d - _aoes.Count)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Towerfall)
        {
            _aoes.Clear();
            _kb.TowerDanger = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Knockbacks.Count != 0 & _aoes.Count == 2)
        {
            var activation = _kb.Knockbacks[0].Activation;
            var distance = MathF.Round(Math.Abs(_aoes[0].Origin.Z - _aoes[1].Origin.Z));
            var forbidden = new Func<WPos, float>[2];
            var check = distance is 10f or 30f;
            for (var i = 0; i < 2; ++i)
            {
                var aoe = _aoes[i];
                forbidden[i] = check ? ShapeDistance.InvertedRect(aoe.Origin, aoe.Rotation, 40f, default, 5f) : ShapeDistance.Rect(aoe.Origin, aoe.Rotation, 40f, default, 5f);
            }
            hints.AddForbiddenZone(check ? ShapeDistance.Intersection(forbidden) : ShapeDistance.Union(forbidden), activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class D132DamcyanAntlionStates : StateMachineBuilder
{
    public D132DamcyanAntlionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SandblastVoidzone>()
            .ActivateOnEnter<Sandblast>()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<EarthenGeyser>()
            .ActivateOnEnter<QuicksandVoidzone>()
            .ActivateOnEnter<PoundSand>()
            .ActivateOnEnter<AntlionMarch>()
            .ActivateOnEnter<Towerfall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12484)]
public class D132DamcyanAntlion(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(default, 60f);
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsRect(19.5f, 25f);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsRect(19.5f, 20f);
}
