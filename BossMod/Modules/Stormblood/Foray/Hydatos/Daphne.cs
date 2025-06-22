namespace BossMod.Stormblood.Foray.Hydatos.Daphne;

public enum OID : uint
{
    Boss = 0x2744, // R6.875, x1
    Tentacle = 0x276E, // R7.2
    Gorpokkur = 0x26BA, // R1.0-3.0
    Helper1 = 0x276A,
    Helper2 = 0x276B
}

public enum AID : uint
{
    AutoAttack1 = 14926, // Boss->player, no cast, single-target
    AutoAttack2 = 15223, // Gorpokkur->player, no cast, single-target

    SpellwindCast = 15031, // Boss->self, 4.0s cast, single-target
    SpellwindAOE = 15032, // Helper1->location, no cast, range 40 circle
    Upburst = 15025, // Tentacle->self, 3.5s cast, range 8 circle
    RoilingReach = 15029, // Boss->self, 4.5s cast, range 32 width 7 cross
    Wallop = 15027, // Tentacle->self, 4.0s cast, range 50 width 7 rect
    ChillingGlare = 15030, // Boss->self, 4.0s cast, range 40 circle
    NetherwaterVisual = 15037, // Boss->self, 6.0s cast, single-target
    Netherwater = 15033, // Helper2->player, no cast, range 6 circle

    Tentacle = 15034, // Boss->self, no cast, single-target
    TentacleFinish = 15044, // Tentacle->self, no cast, single-target

    Mutation = 15142, // Gorpokkur->self, 5.0s cast, single-target
    Spiritus = 15417 // Gorpokkur->self, 3.0s cast, range 5 60-degree cone
}

public enum IconID : uint
{
    Stackmarker = 62 // player->self
}

class Spellwind(BossModule module) : Components.RaidwideCast(module, (uint)AID.SpellwindCast);
class Upburst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Upburst, 8f);
class RoilingReach(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoilingReach, new AOEShapeCross(32f, 3.5f));
class Wallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop, new AOEShapeRect(50f, 3.5f));
class ChillingGlare(BossModule module) : Components.CastGaze(module, (uint)AID.ChillingGlare);
class Netherwater(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.Netherwater, 6, 4.9f, 4, 24);
class Spiritus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spiritus, new AOEShapeCone(5, 30f.Degrees()));

class DaphneStates : StateMachineBuilder
{
    public DaphneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spellwind>()
            .ActivateOnEnter<Upburst>()
            .ActivateOnEnter<RoilingReach>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<ChillingGlare>()
            .ActivateOnEnter<Netherwater>()
            .ActivateOnEnter<Spiritus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan, Malediktus", GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1417, SortOrder = 5)]
public class Daphne(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Gorpokkur));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
