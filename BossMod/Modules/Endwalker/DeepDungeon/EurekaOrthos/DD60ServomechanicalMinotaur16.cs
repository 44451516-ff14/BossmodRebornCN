namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD60ServomechanicalMinotaur16;

public enum OID : uint
{
    Boss = 0x3DA1, // R6.0
    BallOfLevin = 0x3DA2, // R1.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    OctupleSwipeTelegraph = 31867, // Helper->self, 1.0s cast, range 40 90-degree cone
    OctupleSwipe = 31872, // Boss->self, 10.8s cast, range 40 90-degree cone
    BullishSwipe1 = 31868, // Boss->self, no cast, range 40 90-degree cone
    BullishSwipe2 = 31869, // Boss->self, no cast, range 40 90-degree cone
    BullishSwipe3 = 31870, // Boss->self, no cast, range 40 90-degree cone
    BullishSwipe4 = 31871, // Boss->self, no cast, range 40 90-degree cone
    DisorientingGroan = 31876, // Boss->self, 5.0s cast, range 60 circle, knockback 15, away from center
    BullishSwipe = 32795, // Boss->self, 5.0s cast, range 40 90-degree cone
    Thundercall = 31873, // Boss->self, 5.0s cast, range 60 circle
    Shock = 31874, // BallOfLevin->self, 2.5s cast, range 5 circle
    BullishSwing = 31875 // Boss->self, 5.0s cast, range 13 circle
}

sealed class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly AOEShapeCone cone = new(40f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OctupleSwipeTelegraph)
        {
            var rot = spell.Rotation;
            var loc = spell.LocXZ;
            var count = _aoes.Count;
            _aoes.Add(new(cone, loc, rot, count == 0 ? Module.CastFinishAt(spell, 8.7d) : _aoes.Ref(0).Activation.AddSeconds(count * 2d), shapeDistance: cone.Distance(loc, rot)));
            if (count == 1)
            {
                _aoes.Ref(0).Color = Colors.Danger;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = _aoes.Count;
        if (count != 0 && spell.Action.ID is (uint)AID.BullishSwipe1 or (uint)AID.BullishSwipe2 or (uint)AID.BullishSwipe3 or (uint)AID.BullishSwipe4 or (uint)AID.OctupleSwipe)
        {
            _aoes.RemoveAt(0);
            if (count > 1)
            {
                _aoes.Ref(0).Color = Colors.Danger;
            }
        }
    }
}

sealed class BullishSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BullishSwing, 13f);
sealed class BullishSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BullishSwipe, new AOEShapeCone(40f, 45f.Degrees()));

sealed class DisorientingGroan(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DisorientingGroan, 15f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 5f), c.Activation);
        }
    }
}

sealed class Shock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(15);
    private readonly AOEShapeCircle circle = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BallOfLevin)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(13d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shock)
        {
            _aoes.Clear();
        }
    }
}

sealed class DD60ServomechanicalMinotaur16States : StateMachineBuilder
{
    public DD60ServomechanicalMinotaur16States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OctupleSwipe>()
            .ActivateOnEnter<BullishSwipe>()
            .ActivateOnEnter<BullishSwing>()
            .ActivateOnEnter<DisorientingGroan>()
            .ActivateOnEnter<Shock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 902, NameID = 12267)]
public sealed class DD60ServomechanicalMinotaur16(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600f, -300f), new ArenaBoundsCircle(20f));
