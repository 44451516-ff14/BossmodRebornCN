namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash2;

abstract class GravityForce(BossModule module, uint aid) : Components.StackWithCastTargets(module, aid, 6f, 4, 4);
sealed class NGravityForce(BossModule module) : GravityForce(module, (uint)AID.NGravityForce);
sealed class SGravityForce(BossModule module) : GravityForce(module, (uint)AID.SGravityForce);

abstract class IsleDrop(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
sealed class NIsleDrop(BossModule module) : IsleDrop(module, (uint)AID.NIsleDrop);
sealed class SIsleDrop(BossModule module) : IsleDrop(module, (uint)AID.SIsleDrop);

abstract class C030IslekeeperStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030IslekeeperStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        AncientQuaga(id, 11.9f);
        GravityForce(id + 0x10000u, 6.3f);
        IsleDrop(id + 0x20000u, 2.1f);
        AncientQuaga(id + 0x30000u, 8.5f);
        Cast(id + 0x40000u, _savage ? AID.SAncientQuagaEnrage : AID.NAncientQuagaEnrage, 4.1f, 10f, "Enrage");
    }

    private void AncientQuaga(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAncientQuaga : AID.NAncientQuaga, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GravityForce(uint id, float delay)
    {
        Cast(id, _savage ? AID.SGravityForce : AID.NGravityForce, delay, 5f, "Stack")
            .ActivateOnEnter<NGravityForce>(!_savage)
            .ActivateOnEnter<SGravityForce>(_savage)
            .DeactivateOnExit<GravityForce>();
    }

    private void IsleDrop(uint id, float delay)
    {
        Cast(id, _savage ? AID.SIsleDrop : AID.NIsleDrop, delay, 5f, "Puddle")
            .ActivateOnEnter<NIsleDrop>(!_savage)
            .ActivateOnEnter<SIsleDrop>(_savage)
            .DeactivateOnExit<IsleDrop>();
    }
}

sealed class C030NIslekeeperStates(BossModule module) : C030IslekeeperStates(module, false);
sealed class C030SIslekeeperStates(BossModule module) : C030IslekeeperStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NIslekeeper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979u, NameID = 12561u, SortOrder = 7)]
public sealed class C030NIslekeeper(WorldState ws, Actor primary) : C030Trash2(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SIslekeeper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980u, NameID = 12561u, SortOrder = 7)]
public sealed class C030SIslekeeper(WorldState ws, Actor primary) : C030Trash2(ws, primary);
