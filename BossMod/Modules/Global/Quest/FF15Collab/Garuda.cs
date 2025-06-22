namespace BossMod.Global.Quest.FF15Collab.Garuda;

public enum OID : uint
{
    Boss = 0x257A, //R=1.7
    Helper = 0x233C,
    Monolith = 0x2654, //R=2.3
    Noctis = 0x2651,
    GravityVoidzone = 0x1E91C1,
    Turbulence = 0x2653 // cage just before quicktime event
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    MistralShriek = 14611, // Boss->self, 7.0s cast, range 30 circle
    MistralSong = 14616, // Boss->self, 3.5s cast, range 20 150-degree cone
    MiniSupercell = 14588, // Helper->Noctis, no cast, single-target, linestack target
    MiniSupercell2 = 14612, // Boss->self, 5.0s cast, range 45 width 6 rect, line stack, knockback 50, away from source
    GravitationalForce = 14614, // Boss->self, 3.5s cast, single-target
    GravitationalForce2 = 14615, // Helper->location, 3.5s cast, range 5 circle
    Vortex = 14677, // Helper->self, no cast, range 50 circle
    Vortex2 = 14620, // Helper->self, no cast, range 50 circle
    Vortex3 = 14622, // Helper->self, no cast, range 50 circle
    Vortex4 = 14623, // Helper->self, no cast, range 50 circle
    Microburst = 14619, // Boss->self, 17.3s cast, range 25 circle
    GustFront = 14617, // Boss->self, no cast, single-target, dorito stack
    GustFront2 = 14618, // Helper->player/Noctis, no cast, single-target
    WickedTornado = 14613, // Boss->self, 3.5s cast, range 8-20 donut
    MistralGaol = 14621, // Boss->self, 5.0s cast, range 6 circle, quick time event starts
    Microburst2 = 14624, // Boss->self, no cast, range 25 circle, quick time event failed (enrage)
    Warpstrike = 14597, // duty action for player
}

class GustFront(BossModule module) : Components.UniformStackSpread(module, 1.2f, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GustFront)
            AddStack(Module.Enemies((uint)OID.Noctis)[0]);
        else if (spell.Action.ID == (uint)AID.GustFront2)
            Stacks.Clear();
    }
}

class Microburst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Microburst, 18f)
{
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.Microburst)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.Microburst)
            casting = false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add($"Keep using duty action on the {Module.Enemies(OID.Monolith)[0].Name}s to stay out of the AOE!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (casting && actor.Position.AlmostEqual(Module.PrimaryActor.Position, 15f))
            hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Warpstrike), Module.Enemies((uint)OID.Monolith)[0], ActionQueue.Priority.High);
    }
}

class MistralShriek(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MistralShriek, 30f)
{
    private bool casting;
    private DateTime done;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.MistralShriek)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.MistralShriek)
            casting = false;
        done = WorldState.CurrentTime;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add($"Use duty action to teleport to the {Module.Enemies(OID.Monolith)[0].Name} at the opposite side of Garuda!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (casting)
        {
            var primaryActor = Module.PrimaryActor;
            var primaryPosition = primaryActor.Position;

            var monoliths = Module.Enemies((uint)OID.Monolith);
            var count = monoliths.Count;
            for (var i = 0; i < count; ++i)
            {
                var m = monoliths[i];
                if (!m.Position.AlmostEqual(primaryPosition, 5f))
                {
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Warpstrike), m, ActionQueue.Priority.High);
                    break;
                }
            }
        }
        if (WorldState.CurrentTime > done && WorldState.CurrentTime < done.AddSeconds(2d))
            hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Warpstrike), Module.PrimaryActor, ActionQueue.Priority.High);
    }
}

class MistralSong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MistralSong, new AOEShapeCone(20f, 75f.Degrees()));
class WickedTornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WickedTornado, new AOEShapeDonut(8f, 20f));
class MiniSupercell(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.MiniSupercell, (uint)AID.MiniSupercell2, 5f, 45f, 3f, 2);
class MiniSupercellKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.MiniSupercell2, 50f, shape: new AOEShapeRect(45f, 3f), stopAtWall: true);

class GravitationalForce(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.GravitationalForce2, GetVoidzones, 0f)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.GravityVoidzone);
}
class MistralGaol(BossModule module) : Components.CastHint(module, (uint)AID.MistralGaol, "Prepare for Quick Time Event (spam buttons when it starts)");

class GarudaStates : StateMachineBuilder
{
    public GarudaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MistralShriek>()
            .ActivateOnEnter<GustFront>()
            .ActivateOnEnter<MistralSong>()
            .ActivateOnEnter<GravitationalForce>()
            .ActivateOnEnter<MiniSupercell>()
            .ActivateOnEnter<MiniSupercellKB>()
            .ActivateOnEnter<Microburst>()
            .ActivateOnEnter<WickedTornado>()
            .ActivateOnEnter<MistralGaol>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68696, NameID = 7893)] // also: CFC 646
public class Garuda(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(22))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Noctis), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.Monolith), Colors.Object);
    }
}
