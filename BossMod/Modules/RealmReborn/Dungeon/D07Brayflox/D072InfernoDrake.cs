namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D072InfernoDrake;

public enum OID : uint
{
    // Boss
    Boss = 0x3DE, // Inferno Drake

    // Trash
    TemplestBiast = 0x3DF // Templest Biast
}

public enum AID : uint
{
    // Boss
    AutoAttack = 870, // Boss->player, no cast
    BurningCyclone = 984, // Boss->self, 0.5s, range 9.6 ?-degree cone cleave (120-degree)

    // Trash
    AutoAttackTrash = 871, // Trash->player, no cast
    Levinshower = 985, // Trash->self, 0.5s cast, range 8.2 ?-degree cone cleave
    Levinfang = 519 // Trash->player, no cast, single target
}

class BurningCyclone(BossModule module) : Components.Cleave(module, (uint)AID.BurningCyclone, new AOEShapeCone(8.6f, 60.Degrees()));

class D072InfernoDrakeStates : StateMachineBuilder
{
    public D072InfernoDrakeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningCyclone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1284)]
public class D072InfernoDrake(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] shape = [new(-6.17f, -107.64f), new(2.20f, -106.20f), new(2.74f, -105.76f), new(4.22f, -101.99f), new(4.58f, -101.54f),
    new(8.85f, -99.59f), new(13.11f, -91.34f), new(13.28f, -90.67f), new(13.67f, -87.29f), new(12.07f, -83.69f),
    new(12.09f, -83.11f), new(12.76f, -80.49f), new(12.81f, -79.98f), new(11.50f, -76.59f), new(8.40f, -73.40f),
    new(8.20f, -72.89f), new(8.82f, -71.78f), new(8.58f, -71.14f), new(2.24f, -64.99f), new(0.60f, -65.77f),
    new(-0.01f, -65.96f), new(-4.44f, -64.56f), new(-4.95f, -64.51f), new(-5.90f, -64.96f), new(-6.51f, -64.92f),
    new(-7.16f, -64.79f), new(-8.08f, -65.63f), new(-8.59f, -65.87f), new(-9.20f, -65.73f), new(-9.76f, -64.49f),
    new(-12.25f, -65.82f), new(-12.66f, -66.13f), new(-13.10f, -66.61f), new(-13.69f, -66.98f), new(-14.13f, -67.56f),
    new(-14.42f, -68.23f), new(-14.88f, -68.44f), new(-15.56f, -68.30f), new(-16.21f, -69.88f), new(-16.77f, -70.15f),
    new(-17.42f, -70.29f), new(-17.90f, -70.49f), new(-19.24f, -72.09f), new(-19.58f, -72.69f), new(-19.50f, -73.39f),
    new(-19.06f, -75.30f), new(-18.80f, -75.75f), new(-18.61f, -76.41f), new(-18.64f, -77.01f), new(-18.97f, -77.45f),
    new(-19.61f, -77.61f), new(-20.27f, -77.56f), new(-20.48f, -77.03f), new(-20.94f, -76.75f), new(-21.61f, -76.65f),
    new(-22.24f, -76.74f), new(-22.89f, -76.64f), new(-23.38f, -76.79f), new(-23.47f, -77.30f), new(-23.60f, -79.38f),
    new(-23.47f, -80.11f), new(-22.41f, -80.29f), new(-22.20f, -80.95f), new(-22.36f, -82.28f), new(-22.20f, -82.98f),
    new(-21.83f, -83.54f), new(-21.71f, -84.22f), new(-21.94f, -84.80f), new(-22.46f, -85.27f), new(-22.97f, -85.63f),
    new(-23.43f, -85.39f), new(-23.93f, -85.61f), new(-24.47f, -85.95f), new(-27.97f, -90.81f), new(-27.24f, -90.72f),
    new(-26.71f, -91.02f), new(-26.04f, -91.11f), new(-24.85f, -90.98f), new(-24.10f, -91.13f), new(-23.53f, -91.36f),
    new(-23.15f, -91.89f), new(-22.64f, -92.25f), new(-22.33f, -92.77f), new(-21.26f, -95.71f), new(-21.37f, -96.29f),
    new(-21.91f, -96.60f), new(-22.57f, -96.82f), new(-23.21f, -96.77f), new(-23.85f, -97.14f), new(-23.96f, -97.66f),
    new(-24.31f, -98.15f), new(-24.14f, -98.71f), new(-23.87f, -99.38f), new(-23.48f, -100.02f), new(-22.94f, -99.98f),
    new(-22.45f, -99.75f), new(-22.02f, -99.44f), new(-20.54f, -98.01f), new(-20.13f, -97.70f), new(-19.53f, -97.50f),
    new(-18.96f, -97.84f), new(-18.43f, -97.74f), new(-17.84f, -97.96f), new(-17.27f, -98.00f), new(-16.85f, -98.28f),
    new(-14.68f, -100.60f), new(-14.41f, -101.18f), new(-14.49f, -101.80f), new(-14.29f, -102.44f), new(-14.28f, -103.07f),
    new(-14.60f, -103.63f), new(-14.71f, -104.28f), new(-14.37f, -104.64f), new(-6.83f, -107.50f), new(-6.17f, -107.64f)];
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
                (uint)OID.TemplestBiast => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.TemplestBiast));
    }
}
