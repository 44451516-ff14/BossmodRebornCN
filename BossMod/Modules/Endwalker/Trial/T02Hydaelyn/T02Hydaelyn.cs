namespace BossMod.Endwalker.Trial.T02Hydaelyn;

sealed class MousasScorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.MousasScorn, 4f);

sealed class HerossSundering(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HerossSundering, new AOEShapeCone(40f, 45f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class MagossHerossRadianceRadiantHalo(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.RadiantHalo, (uint)AID.MagossRadiance, (uint)AID.HerossRadiance]);

sealed class CrystallineStoneIII(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.CrystallineStoneIII2, 6f, 8, 8);
sealed class CrystallineBlizzardIII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.CrystallineBlizzardIII2, 5f);
sealed class Beacon1(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Beacon1, 3f);
sealed class Beacon2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Beacon2, new AOEShapeRect(45f, 3f), 10);
sealed class HydaelynsRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HydaelynsRay, new AOEShapeRect(45f, 15f));

sealed class T02HydaelynStates : StateMachineBuilder
{
    public T02HydaelynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ParhelicCircle>()
            .ActivateOnEnter<MousasScorn>()
            .ActivateOnEnter<Echoes>()
            .ActivateOnEnter<Beacon1>()
            .ActivateOnEnter<Beacon2>()
            .ActivateOnEnter<CrystallineStoneIII>()
            .ActivateOnEnter<CrystallineBlizzardIII>()
            .ActivateOnEnter<HerossSundering>()
            .ActivateOnEnter<MagossHerossRadianceRadiantHalo>()
            .ActivateOnEnter<HydaelynsRay>()
            .ActivateOnEnter<Lightwave>()
            .ActivateOnEnter<WeaponTracker>()
            .ActivateOnEnter<Exodus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 790u, NameID = 10453u)]
public sealed class T02Hydaelyn(WorldState ws, Actor primary) : HydaelynTrial(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.CrystalOfLight), Colors.Object);
    }
}

public abstract class HydaelynTrial : BossModule
{
    public HydaelynTrial(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private HydaelynTrial(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(100f, 100f), 20f, 48)]);
        return (arena.Center, arena);
    }
}
