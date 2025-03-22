namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D091LindblumZaghnal;

public enum OID : uint
{
    Boss = 0x4641, // R9.0
    RawElectrope = 0x4642, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 40622, // Boss->location, no cast, single-target

    ElectricalOverload = 40635, // Boss->self, 5.0s cast, range 40 circle

    Gore1 = 40630, // Boss->self, 3.0s cast, single-target
    Gore2 = 41266, // Boss->self, 3.0s cast, single-target
    CaberToss = 40624, // Boss->self, 19.0s cast, single-target
    LineVoltageWide1 = 41121, // Helper->self, 3.3s cast, range 50 width 10 rect
    LineVoltageWide2 = 40627, // Helper->self, 3.5s cast, range 50 width 10 rect
    LineVoltageNarrow1 = 41122, // Helper->self, 3.0s cast, range 50 width 5 rect
    LineVoltageNarrow2 = 40625, // Helper->self, 4.0s cast, range 50 width 5 rect
    CellShock = 40626, // Helper->self, 2.0s cast, range 26 circle

    LightningStormVisual = 40636, // Boss->self, 4.5s cast, single-target
    LightningStorm = 40637, // Helper->player, 5.0s cast, range 5 circle, spread

    SparkingFissureVisual = 40632, // Boss->self, 13.0s cast, single-target
    SparkingFissure = 41258, // Helper->self, 13.7s cast, range 40 circle
    SparkingFissureFirst = 41267, // Helper->self, 5.2s cast, range 40 circle
    SparkingFissureRepeat = 40631, // Helper->self, no cast, range 40 circle

    LightningBolt = 40638, // Helper->location, 5.0s cast, range 6 circle
    Electrify = 40634, // RawElectrope->self, 16.0s cast, range 40 circle
}

abstract class LineVoltage(BossModule module, uint narrow, double delay, uint? wide1 = null, uint? wide2 = null) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rectNarrow = new(50f, 2.5f), rectWide = new(50f, 5);
    public readonly List<AOEInstance> AOEs = new(18);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var deadline = aoes[0].Activation.AddSeconds(delay);
        var isNotLastSet = aoes[^1].Activation > deadline;
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (isNotLastSet)
                    aoe.Color = color;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == narrow)
            AddAOE(rectNarrow);
        else if (spell.Action.ID == wide1 || spell.Action.ID == wide2)
            AddAOE(rectWide);

        void AddAOE(AOEShapeRect shape)
        {
            AOEs.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
            if (AOEs.Count > 1)
                AOEs.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == narrow || spell.Action.ID == wide1 || spell.Action.ID == wide2)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (AOEs[i].ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class LineVoltage1(BossModule module) : LineVoltage(module, (uint)AID.LineVoltageNarrow1, 1.5d);
class LineVoltage2(BossModule module) : LineVoltage(module, (uint)AID.LineVoltageNarrow2, 2d, (uint)AID.LineVoltageWide1, (uint)AID.LineVoltageWide2);

class LightningBolt(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightningBolt), 6);
class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.LightningStorm), 5);
class ElectricalOverload(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ElectricalOverload));
class SparkingFissure(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SparkingFissure));
class SparkingFissureFirst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SparkingFissureFirst));

class CellShock(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(26);
    private AOEInstance? _aoe;
    private readonly LineVoltage1 _aoes = module.FindComponent<LineVoltage1>()!;

    private static readonly Dictionary<byte, WPos> initialPositions = new()
    {
        { 0x0D, new(81.132f, 268.868f) }, { 0x0E, new(81.132f, 285.132f) },
        { 0x0F, new(64.868f, 268.868f) }, { 0x10, new(64.868f, 285.132f) }
    };

    private static readonly Dictionary<byte, byte> pairsWithSamePositions = new()
    {
        { 0x0D, 0x10 }, { 0x0E, 0x0F }, { 0x0F, 0x0E }, { 0x10, 0x0D }
    };

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.AOEs.Count == 0 ? Utils.ZeroOrOne(ref _aoe) : [];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state is 0x00020001 or 0x00200010)
        {
            if (state == 0x00200010 && pairsWithSamePositions.TryGetValue(index, out var remappedIndex))
                index = remappedIndex;

            if (initialPositions.TryGetValue(index, out var position))
                _aoe = new(circle, position, default, WorldState.FutureTime(8));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CellShock)
            _aoe = null;
    }
}

class D091LindblumZaghnalStates : StateMachineBuilder
{
    public D091LindblumZaghnalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LineVoltage1>()
            .ActivateOnEnter<LineVoltage2>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<ElectricalOverload>()
            .ActivateOnEnter<SparkingFissure>()
            .ActivateOnEnter<SparkingFissureFirst>()
            .ActivateOnEnter<CellShock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13623, SortOrder = 3)]
public class D091LindblumZaghnal(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(73f, 277f), 19.5f, 64)], [new Rectangle(new(72f, 297f), 20f, 1.1f),
    new Rectangle(new(72f, 257f), 20f, 1.05f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.RawElectrope));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.RawElectrope => 1,
                _ => 0
            };
        }
    }
}
