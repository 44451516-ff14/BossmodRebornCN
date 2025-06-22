namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash2;

abstract class GravityForce(BossModule module, uint aid) : Components.StackWithCastTargets(module, aid, 6, 4, 4);
class NGravityForce(BossModule module) : GravityForce(module, (uint)AID.NGravityForce);
class SGravityForce(BossModule module) : GravityForce(module, (uint)AID.SGravityForce);

abstract class IsleDrop(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6);
class NIsleDrop(BossModule module) : IsleDrop(module, (uint)AID.NIsleDrop);
class SIsleDrop(BossModule module) : IsleDrop(module, (uint)AID.SIsleDrop);

class C030IslekeeperStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030IslekeeperStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        AncientQuaga(id, 11.9f);
        GravityForce(id + 0x10000, 6.3f);
        IsleDrop(id + 0x20000, 2.1f);
        AncientQuaga(id + 0x30000, 8.5f);
        Cast(id + 0x40000, _savage ? (uint)AID.SAncientQuagaEnrage : (uint)AID.NAncientQuagaEnrage, 4.1f, 10, "Enrage");
    }

    private void AncientQuaga(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SAncientQuaga : (uint)AID.NAncientQuaga, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GravityForce(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SGravityForce : (uint)AID.NGravityForce, delay, 5, "Stack")
            .ActivateOnEnter<NGravityForce>(!_savage)
            .ActivateOnEnter<SGravityForce>(_savage)
            .DeactivateOnExit<GravityForce>();
    }

    private void IsleDrop(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SIsleDrop : (uint)AID.NIsleDrop, delay, 5, "Puddle")
            .ActivateOnEnter<NIsleDrop>(!_savage)
            .ActivateOnEnter<SIsleDrop>(_savage)
            .DeactivateOnExit<IsleDrop>();
    }
}
class C030NIslekeeperStates(BossModule module) : C030IslekeeperStates(module, false);
class C030SIslekeeperStates(BossModule module) : C030IslekeeperStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NIslekeeper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12561, SortOrder = 7)]
public class C030NIslekeeper(WorldState ws, Actor primary) : C030Trash2(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SIslekeeper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12561, SortOrder = 7)]
public class C030SIslekeeper(WorldState ws, Actor primary) : C030Trash2(ws, primary);
