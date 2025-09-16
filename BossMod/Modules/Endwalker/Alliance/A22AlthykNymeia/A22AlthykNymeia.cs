namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class MythrilGreataxe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MythrilGreataxe, new AOEShapeCone(71f, 30f.Degrees()));
class Hydroptosis(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HydroptosisAOE, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Althyk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12244, PlanLevel = 90)]
public class A22AlthykNymeia(WorldState ws, Actor primary) : BossModule(ws, primary, new(50f, -750f), new ArenaBoundsSquare(25f))
{
    private Actor? _nymeia;

    public Actor? Althyk() => PrimaryActor;
    public Actor? Nymeia() => _nymeia;

    protected override void UpdateModule()
    {
        _nymeia ??= GetActor((uint)OID.Nymeia);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_nymeia);
    }
}
