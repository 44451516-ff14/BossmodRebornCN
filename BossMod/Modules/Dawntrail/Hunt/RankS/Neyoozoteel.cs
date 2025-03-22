namespace BossMod.Dawntrail.Hunt.RankS.Neyoozoteel;

public enum OID : uint
{
    Boss = 0x4233 // R6.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WhirlingOmen1 = 37377, // Boss->self, 3.0s cast, single-target -> rear, right, right
    WhirlingOmen2 = 37376, // Boss->self, 3.0s cast, single-target -> left, rear, right
    WhirlingOmen3 = 37378, // Boss->self, 3.0s cast, single-target -> right, left, rear
    WhirlingOmen4 = 37379, // Boss->self, 3.0s cast, single-target -> left, rear, left
    SapSpiller = 37397, // Boss->self, 12.0s cast, single-target
    NoxiousSap1 = 37308, // Boss->self, 5.0s cast, range 30 120-degree cone
    NoxiousSap2 = 37370, // Boss->self, no cast, range 30 120-degree cone
    NoxiousSap3 = 37371, // Boss->self, no cast, range 30 120-degree cone
    NoxiousSap4 = 42172, // Boss->self, no cast, range 30 120-degree cone
    NoxiousSap5 = 42173, // Boss->self, no cast, range 30 120-degree cone
    NoxiousSap6 = 42174, // Boss->self, no cast, range 30 120-degree cone
    NoxiousSap7 = 37394, // Boss->self, no cast, range 30 120-degree cone
    NoxiousSap8 = 37395, // Boss->self, no cast, range 30 120-degree cone
    NoxiousSap9 = 37396, // Boss->self, no cast, range 30 120-degree cone

    Neurotoxify = 38331, // Boss->self, 5.0s cast, range 40 circle

    Cocopult = 37307, // Boss->players, 5.0s cast, range 5 circle, stack
    RavagingRootsCW = 37373, // Boss->self, 5.0s cast, range 30 width 6 cross, 8x, -45° increment
    RavagingRootsCCW = 37374, // Boss->self, 5.0s cast, range 30 width 6 cross, 8x, 45° increment
    RavagingRootsRest = 37375 // Boss->self, no cast, range 30 width 6 cross
}

class Neurotoxify(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Neurotoxify));
class NoxiousSap1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.NoxiousSap1), new AOEShapeCone(40f, 60f.Degrees()));
class Cocopult(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Cocopult), 5f, 8);

class RavagingRoots(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross cross = new(30f, 3f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RavagingRootsCW:
                AddSequence(45f.Degrees());
                break;
            case (uint)AID.RavagingRootsCCW:
                AddSequence(-45f.Degrees());
                break;
        }
        void AddSequence(Angle increment) => Sequences.Add(new(cross, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 2.4f, 8));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RavagingRootsCCW or (uint)AID.RavagingRootsCW or (uint)AID.RavagingRootsRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class SapSpiller(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 60f.Degrees());
    private static readonly Angle a180 = 180f.Degrees(), a90 = 90f.Degrees();
    private readonly List<AOEInstance> _aoes = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WhirlingOmen1:
                AddAOEs([a180, -a90, -a90]);
                break;
            case (uint)AID.WhirlingOmen2:
                AddAOEs([a90, a180, -a90]);
                break;
            case (uint)AID.WhirlingOmen3:
                AddAOEs([-a90, a90, a180]);
                break;
            case (uint)AID.WhirlingOmen4:
                AddAOEs([a90, a180, a90]);
                break;
        }
        void AddAOEs(Angle[] angles)
        {
            for (var i = 0; i < 3; ++i)
            {
                var angle = (i == 0 ? spell.Rotation : _aoes[i - 1].Rotation) + angles[i];
                _aoes.Add(new(cone, Module.PrimaryActor.Position, angle, Module.CastFinishAt(spell, 14.6f + 2.2f * i)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.NoxiousSap2:
                case (uint)AID.NoxiousSap3:
                case (uint)AID.NoxiousSap4:
                case (uint)AID.NoxiousSap5:
                case (uint)AID.NoxiousSap6:
                case (uint)AID.NoxiousSap7:
                case (uint)AID.NoxiousSap8:
                case (uint)AID.NoxiousSap9:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class NeyoozoteelStates : StateMachineBuilder
{
    public NeyoozoteelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NoxiousSap1>()
            .ActivateOnEnter<SapSpiller>()
            .ActivateOnEnter<Cocopult>()
            .ActivateOnEnter<Neurotoxify>()
            .ActivateOnEnter<RavagingRoots>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 12754)]
public class Neyoozoteel(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
