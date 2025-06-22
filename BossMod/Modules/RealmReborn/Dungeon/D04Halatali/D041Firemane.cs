namespace BossMod.RealmReborn.Dungeon.D04Halatali.D041Firemane;

public enum OID : uint
{
    Boss = 0x4643,
    Helper = 0x233C
}

public enum AID : uint
{
    Fire = 40055, // Boss->player, no cast, single-target
    FireII = 40592, // Boss->location, 4.0s cast, range 5 circle

    FireflowVisual = 40587, // Boss->self, 6.0s cast, single-target
    Fireflow1 = 40588, // Helper->self, 6.0s cast, range 60 45-degree cone
    Fireflow2 = 40589, // Helper->self, 9.0s cast, range 60 45-degree cone

    BurningBoltVisual = 40590, // Boss->self, 5.0s cast, single-target
    BurningBolt = 40591 // Helper->player, 5.0s cast, single-target
}

class Fireflow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Fireflow1, (uint)AID.Fireflow2], new AOEShapeCone(60f, 22.5f.Degrees()), 4, 8);
class BurningBolt(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BurningBolt);
class FireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireII, 5f);

class D041FiremaneStates : StateMachineBuilder
{
    public D041FiremaneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fireflow>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<BurningBolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7, NameID = 1194)]
public class D041Firemane(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(18.77f, 105.52f), new(20.74f, 105.75f), new(21.4f, 106.03f), new(31.94f, 110.21f), new(32.51f, 110.54f),
    new(34.44f, 112.19f), new(35f, 112.58f), new(42.3f, 119.87f), new(44.11f, 121.45f), new(47.1f, 124.46f),
    new(47.65f, 124.87f), new(48.29f, 125.2f), new(51.14f, 127.53f), new(51.48f, 127.91f), new(52.34f, 129.13f),
    new(52.7f, 129.77f), new(52.99f, 130.44f), new(55.04f, 140.77f), new(55, 141.44f), new(54.86f, 142.09f),
    new(54.61f, 142.71f), new(50.46f, 148.48f), new(49.99f, 148.92f), new(40.54f, 154.16f), new(33.76f, 155.83f),
    new(33.12f, 155.85f), new(32.49f, 155.8f), new(31.87f, 155.66f), new(31.28f, 155.44f), new(30.72f, 155.14f),
    new(29.71f, 154.44f), new(29.17f, 154.14f), new(28.6f, 153.92f), new(28.04f, 154.05f), new(27.42f, 154.27f),
    new(23.59f, 154.03f), new(22.96f, 153.91f), new(20.04f, 152.66f), new(19.51f, 152.3f), new(14.21f, 147.06f),
    new(13.33f, 147.02f), new(12.73f, 147.36f), new(8.31f, 146.53f), new(7.69f, 146.35f), new(7.1f, 146.06f),
    new(6.62f, 145.62f), new(6.33f, 145.04f), new(6.23f, 144.41f), new(6.12f, 142.58f), new(6.01f, 141.98f),
    new(5.29f, 139.55f), new(4.6f, 122.6f), new(4.46f, 121.96f), new(4.46f, 121.32f), new(4.51f, 120.69f),
    new(4.38f, 118.63f), new(4.24f, 117.97f), new(8.04f, 108.8f), new(8.48f, 108.32f), new(8.99f, 107.92f),
    new(11.32f, 106.77f), new(11.95f, 106.56f), new(18.09f, 105.52f), new(18.77f, 105.52f)];
    public static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
}
