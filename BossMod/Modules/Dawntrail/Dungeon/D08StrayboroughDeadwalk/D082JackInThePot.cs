namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D082JackInThePot;

public enum OID : uint
{
    Boss = 0x41CA, // R4.16
    SpectralSamovar = 0x41CB, // R2.88
    StrayPhantagenitrix = 0x41D2, // R2.1
    TeacupHelper = 0x41D5, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 37169, // Boss->player, no cast, single-target

    TroublingTeacups = 36716, // Boss->self, 3.0s cast, single-target, Spawns teacups
    TeaAwhirl = 36717, // Boss->self, 6.0s cast, single-target, ghost(s) tether teacup and enters, teacups spin then possesed teacup explodes in AOE
    TricksomeTreat = 36720, // StrayPhantagenitrix->self, 3.0s cast, range 19 circle, TeaAwhirl AOE

    ToilingTeapots = 36722, // Boss->self, 3.0s cast, single-target, spawns 13 teacups

    Puppet = 36721, // StrayPhantagenitrix->location, 4.0s cast, single-target
    PipingPour = 36723, // SpectralSamovar->location, 2.0s cast, single-target, spreading AOE

    MadTeaParty = 36724, // Helper->self, no cast, range 0 circle, DOT applied to players in puddles

    LastDrop = 36726, // Boss->player, 5.0s cast, single-target, tankbuster

    SordidSteam = 36725 // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum TetherID : uint
{
    CupTether = 276 // UnknownActor->StrayPhantagenitrix
}

public enum SID : uint
{
    AreaOfInfluenceUp = 1909 // none->Helper, extra=0x1/0x2/0x3/0x4
}

class PipingPour(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (_aoes.Count > 0 && id == 0x11DD && (OID)actor.OID == OID.SpectralSamovar)
            _aoes.RemoveAt(0);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AreaOfInfluenceUp && status.Extra == 0x1)
            _aoes.Add(new(circle, actor.Position));
    }
}

class TeaAwhirl : Components.GenericAOEs
{
    private static readonly AOEShapeCircle circle = new(19);
    private readonly List<Actor> _cups = [];
    private readonly List<AOEInstance> _aoes = [];
    private readonly Dictionary<uint, Action> cupPositions;

    public TeaAwhirl(BossModule module) : base(module)
    {
        cupPositions = new Dictionary<uint, Action>
        {
            { 0x02000100, () => HandleActivation(11.5f,
                [
                    (new(17, -163), new(17, -177), [new(3.5f, -161.5f), new(30.5f, -178.5f)]),
                    (new(17, -153), new(10, -170), [new(25.5f, -156.5f), new(20.5f, -178.5f)]),
                    (new(17, -153), new(17, -177), [new(20.5f, -178.5f), new(3.5f, -161.5f)]),
                    (new(34, -170), null, [new(8.5f, -173.5f)]),
                    (new(0, -170), null, [new(25.5f, -166.5f)])
                ])
            },
            { 0x10000800, () => HandleActivation(14.5f,
                [
                    (new(0, -170), new(34, -170), [new(8.5f, -156.5f), new(25.5f, -183.5f)]),
                    (new(0, -170), new(17, -187), [new(3.5f, -178.5f), new(8.5f, -156.5f)]),
                    (new(17, -187), new(17, -153), [new(30.5f, -161.5f), new(3.5f, -178.5f)])
                ])
            },
            { 0x00100001, () => AddAOEs(WorldState.FutureTime(16), _cups[0].Position, _cups[1].Position) },
            { 0x00400020, () => HandleActivation(19f,
                [
                    (new(0, -170), new(17, -163), [new(5, -165), new(22, -182)]),
                    (new(17, -177), new(17, -153), [new(5, -175), new(29, -175)])
                ])
            }
        };
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.CupTether)
            _cups.Add(source);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is 0x23 or 0x01)
        {
            if (cupPositions.TryGetValue(state, out var action))
                action.Invoke();
        }
    }

    private void HandleActivation(float futureTime, List<(WPos pos1, WPos? pos2, WPos[] positions)> cups)
    {
        var activation = WorldState.FutureTime(futureTime);
        foreach (var (pos1, pos2, positions) in cups)
            if (CheckPositions(pos1, pos2))
            {
                AddAOEs(activation, positions);
                return;
            }
    }

    private bool CheckPositions(WPos pos1, WPos? pos2) => pos2 != null ? _cups.Any(x => x.Position == pos1) && _cups.Any(x => x.Position == pos2) : _cups.Any(x => x.Position == pos1);

    private void AddAOEs(DateTime activation, params WPos[] positions)
    {
        foreach (var pos in positions)
            _aoes.Add(new(circle, pos, default, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TricksomeTreat)
        {
            _aoes.Clear();
            _cups.Clear();
        }
    }
}

class SordidSteam(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SordidSteam));
class LastDrop(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.LastDrop));

class D082JackInThePotStates : StateMachineBuilder
{
    public D082JackInThePotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TeaAwhirl>()
            .ActivateOnEnter<PipingPour>()
            .ActivateOnEnter<SordidSteam>()
            .ActivateOnEnter<LastDrop>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12760)]
public class D082JackInThePot(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(17, -170), 19.5f)], [new Rectangle(new(17, -150.15f), 20, 1.25f), new Rectangle(new(17, -189.5f), 20, 1.25f)]);
}
