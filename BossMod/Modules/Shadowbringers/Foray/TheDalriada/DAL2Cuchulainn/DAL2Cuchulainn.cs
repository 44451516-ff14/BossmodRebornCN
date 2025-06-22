namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

class PutrifiedSoul1(BossModule module) : Components.RaidwideCast(module, (uint)AID.PutrifiedSoul1);
class PutrifiedSoul2(BossModule module) : Components.RaidwideCast(module, (uint)AID.PutrifiedSoul2);
class MightOfMalice(BossModule module) : Components.SingleTargetCast(module, (uint)AID.MightOfMalice);
class BurgeoningDread(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);
class NecroticBillowAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NecroticBillowAOE, 8);
class AmbientPulsationAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AmbientPulsationAOE, 12);

class FellFlow1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FellFlow1, new AOEShapeCone(50, 60.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 32, SortOrder = 3)] //BossNameID = 10004
public class DAL2Cuchulainn(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsComplex arena = new([new Polygon(new(650f, -187.4f), 25.199f, 48)], [new Rectangle(new(650f, -162f), 20f, 1.25f), new Rectangle(new(650f, -213f), 20f, 1.25f)]);
    //small circles: SW: new(637.270f, -174.667f), SE: new(662.710f, -174.667f), NE: new(662.710f, -200.133f), NW: new(637.270f, -200.133f) - 40 vertices, radius 5.676
}
