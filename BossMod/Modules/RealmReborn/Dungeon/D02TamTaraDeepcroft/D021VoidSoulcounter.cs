namespace BossMod.RealmReborn.Dungeon.D02TamTaraDeepcroft.D021VoidSoulcounter;

public enum OID : uint
{
    Boss = 0x237 //  // R1.0
}

public enum AID : uint
{
    // Boss
    AutoAttack = 870, // Boss->player, no cast
    Enthunder = 430, // Boss->self, 1.5s cast, single target
    DarkOrbs = 911, // Boss->player, no cast, single target
    Condemnation = 912 // Boss->self, 2.5s cast, range 6+R (7) 90-degree cone aoe
}

class Condemnation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Condemnation, new AOEShapeCone(7f, 45f.Degrees()));

class D021VoidSoulcounterStates : StateMachineBuilder
{
    public D021VoidSoulcounterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Condemnation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 2, NameID = 455)]
public class D021VoidSoulcounter(WorldState ws, Actor primary) : BossModule(ws, primary, IsFirstArena(primary) ? FirstArena.Center : (IsSecondArena(primary) ? SecondArena.Center : ThirdArena.Center), IsFirstArena(primary) ? FirstArena : (IsSecondArena(primary) ? SecondArena : ThirdArena))
{
    private static bool IsFirstArena(Actor primary) => primary.PosRot.X > -10f;
    private static bool IsSecondArena(Actor primary) => primary.PosRot.X is > -25f and < -15f;

    // First encounter
    private static readonly WPos[] first = [new(7.92f, -25.67f), new(9.92f, -25.80f), new(11.36f, -24.41f), new(11.95f, -24.43f), new(14.57f, -24.76f),
    new(16.98f, -25.39f), new(16.99f, -22.26f), new(17.10f, -20.96f), new(17.37f, -20.48f), new(17.93f, -20.47f),
    new(18.44f, -20.63f), new(18.84f, -18.15f), new(18.26f, -17.89f), new(17.82f, -17.60f), new(17.86f, -16.99f),
    new(18.05f, -16.36f), new(18.31f, -15.73f), new(18.61f, -15.15f), new(18.77f, -13.85f), new(19.33f, -12.64f),
    new(19.07f, -11.98f), new(18.75f, -11.42f), new(18.58f, -10.79f), new(18.33f, -10.35f), new(17.98f, -10.86f),
    new(17.53f, -11.25f), new(16.92f, -11.39f), new(16.32f, -11.70f), new(15.82f, -11.41f), new(15.65f, -10.79f),
    new(15.61f, -8.88f), new(15.44f, -8.40f), new(6.94f, -8.40f), new(5.98f, -9.34f), new(5.64f, -9.93f),
    new(5.24f, -10.37f), new(4.67f, -10.58f), new(4.11f, -10.35f), new(3.72f, -9.95f), new(3.65f, -9.36f),
    new(3.78f, -8.85f), new(-10.23f, -8.87f), new(-12.48f, -10.37f), new(-14.22f, -12.77f), new(-14.99f, -15.34f),
    new(-15.02f, -17.36f), new(-14.95f, -18.06f), new(-14.37f, -20.09f), new(-14.04f, -20.66f), new(-12.72f, -22.42f),
    new(-12.18f, -22.86f), new(-10.51f, -23.97f), new(-9.89f, -24.23f), new(3.14f, -24.30f), new(3.63f, -23.85f),
    new(3.78f, -23.25f), new(4.20f, -22.82f), new(4.71f, -22.62f), new(5.30f, -22.88f), new(5.68f, -23.31f),
    new(5.73f, -23.95f), new(6.21f, -24.17f), new(6.84f, -24.22f), new(7.03f, -24.71f), new(6.90f, -25.35f),
    new(7.04f, -25.87f)];

