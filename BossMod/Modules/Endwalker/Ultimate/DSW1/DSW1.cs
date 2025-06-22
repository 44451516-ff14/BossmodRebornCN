namespace BossMod.Endwalker.Ultimate.DSW1;

class EmptyDimension(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptyDimension, new AOEShapeDonut(6f, 70f));
class FullDimension(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FullDimension, 6f);
class HoliestHallowing(BossModule module) : Components.CastHint(module, (uint)AID.HoliestHallowing, "Interrupt!");

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SerAdelphel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 788, PlanLevel = 90)]
public class DSW1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(22f))
{
    private Actor? _grinnaux;
    private Actor? _charibert;
    public Actor? SerAdelphel() => PrimaryActor;
    public Actor? SerGrinnaux() => _grinnaux;
    public Actor? SerCharibert() => _charibert;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _grinnaux ??= Enemies((uint)OID.SerGrinnaux)[0];
        _charibert ??= Enemies((uint)OID.SerCharibert)[0];
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(SerAdelphel());
        Arena.Actor(SerGrinnaux());
        Arena.Actor(SerCharibert());
    }
}
