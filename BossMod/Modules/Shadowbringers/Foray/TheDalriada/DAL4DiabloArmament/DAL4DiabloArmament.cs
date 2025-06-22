namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

class AdvancedDeathIVAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AdvancedDeathIVAOE, 1);

class AdvancedNox(BossModule module) : Components.StandardChasingAOEs(module, 10f, (uint)AID.AdvancedNoxAOEFirst, (uint)AID.AdvancedNoxAOERest, 5.5f, 1.6f, 5, true);

class AccelerationBomb(BossModule module) : Components.StayMove(module)
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

class AssaultCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultCannon, new AOEShapeRect(100, 3));
class DeadlyDealingAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadlyDealingAOE, 6);

class Explosion1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion1, new AOEShapeRect(60, 11));
class Explosion2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion2, new AOEShapeRect(60, 11));
class Explosion3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion3, new AOEShapeRect(60, 11));
class Explosion4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion4, new AOEShapeRect(60, 11));
class Explosion5(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion5, new AOEShapeRect(60, 11));
class Explosion6(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion6, new AOEShapeRect(60, 11));
class Explosion7(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion7, new AOEShapeRect(60, 11));
class Explosion8(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion8, new AOEShapeRect(60, 11));
class Explosion9(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion9, new AOEShapeRect(60, 11));

class LightPseudopillarAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightPseudopillarAOE, 10);

class PillarOfShamash1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarOfShamash1, new AOEShapeCone(70, 10.Degrees()));
class PillarOfShamash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarOfShamash2, new AOEShapeCone(70, 10.Degrees()));
class PillarOfShamash3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarOfShamash3, new AOEShapeCone(70, 10.Degrees()));

class UltimatePseudoterror(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UltimatePseudoterror, new AOEShapeDonut(15, 70));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 32, SortOrder = 5)] //BossNameID = 10007
public class DAL4DiabloArmament(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(-720f, -760f);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Polygon(ArenaCenter, 29.5f, 48)]);
    public static readonly ArenaBoundsCircle SmallArena = new(17); // this is a pulsing donut aoe
}
