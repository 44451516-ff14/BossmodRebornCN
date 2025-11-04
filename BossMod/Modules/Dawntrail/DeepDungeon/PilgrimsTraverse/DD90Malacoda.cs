namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD90Malacoda;

public enum OID : uint
{
    Malacoda = 0x48F5, // R4.5
    ArcaneCylinder1 = 0x48F7, // R1.0
    ArcaneCylinder2 = 0x48F6, // R4.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45130, // Malacoda->player, no cast, single-target
    Teleport = 44256, // ArcaneCylinder2->location, no cast, single-target

    Backhand1 = 44250, // Malacoda->self, 5.0s cast, range 30 270-degree cone
    Backhand2 = 44251, // Malacoda->self, 5.0s cast, range 30 270-degree cone
    TwinWingedTreachery = 44259, // Malacoda->self, 7.0+0,6s cast, single-target
    DevilsQuarter = 44262, // Helper->self, 7.6s cast, range 35 90-degree cone
    ArcaneBeacon1 = 44257, // ArcaneCylinder2->self, 7.6s cast, range 50 width 10 rect
    ArcaneBeacon2 = 43796, // ArcaneCylinder2->self, 5.6s cast, range 50 width 10 rect

    ForeHindFolly = 44258, // Malacoda->self, 7.0+0,6s cast, single-target
    HotIronVisual = 44267, // Malacoda->self, 3.0s cast, single-target
    HotIron = 44268, // Helper->location, 3.0s cast, range 6 circle
    Skinflayer = 44266 // Malacoda->self, 5.0s cast, range 50 width 50 rect, knockback 30, dir forward
}

[SkipLocalsInit]
sealed class HotIron(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HotIron, 6f);
[SkipLocalsInit]
sealed class Backhand(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Backhand1, (uint)AID.Backhand2], new AOEShapeCone(30f, 135f.Degrees()));
[SkipLocalsInit]
sealed class DevilsQuarter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DevilsQuarter, new AOEShapeCone(35f, 45f.Degrees()));
[SkipLocalsInit]
sealed class ArcaneBeacon(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ArcaneBeacon1, (uint)AID.ArcaneBeacon2], new AOEShapeRect(50f, 5f));

[SkipLocalsInit]
sealed class Skinflayer(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Skinflayer, 30f, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }
        ref readonly var c = ref Casters.Ref(0);
        var act = c.Activation;
        if (!IsImmune(slot, act))
        {
            // square intentionally slightly smaller to prevent sus knockback
            hints.AddForbiddenZone(new SDKnockbackInSquareFixedDirection(Arena.Center, 30f * c.Direction.ToDirection(), 19f, 45f.Degrees()), act);
        }
    }
}

[SkipLocalsInit]
sealed class DD90MalacodaStates : StateMachineBuilder
{
    public DD90MalacodaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotIron>()
            .ActivateOnEnter<Backhand>()
            .ActivateOnEnter<DevilsQuarter>()
            .ActivateOnEnter<ArcaneBeacon>()
            .ActivateOnEnter<Skinflayer>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(DD90MalacodaStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.Malacoda,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1040u,
NameID = 14090u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DD90Malacoda(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsSquare(20f, 45f.Degrees()));
