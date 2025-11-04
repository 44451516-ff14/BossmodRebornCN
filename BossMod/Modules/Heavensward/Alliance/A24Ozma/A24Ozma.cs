namespace BossMod.Heavensward.Alliance.A24Ozma;

class MeteorImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpact, 20);
class HolyKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Holy, 3);
class Holy(BossModule module) : Components.RaidwideCast(module, (uint)AID.Holy);
class ExecrationAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecrationAOE, new AOEShapeRect(40.5f, 5));

class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4896)]
public class A24Ozma(WorldState ws, Actor primary) : BossModule(ws, primary, new(280, -410), arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Donut(new(280, -410), 18, 25), new Rectangle(new(280, -386), 5, 12), new Rectangle(new(260, -422), 5, 12, -120.Degrees()),
    new Rectangle(new(300, -422), 5, 12, 120.Degrees())]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SingularityFragment));
        Arena.Actors(Enemies((uint)OID.SingularityEcho));
        Arena.Actors(Enemies((uint)OID.SingularityRipple));
        Arena.Actors(Enemies((uint)OID.Ozmasphere));
        Arena.Actors(Enemies((uint)OID.Ozmashade));
    }
}