    // Second encounter
    private static readonly WPos[] second = [new(-22.10f, 13.07f), new(-20.07f, 13.40f), new(-19.40f, 13.60f), new(-18.17f, 14.18f), new(-17.69f, 14.68f),
    new(-10.10f, 25.48f), new(-10.40f, 25.89f), new(-10.86f, 26.29f), new(-11.03f, 26.85f), new(-10.85f, 27.46f),
    new(-10.36f, 27.82f), new(-9.73f, 27.89f), new(-9.22f, 27.59f), new(-8.68f, 27.37f), new(-7.52f, 28.04f),
    new(-6.95f, 27.90f), new(-6.36f, 27.62f), new(-3.91f, 25.49f), new(-3.23f, 25.27f), new(-2.71f, 25.75f),
    new(-2.23f, 26.28f), new(-1.50f, 27.32f), new(-1.02f, 27.76f), new(-0.47f, 28.16f), new(0.33f, 28.28f),
    new(0.75f, 28.55f), new(0.53f, 29.21f), new(-0.24f, 30.26f), new(-0.21f, 30.78f), new(0.24f, 31.24f),
    new(0.39f, 31.74f), new(-1.14f, 32.71f), new(-1.62f, 33.08f), new(-2.85f, 34.53f), new(-3.66f, 37.18f),
    new(-3.66f, 37.74f), new(-3.49f, 38.38f), new(-3.72f, 38.85f), new(-4.06f, 39.25f), new(-4.24f, 39.81f),
    new(-4.04f, 40.44f), new(-5.33f, 41.47f), new(-5.82f, 41.07f), new(-6.36f, 41.28f), new(-6.97f, 41.58f),
    new(-7.59f, 41.29f), new(-9.17f, 40.29f), new(-9.79f, 40.09f), new(-10.42f, 39.95f), new(-11.72f, 40.17f),
    new(-13.57f, 40.76f), new(-16.12f, 42.93f), new(-16.59f, 43.12f), new(-21.40f, 36.37f), new(-20.90f, 36.16f),
    new(-20.49f, 35.74f), new(-20.38f, 35.18f), new(-20.54f, 34.61f), new(-21.07f, 34.27f), new(-21.67f, 34.19f),
    new(-22.20f, 34.55f), new(-22.77f, 34.31f), new(-30.55f, 23.18f), new(-30.75f, 22.54f), new(-30.87f, 21.21f),
    new(-30.50f, 19.26f), new(-30.31f, 18.64f), new(-28.87f, 16.24f), new(-26.85f, 14.46f), new(-25.01f, 13.51f),
    new(-24.30f, 13.33f), new(-22.34f, 13.05f)];

