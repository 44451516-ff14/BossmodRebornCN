namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D150ScholaColossus;

public enum OID : uint
{
    Boss = 0x2526, //R=3.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    HomingLaserVisual = 14467, // Boss->self, 4.0s cast, single-target
    HomingLaser = 14468, // Helper->player, 5.0s cast, range 6 circle, spread
    GrandStrike = 14965, // Boss->self, 2.5s cast, range 45+R width 4 rect
}

class HomingLaser(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HomingLaser, 6);
class GrandStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandStrike, new AOEShapeRect(48.2f, 2));

class D0150ScholaColossusStates : StateMachineBuilder
{
    public D0150ScholaColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HomingLaser>()
            .ActivateOnEnter<GrandStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7886, SortOrder = 3)]
public class D0150ScholaColossus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(299.3f, -128.94f), new(299.87f, -128.88f), new(300.44f, -128.78f), new(300.99f, -128.64f), new(301.52f, -128.43f),
    new(302.04f, -128.2f), new(302.54f, -127.93f), new(303.02f, -127.62f), new(303.48f, -127.29f), new(306.03f, -125.07f),
    new(306.47f, -124.72f), new(306.93f, -124.42f), new(307.52f, -124.21f), new(308.29f, -123.77f), new(308.82f, -123.58f),
    new(309.39f, -123.31f), new(312.05f, -123.32f), new(312.45f, -123.64f), new(312.94f, -123.52f), new(313.65f, -122.6f),
    new(314.03f, -122.25f), new(314.55f, -121.96f), new(315.02f, -121.62f), new(316.34f, -120.5f), new(316.75f, -120.11f),
    new(317.15f, -119.69f), new(317.51f, -119.21f), new(317.83f, -118.72f), new(318.74f, -117.17f), new(318.98f, -116.61f),
    new(319.08f, -116), new(319.08f, -115.41f), new(318.96f, -114.23f), new(318.88f, -113.65f), new(318.73f, -113.07f),
    new(318.49f, -112.55f), new(318.2f, -112.02f), new(317.9f, -111.54f), new(316.86f, -110.18f), new(316.6f, -109.23f),
    new(317.09f, -108.75f), new(316.53f, -108.65f), new(316.4f, -108.14f), new(316.52f, -105.92f), new(316.48f, -105.32f),
    new(316.19f, -103.58f), new(315.15f, -99.54f), new(314.96f, -98.97f), new(314.71f, -98.42f), new(314.4f, -97.91f),
    new(314.05f, -97.43f), new(313.64f, -97.01f), new(313.2f, -96.63f), new(312.75f, -96.29f), new(311.81f, -95.64f),
    new(311.31f, -95.35f), new(310.71f, -95.23f), new(310.24f, -95.58f), new(309.81f, -95.97f), new(309.32f, -96.26f),
    new(308.82f, -96.07f), new(307.72f, -95.55f), new(307.13f, -95.31f), new(306.61f, -95.35f), new(306.07f, -95.31f),
    new(305.43f, -95.22f), new(305.38f, -95.73f), new(305.23f, -96.26f), new(304.82f, -96.54f), new(299.87f, -95.09f),
    new(299.37f, -94.85f), new(295.2f, -91.86f), new(294.68f, -91.58f), new(294.1f, -91.42f), new(293.51f, -91.39f),
    new(292.92f, -91.49f), new(279.9f, -95.31f), new(279.4f, -95.49f), new(276.66f, -96.89f), new(276.21f, -97.16f),
    new(275.78f, -97.55f), new(274.44f, -99.1f), new(274.14f, -99.54f), new(273.05f, -101.92f), new(272.9f, -102.49f),
    new(272.88f, -103.02f), new(273.76f, -109.5f), new(276.52f, -119.01f), new(276.7f, -119.49f), new(276.94f, -119.94f),
    new(277.31f, -120.43f), new(280.05f, -123.51f), new(280.49f, -123.91f), new(282.29f, -125.31f), new(282.78f, -125.64f),
    new(283.28f, -125.94f), new(283.79f, -126.2f), new(284.32f, -126.44f), new(284.86f, -126.65f), new(285.41f, -126.82f),
    new(286.47f, -127.13f), new(293.71f, -128.74f), new(294.29f, -128.82f), new(294.86f, -128.87f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
}
