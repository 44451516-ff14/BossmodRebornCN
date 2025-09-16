namespace BossMod.Heavensward.Quest.MSQ.DivineIntervention;

public enum OID : uint
{
    Boss = 0x1010, // R0.5
    IshgardianSteelChain = 0x102C, // R1.0
    SerPaulecrainColdfire = 0x1011, // R0.5
    ThunderPicket = 0xEC4, // R1.0
    Helper = 0xE0F
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->Alphinaud, no cast, single-target
    AutoAttack2 = 871, // SerPaulecrainColdfire->player, no cast, single-target

    Foresight = 32, // Boss->self, no cast, single-target
    LightningBolt = 3993, // ThunderPicket->Helper, 2.0s cast, width 4 rect charge
    IronTempest = 1003, // Boss->self, 3.5s cast, range 5+R circle
    Overpower = 720, // Boss->self, 2.5s cast, range 6+R 90-degree cone
    RingOfFrost = 1316, // SerPaulecrainColdfire->self, 3.0s cast, range 6+R circle
    Rive = 1135, // Boss->self, 2.5s cast, range 30+R width 2 rect
    Heartstopper = 866, // SerPaulecrainColdfire->self, 2.5s cast, range 3+R width 3 rect
    FirstLesson = 3991, // Boss->100F, no cast, single-target
    Bloodbath = 34, // Boss->self, no cast, single-target
    BarbaricSurge = 963, // Boss->self, no cast, single-target
    ThunderThrust = 3992, // SerPaulecrainColdfire->self, 4.0s cast, range 40 circle
    LifeSurge = 83 // SerPaulecrainColdfire->self, no cast, single-target
}

class LightningBolt(BossModule module) : Components.ChargeAOEs(module, (uint)AID.LightningBolt, 2);
class IronTempest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronTempest, 5.5f);
class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(6.5f, 45.Degrees()));
class RingOfFrost(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RingOfFrost, 6.5f);
class Rive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rive, new AOEShapeRect(30.5f, 1));
class Heartstopper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Heartstopper, new AOEShapeRect(3.5f, 1.5f));
class ThunderThrust(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderThrust);

class SerGrinnauxStates : StateMachineBuilder
{
    public SerGrinnauxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<IronTempest>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<RingOfFrost>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<ThunderThrust>()
            .Raw.Update = () => AllDeadOrDestroyed(SerGrinnaux.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67133, NameID = 3850)]
public class SerGrinnaux(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Capsule(new(default, 1.979f), 3.66f, 11.45f, 50, 90f.Degrees())], [new Rectangle(new(default, -9.995f), 4f, 0.7f)]);
    public static readonly uint[] Bosses = [(uint)OID.Boss, (uint)OID.SerPaulecrainColdfire];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Bosses);
        Arena.Actors(Enemies((uint)OID.IshgardianSteelChain), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.IshgardianSteelChain => 1,
                _ => 0
            };
        }
    }
}
