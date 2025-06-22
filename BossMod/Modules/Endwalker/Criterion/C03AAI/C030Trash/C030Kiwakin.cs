namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash1;

class LeadHook(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.NLeadHook, (uint)AID.NLeadHookAOE1,
(uint)AID.NLeadHookAOE2, (uint)AID.SLeadHook, (uint)AID.SLeadHookAOE1, (uint)AID.SLeadHookAOE2]);

abstract class TailScrew(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 4f);
class NTailScrew(BossModule module) : TailScrew(module, (uint)AID.NTailScrew);
class STailScrew(BossModule module) : TailScrew(module, (uint)AID.STailScrew);

class C030KiwakinStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030KiwakinStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
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
        SharpStrike(id + 0x10000, 3.4f);
        TailScrew(id + 0x20000, 4.2f);
        LeadHook(id + 0x30000, 15.1f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void LeadHook(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SLeadHook : (uint)AID.NLeadHook, delay, 4)
            .ActivateOnEnter<LeadHook>();
        ComponentCondition<LeadHook>(id + 2, 0.1f, comp => comp.NumCasts > 0, "Mini tankbuster hit 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<LeadHook>(id + 3, 1.1f, comp => comp.NumCasts > 1)
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<LeadHook>(id + 4, 1.1f, comp => comp.NumCasts > 2, "Mini tankbuster hit 3")
            .DeactivateOnExit<LeadHook>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SharpStrike(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SSharpStrike : (uint)AID.NSharpStrike, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TailScrew(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.STailScrew : (uint)AID.NTailScrew, delay, 5f, "AOE");
    }
}
class C030NKiwakinStates(BossModule module) : C030KiwakinStates(module, false);
class C030SKiwakinStates(BossModule module) : C030KiwakinStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12632, SortOrder = 1)]
public class C030NKiwakin(WorldState ws, Actor primary) : C030Trash1(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SKiwakin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12632, SortOrder = 1)]
public class C030SKiwakin(WorldState ws, Actor primary) : C030Trash1(ws, primary);
