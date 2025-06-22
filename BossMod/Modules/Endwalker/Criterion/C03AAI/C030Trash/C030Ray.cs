namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash1;

abstract class Hydrocannon(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(15f, 3f));
class NHydrocannon(BossModule module) : Hydrocannon(module, (uint)AID.NHydrocannon);
class SHydrocannon(BossModule module) : Hydrocannon(module, (uint)AID.SHydrocannon);

abstract class Expulsion(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 8f);
class NExpulsion(BossModule module) : Expulsion(module, (uint)AID.NExpulsion);
class SExpulsion(BossModule module) : Expulsion(module, (uint)AID.SExpulsion);

abstract class ElectricWhorl(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(8f, 60f));
class NElectricWhorl(BossModule module) : ElectricWhorl(module, (uint)AID.NElectricWhorl);
class SElectricWhorl(BossModule module) : ElectricWhorl(module, (uint)AID.SElectricWhorl);

class C030RayStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030RayStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<NHydrocannon>(!_savage)
            .ActivateOnEnter<SHydrocannon>(_savage)
            .ActivateOnEnter<NExpulsion>(!_savage)
            .ActivateOnEnter<SExpulsion>(_savage)
            .ActivateOnEnter<NElectricWhorl>(!_savage)
            .ActivateOnEnter<SElectricWhorl>(_savage)
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        Hydrocannon(id, 8.3f);
        ExpulsionElectricWhorl(id + 0x10000, 2.1f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Hydrocannon(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SHydrocannon : (uint)AID.NHydrocannon, delay, 5, "Line AOE");
    }

    private void ExpulsionElectricWhorl(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SExpulsion : (uint)AID.NExpulsion, delay, 5, "Out");
        Cast(id + 0x10, _savage ? (uint)AID.SExpulsion : (uint)AID.NExpulsion, 2.1f, 5, "In");
    }
}
class C030NRayStates(BossModule module) : C030RayStates(module, false);
class C030SRayStates(BossModule module) : C030RayStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NRay, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12541, SortOrder = 3)]
public class C030NRay(WorldState ws, Actor primary) : C030Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.NPaddleBiter));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SRay, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12541, SortOrder = 3)]
public class C030SRay(WorldState ws, Actor primary) : C030Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SPaddleBiter));
    }
}
