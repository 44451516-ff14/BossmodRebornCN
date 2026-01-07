namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D012TheGovernor;

public enum OID : uint
{
    TheGovernor = 0x1AFC, // R3.5
    TheGroveller = 0x1AFD, // R1.5
    Helper = 0x18D6 // R0.5
}

public enum AID : uint
{
    AutoAttack = 872, // TheGovernor->player, no cast, single-target

    ShadowFlow1 = 8030, // TheGovernor->self, 3.0s cast, single-target
    ShadowFlow2 = 8034, // TheGroveller->self, no cast, single-target
    ShadowFlowCone = 8031, // TheGroveller->self, no cast, single-target
    Shadowstrike = 8029, // TheGroveller->player, no cast, single-target
    Bloodburst = 8028, // TheGovernor->self, 4.0s cast, range 80+R circle
    EnterNight = 8032, // TheGovernor->player, 3.0s cast, single-target, pull 40 between centers
    ShadowSplit = 8033 // TheGovernor->self, 3.0s cast, single-target
}

public enum TetherID : uint
{
    EnterNight = 61 // TheGovernor->player
}

public enum IconID : uint
{
    EnterNight = 22 // player
}

[SkipLocalsInit]
sealed class EnterNightPull(BossModule module) : Components.GenericKnockback(module)
{
    private int target = -1;
    private Knockback[] _kb;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (slot == target)
        {
            ref readonly var kb = ref _kb[0];
            var pos = Module.PrimaryActor.Position;
            if (pos != kb.Origin) // boss can move after cast started due to interpolation
            {
                _kb = [new(pos, 40f, kb.Activation, default, default, Kind.TowardsOrigin)];
            }
            return _kb;
        }
        return [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == ((uint)IconID.EnterNight))
        {
            target = Raid.FindSlot(targetID);
            _kb = [new(Module.PrimaryActor.Position, 40f, WorldState.FutureTime(3d), default, default, Kind.TowardsOrigin)];
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.EnterNight)
        {
            target = -1;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (target == slot)
        {
            hints.ActionsToExecute.Push(ActionDefinitions.Armslength, actor, ActionQueue.Priority.High);
            hints.ActionsToExecute.Push(ActionDefinitions.Surecast, actor, ActionQueue.Priority.High);
        }
    }
}

[SkipLocalsInit]
sealed class EnterNight(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.EnterNight, 16f, activationDelay: 4.3d);

[SkipLocalsInit]
sealed class ShadowFlow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(6f);
    private readonly AOEShapeCone cone = new(22f, 23f.Degrees());
    private readonly List<AOEInstance> _aoes = new(15);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 5 ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnActorRenderflagsChange(Actor actor, int renderflags)
    {
        if (renderflags == 16384 && actor.OID == (uint)OID.TheGroveller)
        {
            _aoes.Clear();
        }
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
                var grovellers = Module.Enemies((uint)OID.TheGroveller);
                var count = grovellers.Count;
                for (var i = 0; i < count; ++i)
                {
                    _aoes.Add(new(circle, grovellers[i].Position.Quantized(), default, activation));
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class DO12TheGovernorStates : StateMachineBuilder
{
    public DO12TheGovernorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowFlow>()
            .ActivateOnEnter<EnterNightPull>()
            .ActivateOnEnter<EnterNight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DO12TheGovernorStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.TheGovernor,
Contributors = "The Combat Reborn Team (Malediktus), erdelf",
Expansion = BossModuleInfo.Expansion.Stormblood,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 238u,
NameID = 6072u,
SortOrder = 2,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DO12TheGovernor : BossModule
{
    public DO12TheGovernor(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }
    public static readonly WPos ArenaCenter = new(-8f, 79f);

    private DO12TheGovernor(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(ArenaCenter, 19.25f, 32)],
        [new Rectangle(new(-1.5f, 60.5f), 20f, 1.25f, -20f.Degrees()), new Rectangle(new(-8f, 99f), 20f, 1f)]);
        return (arena.Center, arena);
    }
}
