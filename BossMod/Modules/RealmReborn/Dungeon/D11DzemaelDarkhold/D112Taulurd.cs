namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D112Taulurd;

public enum OID : uint
{
    Taulurd = 0x4A8B, // R1.56
    DeepvoidSlave = 0x4A8C // R1.04
}

public enum AID : uint
{
    AutoAttack = 872, // Taulurd->player, no cast, single-target

    DoubleSmash = 45571, // Taulurd->self, 3.0s cast, range 8 120-degree cone
    CallSlaves = 45574, // Taulurd->self, no cast, single-target
    Firewater = 45575, // DeepvoidSlave->location, 4.0s cast, range 5 circle
    StraightPunch = 45573, // Taulurd->player, no cast, single-target
    Boulderdash = 45576 // DeepvoidSlave->player, no cast, single-target
}

[SkipLocalsInit]
sealed class DoubleSmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoubleSmash, new AOEShapeCone(8f, 60f.Degrees()));

[SkipLocalsInit]
sealed class Firewater(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firewater, 5f);

[SkipLocalsInit]
sealed class D112TaulurdStates : StateMachineBuilder
{
    public D112TaulurdStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DoubleSmash>()
            .ActivateOnEnter<Firewater>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(D112TaulurdStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.Taulurd,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.RealmReborn,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 13u,
NameID = 1415u,
SortOrder = 2,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D112Taulurd : BossModule
{
    public D112Taulurd(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D112Taulurd(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(-95.03f, -41.05f), new(-94.48f, -40.84f), new(-93.86f, -40.77f), new(-93.18f, -40.77f), new(-91.84f, -40.56f),
        new(-90.57f, -40.53f), new(-89.93f, -40.24f), new(-89.54f, -39.71f), new(-89.13f, -39.24f), new(-85.04f, -36.61f),
        new(-83.86f, -35.1f), new(-83.51f, -34.52f), new(-83.4f, -34.0f), new(-83.75f, -33.51f), new(-83.92f, -32.89f),
        new(-83.6f, -32.35f), new(-83.6f, -31.59f), new(-83.43f, -30.97f), new(-83.21f, -30.38f), new(-83.05f, -29.77f),
        new(-82.8f, -29.12f), new(-82.75f, -28.45f), new(-82.79f, -27.87f), new(-82.44f, -27.4f), new(-81.95f, -26.92f),
        new(-81.5f, -26.35f), new(-81.75f, -25.69f), new(-81.83f, -25.07f), new(-81.84f, -24.4f), new(-82.15f, -23.86f),
        new(-82.29f, -23.2f), new(-82.21f, -22.57f), new(-82.35f, -21.91f), new(-83.12f, -20.16f), new(-84.18f, -18.48f),
        new(-84.72f, -18.14f), new(-85.37f, -17.06f), new(-86.76f, -15.71f), new(-90.73f, -13.4f), new(-91.28f, -13.25f),
        new(-91.91f, -12.99f), new(-93.13f, -12.73f), new(-94.42f, -12.59f), new(-94.87f, -13.07f), new(-95.41f, -13.42f),
        new(-97.53f, -13.38f), new(-98.12f, -13.72f), new(-98.56f, -14.26f), new(-99.07f, -14.58f),
        new(-101.63f, -14.93f), new(-101.59f, -14.67f), new(-102.07f, -14.43f), new(-102.68f, -14.61f),
        new(-109.26f, -20.68f), new(-109.71f, -21.19f), new(-109.57f, -21.68f), new(-109.58f, -22.27f), new(-109.85f, -22.71f),
        new(-109.47f, -23.09f), new(-107.6f, -23.99f), new(-107.28f, -24.43f), new(-107.34f, -24.99f), new(-108.24f, -26.12f),
        new(-107.95f, -28.12f), new(-107.36f, -30.05f), new(-108.39f, -31.6f), new(-108.39f, -32.28f), new(-108.21f, -32.86f),
        new(-108.42f, -33.42f), new(-108.32f, -34.11f), new(-108.14f, -34.75f), new(-107.82f, -35.37f), new(-107.28f, -35.74f),
        new(-106.73f, -35.96f), new(-106.46f, -36.51f), new(-103.81f, -38.59f), new(-103.32f, -39.05f), new(-102.93f, -39.57f),
        new(-102.44f, -40.05f), new(-101.87f, -40.32f), new(-100.47f, -40.43f), new(-99.82f, -40.25f), new(-99.21f, -40.27f),
        new(-98.07f, -40.81f), new(-97.38f, -41.03f), new(-96.75f, -40.95f), new(-96.15f, -41.03f), new(-95.52f, -41.18f),
        new(-95.03f, -41.05f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }
}
