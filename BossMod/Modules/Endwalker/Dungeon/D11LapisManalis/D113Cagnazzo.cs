namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D113Cagnazzo;

public enum OID : uint
{
    Boss = 0x3AE2, //R=8.0
    FearsomeFlotsam = 0x3AE3, //R=2.4
    Helper2 = 0x3E97, // R2.7
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 31131, // Boss->location, no cast, single-target, boss teleports 

    StygianDeluge = 31139, // Boss->self, 5.0s cast, range 80 circle
    AntediluvianVisual = 31119, // Boss->self, 5.0s cast, single-target
    Antediluvian = 31120, // Helper->self, 6.5s cast, range 15 circle
    BodySlamVisual = 31121, // Boss->location, 6.5s cast, single-target
    BodySlamKB = 31122, // Helper->self, 7.5s cast, range 60 circle, knockback 10, away from source
    BodySlam = 31123, // Helper->self, 7.5s cast, range 8 circle

    HydrobombTelegraph = 32695, // Helper->location, 2.0s cast, range 4 circle
    HydraulicRamTelegraph = 32693, // Helper->location, 2.0s cast, width 8 rect charge
    HydraulicRamVisual = 32692, // Boss->self, 6.0s cast, single-target
    HydraulicRam = 32694, // Boss->location, no cast, width 8 rect charge
    Hydrobomb = 32696, // Helper->location, no cast, range 4 circle
    StartHydrofall = 31126, // Boss->self, no cast, single-target
    Hydrofall = 31375, // Boss->self, 5.0s cast, single-target
    Hydrofall2 = 31376, // Helper->players, 5.5s cast, range 6 circle
    CursedTide = 31130, // Boss->self, 5.0s cast, single-target
    StartLimitbreakPhase = 31132, // Boss->self, no cast, single-target
    NeapTide = 31134, // Helper->player, no cast, range 6 circle
    Hydrovent = 31136, // Helper->location, 5.0s cast, range 6 circle
    SpringTide = 31135, // Helper->players, no cast, range 6 circle
    Tsunami = 31137, // Helper->self, no cast, range 80 width 60 rect
    TsunamiEnrage = 31138, // Helper->self, no cast, range 80 width 60 rect
    VoidcleaverVisual = 31110, // Boss->self, 4.0s cast, single-target
    Voidcleaver = 31111, // Helper->self, no cast, range 100 circle
    VoidMiasma = 32691, // Helper->self, 3.0s cast, range 50 30-degree cone
    LifescleaverVisual = 31112, // Boss->self, 4.0s cast, single-target
    Lifescleaver = 31113, // Helper->self, 5.0s cast, range 50 30-degree cone
    VoidTorrent = 31118 // Boss->self/player, 5.0s cast, range 60 width 8 rect
}

public enum IconID : uint
{
    Stackmarker = 161, // player
    Spreadmarker = 139 // player
}

public enum TetherID : uint
{
    BaitAway = 1 // Helper2->player
}

public enum NPCYell : uint
{
    LimitBreakStart = 15175
}

class StygianDelugeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D113Cagnazzo.ArenaCenter, 30f)], [new Square(D113Cagnazzo.ArenaCenter, 20f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StygianDeluge && Arena.Bounds == D113Cagnazzo.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = D113Cagnazzo.DefaultBounds;
            _aoe = null;
        }
    }
}

class VoidTorrent(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.VoidTorrent), new AOEShapeRect(60f, 4f), tankbuster: true);

class Voidcleaver(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Voidcleaver));
class VoidMiasmaBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(50f, 15f.Degrees()), (uint)TetherID.BaitAway);

class Cleaver(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(50f, 15f.Degrees()));
class VoidMiasma(BossModule module) : Cleaver(module, AID.VoidMiasma);
class Lifescleaver(BossModule module) : Cleaver(module, AID.Lifescleaver);

class Tsunami(BossModule module) : Components.RaidwideAfterNPCYell(module, ActionID.MakeSpell(AID.Tsunami), (uint)NPCYell.LimitBreakStart, 4.5f);
class StygianDeluge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StygianDeluge));
class Antediluvian(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Antediluvian), 15)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (NumCasts == 6 && spell.Action.ID == (uint)AID.Antediluvian)
            NumCasts = 0;
    }
}

class BodySlam(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BodySlam), 8f);
class BodySlamKB(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.BodySlamKB), 10f, true)
{
    private readonly Antediluvian _aoe = module.FindComponent<Antediluvian>()!;

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
        if (Casters.Count != 0 && _aoe.NumCasts >= 4)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(source.Position, 10f), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class HydraulicRam(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydraulicRamTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 4f), WPos.ClampToGrid(caster.Position), Angle.FromDirection(dir), Module.CastFinishAt(spell, 5.7f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.HydraulicRam)
            _aoes.RemoveAt(0);
    }
}

class Hydrobomb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private static readonly AOEShapeCircle circle = new(4f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                aoes[i] = count > 2 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydrobombTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 6.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Hydrobomb)
            _aoes.RemoveAt(0);
    }
}

class Hydrovent(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Hydrovent), 6f);
class NeapTide(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.NeapTide), 6f, 5);

class SpringTideHydroFall(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4) // both use the same icon
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Stackmarker)
            AddStack(actor, WorldState.FutureTime(5d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SpringTide or (uint)AID.Hydrofall2)
            Stacks.Clear();
    }
}

class D113CagnazzoStates : StateMachineBuilder
{
    public D113CagnazzoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StygianDelugeArenaChange>()
            .ActivateOnEnter<Voidcleaver>()
            .ActivateOnEnter<Lifescleaver>()
            .ActivateOnEnter<VoidMiasma>()
            .ActivateOnEnter<VoidMiasmaBait>()
            .ActivateOnEnter<Antediluvian>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<BodySlamKB>()
            .ActivateOnEnter<HydraulicRam>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<SpringTideHydroFall>()
            .ActivateOnEnter<NeapTide>()
            .ActivateOnEnter<StygianDeluge>()
            .ActivateOnEnter<Hydrovent>()
            .ActivateOnEnter<VoidTorrent>()
            .ActivateOnEnter<Tsunami>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 11995)]
public class D113Cagnazzo(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-250f, 130f);
    public static readonly ArenaBoundsSquare StartingBounds = new(29.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.FearsomeFlotsam));
    }
}
