namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D053ForgivenWhimsy;

public enum OID : uint
{
    Boss = 0x27CC, //R=20.00
    Brightsphere = 0x27CD, //R=1.0
    Towers = 0x1EAACF, //R=0.5
    Helper2 = 0x2E8, //R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15624, // Boss->player, no cast, single-target

    SacramentOfPenanceVisual = 15627, // Boss->self, 4.0s cast, single-target
    SacramentOfPenance = 15628, // Helper->self, no cast, range 50 circle
    Reformation = 15620, // Boss->self, no cast, single-target, boss changes pattern
    ExegesisA = 16989, // Boss->self, 5.0s cast, single-target
    ExegesisB = 16987, // Boss->self, 5.0s cast, single-target
    ExegesisC = 15622, // Boss->self, 5.0s cast, single-target
    ExegesisD = 16988, // Boss->self, 5.0s cast, single-target
    Exegesis = 15623, // Helper->self, no cast, range 10 width 10 rect
    Catechism = 15625, // Boss->self, 4.0s cast, single-target
    Catechism2 = 15626, // Helper->player, no cast, single-target
    JudgmentDay = 15631, // Boss->self, 3.0s cast, single-target, tower circle 5
    Judged = 15633, // Helper->self, no cast, range 5 circle, tower success
    FoundWanting = 15632, // Helper->self, no cast, range 40 circle, tower fail
    RiteOfTheSacrament = 15629, // Boss->self, no cast, single-target
    PerfectContrition = 15630 // Brightsphere->self, 6.0s cast, range 5-15 donut
}

class PerfectContrition(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5f, 15f);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Brightsphere)
            _aoes.Add(new(donut, actor.Position.Quantized(), default, WorldState.FutureTime(10.6d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PerfectContrition)
            _aoes.Clear();
    }
}

class Catechism(BossModule module) : Components.SingleTargetCastDelay(module, (uint)AID.Catechism, (uint)AID.Catechism2, 0.5f);
class SacramentOfPenance(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.SacramentOfPenanceVisual, (uint)AID.SacramentOfPenance, 0.5f);

class JudgmentDay(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Towers)
            Towers.Add(new(actor.Position.Quantized(), 5f, activation: WorldState.FutureTime(7.6d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Towers.Count != 0 && spell.Action.ID is (uint)AID.Judged or (uint)AID.FoundWanting)
            Towers.RemoveAt(0);
    }
}

class Exegesis(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeRect rect = new(5f, 5f, 5f);
    private static readonly AOEShapeCross cross = new(15f, 5f);
    private static readonly WPos[] diagonalPositions = [new(-240f, -50f), new(-250f, -40f), new(-230f, -40f), new(-250f, -60f), new(-230f, -60f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExegesisA: // diagonal
                for (var i = 0; i < 5; ++i)
                    AddAOE(rect, diagonalPositions[i]);
                break;
            case (uint)AID.ExegesisB: // east+west
                AddAOE(rect, new(-250f, -50f));
                AddAOE(rect, new(-230f, -50f));
                break;
            case (uint)AID.ExegesisC: // north+south
                AddAOE(rect, new(-240f, -60f));
                AddAOE(rect, new(-240f, -40f));
                break;
            case (uint)AID.ExegesisD: // cross
                AddAOE(cross, new(-240f, -50f));
                break;
        }
        void AddAOE(AOEShape shape, WPos position) => _aoes.Add(new(shape, position, default, Module.CastFinishAt(spell, 0.4f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Exegesis)
            _aoes.Clear();
    }
}

class D053ForgivenWhimsyStates : StateMachineBuilder
{
    public D053ForgivenWhimsyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Catechism>()
            .ActivateOnEnter<SacramentOfPenance>()
            .ActivateOnEnter<PerfectContrition>()
            .ActivateOnEnter<JudgmentDay>()
            .ActivateOnEnter<Exegesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8261)]
public class D053ForgivenWhimsy(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240f, -50f), new ArenaBoundsSquare(14.5f));
