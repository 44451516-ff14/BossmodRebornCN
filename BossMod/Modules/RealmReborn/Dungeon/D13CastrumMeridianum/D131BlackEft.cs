namespace BossMod.RealmReborn.Dungeon.D13CastrumMeridianum.D131BlackEft;

public enum OID : uint
{
    Boss = 0x38CA, // R3.0
    EigthCohortSignifer = 0x38CC, // R0.5, x2, and more spawn during fight
    EigthCohortLaquearius = 0x38CB, // R0.5, x2, and more spawn during fight
    MagitekColossus = 0x394C, // R2.5, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // EigthCohortLaquearius/MagitekColossus->player, no cast, single-target

    Fire = 966, // EigthCohortSignifer->player, 1.0s cast, single-target
    Fortis = 28777, // EigthCohortLaquearius/EigthCohortSignifer->self, 6.0s cast, single-target
    PhotonStream = 28776, // Boss->player, no cast, single-target, auto-attack
    IncendiarySupportVisual = 29268, // Boss->self, 3.0s cast, single-target, visual
    IncendiarySupport = 29269, // Helper->self, no cast, range 60 circle, raidwide
    HighPoweredMagitekRay = 28773, // Boss->self, 5.0s cast, range 60 width 4 rect aoe
    RequestAssistance = 28774, // Boss->self, 4.0s cast, single-target, summon adds
    MagitekCannon = 28775 // Boss->location, 3.0s cast, range 6 circle aoe
}

class IncendiarySupport(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.IncendiarySupportVisual), ActionID.MakeSpell(AID.IncendiarySupport), 1, "Raidwide x3");
class HighPoweredMagitekRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HighPoweredMagitekRay), new AOEShapeRect(50, 2));
class MagitekCannon(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekCannon), 6);

class D131BlackEftStates : StateMachineBuilder
{
    public D131BlackEftStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiarySupport>()
            .ActivateOnEnter<HighPoweredMagitekRay>()
            .ActivateOnEnter<MagitekCannon>();
    }
}

// adds:
// initial = 2x signifier + 2x laquearius
// first wave = 3x signifier + 3x laquearius
// second wave = 2x colossus
// third wave = 2x colossus + 2x signifier + 2x laquearius
[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 15, NameID = 557)]
public class D131BlackEft(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(12.14f, -73.48f), new(12.44f, -73), new(12.44f, -71.9f), new(12.74f, -71.3f), new(13.13f, -70.8f),
    new(14.31f, -70.32f), new(14.92f, -70.35f), new(15.56f, -70.61f), new(16.13f, -70.92f), new(16.46f, -71.45f),
    new(16.68f, -71.98f), new(16.68f, -72.5f), new(17.4f, -72.64f), new(18.91f, -72.52f), new(19.53f, -72.62f),
    new(21.04f, -72.53f), new(21.62f, -72.63f), new(23.79f, -72.52f), new(24.31f, -72.74f), new(25.82f, -72.9f),
    new(26.5f, -72.76f), new(26.9f, -71.49f), new(27.15f, -71.02f), new(30.92f, -68.56f), new(32.19f, -67.16f),
    new(32.23f, -66.49f), new(32.15f, -65.2f), new(31.38f, -61.39f), new(31.59f, -51.28f), new(31.72f, -50.07f),
    new(32.17f, -49.85f), new(32.84f, -49.87f), new(32.96f, -49.35f), new(33.1f, -43.09f), new(32.8f, -42.43f),
    new(32.34f, -41.87f), new(32.26f, -41.19f), new(32.39f, -30.8f), new(32.34f, -30.14f), new(32.44f, -29.65f),
    new(32.82f, -29.25f), new(34.14f, -29.04f), new(34.56f, -28.75f), new(34.51f, -26.78f), new(34.67f, -26.17f),
    new(34.91f, -25.53f), new(35.3f, -25.1f), new(36.22f, -24.24f), new(36.7f, -23.91f), new(37.36f, -23.78f),
    new(37.93f, -23.44f), new(39.51f, -19.31f), new(38.94f, -19.25f), new(38.45f, -18.85f), new(38.13f, -18.26f),
    new(38.9f, -15.76f), new(39.17f, -15.22f), new(41.47f, -13.7f), new(39.97f, -10.11f), new(39.6f, -9.75f),
    new(38.28f, -9.88f), new(37, -9.59f), new(36.52f, -9.32f), new(35.07f, -7.95f), new(34.68f, -7.49f),
    new(34.85f, -5.75f), new(34.3f, -5.68f), new(33.65f, -5.66f), new(33.05f, -5.48f), new(32.34f, -5.33f),
    new(31.71f, -5.34f), new(31.07f, -5.21f), new(30.46f, -4.98f), new(29.74f, -4.98f), new(29.21f, -5.43f),
    new(28.76f, -5.9f), new(27.55f, -6.21f), new(26.91f, -6.13f), new(26.3f, -5.8f), new(25.81f, -5.45f),
    new(25.52f, -4.82f), new(25.36f, -4.21f), new(25.58f, -2.94f), new(25.56f, -2.18f), new(14.1f, -0.16f),
    new(13.75f, -0.74f), new(13.56f, -2.05f), new(13.21f, -2.58f), new(12.8f, -3.06f), new(12.15f, -3.27f),
    new(11.54f, -3.55f), new(10.92f, -3.32f), new(9.78f, -2.59f), new(9.22f, -1.36f), new(8.57f, -1.1f),
    new(7.87f, -1.1f), new(7.24f, -1.01f), new(6.62f, -0.76f), new(6.41f, -1.27f), new(6.58f, -1.92f),
    new(7.01f, -2.45f), new(7.26f, -3.78f), new(6.97f, -4.3f), new(4.51f, -4.9f), new(3.83f, -5.02f),
    new(3.41f, -4.73f), new(2.79f, -3.67f), new(2.47f, -3.23f), new(0.05f, -6.12f), new(-10.46f, -65.92f),
    new(-10.32f, -66.6f), new(-9.43f, -69.1f), new(-9.18f, -69.62f), new(-7.8f, -68.49f), new(-7.31f, -68.23f),
    new(-6.85f, -68.53f), new(-4.91f, -70.35f), new(-4.89f, -70.86f), new(-5.72f, -71.82f), new(-5.86f, -72.3f),
    new(-5.22f, -72.55f), new(-4.3f, -72.65f), new(-3.72f, -72.47f), new(-3.82f, -71.92f), new(-3.07f, -70.9f),
    new(-1.82f, -70.34f), new(-1.22f, -70.34f), new(0.02f, -70.85f), new(0.34f, -71.37f), new(0.61f, -71.94f),
    new(0.61f, -73.48f), new(12.14f, -73.48f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.EigthCohortLaquearius).Concat([PrimaryActor]).Concat(Enemies(OID.EigthCohortSignifer)).Concat(Enemies(OID.MagitekColossus)));
    }
}
