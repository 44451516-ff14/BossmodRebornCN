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
    FrontHead1 = 16874,
    FrontHead2 = 16881,
    FrontHead3 = 16882,
    FrontHead4 = 16885,
    FrontHead5 = 16888,
    FrontHead6 = 16889,
    FrontHead7 = 16896,
    FrontHead8 = 16897,
    FrontHead9 = 16900,
    FrontHead10 = 16903,
    RightHead1 = 16875,
    RightHead2 = 16878,
    RightHead3 = 16879,
    RightHead4 = 16886,
    RightHead5 = 16887,
    RightHead6 = 16890,
    RightHead7 = 16893,
    RightHead8 = 16894,
    RightHead9 = 16901,
    RightHead10 = 16902,
    LeftHead1 = 16876,
    LeftHead2 = 16877,
    LeftHead3 = 16880,
    LeftHead4 = 16883,
    LeftHead5 = 16884,
    LeftHead6 = 16891,
    LeftHead7 = 16892,
    LeftHead8 = 16895,
    LeftHead9 = 16898,
    LeftHead10 = 16899
}

sealed class PyricBlast(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PyricBlast, 6f, 8);
sealed class Intimidation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Intimidation);
sealed class Brutality(BossModule module) : Components.CastHint(module, (uint)AID.Brutality, "Applies Haste");

sealed class BreathSequence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly Angle angle = 120f.Degrees();
    private static readonly AOEShapeCone cone = new(40f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes[..max];
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id > 16903)
            return;
        void AddAOE(Angle offset = default) => _aoes.Add(new(cone, actor.Position, actor.Rotation + offset, WorldState.FutureTime(10.5d))); // activation is a placeholder, gets replaced when sequence starts
        switch (id)
        {
            case (ushort)NPCYell.FrontHead1:
            case (ushort)NPCYell.FrontHead2:
            case (ushort)NPCYell.FrontHead3:
            case (ushort)NPCYell.FrontHead4:
            case (ushort)NPCYell.FrontHead5:
            case (ushort)NPCYell.FrontHead6:
            case (ushort)NPCYell.FrontHead7:
            case (ushort)NPCYell.FrontHead8:
            case (ushort)NPCYell.FrontHead9:
            case (ushort)NPCYell.FrontHead10:
                AddAOE();
                break;
            case (ushort)NPCYell.LeftHead1:
            case (ushort)NPCYell.LeftHead2:
            case (ushort)NPCYell.LeftHead3:
            case (ushort)NPCYell.LeftHead4:
            case (ushort)NPCYell.LeftHead5:
            case (ushort)NPCYell.LeftHead6:
            case (ushort)NPCYell.LeftHead7:
            case (ushort)NPCYell.LeftHead8:
            case (ushort)NPCYell.LeftHead9:
            case (ushort)NPCYell.LeftHead10:
                AddAOE(angle);
                break;
            case (ushort)NPCYell.RightHead1:
            case (ushort)NPCYell.RightHead2:
            case (ushort)NPCYell.RightHead3:
            case (ushort)NPCYell.RightHead4:
            case (ushort)NPCYell.RightHead5:
            case (ushort)NPCYell.RightHead6:
            case (ushort)NPCYell.RightHead7:
            case (ushort)NPCYell.RightHead8:
            case (ushort)NPCYell.RightHead9:
            case (ushort)NPCYell.RightHead10:
                AddAOE(-angle);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        if (count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.BreathSequenceFirstFront:
                case (uint)AID.BreathSequenceFirstLeft:
                case (uint)AID.BreathSequenceFirstRight:
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    for (var i = 0; i < count; ++i)
                    {
                        aoes[i].Activation = Module.CastFinishAt(spell, 2.3d * i);
                    }
                    break;
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.BreathSequenceFirstFront:
                case (uint)AID.BreathSequenceFirstLeft:
                case (uint)AID.BreathSequenceFirstRight:
                case (uint)AID.BreathSequenceRestFront:
                case (uint)AID.BreathSequenceRestLeft:
                case (uint)AID.BreathSequenceRestRight:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

sealed class AtticusThePrimogenitorStates : StateMachineBuilder
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
public sealed class AtticusThePrimogenitor(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