    // Third encounter
    private static readonly WPos[] third = [new(-116.51f, -13.11f), new(-116.08f, -12.64f), new(-115.75f, -12.02f), new(-115.66f, -11.33f), new(-114.97f, -7.20f),
    new(-111.75f, 2.78f), new(-110.23f, 5.94f), new(-109.72f, 6.04f), new(-109.14f, 6.46f), new(-108.97f, 7.10f),
    new(-108.76f, 7.63f), new(-108.28f, 7.93f), new(-107.67f, 8.00f), new(-107.16f, 7.74f), new(-106.85f, 7.25f),
    new(-106.87f, 6.66f), new(-107.18f, 6.17f), new(-107.86f, 5.77f), new(-98.99f, 0.67f), new(-98.47f, 0.58f),
    new(-98.21f, 1.15f), new(-97.73f, 1.32f), new(-97.22f, 1.07f), new(-96.89f, 0.59f), new(-97.24f, 0.07f),
    new(-96.98f, -0.50f), new(-95.87f, -1.12f), new(-95.19f, -1.26f), new(-94.01f, -1.15f), new(-93.34f, -0.97f),
    new(-90.98f, 0.11f), new(-88.93f, 1.62f), new(-88.43f, 1.51f), new(-87.68f, 1.12f), new(-88.24f, 2.08f),
    new(-87.91f, 2.56f), new(-87.45f, 2.84f), new(-86.87f, 2.46f), new(-84.15f, 7.53f), new(-84.34f, 8.01f),
    new(-84.74f, 8.45f), new(-84.56f, 8.93f), new(-84.13f, 9.34f), new(-83.33f, 9.13f), new(-83.75f, 9.56f),
    new(-85.61f, 10.42f), new(-85.85f, 10.91f), new(-86.05f, 11.56f), new(-85.48f, 11.44f), new(-84.18f, 10.75f),
    new(-84.16f, 12.09f), new(-84.40f, 14.74f), new(-85.20f, 16.63f), new(-85.74f, 17.12f), new(-86.85f, 17.77f),
    new(-87.53f, 17.98f), new(-87.89f, 17.62f), new(-88.38f, 17.29f), new(-88.91f, 17.41f), new(-89.32f, 17.83f),
    new(-89.26f, 18.34f), new(-88.83f, 18.91f), new(-96.88f, 23.56f), new(-97.43f, 23.66f), new(-97.47f, 23.12f),
    new(-97.76f, 22.62f), new(-98.28f, 22.35f), new(-98.84f, 22.39f), new(-99.31f, 22.70f), new(-99.52f, 23.26f),
    new(-99.45f, 23.82f), new(-99.06f, 24.25f), new(-98.41f, 24.43f), new(-98.92f, 25.44f), new(-98.93f, 26.03f),
    new(-95.20f, 30.10f), new(-93.18f, 31.76f), new(-92.24f, 32.78f), new(-92.05f, 33.44f), new(-91.61f, 33.94f),
    new(-91.11f, 34.25f), new(-90.62f, 34.80f), new(-88.43f, 37.36f), new(-86.94f, 38.58f), new(-86.62f, 39.19f),
    new(-86.87f, 39.85f), new(-87.49f, 41.04f), new(-87.52f, 41.62f), new(-87.43f, 43.51f), new(-87.51f, 44.22f),
    new(-89.34f, 47.75f), new(-89.98f, 47.52f), new(-90.44f, 47.01f), new(-90.89f, 46.42f), new(-90.47f, 45.86f),
    new(-90.17f, 45.33f), new(-90.38f, 44.87f), new(-92.05f, 43.73f), new(-92.56f, 43.61f), new(-92.95f, 44.03f),
    new(-93.36f, 44.37f), new(-93.94f, 44.01f), new(-94.44f, 43.94f), new(-94.94f, 44.47f), new(-96.94f, 42.93f),
    new(-96.81f, 42.28f), new(-96.89f, 41.77f), new(-97.94f, 40.92f), new(-98.32f, 40.43f), new(-101.90f, 37.47f),
    new(-102.36f, 36.95f), new(-103.85f, 34.75f), new(-104.74f, 32.02f), new(-106.24f, 29.94f), new(-109.27f, 27.42f),
    new(-111.00f, 26.45f), new(-113.65f, 24.65f), new(-114.16f, 24.20f), new(-114.52f, 23.70f), new(-114.85f, 23.10f),
    new(-114.84f, 22.44f), new(-115.06f, 21.19f), new(-115.13f, 20.54f), new(-115.00f, 18.01f), new(-114.87f, 17.37f),
    new(-114.45f, 16.14f), new(-114.43f, 14.81f), new(-114.57f, 14.25f), new(-115.59f, 11.20f), new(-117.08f, 9.16f),
    new(-117.41f, 8.59f), new(-119.62f, 3.05f), new(-119.17f, 2.57f), new(-118.66f, 2.26f), new(-118.71f, 1.72f),
    new(-119.18f, -0.19f), new(-119.63f, -0.45f), new(-120.25f, -0.30f), new(-120.77f, -0.25f), new(-121.19f, -2.12f),
    new(-121.65f, -3.40f), new(-122.24f, -6.02f), new(-122.19f, -8.02f), new(-122.08f, -8.62f), new(-122.07f, -9.30f),
    new(-122.13f, -9.96f), new(-122.55f, -10.35f), new(-123.56f, -11.09f), new(-124.11f, -11.69f), new(-122.27f, -12.03f),
    new(-120.90f, -11.97f), new(-117.18f, -13.23f), new(-116.51f, -13.11f)];

    public static readonly ArenaBoundsCustom FirstArena = new([new PolygonCustom(first)]);
    public static readonly ArenaBoundsCustom SecondArena = new([new PolygonCustom(second)]);
    public static readonly ArenaBoundsCustom ThirdArena = new([new PolygonCustom(third)]);
}
