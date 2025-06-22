namespace BossMod.Heavensward.Alliance.A32FerdiadHollow;

class Blackbolt(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Blackbolt, 6, 8);

class Blackfire2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Blackfire2, 7); // expanding aoe circle

class JestersJig1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JestersJig1, 9);

class JestersReap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JestersReap, new AOEShapeCone(13.4f, 60.Degrees()));
class JestersReward(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JestersReward, new AOEShapeCone(31.4f, 90.Degrees()));

class JongleursX(BossModule module) : Components.SingleTargetCast(module, (uint)AID.JongleursX);
class JugglingSphere(BossModule module) : Components.ChargeAOEs(module, (uint)AID.JugglingSphere, 3);
class JugglingSphere2(BossModule module) : Components.ChargeAOEs(module, (uint)AID.JugglingSphere2, 3);

class LuckyPierrot1(BossModule module) : Components.ChargeAOEs(module, (uint)AID.LuckyPierrot1, 2.5f);
class LuckyPierrot2(BossModule module) : Components.ChargeAOEs(module, (uint)AID.LuckyPierrot2, 2.5f);

class PetrifyingEye(BossModule module) : Components.CastGaze(module, (uint)AID.PetrifyingEye);

class Flameflow1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Flameflow1);
class Flameflow2(BossModule module) : Components.RaidwideCast(module, (uint)AID.Flameflow2);
class Flameflow3(BossModule module) : Components.RaidwideCast(module, (uint)AID.Flameflow3);

class Unknown4(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Unknown4, 3);
class Unknown6(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Unknown6, 3);

class AtmosAOE1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AtmosAOE1, 20);
class AtmosAOE2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AtmosAOE2, 20);
class AtmosDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AtmosDonut, new AOEShapeDonut(6, 20));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5509)]
public class A32FerdiadHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, 225), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.FerdiadsFool));
    }
}
