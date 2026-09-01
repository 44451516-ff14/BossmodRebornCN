namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D060ClonedConjurer;

public enum OID : uint
{
    Boss = 0xF56, // R0.6
    ClonedThaumaturge = 0xF57 // R0.6
}

public enum AID : uint
{
    Fire = 966, // ClonedThaumaturge->player, 1.0s cast, single-target
    Aero = 969, // Boss->player, 1.0s cast, single-target
    Tornado = 900, // Boss->player, 5.0s cast, range 6 circle
    Breakga = 2340, // ClonedThaumaturge->player, 4.0s cast, range 5 circle
    Drain = 2339 // ClonedThaumaturge->player, 4.0s cast, single-target
}

sealed class Tornado(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Tornado, 6f);

sealed class D060ClonedConjurerStates : StateMachineBuilder
{
    public D060ClonedConjurerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Tornado>()
            .Raw.Update = () => AllDeadOrDestroyed(D060ClonedConjurer.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38u, NameID = 3838u, SortOrder = 8)]
public sealed class D060ClonedConjurer : BossModule
{
    public D060ClonedConjurer(WorldState ws, Actor primary) : this(ws, primary, primary.PosRot.Z > 200f ? BuildArena1() : BuildArena2()) { }

    private D060ClonedConjurer(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena1()
    {
        WPos[] vertices = [new(266.39f, 227.51f), new(274.35f, 232.12f), new(274.55f, 232.58f), new(274.59f, 237.89f), new(274.8f, 238.36f),
            new(275.3f, 238.28f), new(278.46f, 236.46f), new(278.89f, 236.76f), new(281.09f, 238.03f), new(281.5f, 241.98f),
            new(281.8f, 242.4f), new(284.99f, 244.22f), new(285.46f, 244.4f), new(288.59f, 242.6f), new(289.1f, 242.61f),
            new(291.76f, 244.15f), new(291.91f, 247.73f), new(291.99f, 248.24f), new(292.41f, 248.51f), new(294.63f, 249.8f),
            new(294.26f, 250.15f), new(285.84f, 255.3f), new(285.33f, 255.5f), new(282.58f, 253.91f), new(282.31f, 249.97f),
            new(282, 249.58f), new(278.86f, 247.78f), new(278.37f, 247.61f), new(277.92f, 247.84f), new(275.27f, 249.37f),
            new(274.76f, 249.41f), new(272.08f, 247.86f), new(271.92f, 243.95f), new(271.62f, 243.54f), new(271.12f, 243.7f),
            new(266.63f, 246.27f), new(266.12f, 246.35f), new(261.58f, 243.76f), new(261.12f, 243.54f), new(260.64f, 243.34f),
            new(258.42f, 242.05f), new(258.01f, 238.19f), new(258.08f, 237.6f), new(258.1f, 232.2f), new(266.15f, 227.56f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena2()
    {
        WPos[] vertices2 = [new(193.02f, 94.99f), new(201.06f, 99.63f), new(201.14f, 100.29f), new(201.14f, 100.95f), new(201.13f, 101.69f),
            new(201.13f, 102.4f), new(201.15f, 102.93f), new(201.16f, 109.28f), new(201.28f, 109.79f), new(205.42f, 112.15f),
            new(205.91f, 112.37f), new(209.04f, 114.17f), new(209.48f, 114.47f), new(212.14f, 116.01f), new(212.57f, 116.27f),
            new(214.82f, 117.57f), new(215.04f, 118.25f), new(215.04f, 123.26f), new(215.06f, 123.78f), new(215.61f, 123.89f),
            new(216.06f, 123.65f), new(223.69f, 119.26f), new(224.2f, 119.08f), new(232.37f, 123.79f), new(232.33f, 133.26f),
            new(225.68f, 137.1f), new(225.21f, 137.34f), new(224.32f, 137.85f), new(223.78f, 137.82f), new(215.84f, 133.24f),
            new(215.37f, 133.06f), new(205.83f, 138.55f), new(205.38f, 138.77f), new(204.96f, 139.05f), new(204.68f, 139.47f),
            new(205f, 139.86f), new(207.68f, 141.41f), new(208.11f, 141.7f), new(208.13f, 145.26f), new(207.65f, 145.48f),
            new(205.44f, 146.76f), new(204.94f, 146.89f), new(201.74f, 145.05f), new(201.25f, 145.2f), new(198.1f, 147.05f),
            new(197.77f, 150.38f), new(197.75f, 151.26f), new(194.57f, 153.13f), new(194.32f, 153.63f), new(193.95f, 153.99f),
            new(193.49f, 153.77f), new(191.37f, 151.91f), new(191.67f, 151.49f), new(191.66f, 147.7f), new(194.84f, 145.86f), new(195.11f, 145.25f),
            new(195.11f, 144.54f), new(195.13f, 143.72f), new(195.12f, 141.67f), new(198.33f, 139.82f), new(198.58f, 139.37f),
            new(198.21f, 139.01f), new(187.87f, 133.08f), new(187.38f, 133.21f), new(180.53f, 137.06f), new(180.05f, 137.29f),
            new(179.13f, 137.81f), new(178.66f, 137.59f), new(171.89f, 133.68f), new(171.47f, 133.4f), new(171.02f, 133.15f),
            new(170.89f, 125.72f), new(170.92f, 123.59f), new(173.59f, 122.04f), new(174.05f, 121.8f), new(178.93f, 118.98f),
            new(179.44f, 119.04f), new(186.82f, 123.3f), new(187.26f, 123.57f), new(187.71f, 123.78f), new(188.14f, 123.49f),
            new(188.15f, 122.7f), new(188.15f, 118.15f), new(188.24f, 117.64f), new(193.19f, 114.78f), new(193.26f, 114.27f),
            new(192.85f, 113.97f), new(192.4f, 113.71f), new(191.96f, 113.43f), new(191.5f, 113.18f), new(191.04f, 112.91f),
            new(190.6f, 112.61f), new(184.68f, 109.13f), new(184.68f, 99.94f), new(184.97f, 99.5f), new(192.11f, 95.37f),
            new(192.57f, 95.15f)];
        WPos[] vertices2h1 = [new(190.91f, 99.94f), new(191.38f, 100.19f), new(192.93f, 99.94f), new(193.44f, 100.01f), new(193.95f, 100.09f),
            new(194.45f, 100.23f), new(194.91f, 100.02f), new(195.43f, 100.12f), new(195.81f, 100.49f), new(197.26f, 101.94f),
            new(197.37f, 102.44f), new(197.1f, 102.87f), new(197.24f, 103.37f), new(197.28f, 103.86f), new(197.36f, 104.36f),
            new(197.32f, 104.88f), new(197.24f, 105.38f), new(197.1f, 105.88f), new(197.29f, 106.39f), new(197.22f, 106.91f),
            new(195.37f, 108.76f), new(194.87f, 108.89f), new(194.44f, 108.6f), new(193.94f, 108.71f), new(192.92f, 108.87f),
            new(192.42f, 108.82f), new(191.43f, 108.66f), new(190.92f, 108.8f), new(190.4f, 108.72f), new(189.99f, 108.4f),
            new(189.66f, 108.01f), new(188.55f, 106.91f), new(188.44f, 106.4f), new(188.69f, 105.93f), new(188.45f, 104.39f),
            new(188.51f, 103.87f), new(188.59f, 103.35f), new(188.75f, 102.85f), new(188.51f, 102.4f), new(188.61f, 101.88f),
            new(190.45f, 100.04f)];
        WPos[] vertices2h2 = [new(222.11f, 124.13f), new(222.54f, 124.45f), new(223.03f, 124.28f), new(223.54f, 124.25f), new(224.04f, 124.17f),
            new(224.55f, 124.2f), new(225.06f, 124.28f), new(225.57f, 124.42f), new(226.03f, 124.2f), new(226.57f, 124.3f),
            new(228.43f, 126.15f), new(228.41f, 126.69f), new(228.29f, 127.2f), new(228.34f, 127.7f), new(228.42f, 128.21f),
            new(228.45f, 128.72f), new(228.22f, 130.25f), new(228.43f, 130.72f), new(228.16f, 131.13f), new(226.7f, 132.6f),
            new(226.25f, 132.88f), new(225.79f, 132.67f), new(225.27f, 132.68f), new(224.26f, 132.84f), new(223.72f, 132.8f),
            new(222.7f, 132.64f), new(222.21f, 132.76f), new(221.7f, 132.74f), new(221.32f, 132.39f), new(219.84f, 130.92f),
            new(219.78f, 130.4f), new(220.02f, 129.95f), new(219.88f, 129.46f), new(219.8f, 128.95f), new(219.76f, 128.45f),
            new(219.92f, 127.43f), new(220.06f, 126.92f), new(219.81f, 126.46f), new(219.97f, 125.97f), new(221.43f, 124.51f),
            new(221.79f, 124.17f)];
        WPos[] vertices2h3 = [new(177.03f, 124.04f), new(177.53f, 124.26f), new(178.04f, 124.22f), new(179.07f, 124.06f), new(179.57f, 124.11f),
            new(180.1f, 124.2f), new(180.6f, 124.35f), new(181.09f, 124.13f), new(181.6f, 124.26f), new(183.43f, 126.09f),
            new(183.36f, 126.61f), new(183.24f, 127.11f), new(183.33f, 127.61f), new(183.41f, 128.13f), new(183.42f, 128.67f),
            new(183.26f, 129.7f), new(183.28f, 130.21f), new(183.38f, 130.71f), new(183.04f, 131.14f), new(181.56f, 132.63f),
            new(181.05f, 132.78f), new(180.6f, 132.55f), new(180.09f, 132.65f), new(179.07f, 132.8f), new(178.57f, 132.7f),
            new(178.06f, 132.62f), new(177.57f, 132.51f), new(177.09f, 132.7f), new(176.57f, 132.59f), new(175.1f, 131.12f),
            new(174.75f, 130.75f), new(174.7f, 130.25f), new(174.79f, 129.74f), new(174.82f, 129.21f), new(174.74f, 128.71f),
            new(174.75f, 128.19f), new(174.92f, 127.16f), new(174.91f, 126.65f), new(174.78f, 126.15f), new(175.09f, 125.72f),
            new(176.55f, 124.25f), new(177.02f, 124.04f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices2)], [new PolygonCustom(vertices2h1), new PolygonCustom(vertices2h2), new PolygonCustom(vertices2h3)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.ClonedThaumaturge];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    public override bool ShouldPrioritizeAllEnemies => true;
}
