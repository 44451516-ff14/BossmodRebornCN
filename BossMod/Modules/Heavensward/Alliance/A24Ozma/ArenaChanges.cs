namespace BossMod.Heavensward.Alliance.A24Ozma;

[SkipLocalsInit]
sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    private readonly uint[] arenaStatus = new uint[PartyState.MaxAllianceSize]; // 0 default arena, 1 split arena, 2 ozmashade arena

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var status = arenaStatus[pcSlot];
        var pos = pc.Position;
        if (status != 1u && pos.InRect(new(300f, 260f), 34f, 19f))
        {
            HandleSplitArena();
            SetArenaStatus(pcSlot, 1u);
        }
        else if (status != 2u && pos.InSquare(new(301.5f, 205.5f), 31.5f))
        {
            HandleOzmaShadeArena();
            SetArenaStatus(pcSlot, 2u);
        }
        else if (status > 0u && pos.InSquare(new(280f, -404.5f), 40f))
        {
            var arena = A24Ozma.BuildArena();
            Arena.Bounds = arena.arena;
            Arena.Center = arena.center;
            SetArenaStatus(pcSlot, default);
        }
    }

    private void SetArenaStatus(int slot, uint status) => arenaStatus[slot] = status;

    private void HandleSplitArena()
    {
        // extracted from collision data - material ID: 000070004
        WPos[] arena1 = [new(273.306f, 242.08784f), new(271.97888f, 242.14471f), new(270.55304f, 242.62935f), new(265.99969f, 242.87611f),
        new(265.99969f, 243.15482f), new(265.99969f, 243.29796f), new(265.99969f, 255.9353f), new(265.99969f, 272.87061f), new(266.20413f, 274.4176f),
        new(266.80356f, 275.85919f), new(267.75705f, 277.09711f), new(268.99969f, 278.047f), new(270.44678f, 278.6441f), new(271.99969f, 278.84778f),
        new(273.55261f, 278.6441f), new(274.99969f, 278.047f), new(276.24234f, 277.09711f), new(277.19586f, 275.85919f), new(277.79526f, 274.4176f),
        new(277.99969f, 272.87061f), new(277.99969f, 255.9353f), new(277.99969f, 242.98376f), new(277.99969f, 242.65578f), new(276.95642f, 243.15564f),
        new(276.89267f, 243.21394f), new(275.28152f, 243.03877f)];
        WPos[] arena2 = [new(301.306f, 242.08784f), new(299.97888f, 242.14471f), new(298.55304f, 242.62935f), new(293.99969f, 242.87611f),
        new(293.99969f, 243.15482f), new(293.99969f, 243.29796f), new(293.99969f, 255.9353f), new(293.99969f, 272.87061f), new(294.20413f, 274.4176f),
        new(294.80356f, 275.85919f), new(295.75705f, 277.09711f), new(296.99969f, 278.047f), new(298.44678f, 278.6441f), new(299.99969f, 278.84778f),
        new(301.55261f, 278.6441f), new(302.99969f, 278.047f), new(304.24234f, 277.09711f), new(305.19586f, 275.85919f), new(305.79526f, 274.4176f),
        new(305.99969f, 272.87061f), new(305.99969f, 255.9353f), new(305.99969f, 242.98376f), new(305.99969f, 242.65578f), new(304.95642f, 243.15564f),
        new(304.89267f, 243.21394f), new(303.28152f, 243.03877f)];
        WPos[] arena3 = [new(329.306f, 242.08795f), new(327.97888f, 242.14482f), new(326.55304f, 242.62946f), new(321.99969f, 242.87622f),
        new(321.99969f, 243.15492f), new(321.99969f, 243.29807f), new(321.99969f, 255.93541f), new(321.99969f, 272.87073f), new(322.20413f, 274.41772f),
        new(322.80356f, 275.85931f), new(323.75705f, 277.0972f), new(324.99969f, 278.04712f), new(326.44678f, 278.64423f), new(327.99969f, 278.84787f),
        new(329.55261f, 278.64423f), new(330.99969f, 278.04712f), new(332.24234f, 277.09720f), new(333.19586f, 275.85931f), new(333.79526f, 274.41772f),
        new(333.99969f, 272.87073f), new(333.99969f, 255.93541f), new(333.99969f, 242.98387f), new(333.99969f, 242.65588f), new(332.95642f, 243.15575f),
        new(332.89267f, 243.21405f), new(331.28152f, 243.03888f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(arena1), new PolygonCustom(arena2), new PolygonCustom(arena3)], AdjustForHitboxInwards: true);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }

    private void HandleOzmaShadeArena()
    {
        WPos[] vertices = [new(316.32f, 178.61f), new(321.93f, 181.74f), new(323.75f, 183.66f), new(323.94f, 184.29f), new(324.09f, 185.57f),
        new(324.04f, 187f), new(324.11f, 187.64f), new(324.87f, 190.17f), new(325.28f, 192.79f), new(325.02f, 193.42f),
        new(324.12f, 195.09f), new(324.34f, 195.65f), new(325.59f, 197.94f), new(326.56f, 198.8f), new(327.02f, 199.31f),
        new(327.97f, 201.05f), new(331.02f, 204.13f), new(331.39f, 204.71f), new(331.75f, 206.63f), new(331.53f, 207.33f),
        new(329.13f, 211.69f), new(329.36f, 212.16f), new(331.19f, 213.91f), new(333.42f, 222.11f), new(333.45f, 222.8f),
        new(332.29f, 226.92f), new(331.92f, 227.51f), new(327.81f, 230.5f), new(323.64f, 232.32f), new(322.98f, 232.38f),
        new(322.4f, 232.13f), new(321.34f, 231.47f), new(320.77f, 231.33f), new(320.12f, 231.5f), new(319.62f, 231.82f),
        new(318.88f, 232.84f), new(318.28f, 233.18f), new(317.62f, 233.16f), new(317.05f, 233.31f), new(315.38f, 234.57f),
        new(314.81f, 234.88f), new(314.2f, 235.06f), new(313.54f, 235.17f), new(312.85f, 235.13f), new(311.54f, 234.93f),
        new(310.36f, 234.64f), new(309.73f, 234.58f), new(305.09f, 234.65f), new(304.43f, 234.48f), new(302.87f, 233.44f),
        new(300.98f, 233.55f), new(300.42f, 233.75f), new(299.91f, 234.23f), new(299.38f, 234.54f), new(298.65f, 235.62f),
        new(298.04f, 235.97f), new(296.73f, 235.97f), new(294.85f, 235.72f), new(293.4f, 234.43f), new(292.79f, 234.47f),
        new(292.18f, 234.62f), new(291.59f, 234.87f), new(290.93f, 234.85f), new(287.08f, 233.96f), new(282.23f, 232.29f),
        new(278.65f, 230.46f), new(270.05f, 224.77f), new(269.84f, 224.14f), new(270.43f, 219.57f), new(271.74f, 215.83f),
        new(271.76f, 215.21f), new(271.69f, 214.56f), new(272.81f, 208.91f), new(273.57f, 207.08f), new(273.67f, 204.52f),
        new(274.9f, 198.63f), new(274.96f, 195.95f), new(275.45f, 193.4f), new(275.4f, 192.73f), new(275.54f, 191.46f),
        new(275.76f, 190.8f), new(277.32f, 189.73f), new(278.16f, 188.59f), new(278.62f, 188.06f), new(281.15f, 185.67f),
        new(281.59f, 184.41f), new(281.89f, 183.8f), new(282.99f, 182.19f), new(283.43f, 181.66f), new(284.61f, 181.28f),
        new(285.64f, 180.53f), new(287.99f, 179.29f), new(288.49f, 178.97f), new(290.95f, 176.69f), new(291.58f, 176.43f),
        new(292.93f, 176.51f), new(294.11f, 176.22f), new(296.03f, 176.09f), new(299.41f, 176.44f), new(300.01f, 176.65f),
        new(300.65f, 176.7f), new(302.02f, 176.61f), new(302.54f, 176.42f), new(304.73f, 175.09f), new(305.44f, 175.06f),
        new(309.67f, 175.36f), new(310.88f, 175.02f), new(311.59f, 174.95f), new(313.59f, 174.92f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)], [new Polygon(new(325.97046f, 222.90717f), 3.39367f, 16)]);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }
}
