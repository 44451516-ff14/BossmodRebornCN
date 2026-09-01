namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D060FacilityDreadnaught;

public enum OID : uint
{
    Boss = 0xF54, // R3.0
    MonitoringDrone = 0xF55 // R2.4
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    AutoCannons = 4825, // MonitoringDrone->self, 3.0s cast, range 40+R width 5 rect
    Rotoswipe = 4556, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    WreckingBall = 4557 // Boss->location, 4.0s cast, range 8 circle
}

sealed class Rotoswipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rotoswipe, new AOEShapeCone(11f, 60f.Degrees()));
sealed class AutoCannons(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AutoCannons, new AOEShapeRect(42.4f, 2.5f));
sealed class WreckingBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WreckingBall, 8f);

class D060FacilityDreadnaughtStates : StateMachineBuilder
{
    public D060FacilityDreadnaughtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rotoswipe>()
            .ActivateOnEnter<AutoCannons>()
            .ActivateOnEnter<WreckingBall>()
            .Raw.Update = () => AllDeadOrDestroyed(D060FacilityDreadnaught.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38u, NameID = 3836u, SortOrder = 7)]
public sealed class D060FacilityDreadnaught : BossModule
{
    public D060FacilityDreadnaught(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D060FacilityDreadnaught(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-360f, -250f), 9f, 6)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.MonitoringDrone];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }

    public override bool ShouldPrioritizeAllEnemies => true;
}
