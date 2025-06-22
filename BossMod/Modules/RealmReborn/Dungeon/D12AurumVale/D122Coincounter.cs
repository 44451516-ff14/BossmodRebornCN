namespace BossMod.RealmReborn.Dungeon.D12AurumVale.D122Coincounter;

public enum OID : uint
{
    Boss = 0x5BE // x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    TenTonzeSwipe = 1034, // Boss->self, no cast, range 6+R ?-degree cone cleave
    HundredTonzeSwipe = 1035, // Boss->self, 3.0s cast, range 6+R 120-degree cone aoe
    HundredTonzeSwing = 628, // Boss->self, 4.0s cast, range 8+R circle aoe
    Glower = 629, // Boss->self, 3.0s cast, range 17+R width 7 rect aoe
    AnimalInstinct = 630, // Boss->self, no cast, single-target
    EyeOfTheBeholder = 631 // Boss->self, 2.5s cast, range 8-15+R donut 270-degree cone aoe
}

class TenTonzeSwipe(BossModule module) : Components.Cleave(module, (uint)AID.TenTonzeSwipe, new AOEShapeCone(10, 60.Degrees())); // TODO: verify angle
class HundredTonzeSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HundredTonzeSwipe, new AOEShapeCone(10, 60.Degrees()));
class HundredTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HundredTonzeSwing, 12);
class Glower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Glower, new AOEShapeRect(21, 3.5f));
class EyeOfTheBeholder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EyeOfTheBeholder, new AOEShapeDonutSector(8, 19, 135.Degrees()));

class D122CoincounterStates : StateMachineBuilder
{
    public D122CoincounterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TenTonzeSwipe>()
            .ActivateOnEnter<HundredTonzeSwipe>()
            .ActivateOnEnter<HundredTonzeSwing>()
            .ActivateOnEnter<Glower>()
            .ActivateOnEnter<EyeOfTheBeholder>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 5, NameID = 1533)]
