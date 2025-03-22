namespace BossMod.Dawntrail.Quest.Role.BarThePassage.Apyaahi;

public enum OID : uint
{
    Boss = 0x466A, // R0.500, x1
    SporeCloud = 0x4671, // R1.000, x12
    Mycotender = 0x4670, // R7.0
    FilthyShackle = 0x46DA, // R1.5
    MycotenderArms = 0x4672, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Mycotender->allies, no cast, single-target
    Teleport1 = 40967, // Boss->location, no cast, single-target
    Teleport2 = 40923, // Mycotender->location, no cast, single-target

    FlamesOfTheFallingOrderVisual = 40913, // Boss->self, 7.0s cast, single-target
    FlamesOfTheFallingOrder = 40914, // Helper->location, 7.0s cast, range 45 circle, proximity AOE, optimal distance 12
    PathogenicPowerKB = 40909, // Mycotender->location, 6.0s cast, range 50 circle, raidwide + knockback 13 from source
    PathogenicPowerAOE = 40910, // Helper->location, 2.5s cast, range 6 circle
    RottingRoar = 40922, // Mycotender->self, 5.0s cast, range 50 circle, raidwide
    MoldSpores = 40911, // Mycotender->self, 3.0s cast, single-target
    SporeBurst = 40912, // SporeCloud->self, 6.0s cast, range 6 circle
    SporeStream = 40915, // Mycotender->self, 3.0s cast, single-target
    FleshBoilingPunishmentLakeVisual = 40919, // Boss->self, 8.5s cast, single-target
    FleshBoilingPunishmentLake = 40920, // Helper->self, 8.5s cast, range 5-40 donut
    Mycocyclone = 40916, // Mycotender->self, 10.0s cast, range 60 width 70 rect, raidwide
    BurstFirst = 40917, // SporeCloud->self, 9.0s cast, range 6 circle, exaflare
    BurstRepeat = 40918, // Helper->location, 1.0s cast, range 6 circle
    SupercurseOfNoEscape = 40924, // Boss->player, 5.0s cast, single-target
    SummonsToTheSoonToBeDeparted = 40925, // Helper->self, 6.0s cast, range 25 circle, pull 30 between centers
    SpoilingSmashVisual1 = 40926, // Mycotender->self, 10.5s cast, single-target
    SpoilingSmashVisual2 = 40927, // MycotenderArms->self, 30.0s cast, single-target
    SpoilingSmashEnrage = 40929, // Mycotender->self, no cast, range 100 circle
    EnrageFailed = 40928, // Mycotender->self, no cast, single-target
    Fire = 41350, // Boss->allies, 1.0s cast, single-target
    MyceliumStomp = 40930, // Mycotender->players, 5.0s cast, range 6 circle, stack
    BlazingBlazeOfGloryVisual = 40931, // Boss->self, 7.0s cast, single-target
    BlazingBlazeOfGlory = 40932 // Helper->allies, 7.0s cast, range 6 circle
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20f, 25f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(float delay) => _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, delay));
        if (Arena.Bounds != ApyaahiTheArchitect.StartingArena)
        {
            if (spell.Action.ID == (uint)AID.FlamesOfTheFallingOrder)
                AddAOE(0.5f);
            else if (spell.Action.ID == (uint)AID.FleshBoilingPunishmentLake)
                AddAOE(2f);
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        void SetArena(ArenaBoundsComplex bounds)
        {
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
        }
        if (index == 0x06)
        {
            if (state == 0x00020001)
            {
                SetArena(ApyaahiTheArchitect.DefaultArena);
                _aoe = null;
            }
            else if (state == 0x0008004)
                SetArena(ApyaahiTheArchitect.StartingArena);
        }
    }
}

class FlamesOfTheFallingOrder(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FlamesOfTheFallingOrder), 12f);
class RottingRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RottingRoar));
class Mycocyclone(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Mycocyclone));

class PathogenicPowerAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = new(20);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var act0 = _aoes[0].Activation;
        var compareFL = (_aoes[count - 1].Activation - act0).TotalSeconds > 1d;
        var aoes = new AOEInstance[count];
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            aoes[i] = (aoe.Activation - act0).TotalSeconds < 1d ? aoe with { Color = compareFL ? color : 0 } : aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PathogenicPowerAOE)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.PathogenicPowerAOE)
            _aoes.RemoveAt(0);
    }
}
class PathogenicPowerKB(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.PathogenicPowerKB), 13f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0].CastInfo!;
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(source.LocXZ, 6f), Module.CastFinishAt(source));
        }
    }
}

class SporeBurst(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SporeBurst), 6f);
class FleshBoilingPunishmentLake(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FleshBoilingPunishmentLake), new AOEShapeDonut(5f, 40f));
class BlazingBlazeOfGlory(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BlazingBlazeOfGlory), 6f);
class MyceliumStomp(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.MyceliumStomp), 6f);
class SpoilingSmashEnrage(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SpoilingSmashVisual2), "Enrage!", true);

class Burst(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BurstFirst)
            Lines.Add(new() { Next = caster.Position, Advance = 5f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.2f, ExplosionsLeft = 9, MaxShownExplosions = 4 });
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BurstFirst or (uint)AID.BurstRepeat)
        {
            var count = Lines.Count;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

class ApyaahiTheArchitectStates : StateMachineBuilder
{
    public ApyaahiTheArchitectStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<RottingRoar>()
            .ActivateOnEnter<Mycocyclone>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<PathogenicPowerKB>()
            .ActivateOnEnter<PathogenicPowerAOE>()
            .ActivateOnEnter<SporeBurst>()
            .ActivateOnEnter<FleshBoilingPunishmentLake>()
            .ActivateOnEnter<FlamesOfTheFallingOrder>()
            .ActivateOnEnter<MyceliumStomp>()
            .ActivateOnEnter<SpoilingSmashEnrage>()
            .ActivateOnEnter<BlazingBlazeOfGlory>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1016, NameID = 13669)]
public class ApyaahiTheArchitect(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArena.Center, StartingArena)
{
    public static readonly ArenaBoundsComplex StartingArena = new([new Polygon(new(default, 379.27f), 24.5f, 20)]);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Polygon(new(default, 379.241f), 20f, 64)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies([(uint)OID.Mycotender, (uint)OID.MycotenderArms]));
    }

    protected override bool CheckPull() => Enemies((uint)OID.Mycotender) is var mycotender && mycotender.Count != 0 && mycotender[0].InCombat;
}
