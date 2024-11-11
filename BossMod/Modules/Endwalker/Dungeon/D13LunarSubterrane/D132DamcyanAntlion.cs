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

class Sandblast(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Sandblast));

class SandblastVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom rect = new([new Rectangle(D132DamcyanAntlion.ArenaCenter, 19.5f, 25)], [new Rectangle(D132DamcyanAntlion.ArenaCenter, 19.5f, 20)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Sandblast && Arena.Bounds == D132DamcyanAntlion.StartingBounds)
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

class Landslip(BossModule module) : Components.Knockback(module)
{
    public bool TowerDanger;
    private readonly List<Actor> _casters = [];
    public DateTime Activation;
    private static readonly AOEShapeRect rect = new(40, 5);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(c.Position, 20, Activation, rect, c.Rotation, Kind.DirForward);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip)
        {
            Activation = Module.CastFinishAt(spell);
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip)
        {
            _casters.Remove(caster);
            if (++NumCasts > 4)
                TowerDanger = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var length = Arena.Bounds.Radius * 2; // casters are at the border, orthogonal to borders
        foreach (var c in _casters)
            hints.AddForbiddenZone(ShapeDistance.Rect(c.Position, c.Rotation, length, 20 - length, 5), Activation);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Towerfall>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class EarthenGeyser(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.EarthenGeyser), 10, 4, 4);
class QuicksandVoidzone(BossModule module) : Components.PersistentVoidzone(module, 10, m => m.Enemies(OID.QuicksandVoidzone).Where(z => z.EventState != 7));
class PoundSand(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PoundSand), 12);

class AntlionMarch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Activation = _activation, Color = Colors.Danger };
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Activation = _activation };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AntilonMarchTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 4.5f), caster.Position, Angle.FromDirection(dir))); // actual charge is only 4 halfwidth, but the telegraphs and actual AOEs can be in different positions by upto 0.5y according to my logs
        }
        if ((AID)spell.Action.ID == AID.AntlionMarch)
            _activation = Module.CastFinishAt(spell, 0.2f); // since these are charges of different length with 0s cast time, the activation times are different for each and there are different patterns, so we just pretend that they all start after the telegraphs end
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.AntlionMarch)
            _aoes.RemoveAt(0);
    }
}

class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 5);
    private const int X = 20;
    private static readonly Dictionary<byte, (WPos position, Angle direction)> _towerPositions = [];

    static Towerfall()
    {
        int[] xPositions = [-X, X];
        Angle[] angles = [Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]];
        var zStart = 45;
        var zStep = 10;
        byte index = 1;

        for (var i = 0; i < 2; ++i)
            for (var j = 0; j < 4; ++j)
                _towerPositions[index++] = new(new(xPositions[i], zStart + j * zStep), angles[i]);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _aoes.Count; ++i)
            yield return new(rect, _aoes[i].Origin, _aoes[i].Rotation, Module.FindComponent<Landslip>()!.Activation.AddSeconds(0.7f), Risky: Module.FindComponent<Landslip>()!.TowerDanger);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && _towerPositions.TryGetValue(index, out var towers))
            _aoes.Add(new(rect, towers.position, towers.direction));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Towerfall)
        {
            _aoes.Clear();
            Module.FindComponent<Landslip>()!.TowerDanger = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<Landslip>()!.Sources(slot, actor).Any())
        {
            var forbiddenInverted = new List<Func<WPos, float>>();
            var forbidden = new List<Func<WPos, float>>();
            if (_aoes.Count == 2)
            {
                var distance = Math.Abs(_aoes[0].Origin.Z - _aoes[1].Origin.Z);
                if (distance is 10 or 30)
                    for (var i = 0; i < _aoes.Count; ++i)
                        forbiddenInverted.Add(ShapeDistance.InvertedRect(_aoes[i].Origin, _aoes[i].Rotation, rect.LengthFront, default, rect.HalfWidth));
                else
                    for (var i = 0; i < _aoes.Count; ++i)
                        forbidden.Add(ShapeDistance.Rect(_aoes[i].Origin, _aoes[i].Rotation, rect.LengthFront, default, rect.HalfWidth));
            }
            var activation = Module.FindComponent<Landslip>()!.Activation.AddSeconds(0.7f);
            if (forbiddenInverted.Count > 0)
                hints.AddForbiddenZone(p => forbiddenInverted.Max(f => f(p)), activation);
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Min(f => f(p)), activation);
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
            .ActivateOnEnter<Components.StayInBounds>()
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
    public static readonly WPos ArenaCenter = new(0, 60);
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsRect(19.5f, 25);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsRect(19.5f, 20);
}
