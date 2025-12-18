namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD90TheGodmother;

public enum OID : uint
{
    Boss = 0x1817, // R3.75
    LavaBomb = 0x18E9, // R0.6
    GreyBomb = 0x18E8, // R1.2
    GiddyBomb = 0x18EA // R1.2
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Burst = 7105, // GreyBomb->self, 20.0s cast, range 50+R circle
    HypothermalCombustion = 7104, // GiddyBomb->self, 5.0s cast, range 6+R circle
    MassiveBurst = 7102, // Boss->self, 25.0s cast, range 50 circle
    Sap = 7101, // Boss->location, 3.5s cast, range 8 circle
    ScaldingScolding = 7100, // Boss->self, no cast, range 8+R 120-degree cone
    SelfDestruct = 7106 // LavaBomb->self, 3.0s cast, range 6+R circle
}

sealed class GreyBomb(BossModule module) : Components.Adds(module, (uint)OID.GreyBomb, 5, true);
sealed class Burst(BossModule module) : Components.RaidwideCast(module, (uint)AID.Burst, "Kill the Grey Bomb! or take 80% of your Max HP");
// future thing to do: maybe add a tether between bomb/boss to show it needs to show the aoe needs to explode on them. . . 
sealed class HypothermalCombustion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HypothermalCombustion, 7.2f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var bomb = Module.Enemies((uint)OID.GiddyBomb);
        if (bomb.Count != 0 && bomb[0] is Actor g && Module.PrimaryActor.Position.InCircle(g.Position, 7.2f))
        {
            hints.SetPriority(g, AIHints.Enemy.PriorityForbidden);
        }
    }
}

sealed class GiddyBomb(BossModule module) : BossComponent(module)
{
    public static readonly WPos[] BombSpawns = [new(-305f, -240f), new(-295f, -240f), new(-295f, -240f), new(-300f, -235f)];

    private int _index;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HypothermalCombustion)
        {
            ++_index;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // not tanking
        if (Module.PrimaryActor.TargetID != actor.InstanceID)
        {
            return;
        }

        // giddy bomb is alive, don't pull anywhere
        var giddybombs = Module.Enemies((uint)OID.GiddyBomb);
        if (giddybombs.Count == 0 || !giddybombs[0].IsDead)
        {
            return;
        }

        var nextBombSpot = BombSpawns[_index & 3];
        hints.GoalZones.Add(hints.PullTargetToLocation(Module.PrimaryActor, nextBombSpot));
    }
}
sealed class MassiveBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.MassiveBurst, "Knock the Giddy bomb into the boss and let it explode on the boss. \n or else take 99% damage!");
sealed class Sap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sap, 8f);
sealed class ScaldingScolding(BossModule module) : Components.Cleave(module, (uint)AID.ScaldingScolding, new AOEShapeCone(11.75f, 60f.Degrees()))
{
    private readonly MassiveBurst _raidwide = module.FindComponent<MassiveBurst>()!;
    private readonly Sap _aoe = module.FindComponent<Sap>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_raidwide.Active && _aoe.Casters.Count == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_raidwide.Active && _aoe.Casters.Count == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_raidwide.Active && _aoe.Casters.Count == 0)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}

sealed class SelfDestruct(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6.6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LavaBomb)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(10d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SelfDestruct)
        {
            _aoes.Clear();
        }
    }
}

sealed class DD90TheGodmotherStates : StateMachineBuilder
{
    public DD90TheGodmotherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GreyBomb>()
            .ActivateOnEnter<GiddyBomb>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<HypothermalCombustion>()
            .ActivateOnEnter<MassiveBurst>()
            .ActivateOnEnter<Sap>()
            .ActivateOnEnter<ScaldingScolding>()
            .ActivateOnEnter<SelfDestruct>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 207, NameID = 5345)]
public sealed class DD90TheGodmother(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds2090110.Center, SharedBounds.ArenaBounds2090110);
