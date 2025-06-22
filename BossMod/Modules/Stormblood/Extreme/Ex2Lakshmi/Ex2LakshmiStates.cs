namespace BossMod.Stormblood.Extreme.Ex2Lakshmi;

class DivineDenial(BossModule module) : Components.RaidwideCast(module, (uint)AID.DivineDenial);
class ThePallOfLight(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThePallOfLight, 7, 8, 8);
class InnerDemonsGaze(BossModule module) : Components.CastGaze(module, (uint)AID.InnerDemons);
class InnerDemonsAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InnerDemons, 7);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 264, NameID = 6385)]
public class Ex2Lakshmi : BossModule
{
    public readonly List<Actor> DreamingKshatriya;

    public Ex2Lakshmi(WorldState ws, Actor primary) : base(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
    {
        DreamingKshatriya = Enemies(OID.DreamingKshatriya);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(DreamingKshatriya);
    }
}
