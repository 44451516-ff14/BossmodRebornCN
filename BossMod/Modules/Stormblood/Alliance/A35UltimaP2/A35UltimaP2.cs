namespace BossMod.Stormblood.Alliance.A35UltimaP2;

class Redemption(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Redemption);
class Auralight1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Auralight1, new AOEShapeRect(50, 5));
class Auralight2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Auralight2, new AOEShapeRect(25, 5));
class Bombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bombardment, 6);
class Embrace2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Embrace2, 3);
class GrandCrossAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossAOE, new AOEShapeCross(60, 7.5f));
class Holy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Holy, 2);
class HolyIVBait(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyIVBait, 6);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HolyIVSpread, 6);
class Plummet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plummet, new AOEShapeRect(15, 7.5f));

class Cataclysm(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A35UltimaP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -600), new ArenaBoundsSquare(30));
