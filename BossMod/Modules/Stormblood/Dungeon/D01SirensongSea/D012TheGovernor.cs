namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D012TheGovernor;

public enum OID : uint
{
    Boss = 0x1AFC, // R3.5
    TheGroveller = 0x1AFD, // R1.5
    Helper = 0x18D6 // R0.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    ShadowFlow1 = 8030, // Boss->self, 3.0s cast, single-target
    ShadowFlow2 = 8034, // TheGroveller->self, no cast, single-target
    ShadowFlowCone = 8031, // TheGroveller->self, no cast, single-target
    Shadowstrike = 8029, // TheGroveller->player, no cast, single-target
    Bloodburst = 8028, // Boss->self, 4.0s cast, range 80+R circle
    EnterNight = 8032, // Boss->player, 3.0s cast, single-target, pull 40 between centers
    ShadowSplit = 8033 // Boss->self, 3.0s cast, single-target
}

public enum TetherID : uint
{
    EnterNight = 61 // Boss->player
}

public enum IconID : uint
{
    EnterNight = 22 // player
}

class EnterNightPull(BossModule module) : Components.GenericKnockback(module)
{
    private (Actor, DateTime) target;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (target.Item1 == actor)
            return new Knockback[1] { new(Module.PrimaryActor.Position, 40f, target.Item2, default, default, Kind.TowardsOrigin) };
        return [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is ((uint)IconID.EnterNight))
            target = (actor, WorldState.FutureTime(3d));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.EnterNight)
            target = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (target.Item1 == actor)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
        }
    }
}

class EnterNight(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.EnterNight, 16f, activationDelay: 4.3f);

class ShadowFlow(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private static readonly AOEShapeCone cone = new(22f, 23.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 5 ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void Update()
    {
        if (_aoes.Count != 0)
            _aoes.RemoveAll(aoe => aoe.Activation < WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShadowFlowCone)
        {
            var activation = WorldState.FutureTime(8d);
            var pos = DO12TheGovernor.ArenaCenter.Quantized();
            _aoes.Add(new(cone, pos, caster.Rotation, activation));
            if (_aoes.Count == 6)
            {
                _aoes.Add(new(circle, pos, default, activation));
                foreach (var g in Module.Enemies((uint)OID.TheGroveller))
                    _aoes.Add(new(circle, g.Position.Quantized(), default, activation));
            }
        }
    }
}

class DO12TheGovernorStates : StateMachineBuilder
{
    public DO12TheGovernorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowFlow>()
            .ActivateOnEnter<EnterNightPull>()
            .ActivateOnEnter<EnterNight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 238, NameID = 6072)]
public class DO12TheGovernor(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(-8, 79);
    private static readonly ArenaBoundsComplex arena = new([new Circle(ArenaCenter, 19.25f)], [new Rectangle(new(-1.5f, 60.5f), 20, 1.25f, -20.Degrees()), new Rectangle(new(-8, 99), 20, 1)]);
}
