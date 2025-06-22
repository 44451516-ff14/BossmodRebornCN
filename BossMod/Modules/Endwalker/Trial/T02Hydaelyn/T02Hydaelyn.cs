namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class MousasScorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.MousasScorn, 4f);

class HerossSundering(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HerossSundering, new AOEShapeCone(40f, 45f.Degrees()), tankbuster: true);

class HerossRadiance(BossModule module) : Components.RaidwideCast(module, (uint)AID.HerossRadiance);
class MagossRadiance(BossModule module) : Components.RaidwideCast(module, (uint)AID.MagossRadiance);
class RadiantHalo(BossModule module) : Components.RaidwideCast(module, (uint)AID.RadiantHalo);
class CrystallineStoneIII(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.CrystallineStoneIII2, 6f, 8, 8);
class CrystallineBlizzardIII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.CrystallineBlizzardIII2, 5f);
class Beacon1(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Beacon1, 3f);
class Beacon2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Beacon2, new AOEShapeRect(45f, 3f), 10);
class HydaelynsRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HydaelynsRay, new AOEShapeRect(45f, 15f));

class T02HydaelynStates : StateMachineBuilder
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
            .ActivateOnEnter<HerossRadiance>()
            .ActivateOnEnter<MagossRadiance>()
            .ActivateOnEnter<HydaelynsRay>()
            .ActivateOnEnter<RadiantHalo>()
            .ActivateOnEnter<Lightwave>()
            .ActivateOnEnter<WeaponTracker>()
            .ActivateOnEnter<Exodus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 790, NameID = 10453)]
public class T02Hydaelyn(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(ArenaCenter, 20, 48)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.CrystalOfLight), Colors.Object);
    }
}
