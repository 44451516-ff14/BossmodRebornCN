namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D063Eliminator;

public enum OID : uint
{
    Boss = 0x41CE, // R6.001
    Elimbit = 0x41D0, // R2.0
    EliminationClaw = 0x41CF, // R2.0
    LightningGenerator = 0x41D1, // R3.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36764, // Boss->player, no cast, single-target
    Teleport = 36763, // Boss->location, no cast, single-target

    Disruption = 36765, // Boss->self, 5.0s cast, range 60 circle, raidwide

    PartitionVisual1 = 39599, // Boss->self, 6.2+0.6s cast, single-target
    PartitionVisual2 = 39600, // Boss->self, 6.2+0.6s cast, single-target
    PartitionVisual3 = 36768, // Boss->self, 4.3+0.7s cast, single-target
    PartitionVisual4 = 36766, // Boss->self, no cast, single-target
    PartitionVisual5 = 36767, // Boss->self, no cast, single-target
    Partition1 = 39007, // Helper->self, 5.0s cast, range 40 180-degree cone
    Partition2 = 39238, // Helper->self, 7.0s cast, range 40 180-degree cone
    Partition3 = 39249, // Helper->self, 7.0s cast, range 40 180-degree cone

    Subroutine1 = 36781, // Boss->self, 3.0s cast, single-target, summons adds
    Subroutine2 = 36775, // Boss->self, 3.0s cast, single-target
    Subroutine3 = 36772, // Boss->self, 3.0s cast, single-target
    SpawnClaw = 36774, // Boss->self, no cast, single-target
    SpawnElimbit = 36777, // Boss->self, no cast, single-target
    SpawnClawAndElimbit = 36788, // Boss->self, no cast, single-target

    ReconfiguredPartition1 = 39248, // Boss->self, 1.2+5.6s cast, single-target
    ReconfiguredPartition2 = 39247, // Boss->self, 1.2+5.6s cast, single-target

    TerminateVisual = 36773, // EliminationClaw->self, 6.2+0.6s cast, single-target
    Terminate = 39615, // Helper->self, 7.0s cast, range 40 width 10 rect

    HaloOfDestructionVisual = 36776, // Elimbit->self, 6.4+0.4s cast, single-target
    HaloOfDestruction = 39616, // Helper->self, 7.0s cast, range 6-40 donut

    OverexposureMarker = 36778, // Helper->player, no cast, single-target
    OverexposureVisual = 36779, // Boss->self, 4.3+0.7s cast, single-target
    Overexposure = 36780, // Helper->self, no cast, range 40 width 6 rect, line stack

    Electray = 39243, // Helper->player, 5.0s cast, range 6 circle

    HoloArk1 = 36789, // Boss->self, no cast, single-target
    HoloArk2 = 36790, // Helper->self, no cast, range 60 circle
    ChargeLimitBreakBar = 36791, // LightningGenerator->Boss, no cast, single-target

    CompressionVisual = 36792, // EliminationClaw->location, 5.3s cast, single-target
    Compression = 36793, // Helper->self, 6.0s cast, range 6 circle

    Impact = 36794, // Helper->self, 6.0s cast, range 60 circle, knockback 15, away from source

    LightOfSalvationVisual = 36782, // Elimbit->self, 6.0s cast, single-target
    LightOfSalvationMarker = 36783, // Helper->player, 5.9s cast, single-target
    LightOfSalvation = 36784, // Helper->self, no cast, range 40 width 6 rect

    LightOfDevotionVisual = 36785, // EliminationClaw->self, 5.0s cast, single-target
    LightOfDevotionMarker = 36786, // Helper->player, no cast, single-target
    LightOfDevotion = 36787, // Helper->self, no cast, range 40 width 6 rect

    Elimination1 = 36795, // Boss->self, 4.0s cast, single-target
    Elimination2 = 36796, // Helper->self, no cast, range 60 circle

    Explosion = 39239 // Helper->self, 8.5s cast, range 50 width 8 rect
}

class DisruptionArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D063Eliminator.ArenaCenter, 16f)], [new Square(D063Eliminator.ArenaCenter, 15f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Disruption && Arena.Bounds == D063Eliminator.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x28)
        {
            Arena.Bounds = D063Eliminator.DefaultBounds;
            _aoe = null;
        }
    }
}

class Disruption(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Disruption));

abstract class Partition(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 90f.Degrees()));
class Partition1(BossModule module) : Partition(module, AID.Partition1);
class Partition2(BossModule module) : Partition(module, AID.Partition2);
class Partition3(BossModule module) : Partition(module, AID.Partition3);

class Terminate(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Terminate), new AOEShapeRect(40f, 5f));
class HaloOfDestruction(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HaloOfDestruction), new AOEShapeDonut(6f, 40f));

class Electray(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Electray), 6f)
{
    private readonly HaloOfDestruction _aoe1 = module.FindComponent<HaloOfDestruction>()!;
    private readonly Partition2 _aoe2 = module.FindComponent<Partition2>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe1.Casters.Count != 0 || _aoe2.Casters.Count != 0)
        { }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
            if (ActiveSpreads.Count != 0)
                hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center - new WDir(default, 15f), 15f), ActiveSpreads[0].Activation);
        }
    }
}

class Explosion : Components.SimpleAOEs
{
    public Explosion(BossModule module) : base(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeRect(50f, 4f))
    {
        MaxDangerColor = 2;
        MaxRisky = 4;
    }
}

class Impact(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.Impact), 15f)
{
    private static readonly Angle halfAngle = 45f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(source.Position, 6f, 8f, source.Position.Z == -640f ? 180f.Degrees() : default, halfAngle), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class Compression(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Compression), 6f);

class Overexposure(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.OverexposureMarker), ActionID.MakeSpell(AID.Overexposure), 5f, 40f, 3f);
class LightOfDevotion(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.LightOfDevotionMarker), ActionID.MakeSpell(AID.LightOfDevotion), 5.5f, 40f, 3f)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x2F && state == 0x00080004) // as soon as limit break phase ends the line stack gets cancelled
            CurrentBaits.Clear();
    }
}

class LightOfSalvation(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly Impact _kb = module.FindComponent<Impact>()!;
    private static readonly AOEShapeRect rect = new(40f, 3f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightOfSalvationMarker)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.TargetID)!, rect, Module.CastFinishAt(spell, 0.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightOfSalvation)
            CurrentBaits.Clear();
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x2F && state == 0x00080004) // as soon as limit break phase ends the line stack gets cancelled
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Casters.Count != 0)
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class D063EliminatorStates : StateMachineBuilder
{
    public D063EliminatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DisruptionArenaChange>()
            .ActivateOnEnter<Disruption>()
            .ActivateOnEnter<Partition1>()
            .ActivateOnEnter<Partition2>()
            .ActivateOnEnter<Partition3>()
            .ActivateOnEnter<Terminate>()
            .ActivateOnEnter<HaloOfDestruction>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<Compression>()
            .ActivateOnEnter<LightOfDevotion>()
            .ActivateOnEnter<LightOfSalvation>()
            .ActivateOnEnter<Overexposure>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12729)]
public class D063Eliminator(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-759f, -648f);
    public static readonly ArenaBoundsSquare StartingBounds = new(15.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(15f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LightningGenerator), Colors.Object);
    }
}
