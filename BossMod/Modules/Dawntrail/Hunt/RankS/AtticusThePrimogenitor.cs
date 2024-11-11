namespace BossMod.Dawntrail.Hunt.RankS.AtticusThePrimogenitor;

public enum OID : uint
{
    Boss = 0x416D // R4.7
}

public enum AID : uint
{
    AutoAttack = 39015, // Boss->player, no cast, single-target

    HeadSpeaks = 39883, // Boss->self, no cast, single-target
    HeadSpeaks2 = 39884, // Boss->self, no cast, single-target
    BreathSequenceFirstFront = 39003, // Boss->self, 5.0s cast, range 60 120-degree cone
    BreathSequenceFirstRight = 39004, // Boss->self, 5.0s cast, range 60 120-degree cone
    BreathSequenceFirstLeft = 39005, // Boss->self, 5.0s cast, range 60 120-degree cone
    BreathSequenceRestFront = 39009, // Boss->self, no cast, range 60 120-degree cone
    BreathSequenceRestRight = 39010, // Boss->self, no cast, range 60 120-degree cone
    BreathSequenceRestLeft = 39011, // Boss->self, no cast, range 60 120-degree cone
    Intimidation = 39014, // Boss->self, 4.0s cast, range 40 circle
    Brutality = 39012, // Boss->self, 3.0s cast, single-target, applies Haste
    PyricBlast = 39013 // Boss->players, 5.0s cast, range 6 circle, stack
}

public enum NPCYell : ushort
{
    AttackThreeTimes1 = 16904,
    AttackSixTimes1 = 16905,
    Brutality = 16906,
    FrontHead1 = 16881,
    FrontHead2 = 16882,
    FrontHead3 = 16888,
    FrontHead4 = 16889,
    FrontHead5 = 16874,
    FrontHead6 = 16896,
    FrontHead7 = 16885,
    FrontHead8 = 16900,
    FrontHead9 = 16897,
    FrontHead10 = 16903,
    RightHead1 = 16879,
    RightHead2 = 16887,
    RightHead3 = 16893,
    RightHead4 = 16875,
    RightHead5 = 16894,
    RightHead6 = 16886,
    RightHead7 = 16901,
    RightHead8 = 16878,
    RightHead9 = 16902,
    RightHead10 = 16890,
    LeftHead1 = 16883,
    LeftHead2 = 16880,
    LeftHead3 = 16884,
    LeftHead4 = 16892,
    LeftHead5 = 16876,
    LeftHead6 = 16895,
    LeftHead7 = 16899,
    LeftHead8 = 16877,
    LeftHead9 = 16898,
    LeftHead10 = 16891
}

class PyricBlast(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.PyricBlast), 6, 8);
class Intimidation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Intimidation));
class Brutality(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Brutality), "Applies Haste");

class BreathSequence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly Angle angle = 120.Degrees();
    private static readonly AOEShapeCone cone = new(40, 60.Degrees());
    private static readonly HashSet<ushort> frontHead = new(
        new[] { NPCYell.FrontHead1, NPCYell.FrontHead2, NPCYell.FrontHead3, NPCYell.FrontHead4,
                NPCYell.FrontHead5, NPCYell.FrontHead6, NPCYell.FrontHead7, NPCYell.FrontHead8, NPCYell.FrontHead9, NPCYell.FrontHead10 }
        .Select(x => (ushort)x));
    private static readonly HashSet<ushort> leftHead = new(
        new[] { NPCYell.LeftHead1, NPCYell.LeftHead2, NPCYell.LeftHead3, NPCYell.LeftHead4,
                NPCYell.LeftHead5, NPCYell.LeftHead6, NPCYell.LeftHead7, NPCYell.LeftHead8, NPCYell.LeftHead9, NPCYell.LeftHead10 }
        .Select(x => (ushort)x));
    private static readonly HashSet<ushort> rightHead = new(
        new[] { NPCYell.RightHead1, NPCYell.RightHead2, NPCYell.RightHead3, NPCYell.RightHead4,
                NPCYell.RightHead5, NPCYell.RightHead6, NPCYell.RightHead7, NPCYell.RightHead8, NPCYell.RightHead9, NPCYell.RightHead10 }
        .Select(x => (ushort)x));
    private static readonly HashSet<AID> castEnd = [AID.BreathSequenceFirstFront, AID.BreathSequenceFirstLeft, AID.BreathSequenceFirstRight,
               AID.BreathSequenceRestFront, AID.BreathSequenceRestLeft, AID.BreathSequenceRestRight];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (count > 1)
            yield return _aoes[1];
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        var activation = WorldState.FutureTime(10.5f); // placeholder, gets replaced when sequence starts
        if (frontHead.Contains(id))
            _aoes.Add(new(cone, actor.Position, actor.Rotation, activation));
        else if (leftHead.Contains(id))
            _aoes.Add(new(cone, actor.Position, actor.Rotation + angle, activation));
        else if (rightHead.Contains(id))
            _aoes.Add(new(cone, actor.Position, actor.Rotation - angle, activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        if (count > 0)
            switch ((AID)spell.Action.ID)
            {
                case AID.BreathSequenceFirstFront:
                case AID.BreathSequenceFirstLeft:
                case AID.BreathSequenceFirstRight:
                    for (var i = 0; i < count; ++i)
                        _aoes[i] = _aoes[i] with { Activation = Module.CastFinishAt(spell, 2.3f * i) };
                    break;
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class AtticusThePrimogenitorStates : StateMachineBuilder
{
    public AtticusThePrimogenitorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BreathSequence>()
            .ActivateOnEnter<Brutality>()
            .ActivateOnEnter<Intimidation>()
            .ActivateOnEnter<PyricBlast>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13156)]
public class AtticusThePrimogenitor(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
