namespace BossMod.Heavensward.Alliance.A35Diabolos;

class Nightmare(BossModule module) : Components.CastGaze(module, (uint)AID.Nightmare);
class NightTerror(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.NightTerror, 10, 8);
class RuinousOmen1(BossModule module) : Components.RaidwideCast(module, (uint)AID.RuinousOmen1);
class RuinousOmen2(BossModule module) : Components.RaidwideCast(module, (uint)AID.RuinousOmen2);
class UltimateTerror(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UltimateTerror, new AOEShapeDonut(5, 19.5f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5526)]
public class A35Diabolos(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -445), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Deathgate));
        Arena.Actors(Enemies((uint)OID.Lifegate));
    }
}
