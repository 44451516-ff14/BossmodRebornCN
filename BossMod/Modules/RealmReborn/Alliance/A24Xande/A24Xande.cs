namespace BossMod.RealmReborn.Alliance.A24Xande;

class KnucklePress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KnucklePress, 10);
class BurningRave1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningRave1, 8);
class BurningRave2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningRave2, 8);
class AncientQuake(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientQuake);
class AncientQuaga(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientQuaga);
class AuraCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AuraCannon, new AOEShapeRect(60, 5));
//class Stackmarker(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.KnucklePress, 6, 2, 4);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2824)]
public class A24Xande(WorldState ws, Actor primary) : BossModule(ws, primary, new(-400, -200), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.StonefallCircle));
        Arena.Actors(Enemies(OID.StarfallCircle));
    }
}
