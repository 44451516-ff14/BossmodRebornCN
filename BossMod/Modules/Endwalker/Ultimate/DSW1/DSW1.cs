namespace BossMod.Endwalker.Ultimate.DSW1;

sealed class EmptyDimension(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptyDimension, new AOEShapeDonut(6f, 70f));
sealed class FullDimension(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FullDimension, 6f);
sealed class HoliestHallowing(BossModule module) : Components.CastHint(module, (uint)AID.HoliestHallowing, "Interrupt!");

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SerAdelphel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 788, PlanLevel = 90)]
public sealed class DSW1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(22f))
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
        if (_grinnaux == null)
        {
            var b = Enemies((uint)OID.SerGrinnaux);
            _grinnaux = b.Count != 0 ? b[0] : null;
        }
        if (_charibert == null)
        {
            var b = Enemies((uint)OID.SerCharibert);
            _charibert = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_grinnaux);
        Arena.Actor(_charibert);
    }
}
