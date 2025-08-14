namespace BossMod.Shadowbringers.Quest.Role.CourageBornOfFear;

public enum OID : uint
{
    Boss = 0x29E1, // r=0.5
    Helper = 0x233C,
    Andreia = 0x29E0,
    Knight = 0x29E4,
}

public enum AID : uint
{
    Overcome = 17088, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    SanctifiedFireII1 = 17188, // 29E3->29DF, no cast, range 5 circle
    MythrilCyclone1 = 17087, // 29DD->self, 4.0s cast, range 50 circle
    SanctifiedMeltdown = 17323, // 29DD->player/29DF, 5.0s cast, range 6 circle
    MythrilCyclone2 = 17207, // 29DD->self, 8.0s cast, range 8-20 donut
    UncloudedAscension1 = 17335, // 2AD1->self, 5.0s cast, range 10 circle
    ThePathOfLight = 17230, // 2A3F->self, 5.5s cast, range 15 circle
    InquisitorsBlade = 17095, // 29E4->self, 5.0s cast, range 40 180-degree cone
    RainOfLight = 17082, // 29DD->location, 3.0s cast, range 4 circle
    ArrowOfFortitude = 17211, // Andreia->self, 4.0s cast, range 30 width 8 rect
    BodkinVolley = 17189, // Andreia->29DF, 6.0s cast, range 5 circle
}

class ArrowOfFortitude(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArrowOfFortitude, new AOEShapeRect(30f, 4f));
class BodkinVolley(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BodkinVolley, 5f, minStackSize: 1);
class RainOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RainOfLight, 4f);
class ThePathOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThePathOfLight, 15f);
class InquisitorsBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InquisitorsBlade, new AOEShapeCone(40f, 90f.Degrees()));
class MythrilCycloneKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.MythrilCyclone1, 18f, stopAtWall: true);
class MythrilCycloneDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MythrilCyclone2, new AOEShapeDonut(8f, 20f));
class SanctifiedMeltdown(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SanctifiedMeltdown, 6f);
class UncloudedAscension(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UncloudedAscension1, 10f);
class Overcome(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overcome, new AOEShapeCone(8.5f, 60f.Degrees()));

class SanctifiedFireII(BossModule module) : Components.BaitAwayIcon(module, 5f, 23u)
{
    private DateTime Timeout = DateTime.MaxValue;

    public override void Update()
    {
        // for some reason, the magus can just forget to cast the two followups, leaving lue-reeq to run around like a moron
        if (WorldState.CurrentTime > Timeout && CurrentBaits.Count > 0)
            Reset();
    }

    private void Reset()
    {
        CurrentBaits.Clear();
        NumCasts = 0;
        Timeout = DateTime.MaxValue;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);
        if (iconID == IID)
            Timeout = WorldState.FutureTime(10d);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x29E5u && ++NumCasts >= 3)
            Reset();
    }
}

class FireVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.SanctifiedFireII1, m => m.Enemies(0x29E5).Where(e => e.EventState != 7), 0.25f);

class ImmaculateWarriorStates : StateMachineBuilder
{
    public ImmaculateWarriorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overcome>()
            .ActivateOnEnter<SanctifiedFireII>()
            .ActivateOnEnter<FireVoidzone>()
            .ActivateOnEnter<MythrilCycloneDonut>()
            .ActivateOnEnter<MythrilCycloneKB>()
            .ActivateOnEnter<SanctifiedMeltdown>()
            .ActivateOnEnter<UncloudedAscension>()
            .ActivateOnEnter<ThePathOfLight>()
            .ActivateOnEnter<InquisitorsBlade>()
            .ActivateOnEnter<RainOfLight>()
            .ActivateOnEnter<ArrowOfFortitude>()
            .ActivateOnEnter<BodkinVolley>()
            .Raw.Update = () => Module.Enemies(OID.Andreia).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68814, NameID = 8782)]
public class ImmaculateWarrior(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247, 688.5f), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var h = hints.PotentialTargets[i];
            h.Priority = h.Actor.TargetID == actor.InstanceID ? 1 : 0;
        }
    }
}
