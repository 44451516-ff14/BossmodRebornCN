namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D062Harmachis;

public enum OID : uint
{
    Boss = 0xE9A, // R2.000-5.300, x1
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BallisticMissile1 = 4334, // Boss->self, 3.0s cast, single-target
    BallisticMissile2 = 4335, // Helper->self, no cast, ???
    BallisticMissileVisual = 4771, // Helper->self, 4.0s cast, range 4 circle

    ChthonicHush = 4327, // Boss->self, no cast, range 12+R (R=5.3) 120-degree cone
    CircleOfFlames = 4332, // Boss->player, no cast, range 5 circle
    GaseousBomb = 4336, // Boss->player, no cast, range 5 circle
    HoodSwing = 4329, // Boss->self, no cast, range 8+R ?-degree cone
    InertiaStream = 4333, // Boss->player, no cast, single-target
    Ka = 4326, // Boss->self, 3.0s cast, range 40+R 60-degree cone
    Paradox = 4325, // Helper->location, 3.0s cast, range 5 circle
    Petrifaction = 4331, // Boss->self, 3.0s cast, range 60 circle
    RiddleOfTheSphinx = 4324, // Boss->self, 3.0s cast, single-target
    SteelScales = 4330, // Boss->self, no cast, single-target

    WeighingOfTheHeart1 = 3790, // Boss->self, 3.0s cast, single-target
    WeighingOfTheHeart2 = 3792, // Boss->self, 3.0s cast, single-target
    WeighingOfTheHeart3 = 4328, // Boss->self, 3.0s cast, single-target
    WeighingOfTheHeartSphinxForm = 5007 // Helper->self, no cast, single-target
}

public enum SID : uint
{
    Bind = 2518 // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Stack = 93 // player
}

sealed class Paradox(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Paradox, 5f);
sealed class Petrifaction(BossModule module) : Components.CastGaze(module, (uint)AID.Petrifaction);
sealed class Ka(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ka, new AOEShapeCone(45f, 30f.Degrees()));
sealed class GaseousBomb(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, (uint)AID.GaseousBomb, 5f, 4.1d, 4, 4);

sealed class BallisticMissile(BossModule module) : Components.UniformStackSpread(module, 4f, default, 2, 2)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Bind)
            AddStack(actor, WorldState.FutureTime(6.2d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BallisticMissile2)
            Stacks.Clear();
    }
}

sealed class ChthonicHush(BossModule module) : Components.Cleave(module, (uint)AID.ChthonicHush, new AOEShapeCone(13.3f, 60f.Degrees()))
{
    private readonly GaseousBomb _stack1 = module.FindComponent<GaseousBomb>()!;
    private readonly BallisticMissile _stack2 = module.FindComponent<BallisticMissile>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_stack1.Stacks.Count == 0 && _stack2.Stacks.Count == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_stack1.Stacks.Count == 0 && _stack2.Stacks.Count == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_stack1.Stacks.Count == 0 && _stack2.Stacks.Count == 0)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}

sealed class D062HarmachisStates : StateMachineBuilder
{
    public D062HarmachisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Paradox>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<Ka>()
            .ActivateOnEnter<GaseousBomb>()
            .ActivateOnEnter<BallisticMissile>()
            .ActivateOnEnter<ChthonicHush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38u, NameID = 3821u, SortOrder = 6)]
public sealed class D062Harmachis : BossModule
{
    public D062Harmachis(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D062Harmachis(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(248f, 272f), 19.25f, 44)], [new Rectangle(new(228f, 272f), 1.8f, 20f),
            new Rectangle(new(268.25f, 272f), 2f, 20f)]);
        return (arena.Center, arena);
    }
}
