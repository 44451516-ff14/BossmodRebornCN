namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D071GreatYellowPelican;

public enum OID : uint
{
    // Boss
    Boss = 0x1A7, // Great Yellow Pelican

    // Trash
    VioletBack = 0x1A9, // Violet Back
    SableBack = 0x1AA // Sable Back
}

public enum AID : uint
{
    // Boss
    AutoAttackBoss = 871, // Boss->player, no cast
    HammerBeak = 504, // Boss->self, no cast, single target
    NumbingBreath = 506, // Boss->self, 3.0s cast, range 9.2 120-degree cone aoe

    // Trash
    AutoAttackTrash = 871, // Trash->player, no cast
    PoisonBreath = 1393 // VioletBack->self, no cast, range 7.20 ?-degree cone cleave
}

class NumbingBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NumbingBreath, new AOEShapeCone(9.2f, 60.Degrees()));

class D071GreatYellowPelicanStates : StateMachineBuilder
{
    public D071GreatYellowPelicanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NumbingBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1280)]
public class D071GreatYellowPelican(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] shape = [new(119.50f, -32.71f), new(122.83f, -28.25f), new(123.29f, -27.95f), new(129.68f, -25.59f), new(129.94f, -24.72f),
    new(129.92f, -16.55f), new(130.21f, -16.04f), new(131.04f, -15.00f), new(131.33f, -14.39f), new(127.07f, -6.29f),
    new(126.01f, -3.21f), new(123.63f, 0.03f), new(123.12f, 0.46f), new(119.16f, 3.01f), new(118.72f, 3.44f),
    new(118.17f, 3.84f), new(102.90f, 0.48f), new(102.31f, 0.10f), new(98.46f, -3.51f), new(97.18f, -5.97f),
    new(96.96f, -6.66f), new(96.45f, -9.45f), new(96.59f, -16.66f), new(99.44f, -24.39f), new(103.96f, -29.17f),
    new(105.59f, -30.38f), new(106.91f, -30.81f), new(107.41f, -30.91f), new(107.78f, -30.39f), new(108.31f, -30.11f),
    new(108.92f, -29.99f), new(109.52f, -30.21f), new(109.90f, -30.73f), new(109.97f, -31.30f), new(111.54f, -31.76f),
    new(112.08f, -32.10f), new(113.35f, -32.38f), new(114.56f, -32.47f), new(115.23f, -32.21f), new(117.03f, -30.08f),
    new(117.73f, -29.32f), new(118.15f, -29.01f), new(118.73f, -29.37f), new(119.11f, -29.88f), new(119.20f, -30.40f),
    new(118.77f, -30.84f), new(116.98f, -32.21f), new(117.49f, -32.48f), new(118.81f, -32.75f), new(119.50f, -32.71f)];
    // Centroid of the polygon is at: (113.403f, -14.391f)

    public static readonly ArenaBoundsComplex arena = new([new PolygonCustom(shape)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.VioletBack => 3,
                (uint)OID.SableBack => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.VioletBack));
    }
}
