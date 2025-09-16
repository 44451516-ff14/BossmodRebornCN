namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D011Chopper;

public enum OID : uint
{
    // Boss
    Boss = 0x19B // Chopper
}

public enum AID : uint
{
    // Boss
    AutoAttack = 870, // Boss->player, no cast
    ChargedWhisker = 351 // Boss->self, 3.0s cast, range 6.15 circle aoe
}

class ChargedWhisker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChargedWhisker, new AOEShapeCircle(6.15f));

class D011ChopperStates : StateMachineBuilder
{
    public D011ChopperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChargedWhisker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1204)]
public class D011Chopper(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly PolygonCustom[] shape = [new([new(91.5f, -67.7f), new(88.7f, -60.2f), new(79.5f, -60.4f),
    new(79.4f, -58.9f), new(78.0f, -58.8f), new(77.4f, -59.9f), new(78.2f, -60.9f), new(76.5f, -61.5f), new(75.9f, -61.4f),
    new(74.8f, -61.9f), new(64.2f, -64.1f), new(60.0f, -61.2f), new(56.7f, -60.7f), new(61.3f, -51.5f), new(63.1f, -50.6f),
    new(63.9f, -48.6f), new(63.0f, -47.5f), new(61.6f, -47.0f), new(59.2f, -38.3f), new(61.8f, -32.7f), new(69.1f, -29.0f),
    new(75.1f, -29.8f), new(75.0f, -30.8f), new(75.4f, -31.6f), new(74.6f, -32.7f), new(75.2f, -33.8f), new(77.7f, -34.3f),
    new(78.9f, -33.5f), new(78.0f, -29.8f), new(84.2f, -28.9f), new(83.5f, -33.0f), new(84.1f, -35.5f), new(86.6f, -37.2f),
    new(87.3f, -41.5f), new(88.5f, -42.0f), new(91.1f, -39.8f), new(88.9f, -37.4f), new(95.7f, -35.9f), new(91.9f, -45.1f),
    new(93.7f, -51.4f), new(101.7f, -54.5f)])];
    public static readonly ArenaBoundsCustom arena = new(shape);
}
