namespace BossMod.Endwalker.Unreal.Un4Zurvan;

abstract class MetalCutter(BossModule module, uint aid, OID oid) : Components.Cleave(module, aid, new AOEShapeCone(37.44f, 45f.Degrees()), [(uint)oid]);
class P1MetalCutter(BossModule module) : MetalCutter(module, (uint)AID.MetalCutterP1, OID.BossP1);

class P1FlareStar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlareStarAOE, 6f);
class P1Purge(BossModule module) : Components.CastCounter(module, (uint)AID.Purge);
class P2MetalCutter(BossModule module) : MetalCutter(module, (uint)AID.MetalCutterP2, OID.BossP2);
class P2IcyVoidzone(BossModule module) : Components.Voidzone(module, 5f, m => m.Enemies((uint)OID.IcyVoidzone).Where(z => z.EventState != 7));
class P2BitingHalberd(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitingHalberd, new AOEShapeCone(55.27f, 135f.Degrees()));
class P2TailEnd(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailEnd, 15f);
class P2Ciclicle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ciclicle, new AOEShapeDonut(10f, 20f)); // TODO: verify inner radius
class P2SouthernCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SouthernCrossAOE, 6f);
class P2SouthernCrossVoidzone(BossModule module) : Components.Voidzone(module, 6f, m => m.Enemies((uint)OID.SouthernCrossVoidzone).Where(z => z.EventState != 7));
class P2WaveCannon(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WaveCannonSolo, new AOEShapeRect(55.27f, 5f));
class P2TyrfingFire(BossModule module) : Components.Cleave(module, (uint)AID.TyrfingFire, new AOEShapeCircle(5), [(uint)OID.BossP2], originAtTarget: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 951, NameID = 5567, PlanLevel = 90)]
public class Un4Zurvan(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20))
{
    private Actor? _bossP2;

    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        _bossP2 ??= GetActor((uint)OID.BossP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }
}
