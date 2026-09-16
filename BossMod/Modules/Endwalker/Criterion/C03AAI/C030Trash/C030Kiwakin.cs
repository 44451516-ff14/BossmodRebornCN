namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash1;

sealed class LeadHook(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.NLeadHook, (uint)AID.NLeadHookAOE1,
(uint)AID.NLeadHookAOE2, (uint)AID.SLeadHook, (uint)AID.SLeadHookAOE1, (uint)AID.SLeadHookAOE2]);

abstract class TailScrew(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 4f);
sealed class NTailScrew(BossModule module) : TailScrew(module, (uint)AID.NTailScrew);
sealed class STailScrew(BossModule module) : TailScrew(module, (uint)AID.STailScrew);

abstract class C030KiwakinStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030KiwakinStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<NTailScrew>(!_savage)
            .ActivateOnEnter<STailScrew>(_savage)
            .ActivateOnEnter<NWater>(!_savage) // note: second pack is often pulled together with first one
            .ActivateOnEnter<SWater>(_savage)
            .ActivateOnEnter<BubbleShowerCrabDribble>()
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        LeadHook(id, 8.1f);
        SharpStrike(id + 0x10000u, 3.4f);
        TailScrew(id + 0x20000u, 4.2f);
        LeadHook(id + 0x30000u, 15.1f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void LeadHook(uint id, float delay)
    {
        Cast(id, _savage ? AID.SLeadHook : AID.NLeadHook, delay, 4f)
            .ActivateOnEnter<LeadHook>();
        ComponentCondition<LeadHook>(id + 2u, 0.1f, static comp => comp.NumCasts > 0, "Mini tankbuster hit 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<LeadHook>(id + 3u, 1.1f, static comp => comp.NumCasts > 1)
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<LeadHook>(id + 4u, 1.1f, static comp => comp.NumCasts > 2, "Mini tankbuster hit 3")
            .DeactivateOnExit<LeadHook>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SharpStrike(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSharpStrike : AID.NSharpStrike, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TailScrew(uint id, float delay)
    {
        Cast(id, _savage ? AID.STailScrew : AID.NTailScrew, delay, 5f, "AOE");
    }
}

sealed class C030NKiwakinStates(BossModule module) : C030KiwakinStates(module, false);
sealed class C030SKiwakinStates(BossModule module) : C030KiwakinStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979u, NameID = 12632u, SortOrder = 1)]
public sealed class C030NKiwakin(WorldState ws, Actor primary) : C030Trash1(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980u, NameID = 12632u, SortOrder = 1)]
public sealed class C030SKiwakin(WorldState ws, Actor primary) : C030Trash1(ws, primary);
