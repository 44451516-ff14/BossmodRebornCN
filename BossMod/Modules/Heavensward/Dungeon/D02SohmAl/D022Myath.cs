namespace BossMod.Heavensward.Dungeon.D02SohmAl.D022Myath;

public enum OID : uint
{
    Boss = 0xE91, // R4.9
    BloodOfTheMountain = 0xE95, // R1.6
    RheumOfTheMountain = 0xE94, // R1.6
    ChymeOfTheMountain = 0xE92 // R3.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ThirdLegForward = 4994, // Boss->self, no cast, range 6+R 90-degree cone
    Overbite = 3803, // Boss->player, no cast, single-target
    RazorScales = 3804, // Boss->self, 3.0s cast, range 60+R 60-degree cone
    PrimordialRoar = 3810, // Boss->self, 3.0s cast, range 60+R circle
    Ensnare = 3805, // Boss->RheumOfTheMountain/BloodOfTheMountain, no cast, single-target
    MadDash = 3808, // Boss->player, 5.0s cast, range 6 circle
    MadDashStack = 3809, // Boss->player, 5.0s cast, range 6 circle
    TheLastSong = 4995 // ChymeOfTheMountain->self, 12.0s cast, range 60 circle
}

class ThirdLegForward(BossModule module) : Components.Cleave(module, (uint)AID.ThirdLegForward, new AOEShapeCone(10.9f, 60f.Degrees()))
{
    private readonly MadDashStack _stack = module.FindComponent<MadDashStack>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_stack.ActiveStacks.Count == 0)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_stack.ActiveStacks.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_stack.ActiveStacks.Count == 0)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class RazorScales(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RazorScales, new AOEShapeCone(64.9f, 30f.Degrees()));
class PrimordialRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.PrimordialRoar);
class MadDashSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MadDash, 6f);
class MadDashStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MadDashStack, 6f, 4, 4);

class D022MyathStates : StateMachineBuilder
{
    public D022MyathStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MadDashStack>()
            .ActivateOnEnter<MadDashSpread>()
            .ActivateOnEnter<ThirdLegForward>()
            .ActivateOnEnter<ThirdLegForward>()
            .ActivateOnEnter<RazorScales>()
            .ActivateOnEnter<PrimordialRoar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 37, NameID = 3793)]
public class D022Myath(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(162.7f, -125.94f), new(166.72f, -125.83f), new(167.19f, -125.64f), new(189.76f, -103.09f), new(189.69f, -102.56f),
    new(189.8f, -101.97f), new(190.27f, -100.43f), new(190.34f, -99.91f), new(190.3f, -92.25f), new(190.5f, -88.93f),
    new(188.68f, -82.66f), new(188.44f, -82.12f), new(187.65f, -80.67f), new(187.42f, -80.14f), new(186.57f, -77.84f),
    new(186.27f, -77.42f), new(183.4f, -73.77f), new(183.06f, -73.37f), new(182.02f, -72.75f), new(181.6f, -72.39f),
    new(180.9f, -71.45f), new(180.37f, -70.57f), new(179.99f, -70.15f), new(179.66f, -69.68f), new(179.45f, -69.19f),
    new(178.64f, -68.51f), new(176.77f, -67.07f), new(176.35f, -66.79f), new(174.29f, -65.97f), new(173.82f, -65.75f),
    new(173.24f, -64.73f), new(172.72f, -64.59f), new(156.94f, -62.12f), new(156.42f, -61.97f), new(153.37f, -63.27f),
    new(152.86f, -63.4f), new(151.74f, -63.6f), new(151.18f, -63.68f), new(146.43f, -64.52f), new(145.93f, -64.84f),
    new(145.47f, -65.15f), new(144.93f, -65.44f), new(143.44f, -66.09f), new(140.13f, -67.92f), new(139.32f, -68.79f),
    new(138.88f, -69.17f), new(137.48f, -70.01f), new(136.33f, -71.33f), new(134.3f, -74.04f), new(131.88f, -76.54f),
    new(131.59f, -76.96f), new(131.16f, -78.12f), new(130.03f, -79.56f), new(129.75f, -80.65f), new(129.57f, -81.2f),
    new(127.71f, -84.05f), new(127.2f, -86.17f), new(127.16f, -87.84f), new(127.12f, -88.39f), new(126.38f, -94.39f),
    new(127.22f, -100.08f), new(127.48f, -101.24f), new(127.59f, -101.83f), new(128.37f, -105.32f), new(128.34f, -108.04f),
    new(128.37f, -108.58f), new(129.65f, -111.26f), new(129.95f, -111.66f), new(133.95f, -114.82f), new(134.34f, -115.23f),
    new(136.35f, -118.48f), new(136.69f, -118.88f), new(138.86f, -120.64f), new(139.38f, -120.88f), new(142.52f, -122.1f),
    new(143.03f, -122.33f), new(145.37f, -123.93f), new(145.86f, -124.17f), new(146.89f, -124.46f), new(147.38f, -124.63f),
    new(148.39f, -125.22f), new(148.94f, -125.42f), new(153, -125.85f), new(153.54f, -125.97f), new(155.21f, -126.39f),
    new(155.78f, -126.44f), new(157.51f, -126.26f), new(158.11f, -126.24f), new(159.83f, -126.55f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ChymeOfTheMountain));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ChymeOfTheMountain => 1,
                _ => 0
            };
        }
    }
}
