namespace BossMod.Shadowbringers.Alliance.A30Trash;

public enum OID : uint
{
    Boss = 0x3193, // R0.512
    TwoPOperatedFlightUnit = 0x3194, // R2.8 (2P operated flight unit)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 23549, // TwoPOperatedFlightUnit->player, no cast, single-target
    Teleport = 23548, // Boss->location, no cast, single-target

    BladeFlurry1 = 23543, // TwoPOperatedFlightUnit->player, no cast, single-target
    BladeFlurry2 = 23544, // TwoPOperatedFlightUnit->player, no cast, single-target

    LightfastBlade = 23550, // TwoPOperatedFlightUnit->self, 12.0s cast, range 48 180-degree cone
    ManeuverStandardLaser = 23551, // TwoPOperatedFlightUnit->self, 2.0s cast, range 52 width 5 rect
    WhirlingAssault = 23547, // Boss->self, 4.0s cast, range 40 width 4 rect
    BalancedEdge = 23546, // Boss->self, 4.0s cast, range 5 circle

    Defeated1 = 22826, // Boss->self, no cast, single-target
    Defeated2 = 23567 // Helper->self, no cast, range 36 circle, doesn't seem to do anything
}

sealed class LightfastBlade : Components.SimpleAOEs
{
    public LightfastBlade(BossModule module) : base(module, (uint)AID.LightfastBlade, new AOEShapeCone(48f, 90f.Degrees()), 2)
    {
        MaxDangerColor = 1;
    }
}

sealed class ManeuverStandardLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManeuverStandardLaser, new AOEShapeRect(52f, 2.5f)); // this is a baited AOE, but there doesn't seem to be a way to determine the source based on the information currently available in logs
sealed class WhirlingAssault(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhirlingAssault, new AOEShapeRect(40f, 2f));
sealed class BalancedEdge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BalancedEdge, 5f);

sealed class A30TrashStates : StateMachineBuilder
{
    public A30TrashStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightfastBlade>()
            .ActivateOnEnter<ManeuverStandardLaser>()
            .ActivateOnEnter<WhirlingAssault>()
            .ActivateOnEnter<BalancedEdge>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(A30Trash.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var e = enemies[i];
                    if (!e.IsDestroyed)
                    {
                        return false;
                    }
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9919, SortOrder = 3)]
public sealed class A30Trash(WorldState ws, Actor primary) : BossModule(ws, primary, new(755f, -749.40625f), new ArenaBoundsCircle(24.5f))
{
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.TwoPOperatedFlightUnit];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(All));
    }
}
