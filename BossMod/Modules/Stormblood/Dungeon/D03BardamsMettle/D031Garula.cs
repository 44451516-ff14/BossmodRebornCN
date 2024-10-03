namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D031Garula;

public enum OID : uint
{
    Boss = 0x1A9E, // R4.0
    SteppeSheep = 0x1A9F, // R0.7
    SteppeYamaa1 = 0x1AA1, // R1.92
    SteppeYamaa2 = 0x1AA0, // R1.92
    SteppeCoeurl = 0x1AA2, // R3.15
    Helper = 0x19A
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Heave = 7927, // Boss->self, 2.5s cast, range 9+R 120-degree cone
    CrumblingCrustVisual = 7928, // Boss->self, 4.0s cast, single-target
    CrumblingCrust = 7955, // Helper->location, 1.5s cast, range 3 circle
    Rush = 7929, // Boss->player, 10.0s cast, width 8 rect charge
    WarCry = 7930, // Boss->self, no cast, range 15+R circle
    Earthquake = 7931, // Boss->self, no cast, range 50+R circle
    Lullaby = 9394, // SteppeSheep->self, 3.0s cast, range 3+R circle, applies sleep
    RushYamaa = 7932, // SteppeYamaa1/SteppeYamaa2->location, 4.5s cast, width 8 rect charge
    WideBlaster = 9395, // SteppeCoeurl->self, 5.5s cast, range 26+R 120-degree cone
}

class CrumblingCrust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CrumblingCrust), 3);
class Heave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Heave), new AOEShapeCone(13, 60.Degrees()));
class WideBlaster(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WideBlaster), new AOEShapeCone(29.15f, 60.Degrees()));
class Rush(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.Rush), 4);
class RushTether(BossModule module) : Components.StretchTetherDuo(module, 16, 10.1f)
{
    public override void Update()
    {
        if (Module.PrimaryActor.CastInfo == null)
            CurrentBaits.Clear();
    }
}

class RushYamaa(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.RushYamaa), 4);
class Lullaby(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Lullaby), new AOEShapeCircle(3.7f));

class D031GarulaStates : StateMachineBuilder
{
    public D031GarulaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrumblingCrust>()
            .ActivateOnEnter<Heave>()
            .ActivateOnEnter<WideBlaster>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<RushTether>()
            .ActivateOnEnter<RushYamaa>()
            .ActivateOnEnter<Lullaby>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6173)]
public class D031Garula(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(7.87f, 224.08f), new(8.22f, 224.47f), new(8.65f, 224.78f), new(9.77f, 224.86f), new(13.65f, 226.15f),
    new(14.72f, 226.61f), new(15.14f, 226.89f), new(16.27f, 228.18f), new(16.78f, 228.48f), new(17.78f, 228.9f),
    new(18.31f, 229.1f), new(18.84f, 229.36f), new(19.57f, 230.25f), new(19.99f, 230.67f), new(20.85f, 231.43f),
    new(21.28f, 231.7f), new(21.58f, 232.11f), new(22.21f, 233.06f), new(22.58f, 233.52f), new(23.97f, 235.06f),
    new(24.33f, 235.55f), new(25.01f, 236.53f), new(25.37f, 237.71f), new(27.2f, 242.23f), new(28.29f, 245.61f),
    new(28.42f, 246.13f), new(28.04f, 248.39f), new(28, 248.93f), new(27.96f, 250.01f), new(28.02f, 251.19f),
    new(27.85f, 252.77f), new(28.1f, 253.37f), new(28.01f, 253.96f), new(27.85f, 254.48f), new(27.61f, 254.96f),
    new(27.35f, 255.41f), new(26.91f, 256.41f), new(26.47f, 257.98f), new(26.31f, 258.46f), new(25.86f, 259.56f),
    new(25.6f, 260.12f), new(25.02f, 260.96f), new(24.79f, 261.5f), new(24.47f, 262.47f), new(24.28f, 262.96f),
    new(23.15f, 264.06f), new(22.55f, 264.9f), new(22.32f, 265.42f), new(22.13f, 265.9f), new(21.52f, 266.74f),
    new(18.94f, 268.9f), new(18.45f, 269.27f), new(17.46f, 269.59f), new(16.99f, 269.77f), new(15.18f, 271.1f),
    new(14.66f, 271.34f), new(13.59f, 271.78f), new(13.17f, 272.09f), new(11.56f, 273.4f), new(-2.51f, 273.4f),
    new(-2.92f, 273.01f), new(-3.25f, 272.61f), new(-3.66f, 272.32f), new(-4.17f, 272.12f), new(-4.63f, 271.89f),
    new(-5.16f, 271.72f), new(-5.73f, 271.5f), new(-7.57f, 270.1f), new(-8.56f, 269.26f), new(-9.07f, 268.94f),
    new(-11.19f, 267.3f), new(-11.64f, 267.03f), new(-12.71f, 266.67f), new(-13.08f, 266.23f), new(-13.35f, 265.76f),
    new(-13.72f, 265.41f), new(-14.15f, 264.27f), new(-14.43f, 263.81f), new(-14.81f, 263.41f), new(-15.72f, 262.85f),
    new(-16.27f, 262.77f), new(-16.47f, 262.28f), new(-16.53f, 261.17f), new(-16.85f, 260.63f), new(-17.2f, 260.21f),
    new(-17.68f, 259.97f), new(-17.95f, 259.44f), new(-18.14f, 257.86f), new(-18.86f, 256.36f), new(-19.15f, 255.93f),
    new(-19.9f, 255.05f), new(-19.89f, 254.47f), new(-19.68f, 252.96f), new(-19.79f, 252.4f), new(-20.47f, 250.33f),
    new(-20.58f, 249.77f), new(-20.12f, 245.03f), new(-20.43f, 243.36f), new(-20.22f, 242.87f), new(-16.98f, 235.96f),
    new(-16.71f, 235.45f), new(-16.43f, 234.96f), new(-15.45f, 233.67f), new(-14.37f, 232.55f), new(-13.92f, 232.15f),
    new(-12.45f, 230.66f), new(-12.03f, 230.31f), new(-9.34f, 228.28f), new(-8.9f, 228), new(-6.05f, 226.45f),
    new(-5.58f, 226.21f), new(-3.53f, 225.55f), new(-2.96f, 225.33f), new(-1.87f, 224.98f), new(-1.31f, 224.82f),
    new(-0.79f, 224.75f), new(-0.24f, 224.57f), new(0.13f, 224.07f), new(5.77f, 224.07f), new(7.73f, 224.02f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
}