public class D122Coincounter(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-139.18f, -177.68f), new(-138.68f, -177.28f), new(-138.39f, -176.65f), new(-138.6f, -176.02f), new(-138.38f, -175.49f),
    new(-138.46f, -174.96f), new(-139.64f, -175.02f), new(-140.22f, -174.91f), new(-140.77f, -174.53f), new(-141.72f, -173.62f),
    new(-141.96f, -173.12f), new(-141.49f, -171.96f), new(-140.81f, -170.8f), new(-140.64f, -169.4f), new(-140.46f, -168.69f),
    new(-140.11f, -168.23f), new(-139.51f, -168.35f), new(-139.01f, -168.66f), new(-137.99f, -169.54f), new(-137.59f, -169.94f),
    new(-136.93f, -171.75f), new(-136.6f, -172.3f), new(-136.14f, -172.76f), new(-135.59f, -173f), new(-135.07f, -173.32f),
    new(-134.91f, -173.88f), new(-134.93f, -174.54f), new(-134.53f, -175.08f), new(-133.99f, -175.49f), new(-133.34f, -175.58f),
    new(-132.7f, -175.31f), new(-132.26f, -174.76f), new(-132.05f, -173.6f), new(-131.81f, -173.07f), new(-130.27f, -171.13f),
    new(-129.68f, -170.96f), new(-129.08f, -170.64f), new(-128.73f, -170.1f), new(-128.18f, -169.89f), new(-127.54f, -169.59f),
    new(-126.62f, -168.58f), new(-124.77f, -168.17f), new(-124.18f, -168.12f), new(-122.91f, -168.26f), new(-122.32f, -167.93f),
    new(-121.94f, -167.36f), new(-121.39f, -166.94f), new(-121.01f, -166.56f), new(-122.2f, -164.92f), new(-122.37f, -164.35f),
    new(-122.46f, -163.68f), new(-122.75f, -163.09f), new(-125.1f, -162.06f), new(-125.62f, -161.73f), new(-126.6f, -160.85f),
    new(-126.95f, -160.34f), new(-127.56f, -159.24f), new(-127.39f, -158f), new(-127.37f, -157.34f), new(-127.74f, -156.73f),
    new(-128.03f, -156.11f), new(-128.25f, -155.46f), new(-128.26f, -154.89f), new(-127.64f, -153.68f), new(-127.05f, -153.66f),
    new(-126.58f, -153.48f), new(-126.68f, -152.32f), new(-126.49f, -151.72f), new(-126.25f, -151.17f), new(-125.74f, -150.84f),
    new(-125.54f, -150.19f), new(-125.87f, -148.91f), new(-125.61f, -147.57f), new(-126.22f, -145.87f), new(-126.35f, -145.18f),
    new(-126.35f, -144.57f), new(-126.25f, -143.91f), new(-126.01f, -143.34f), new(-125.07f, -141.57f), new(-124.9f, -140.86f),
    new(-125.15f, -139.61f), new(-124.85f, -139.2f), new(-123.91f, -138.33f), new(-123.81f, -130.92f), new(-124.27f, -130.37f),
    new(-124.78f, -129.96f), new(-125.46f, -129.65f), new(-126.1f, -129.47f), new(-127.16f, -128.7f), new(-127.61f, -128.27f),
    new(-128.26f, -127.08f), new(-128.59f, -125.75f), new(-128.6f, -125.19f), new(-128.24f, -123.86f), new(-128.75f, -123.39f),
    new(-130.05f, -123.03f), new(-130.56f, -122.68f), new(-130.84f, -122.13f), new(-130.7f, -120.76f), new(-131.26f, -120.77f),
    new(-131.9f, -120.62f), new(-132.58f, -120.56f), new(-133.03f, -120.14f), new(-133.47f, -118.89f), new(-133.95f, -118.7f),
    new(-134.58f, -118.61f), new(-136.27f, -117.8f), new(-137.69f, -117.53f), new(-137.55f, -117f), new(-136.43f, -114.6f),
    new(-135.55f, -112.07f), new(-135.21f, -111.59f), new(-134.13f, -110.91f), new(-133.83f, -110.47f), new(-134.64f, -109.85f),
    new(-136.31f, -110.7f), new(-136.95f, -110.92f), new(-137.62f, -111.04f), new(-138.21f, -111.02f), new(-140.03f, -109.16f),
    new(-141.27f, -108.65f), new(-142.68f, -107.24f), new(-143.36f, -107.06f), new(-144.01f, -106.99f), new(-145.18f, -106.6f),
    new(-145.94f, -105.62f), new(-147.26f, -105.32f), new(-147.85f, -105f), new(-148.53f, -104.88f), new(-149.47f, -105.76f),
    new(-150f, -106.08f), new(-150.54f, -106.26f), new(-151.14f, -106.08f), new(-151.8f, -106f), new(-152.38f, -106.27f),
    new(-153.59f, -106.99f), new(-154.19f, -107.16f), new(-154.86f, -107.11f), new(-155.4f, -106.91f), new(-156.05f, -106.6f),
    new(-156.64f, -107.03f), new(-156.81f, -107.69f), new(-156.37f, -108.19f), new(-155.78f, -108.47f), new(-154.5f, -108.73f),
    new(-153.93f, -109f), new(-153.44f, -109.39f), new(-153.02f, -109.87f), new(-152.02f, -111.49f), new(-151.9f, -112.05f),
    new(-153.87f, -114.71f), new(-154.3f, -115.12f), new(-156.5f, -116.36f), new(-159f, -116.17f), new(-159.56f, -115.96f),
    new(-160.23f, -116.16f), new(-161.4f, -116.91f), new(-161.98f, -117.14f), new(-162.55f, -116.98f), new(-164.3f, -116.29f),
    new(-165.82f, -115.13f), new(-166.13f, -114.69f), new(-166.2f, -113.45f), new(-166.46f, -112.84f), new(-167.09f, -113.14f),
    new(-167.56f, -113.6f), new(-168.09f, -113.87f), new(-168.77f, -113.93f), new(-169.33f, -114.26f), new(-169.92f, -114.49f),
    new(-170.47f, -114.35f), new(-171.06f, -114.12f), new(-171.57f, -113.86f), new(-171.95f, -113.34f), new(-172.31f, -112.99f),
    new(-173.03f, -113f), new(-174.21f, -112.62f), new(-174.63f, -112.22f), new(-175.28f, -111.86f), new(-175.6f, -112.3f),
    new(-175.89f, -112.92f), new(-176.06f, -113.56f), new(-176.47f, -113.92f), new(-176.8f, -114.44f), new(-177.3f, -114.82f),
    new(-178.72f, -116.94f), new(-178.76f, -117.45f), new(-178.37f, -118.77f), new(-177.79f, -120.03f), new(-178.11f, -120.51f),
    new(-178.45f, -120.88f), new(-178.16f, -121.31f), new(-177.71f, -121.74f), new(-177.42f, -122.28f), new(-176.8f, -122.48f),
    new(-174.94f, -122.23f), new(-173.81f, -122.78f), new(-172.53f, -122.66f), new(-172.09f, -122.89f), new(-171.4f, -122.87f),
    new(-170.9f, -123.11f), new(-170.42f, -123.52f), new(-169.27f, -124.21f), new(-169.14f, -124.87f), new(-168.49f, -125.95f),
    new(-168.59f, -127.26f), new(-168.72f, -127.87f), new(-169.1f, -129.07f), new(-169.45f, -129.55f), new(-170.38f, -130.65f),
    new(-169.89f, -130.85f), new(-169.32f, -132.06f), new(-169.13f, -132.73f), new(-169.1f, -133.35f), new(-169.98f, -134.35f),
    new(-170.32f, -135.01f), new(-170.52f, -136.37f), new(-170.73f, -136.97f), new(-171.66f, -137.77f), new(-173.5f, -137.99f),
    new(-174.13f, -138.14f), new(-174.56f, -138.67f), new(-174.78f, -139.28f), new(-175.64f, -140.13f), new(-176.18f, -140.42f),
    new(-176.49f, -141f), new(-176.67f, -141.56f), new(-177.13f, -141.92f), new(-177.16f, -142.59f), new(-176.89f, -143.25f),
    new(-176.34f, -145.23f), new(-176.36f, -145.83f), new(-176.62f, -146.36f), new(-177.11f, -146.76f), new(-177.46f, -147.25f),
    new(-177.53f, -148.97f), new(-177.08f, -149.52f), new(-176.57f, -149.58f), new(-175.94f, -149.39f), new(-175.38f, -149.69f),
    new(-174.96f, -149.38f), new(-175f, -148.17f), new(-174.78f, -147.6f), new(-174.4f, -147.05f), new(-173.85f, -146.66f),
    new(-173.48f, -146.17f), new(-172.99f, -146.08f), new(-172.41f, -146.37f), new(-171.79f, -146.57f), new(-171.25f, -146.94f),
    new(-170.6f, -147.15f), new(-169.99f, -147.15f), new(-169.39f, -147.26f), new(-168.81f, -147.45f), new(-168.31f, -147.74f),
    new(-168.25f, -148.27f), new(-168.5f, -148.8f), new(-168.89f, -149.27f), new(-169.39f, -149.75f), new(-169.7f, -150.27f),
    new(-170.22f, -150.52f), new(-170.8f, -150.73f), new(-171.44f, -150.84f), new(-172.11f, -151.01f), new(-172.71f, -151.33f),
    new(-173.4f, -152.41f), new(-173.92f, -152.75f), new(-174.53f, -153.07f), new(-175.1f, -153.48f), new(-175.49f, -154.02f),
    new(-175.21f, -154.61f), new(-170.62f, -157.06f), new(-170.43f, -157.63f), new(-170.55f, -158.2f), new(-171.06f, -158.51f),
    new(-171.18f, -159.17f), new(-170.97f, -160.44f), new(-171.82f, -161.66f), new(-172.38f, -162.02f), new(-172.99f, -162.24f),
    new(-173.44f, -162.7f), new(-173.57f, -163.35f), new(-173.8f, -164.02f), new(-174.93f, -165.55f), new(-175.25f, -166.16f),
    new(-175.21f, -166.81f), new(-175.83f, -168f), new(-175.96f, -168.49f), new(-170.9f, -173.54f), new(-169.63f, -173.54f),
    new(-168.99f, -173.28f), new(-168.5f, -172.79f), new(-167.31f, -172.29f), new(-166.81f, -171.8f), new(-166.1f, -170.81f),
    new(-165.06f, -170.09f), new(-163.84f, -169.66f), new(-163.15f, -169.51f), new(-162.6f, -169.48f), new(-162.02f, -169.79f),
    new(-160.77f, -170.09f), new(-160.34f, -170.61f), new(-159.97f, -170.95f), new(-158.38f, -170.07f), new(-155.83f, -170.1f),
    new(-155.35f, -170.37f), new(-154.77f, -170.59f), new(-153.18f, -172.01f), new(-152.79f, -172.44f), new(-152.69f, -173.09f),
    new(-152.33f, -173.66f), new(-152.1f, -174.17f), new(-151.93f, -174.78f), new(-151.56f, -175.43f), new(-151.1f, -175.18f),
    new(-150.17f, -174.37f), new(-149.62f, -174.45f), new(-149.03f, -174.66f), new(-148.51f, -174.97f), new(-147.29f, -176.41f),
    new(-146.65f, -176.59f), new(-146.21f, -176.95f), new(-145.81f, -177.54f), new(-145.32f, -177.31f), new(-144.8f, -176.9f),
    new(-144.21f, -176.74f), new(-143.58f, -176.73f), new(-142.97f, -176.41f), new(-142.38f, -176.29f), new(-141.8f, -176.49f),
    new(-140.92f, -177.39f), new(-139.18f, -177.68f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
}
