namespace BossMod.Heavensward.Alliance.A34Scathach;

class ThirtyCries(BossModule module) : Components.Cleave(module, (uint)AID.ThirtyCries, new AOEShapeCircle(12));
class ThirtyThorns4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThirtyThorns4, 8);
class ThirtySouls(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThirtySouls);
class ThirtyArrows2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThirtyArrows2, new AOEShapeRect(35.5f, 4));
class ThirtyArrows1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThirtyArrows1, 8);
class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, 30);

class Shadespin2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadespin2, new AOEShapeCone(30, 45.Degrees()));

class Shadesmite1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadesmite1, 15);
class Shadesmite2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadesmite2, 3);
class Shadesmite3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadesmite3, 3);

class Pitfall(BossModule module) : Components.RaidwideCast(module, (uint)AID.Pitfall);
class FullSwing(BossModule module) : Components.RaidwideCast(module, (uint)AID.FullSwing);

class Nox(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(10), (uint)AID.NoxAOEFirst, (uint)AID.NoxAOERest, 5.5f, 1.6f, 5, true);

abstract class MarrowDrain(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(10.44f, 60.Degrees()));
class MarrowDrain1(BossModule module) : MarrowDrain(module, (uint)AID.MarrowDrain1);
class MarrowDrain2(BossModule module) : MarrowDrain(module, (uint)AID.MarrowDrain2);

class BigHug(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BigHug, new AOEShapeRect(5.25f, 1.5f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5515)]
public class A34Scathach(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -50), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Connla));
        Arena.Actors(Enemies((uint)OID.Connla2));
        Arena.Actors(Enemies((uint)OID.ShadowLimb));
        Arena.Actors(Enemies((uint)OID.ShadowcourtJester));
        Arena.Actors(Enemies((uint)OID.ChimeraPoppet));
        Arena.Actors(Enemies((uint)OID.ShadowcourtHound));
    }
}
